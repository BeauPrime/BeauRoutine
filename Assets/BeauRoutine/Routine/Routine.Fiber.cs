/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Fiber.cs
 * Purpose: Substrate on which Routines are executed.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BeauRoutine
{
    public partial struct Routine
    {
        // Executes a routine.
        private sealed class Fiber : IComparable<Fiber>
        {
            private static readonly IntPtr TYPEHANDLE_INT = typeof(int).TypeHandle.Value;
            private static readonly IntPtr TYPEHANDLE_FLOAT = typeof(float).TypeHandle.Value;
            private static readonly IntPtr TYPEHANDLE_ROUTINE = typeof(Routine).TypeHandle.Value;
            private static readonly IntPtr TYPEHANDLE_WWW = typeof(WWW).TypeHandle.Value;
            private static readonly IntPtr TYPEHANDLE_COMMAND = typeof(Command).TypeHandle.Value;

            private const byte FLAG_PAUSED = 0x01;
            private const byte FLAG_DISPOSING = 0x02;
            private const byte FLAG_CHAINED = 0x04;
            private const byte FLAG_IGNOREOBJECTTIMESCALE = 0x08;

            private Routine m_Handle;
            private MonoBehaviour m_Host;

            private GameObject m_HostGameObject;
            private RoutineIdentity m_HostIdentity;

            private IEnumerator m_RootFunction;
            private Stack<IEnumerator> m_Stack = new Stack<IEnumerator>(8);

            private float m_WaitTime = 0.0f;
            private Coroutine m_UnityWait = null;

            private int m_GroupMask;
            private byte m_Flags;
            private string m_Name;
            private int m_Priority = 0;

            private float m_TimeScale = 1.0f;

            private byte m_Counter = 0;

            private event Action m_OnComplete;
            private event Action m_OnStop;

            public Fiber(uint inIndex)
            {
                Index = inIndex;
            }

            /// <summary>
            /// Index in the Fiber Table.
            /// </summary>
            public readonly uint Index;

            #region Lifecycle

            /// <summary>
            /// Sets up a Fiber to run with the given host and routine.
            /// </summary>
            public Routine Initialize(MonoBehaviour inHost, IEnumerator inStart, bool inChained)
            {
                m_Counter = (byte)(m_Counter == byte.MaxValue ? 1 : m_Counter + 1);

                m_Handle = new Routine(Index, m_Counter);
                m_Host = inHost;

                m_HostGameObject = m_Host.gameObject;
                m_HostIdentity = RoutineIdentity.Find(m_HostGameObject);

                m_WaitTime = 0;
                m_UnityWait = null;
                m_Name = null;
                m_Priority = 0;

                m_GroupMask = ReferenceEquals(m_HostIdentity, null) ? 0 : 1 << m_HostIdentity.Group;

                m_Flags = inChained ? FLAG_CHAINED : (byte)0;

                m_TimeScale = 1.0f;

                m_RootFunction = inStart;
                m_Stack.Push(inStart);

                IRoutineEnumerator callback = inStart as IRoutineEnumerator;
                if (callback != null)
                {
                    if (!callback.OnRoutineStart())
                    {
                        ClearStack();
                        Stop();
                    }
                }

                return m_Handle;
            }

            // Recycles the Fiber.
            private void Dispose()
            {
                if (m_Handle.m_Value == 0)
                    return;

                if (m_UnityWait != null)
                {
                    s_Manager.StopCoroutine(m_UnityWait);
                    m_UnityWait = null;
                }

                bool bKilled = m_Stack.Count > 0;
                bool bChained = (m_Flags & FLAG_CHAINED) != 0;

                ClearStack();
                m_RootFunction = null;

                m_Handle = Routine.Null;
                m_Host = null;

                m_WaitTime = 0;
                m_GroupMask = 0;
                m_Flags = 0;
                m_Name = null;
                m_Priority = 0;

                m_TimeScale = 1.0f;

                if (!bChained)
                    s_Table.RemoveActiveFiber(this);
                s_Table.AddFreeFiber(this);

                if (bKilled)
                {
                    m_OnComplete = null;
                    Action onStop = m_OnStop;
                    m_OnStop = null;
                    if (onStop != null)
                        onStop();
                }
                else
                {
                    m_OnStop = null;
                    Action onComplete = m_OnComplete;
                    m_OnComplete = null;
                    if (onComplete != null)
                        onComplete();
                }
            }

            // Rewinds the enumerator stack.
            private void ClearStack()
            {
                IEnumerator enumerator;
                while (m_Stack.Count > 0)
                {
                    enumerator = m_Stack.Pop();

                    // All auto-generated coroutines are also IDisposable
                    // in order to handle "using" and "try...finally" blocks.
                    ((IDisposable)enumerator).Dispose();
                }
                m_RootFunction = null;
            }

            #endregion

            #region Flow

            /// <summary>
            /// Requests the Fiber pause execution.
            /// </summary>
            public void Pause()
            {
                m_Flags |= FLAG_PAUSED;
            }

            /// <summary>
            /// Requests the Fiber resume execution.
            /// </summary>
            public void Resume()
            {
                m_Flags = (byte)((int)m_Flags & ~FLAG_PAUSED);
            }
            
            /// <summary>
            /// Requests the Fiber end dispose itself.
            /// </summary>
            public void Stop()
            {
                m_Flags |= FLAG_DISPOSING;
            }

            /// <summary>
            /// Time scale for the individual routine.
            /// </summary>
            public float TimeScale
            {
                get { return m_TimeScale; }
                set { m_TimeScale = value; }
            }

            /// <summary>
            /// Execution priority.
            /// </summary>
            public int Priority
            {
                get { return m_Priority; }
                set
                {
                    if (m_Priority != value)
                    {
                        m_Priority = value;
                        s_NeedsSort = true;
                    }
                }
            }

            /// <summary>
            /// Uses time scale for the object.
            /// </summary>
            public void UseObjectTimeScale()
            {
                m_Flags = (byte)((int)m_Flags & ~FLAG_IGNOREOBJECTTIMESCALE);
            }

            /// <summary>
            /// Ignores time scale on the object.
            /// </summary>
            public void IgnoreObjectTimeScale()
            {
                m_Flags |= FLAG_IGNOREOBJECTTIMESCALE;
            }

            /// <summary>
            /// Optional name to use for finding Routines.
            /// </summary>
            public string Name
            {
                get
                {
                    CheckAutoName();
                    return m_Name;
                }
                set { m_Name = value; }
            }

            #endregion

            #region Update

            /// <summary>
            /// Runs the Fiber one frame.
            /// Will dispose itself if requested.
            /// Returns if still running.
            /// </summary>
            public bool Run()
            {
                if (m_Handle.m_Value == 0)
                    return false;

                bool bContinue = Update();
                if (!bContinue)
                    Dispose();
                return bContinue;
            }

            // Updates the routine.
            // Returns if still running.
            private bool Update()
            {
                if (ShouldDispose())
                    return false;

                if (IsPaused() || m_UnityWait != null)
                    return true;

                ApplyDeltaTime();

                if (m_WaitTime > 0)
                {
                    m_WaitTime -= s_ScaledDeltaTime;
                    if (m_WaitTime > 0)
                        return true;
                }

                IEnumerator current = m_Stack.Peek();
                bool bMovedNext = current.MoveNext();

                if (ShouldDispose())
                    return false;

                if (bMovedNext)
                {
                    object result = current.Current;
                    if (result == null)
                        return true; ;

                    IntPtr resultType = result.GetType().TypeHandle.Value;

                    // Check all the easy-to-identify result types

                    if (resultType == TYPEHANDLE_INT)
                    {
                        m_WaitTime = (int)result;
                        return true;
                    }

                    if (resultType == TYPEHANDLE_FLOAT)
                    {
                        m_WaitTime = (float)result;
                        return true;
                    }

                    if (resultType == TYPEHANDLE_ROUTINE)
                    {
                        IEnumerator waitSequence = ((Routine)result).Wait();
                        if (waitSequence != null)
                        {
                            m_Stack.Push(waitSequence);
                            return true;
                        }
                    }

                    if (resultType == TYPEHANDLE_WWW)
                    {
                        m_UnityWait = s_Manager.StartCoroutine(UnityWait((WWW)result));
                        return true;
                    }

                    if (resultType == TYPEHANDLE_COMMAND)
                    {
                        Command c = (Command)result;
                        switch(c)
                        {
                            case Command.Pause:
                                Pause();
                                return true;
                            case Command.Stop:
                                Stop();
                                return false;
                        }
                        return true;
                    }

                    // Check for the subclassable types

                    CustomYieldInstruction customInstruction = result as CustomYieldInstruction;
                    if (customInstruction != null)
                    {
                        m_UnityWait = s_Manager.StartCoroutine(UnityWait(customInstruction));
                        return true;
                    }

                    YieldInstruction instruction = result as YieldInstruction;
                    if (instruction != null)
                    {
                        m_UnityWait = s_Manager.StartCoroutine(UnityWait(instruction));
                        return true;
                    }

                    IEnumerator enumerator = result as IEnumerator;
                    if (enumerator != null)
                    {
                        m_Stack.Push(enumerator);

                        IRoutineEnumerator callback = enumerator as IRoutineEnumerator;
                        if (callback != null)
                            return callback.OnRoutineStart();

                        return true;
                    }
                }
                else
                {
                    m_Stack.Pop();
                    ((IDisposable)current).Dispose();
                    if (m_Stack.Count == 0)
                        return false;
                }

                return true;
            }

            // Applies time modifiers for this Fiber.
            private void ApplyDeltaTime()
            {
                // If we're a chained routine, just accept
                // the parent's delta time.
                if ((m_Flags & FLAG_CHAINED) > 0)
                    return;

                float timeScale = m_TimeScale;
                if (!ReferenceEquals(m_HostIdentity, null))
                {
                    // If we haven't been explicitly told to ignore the object's
                    // time scale, use it.
                    if ((m_Flags & FLAG_IGNOREOBJECTTIMESCALE) == 0)
                        timeScale *= m_HostIdentity.TimeScale;
                    timeScale *= s_GroupTimeScales[m_HostIdentity.Group];
                }

                ScaleDeltaTime(timeScale);
            }

            #endregion

            #region Status Checking

            /// <summary>
            /// Returns the handle for the Fiber.
            /// </summary>
            public Routine GetHandle()
            {
                return m_Handle;
            }

            /// <summary>
            /// Returns if the Fiber has the given handle.
            /// </summary>
            public bool HasHandle(Routine inRoutine)
            {
                return inRoutine.m_Value == m_Handle.m_Value;
            }

            /// <summary>
            /// Returns if the host is the given MonoBehaviour.
            /// </summary>
            public bool HasHost(MonoBehaviour inHost)
            {
                return ReferenceEquals(m_Host, inHost);
            }

            /// <summary>
            /// Returns if the host belongs to the given GameObject.
            /// </summary>
            public bool HasHost(GameObject inHost)
            {
                return ReferenceEquals(m_HostGameObject, inHost);
            }

            /// <summary>
            /// Returns if the routine has the given name.
            /// </summary>
            public bool HasName(string inName)
            {
                CheckAutoName();
                return m_Name == inName;
            }

            // Returns if the Fiber should dispose itself.
            private bool ShouldDispose()
            {
                return (m_Flags & FLAG_DISPOSING) > 0 || !m_Host;
            }

            // Returns if the Fiber has been paused.
            private bool IsPaused()
            {
                return (m_Flags & FLAG_PAUSED) > 0 || (s_PausedGroups & m_GroupMask) != 0 || !m_Host.isActiveAndEnabled;
            }

            // Returns if this fiber is running.
            public bool IsRunning()
            {
                return m_Handle.m_Value > 0;
            }

            #endregion

            #region Wait

            /// <summary>
            /// Waits for the routine to complete.
            /// </summary>
            public IEnumerator Wait()
            {
                return new WaitEnumerator(this);
            }

            private sealed class WaitEnumerator : IEnumerator, IDisposable
            {
                private Fiber m_Fiber;
                private uint m_Current;

                public WaitEnumerator(Fiber inFiber)
                {
                    m_Fiber = inFiber;
                    m_Current = m_Fiber.m_Handle.m_Value;
                }

                public void Dispose()
                {
                    m_Current = 0;
                }

                public object Current
                {
                    get { return null; }
                }

                public bool MoveNext()
                {
                    return m_Current > 0 && m_Fiber.m_Handle.m_Value == m_Current;
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }

                public override string ToString()
                {
                    return "Routine::Wait()";
                }
            }

            #endregion

            #region Callbacks

            /// <summary>
            /// Registers a callback for when the routine is completed.
            /// </summary>
            public void OnComplete(Action inCallback)
            {
                m_OnComplete += inCallback;
            }

            /// <summary>
            /// Registers a callback for when the routine is stopped prematurely.
            /// </summary>
            public void OnStop(Action inCallback)
            {
                m_OnStop += inCallback;
            }

            #endregion

            #region Unity coroutines

            // Waits for the YieldInstruction to finish.
            private IEnumerator UnityWait(YieldInstruction inYieldInstruction)
            {
                yield return inYieldInstruction;
                m_UnityWait = null;
            }

            // Waits for the YieldInstruction to finish.
            private IEnumerator UnityWait(CustomYieldInstruction inYieldInstruction)
            {
                yield return inYieldInstruction;
                m_UnityWait = null;
            }

            // Waits for the WWW to finish loading
            private IEnumerator UnityWait(WWW inWWW)
            {
                yield return inWWW;
                m_UnityWait = null;
            }

            #endregion

            #region Auto Naming

            private void CheckAutoName()
            {
                if (m_Name == null)
                    m_Name = GetTypeName(m_RootFunction.GetType());
            }

            static private Dictionary<IntPtr, string> s_IteratorNames = new Dictionary<IntPtr, string>();

            static private string GetTypeName(Type inType)
            {
                IntPtr typeID = inType.TypeHandle.Value;
                string name;
                if (!s_IteratorNames.TryGetValue(typeID, out name))
                {
                    name = CleanTypeName(inType);
                    s_IteratorNames.Add(typeID, name);
                }
                return name;
            }

            static private string CleanTypeName(Type inType)
            {
                string name = inType.Name;
                if (name.IndexOfAny(INVALID_NAME_CHARS) < 0)
                    return name;

                // Find the portion enclosed within the angular brackets
                // Compiler-generated functions are named something like <FunctionName>c__Iterator78593
                int openBracketIndex = name.IndexOf('<');
                if (openBracketIndex >= 0)
                {
                    int closingBracketIndex = name.IndexOf('>', openBracketIndex + 1);
                    if (closingBracketIndex >= 0)
                        name = name.Substring(openBracketIndex + 1, closingBracketIndex - openBracketIndex - 1);
                    else
                        name = name.Substring(openBracketIndex + 1);
                }

                return name;
            }

            #endregion

            #region Debugger

#if UNITY_EDITOR
            public Editor.RoutineStats GetStats()
            {
                Editor.RoutineStats stats = new Editor.RoutineStats();
                stats.Handle = m_Handle;
                stats.Host = ReferenceEquals(m_Host, s_Manager) ? null : m_Host;

                if (ShouldDispose())
                    stats.State = Editor.RoutineState.Disposing;
                else if (IsPaused())
                    stats.State = Editor.RoutineState.Paused;
                else if (m_WaitTime > 0)
                    stats.State = Editor.RoutineState.WaitTime;
                else if (m_UnityWait != null)
                    stats.State = Editor.RoutineState.WaitUnity;
                else
                    stats.State = Editor.RoutineState.Running;

                stats.TimeScale = m_TimeScale;
                stats.Name = Name;
                stats.Priority = m_Priority;

                if (m_Stack.Count > 0)
                {
                    IEnumerator current = m_Stack.Peek();
                    stats.Function = CleanIteratorName(current);

                    // HACK - to visualize combine iterators properly
                    CombineIterator combine = current as CombineIterator;
                    if (combine != null)
                        stats.Nested = combine.GetStats();
                }
                else
                {
                    stats.Function = null;
                }

                stats.StackDepth = m_Stack.Count;

                return stats;
            }
#endif

            /// <summary>
            /// Writes a description of the Fiber to the StringBuilder.
            /// </summary>
            public void WriteToString(StringBuilder inBuilder)
            {
                if ((m_Flags & FLAG_CHAINED) == 0)
                {
                    inBuilder.Append("HOST/");
                    if (m_Host == null)
                        inBuilder.Append("[Null]");
                    else if (ReferenceEquals(m_Host, s_Manager))
                        inBuilder.Append("[Manager]");
                    else
                        inBuilder.Append(m_Host.ToString());

                    if (!string.IsNullOrEmpty(m_Name))
                    {
                        inBuilder.Append("; NAME/");
                        inBuilder.Append(m_Name);
                    }

                    inBuilder.Append("; ");
                }

                inBuilder.Append("STATUS/");
                if (ShouldDispose())
                    inBuilder.Append("Disposing");
                else if (IsPaused())
                    inBuilder.Append("Paused");
                else if (m_WaitTime > 0)
                    inBuilder.Append("Waiting ").Append(m_WaitTime.ToString("0.00")).Append('s');
                else if (m_UnityWait != null)
                    inBuilder.Append("Waiting Unity");
                else
                    inBuilder.Append("Running");

                inBuilder.Append("; FUNCTION/");
                if (m_Stack.Count > 0)
                {
                    inBuilder.Append(m_Stack.Count - 1).Append('/');
                    inBuilder.Append(CleanIteratorName(m_Stack.Peek()));
                }
                else
                    inBuilder.Append("[Null]");
            }

            static public string CleanIteratorName(IEnumerator inEnumerator)
            {
                Type type = inEnumerator.GetType();
                string iteratorName = inEnumerator.ToString();
                string typeName = type.FullName;

                if (iteratorName != typeName || iteratorName.IndexOfAny(INVALID_NAME_CHARS) < 0)
                    return iteratorName;

                string functionName = GetTypeName(type);
                
                // If the function is a child of the parent, then we should get the appropriate path
                // to the function name here
                Type parentType = inEnumerator.GetType().DeclaringType;
                if (parentType != null)
                {
                    iteratorName = parentType.FullName.Replace(".", "::") + "::" + functionName;
                }
                else
                {
                    iteratorName = functionName;
                }

                List<FieldInfo> fields = new List<FieldInfo>();
                foreach(var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (field.Name.IndexOfAny(INVALID_NAME_CHARS) < 0)
                        fields.Add(field);
                }

                if (fields.Count == 0)
                {
                    iteratorName += "()";
                }
                else
                {
                    iteratorName += "(";
                    for(int i = 0; i < fields.Count; ++i)
                    {
                        if (i > 0)
                            iteratorName += ", ";
                        iteratorName += fields[i].Name + ":";
                        object val = fields[i].GetValue(inEnumerator);
                        iteratorName += val == null ? "null" : val.ToString();
                    }
                    iteratorName += ")";
                }
                return iteratorName;
            }

            static private readonly char[] INVALID_NAME_CHARS = new char[] { '$', '<', '>' };

            #endregion

            #region IComparable

            public int CompareTo(Fiber other)
            {
                return -m_Priority.CompareTo(other.m_Priority);
            }

            #endregion
        }
    }
}

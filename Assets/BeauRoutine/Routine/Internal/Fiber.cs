/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 Apr 2017
 * 
 * File:    Fiber.cs
 * Purpose: Substrate on which Routines are executed.
*/

// CustomYieldInstructions were not introduced until 5.3
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
    #define SUPPORTS_CUSTOMYIELDINSTRUCTION
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BeauRoutine.Internal
{
    public sealed class Fiber
    {
        static private readonly IntPtr TYPEHANDLE_INT = typeof(int).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_FLOAT = typeof(float).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_DOUBLE = typeof(double).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_ROUTINE = typeof(Routine).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_WWW = typeof(WWW).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_COMMAND = typeof(Routine.Command).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_DECORATOR = typeof(RoutineDecorator).TypeHandle.Value;

        private bool m_Paused;
        private bool m_Disposing;
        private bool m_Chained;
        private bool m_IgnoreObjectTimescale;
        private bool m_IgnoreObjectActive;
        private bool m_HostedByManager;
        private bool m_HasIdentity;

        private Routine m_Handle;
        private MonoBehaviour m_Host;
        private RoutineIdentity m_HostIdentity;

        private IEnumerator m_RootFunction;
        private IEnumerator[] m_Stack;
        private short m_StackPosition;
        private short m_StackSize;

        private float m_WaitTime = 0.0f;
        private Coroutine m_UnityWait = null;

        private string m_Name;

        // HACK: Public variable instead of private here.
        // Unity's compiler won't always inline accessors,
        // so this saves a tiny bit of time when sorting
        public int Priority = 0;

        private float m_TimeScale = 1.0f;

        private Action m_OnComplete;
        private Action m_OnStop;

        public Fiber(Manager inManager, uint inIndex)
        {
            Manager = inManager;
            Index = inIndex;
            m_Stack = new IEnumerator[4];
            m_StackPosition = -1;
            m_StackSize = 4;
        }

        /// <summary>
        /// Manager running the fibers.
        /// </summary>
        public readonly Manager Manager;

        /// <summary>
        /// Index in the Fiber Table.
        /// </summary>
        public readonly uint Index;

        private byte m_Counter = 0;

        #region Lifecycle

        /// <summary>
        /// Sets up a Fiber to run with the given host and routine.
        /// </summary>
        public Routine Initialize(MonoBehaviour inHost, IEnumerator inStart, bool inChained)
        {
            if (Manager.DebugMode && !(inStart is IDisposable))
                throw new ArgumentException("IEnumerators must also implement IDisposable.");

            m_Counter = (byte)(m_Counter == byte.MaxValue ? 1 : m_Counter + 1);

            m_Handle = (Routine)Table.GenerateID(Index, m_Counter);
            m_Host = inHost;

            m_HostIdentity = RoutineIdentity.Find(m_Host.gameObject);

            m_WaitTime = 0;

            m_UnityWait = null;
            m_Name = null;
            Priority = 0;

            // Chained routines are always hosted on the manager
            if (inChained)
                m_Chained = m_HostedByManager = true;

            if (!inChained && ReferenceEquals(inHost, Manager.Host))
                m_HostedByManager = true;

            if (!ReferenceEquals(m_HostIdentity, null))
                m_HasIdentity = true;

            m_TimeScale = 1.0f;

            m_RootFunction = inStart;
            m_Stack[m_StackPosition = 0] = inStart;

            return m_Handle;
        }

        /// <summary>
        /// Cleans up the Fiber.
        /// </summary>
        public void Dispose()
        {
            if ((uint)m_Handle == 0)
                return;

            if (m_UnityWait != null)
            {
                Manager.Host.StopCoroutine(m_UnityWait);
                m_UnityWait = null;
            }

            bool bKilled = m_StackPosition >= 0;
            bool bChained = m_Chained;

            ClearStack();

            m_Handle = Routine.Null;
            m_Host = null;
            m_HostIdentity = null;

            m_Chained = m_Disposing = m_HasIdentity
            = m_Paused = m_IgnoreObjectTimescale = m_HostedByManager
            = m_IgnoreObjectActive = false;

            m_WaitTime = 0;
            m_Name = null;
            Priority = 0;

            m_TimeScale = 1.0f;

            Manager.RecycleFiber(this, bChained);

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
            while (m_StackPosition >= 0)
            {
                enumerator = m_Stack[m_StackPosition];
                m_Stack[m_StackPosition--] = null;

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
            m_Paused = true;
        }

        /// <summary>
        /// Requests the Fiber resume execution.
        /// </summary>
        public void Resume()
        {
            m_Paused = false;
        }

        /// <summary>
        /// Requests the Fiber stop itself.
        /// </summary>
        public void Stop()
        {
            m_Disposing = true;
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
        /// Sets the execution priority.
        /// </summary>
        public void SetPriority(int inPriority)
        {
            if (Priority != inPriority)
            {
                Priority = inPriority;
                Manager.Fibers.MarkSortDirty();
            }
        }

        /// <summary>
        /// Uses time scale for the object.
        /// </summary>
        public void UseObjectTimeScale()
        {
            m_IgnoreObjectTimescale = false;
        }

        /// <summary>
        /// Ignores time scale on the object.
        /// </summary>
        public void IgnoreObjectTimeScale()
        {
            m_IgnoreObjectTimescale = true;
        }

        /// <summary>
        /// Uses object's active state.
        /// </summary>
        public void UseObjectActive()
        {
            m_IgnoreObjectActive = false;
        }

        /// <summary>
        /// Ignores object's active state.
        /// </summary>
        public void IgnoreObjectActive()
        {
            m_IgnoreObjectActive = true;
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
            if ((uint)m_Handle == 0)
            {
                return false;
            }

            if (m_Disposing || (!m_HostedByManager && !m_Host))
            {
                Dispose();
                return false;
            }

            if (IsPaused() || m_UnityWait != null)
                return true;

            ApplyDeltaTime();

            if (m_WaitTime > 0)
            {
                m_WaitTime -= Manager.Frame.DeltaTime;
                if (m_WaitTime > 0)
                    return true;
            }

            bool bExecuteStack = true;

            while (bExecuteStack)
            {
                bExecuteStack = false;
                
                IEnumerator current = m_Stack[m_StackPosition];
                bool bMovedNext = current.MoveNext();

                // Don't check for the object being destroyed.
                // Destroy won't go into effect until after
                // all the Fibers are done processing anyways.
                if (m_Disposing)
                {
                    Dispose();
                    return false;
                }

                if (bMovedNext)
                {
                    object result = current.Current;
                    if (result == null)
                        return true;

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

                    if (resultType == TYPEHANDLE_DOUBLE)
                    {
                        m_WaitTime = (float)(double)result;
                        return true;
                    }

                    if (resultType == TYPEHANDLE_ROUTINE)
                    {
                        IEnumerator waitSequence = ((Routine)result).Wait();
                        if (waitSequence != null)
                        {
                            if (m_StackPosition == m_StackSize - 1)
                                Array.Resize(ref m_Stack, m_StackSize *= 2);
                            m_Stack[++m_StackPosition] = waitSequence;
                        }
                        return true;
                    }

                    if (resultType == TYPEHANDLE_WWW)
                    {
                        m_UnityWait = Manager.Host.StartCoroutine(UnityWait((WWW)result));
                        return true;
                    }

                    if (resultType == TYPEHANDLE_COMMAND)
                    {
                        Routine.Command c = (Routine.Command)result;
                        switch (c)
                        {
                            case Routine.Command.Pause:
                                Pause();
                                return true;
                            case Routine.Command.Stop:
                                Stop();
                                Dispose();
                                return false;
                            case Routine.Command.BreakAndResume:
                                m_Stack[m_StackPosition--] = null;
                                ((IDisposable)current).Dispose();
                                if (m_StackPosition < 0)
                                {
                                    Dispose();
                                    return false;
                                }
                                bExecuteStack = true;
                                break;
                        }

                        if (!bExecuteStack)
                            return true;

                        continue;
                    }

                    if (resultType == TYPEHANDLE_DECORATOR)
                    {
                        RoutineDecorator decorator = (RoutineDecorator)result;
                        IEnumerator decoratedEnumerator = decorator.Enumerator;
                        bExecuteStack = (decorator.Flags & RoutineDecoratorFlag.Immediate) != 0;

                        if (decoratedEnumerator != null)
                        {
                            // Check if we need to resize the stack
                            if (m_StackPosition == m_StackSize - 1)
                                Array.Resize(ref m_Stack, m_StackSize *= 2);
                            m_Stack[++m_StackPosition] = decoratedEnumerator;
                        }

                        if (!bExecuteStack)
                            return true;

                        continue;
                    }

                    // Check for the subclassable types

                    #if SUPPORTS_CUSTOMYIELDINSTRUCTION

                    CustomYieldInstruction customInstruction = result as CustomYieldInstruction;
                    if (customInstruction != null)
                    {
                        m_UnityWait = Manager.Host.StartCoroutine(UnityWait(customInstruction));
                        return true;
                    }

                    #endif

                    YieldInstruction instruction = result as YieldInstruction;
                    if (instruction != null)
                    {
                        m_UnityWait = Manager.Host.StartCoroutine(UnityWait(instruction));
                        return true;
                    }

                    IFuture future = result as IFuture;
                    if (future != null)
                    {
                        if (!future.IsDone())
                        {
                            IEnumerator waitSequence = future.Wait();
                            if (waitSequence != null)
                            {
                                if (m_StackPosition == m_StackSize - 1)
                                    Array.Resize(ref m_Stack, m_StackSize *= 2);
                                m_Stack[++m_StackPosition] = waitSequence;
                            }
                        }
                        return true;
                    }

                    IEnumerator enumerator = result as IEnumerator;
                    if (enumerator != null)
                    {
                        // Check if we need to resize the stack
                        if (m_StackPosition == m_StackSize - 1)
                            Array.Resize(ref m_Stack, m_StackSize *= 2);
                        m_Stack[++m_StackPosition] = enumerator;

                        return true;
                    }
                }
                else
                {
                    m_Stack[m_StackPosition--] = null;
                    ((IDisposable)current).Dispose();
                    if (m_StackPosition < 0)
                    {
                        Dispose();
                        return false;
                    }
                }
            }

            return true;
        }

        // Applies time modifiers for this Fiber.
        private void ApplyDeltaTime()
        {
            // If we're a chained routine, just accept
            // the parent's delta time.
            if (m_Chained)
                return;

            float timeScale = m_TimeScale;
            if (m_HasIdentity)
            {
                // If we haven't been explicitly told to ignore the object's
                // time scale, use it.
                if (!m_IgnoreObjectTimescale)
                    timeScale *= m_HostIdentity.TimeScale;
                timeScale *= Manager.Frame.GroupTimeScale[m_HostIdentity.Group];
            }

            Manager.Frame.SetTimeScale(timeScale);
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
            return inRoutine == m_Handle;
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
            return m_Host != null && ReferenceEquals(m_Host.gameObject, inHost);
        }

        /// <summary>
        /// Returns if the routine has the given name.
        /// </summary>
        public bool HasName(string inName)
        {
            CheckAutoName();
            return m_Name == inName;
        }

        /// <summary>
        /// Returns if the routine has the given group.
        /// </summary>
        public bool HasGroups(int inGroupMask)
        {
            return m_HasIdentity && ((1 << m_HostIdentity.Group) & inGroupMask) != 0;
        }

        // Returns if the Fiber has been paused.
        private bool IsPaused()
        {
            if (m_Paused)
                return true;
            if (m_HostedByManager)
                return false;
            return (m_HasIdentity && (Manager.Frame.PauseMask & (1 << m_HostIdentity.Group)) != 0) || (!m_IgnoreObjectActive && !m_Host.isActiveAndEnabled);
        }

        // Returns if this fiber is running.
        public bool IsRunning()
        {
            return (uint)m_Handle > 0;
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
                m_Current = (uint)m_Fiber.m_Handle;
            }

            public void Dispose()
            {
                m_Current = 0;
                m_Fiber = null;
            }

            public object Current
            {
                get { return null; }
            }

            public bool MoveNext()
            {
                return m_Current > 0 && (uint)m_Fiber.m_Handle == m_Current;
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

        #if SUPPORTS_CUSTOMYIELDINSTRUCTION

        // Waits for the CustomYieldInstruction to finish.
        private IEnumerator UnityWait(CustomYieldInstruction inYieldInstruction)
        {
            yield return inYieldInstruction;
            m_UnityWait = null;
        }

        #endif

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
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
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
                for (int i = 0; i < fields.Count; ++i)
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

        #region Debugger

        public RoutineStats GetStats()
        {
            RoutineStats stats = new RoutineStats();
            stats.Handle = m_Handle;
            stats.Host = ReferenceEquals(m_Host, Manager) ? null : m_Host;

            if (m_Disposing || (!m_HostedByManager && !m_Host))
                stats.State = RoutineState.Disposing;
            else if (IsPaused())
                stats.State = RoutineState.Paused;
            else if (m_WaitTime > 0)
                stats.State = RoutineState.WaitTime;
            else if (m_UnityWait != null)
                stats.State = RoutineState.WaitUnity;
            else
                stats.State = RoutineState.Running;

            stats.TimeScale = m_TimeScale;
            stats.Name = Name;
            stats.Priority = Priority;

            if (m_StackPosition >= 0)
            {
                IEnumerator current = m_Stack[m_StackPosition];
                stats.Function = CleanIteratorName(current);

                // HACK - to visualize combine iterators properly
                ParallelFibers combine = current as ParallelFibers;
                if (combine != null)
                    stats.Nested = combine.GetStats();
            }
            else
            {
                stats.Function = null;
            }

            stats.StackDepth = m_StackPosition + 1;

            return stats;
        }

        #endregion
    }
}

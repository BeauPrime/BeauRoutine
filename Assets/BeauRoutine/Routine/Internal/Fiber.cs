/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Apr 2017
 * 
 * File:    Fiber.cs
 * Purpose: Substrate on which Routines are executed.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

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
    internal sealed class Fiber
    {
        public const string RESERVED_NAME_PREFIX = "[BeauRoutine]__";

        static private readonly IntPtr TYPEHANDLE_INT = typeof(int).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_FLOAT = typeof(float).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_DOUBLE = typeof(double).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_ROUTINE = typeof(Routine).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_COMMAND = typeof(Routine.Command).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_DECORATOR = typeof(RoutineDecorator).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_WAITFORFIXEDUPDATE = typeof(WaitForFixedUpdate).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_WAITFORENDOFFRAME = typeof(WaitForEndOfFrame).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_WAITFORLATEUPDATE = typeof(WaitForLateUpdate).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_WAITFORCUSTOMUPDATE = typeof(WaitForCustomUpdate).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_WAITFORTHINKUPDATE = typeof(WaitForThinkUpdate).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_WAITFORREALTIMEUPDATE = typeof(WaitForRealtimeUpdate).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_WAITFORUPDATE = typeof(WaitForUpdate).TypeHandle.Value;
        static private readonly IntPtr TYPEHANDLE_ROUTINEPHASE = typeof(RoutinePhase).TypeHandle.Value;

        static private readonly IntPtr TYPEHANDLE_WAITFORSECONDS = typeof(WaitForSeconds).TypeHandle.Value;
        static private readonly FieldInfo FIELD_WAITFORSECONDS_SECONDS = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.NonPublic);
        static private readonly bool WAITFORSECONDS_BYPASS = FIELD_WAITFORSECONDS_SECONDS != null;

        // Disable obsolete WWW warning
        #pragma warning disable 612, 618

        static private readonly IntPtr TYPEHANDLE_WWW = typeof(WWW).TypeHandle.Value;

        // Restore obsolete WWW warning
        #pragma warning restore 612, 618

        private bool m_Paused;
        private bool m_Disposing;
        private bool m_Chained;
        private bool m_IgnoreObjectTimescale;
        private bool m_IgnoreObjectActive;
        private bool m_HostedByManager;
        private bool m_HasIdentity;

        private bool m_Executing = false;

        private Routine m_Handle;
        private MonoBehaviour m_Host;
        private RoutineIdentity m_HostIdentity;

        private INestedFiberContainer m_Container;
        private Fiber m_RootFiber;

        private IEnumerator m_RootFunction;
        private IEnumerator[] m_Stack;
        private short m_StackPosition;
        private short m_StackSize;

        private float m_WaitTime = 0.0f;
        private int m_LockCount = 0;
        private Coroutine m_UnityWait = null;
        private YieldPhase m_YieldPhase = YieldPhase.None;
        private int m_YieldFrameDelay = 0;

        private string m_Name;

        // Public, to reduce indirection when sorting
        public int Priority = 0;

        private float m_TimeScale = 1.0f;

        private RoutinePhase m_UpdatePhase;

        private Action m_OnComplete;
        private Action m_OnStop;
        private Routine.ExceptionHandler m_OnException;
        private bool m_HandleExceptions = false;

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
            #if DEVELOPMENT
            if (Manager.DebugMode && !(inStart is IDisposable))
                throw new ArgumentException("IEnumerators must also implement IDisposable.");
            #endif // DEVELOPMENT

            m_Counter = (byte) (m_Counter == byte.MaxValue ? 1 : m_Counter + 1);

            m_Handle = (Routine) Table.GenerateID(Index, m_Counter);
            m_Host = inHost;

            m_HostIdentity = RoutineIdentity.Find(m_Host.gameObject);

            m_WaitTime = 0;
            m_LockCount = 0;

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

            CheckForNesting(inStart, true);

            if (!m_Chained)
            {
                m_UpdatePhase = Manager.DefaultPhase;
                Manager.Fibers.AddFiberToUpdateList(this, m_UpdatePhase);
            }

            return m_Handle;
        }

        /// <summary>
        /// Cleans up the Fiber.
        /// </summary>
        public void Dispose()
        {
            if ((uint) m_Handle == 0)
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

            // If this is chained and we have a parallel
            if (bChained && m_Container != null)
            {
                m_Container.RemoveFiber(this);
            }

            m_Chained = m_Disposing = m_HasIdentity = m_Paused = m_IgnoreObjectTimescale = m_HostedByManager = m_IgnoreObjectActive = m_Executing = false;

            m_WaitTime = 0;
            m_LockCount = 0;
            m_Name = null;
            Priority = 0;

            m_Container = null;
            m_RootFiber = null;

            m_TimeScale = 1.0f;
            m_YieldFrameDelay = 0;

            if (m_YieldPhase != YieldPhase.None)
            {
                Manager.Fibers.RemoveFiberFromYieldList(this, m_YieldPhase);
                m_YieldPhase = YieldPhase.None;
            }

            if (!bChained)
                Manager.Fibers.RemoveFiberFromUpdateList(this, m_UpdatePhase);
            Manager.RecycleFiber(this, bChained);

            m_UpdatePhase = Manager.DefaultPhase;
            m_OnException = null;
            m_HandleExceptions = false;

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
                ((IDisposable) enumerator).Dispose();
            }
            m_RootFunction = null;
        }

        // HACK: We need to pass a parent fiber into nested fibers
        //       in order for timescale to work appropriately during a yield update.
        private void CheckForNesting(IEnumerator inEnumerator, bool inbCheckDecorator)
        {
            INestedFiberContainer container = inEnumerator as INestedFiberContainer;
            if (container != null)
                container.SetParentFiber(this);
            else if (inbCheckDecorator)
            {
                RoutineDecorator decorator = inEnumerator as RoutineDecorator;
                if (decorator != null && decorator.Enumerator != null)
                    CheckForNesting(decorator.Enumerator, true);
            }
        }

        #endregion

        #region Nesting

        /// <summary>
        /// Indicates that the Fiber is owned by the given nested container.
        /// </summary>
        public void SetNestedOwner(INestedFiberContainer inParallel)
        {
            m_Container = inParallel;
        }

        /// <summary>
        /// Clears the Fiber's nested owner.
        /// </summary>
        public void ClearNestedOwner()
        {
            m_Container = null;
        }

        /// <summary>
        /// Sets the parent Fiber to use for time scaling.
        /// </summary>
        public void SetParentFiber(Fiber inFiber)
        {
            m_RootFiber = inFiber.m_RootFiber == null ? inFiber : inFiber.m_RootFiber;
        }

        #endregion

        #region Flow

        /// <summary>
        /// Requests the Fiber pause execution.
        /// </summary>
        public void Pause()
        {
            if (m_Chained)
                return;

            m_Paused = true;
        }

        /// <summary>
        /// Requests the Fiber resume execution.
        /// </summary>
        public void Resume()
        {
            if (m_Chained)
                return;

            m_Paused = false;
        }

        /// <summary>
        /// Returns if the Fiber is currently paused.
        /// </summary>
        public bool GetPaused()
        {
            return m_Paused;
        }

        /// <summary>
        /// Requests the Fiber stop itself.
        /// </summary>
        public void Stop()
        {
            m_Disposing = true;

            if (m_UpdatePhase == RoutinePhase.Manual && !m_Executing)
            {
                Dispose();
            }
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

                Manager.Fibers.SetUpdateListDirty(m_UpdatePhase);
                if (m_YieldPhase != YieldPhase.None)
                    Manager.Fibers.SetYieldListDirty(m_YieldPhase);
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
        /// Returns the update timing.
        /// </summary>
        public RoutinePhase GetPhase()
        {
            return m_UpdatePhase;
        }

        /// <summary>
        /// Sets the update timing.
        /// </summary>
        public void SetPhase(RoutinePhase inUpdateMode)
        {
            if (m_Chained || m_UpdatePhase == inUpdateMode)
                return;

            if (inUpdateMode == RoutinePhase.Manual)
            {
                if (m_Disposing && !m_Executing)
                {
                    Dispose();
                    return;
                }

                // Clear out any special yield timing
                // Yield phases are meaningless in the context of manual updates
                if (m_YieldPhase != YieldPhase.None)
                {
                    Manager.Fibers.RemoveFiberFromYieldList(this, m_YieldPhase);
                    m_YieldPhase = YieldPhase.None;
                }
            }

            Manager.Fibers.RemoveFiberFromUpdateList(this, m_UpdatePhase);
            m_UpdatePhase = inUpdateMode;
            Manager.Fibers.AddFiberToUpdateList(this, m_UpdatePhase);
        }

        /// <summary>
        /// Delays by the given number of seconds.
        /// </summary>
        public void AddDelay(float inSeconds)
        {
            m_WaitTime += inSeconds;
        }

        /// <summary>
        /// Adds a lock on the Routine.
        /// </summary>
        public void AddLock()
        {
            ++m_LockCount;
        }

        /// <summary>
        /// Releases a lock on the Routine.
        /// </summary>
        public void ReleaseLock()
        {
            if (m_LockCount == 0)
            {
                #if DEVELOPMENT
                Debug.LogWarning("[BeauRoutine] Mismatched lock count for fiber: " + Name);
                #endif // DEVELOPMENT
                return;
            }
            --m_LockCount;
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
            set
            {
                #if DEVELOPMENT
                if (!string.IsNullOrEmpty(value) && value.StartsWith(RESERVED_NAME_PREFIX, StringComparison.Ordinal))
                {
                    Debug.LogWarning("[BeauRoutine] Cannot set name of BeauRoutine: contains reserved prefix '" + RESERVED_NAME_PREFIX + "'!");
                    return;
                }
                #endif // DEVELOPMENT
                m_Name = value;
            }
        }

        /// <summary>
        /// Sets the name, without checking for reserved prefixes.
        /// </summary>
        public void SetNameUnchecked(string inName)
        {
            m_Name = inName;
        }

        #endregion

        #region Update

        /// <summary>
        /// Returns if the Fiber will update with the given mode.
        /// </summary>
        public bool PrepareUpdate(RoutinePhase inUpdateMode)
        {
            if (m_StackPosition < 0 || m_Executing)
                return false;

            return (m_UpdatePhase == inUpdateMode);
        }

        /// <summary>
        /// Returns if the Fiber can perform a manual update.
        /// </summary>
        public bool PrepareManualUpdate()
        {
            return (m_StackPosition >= 0 && !m_Executing);
        }

        /// <summary>
        /// Runs the Fiber one frame.
        /// Will dispose itself if requested.
        /// Returns if still running.
        /// </summary>
        public bool Update(YieldPhase inYieldUpdate = YieldPhase.None)
        {
            // We don't support recursive Fiber updates
            if (m_Executing)
                return true;

            if (m_StackPosition < 0)
                return false;

            if (m_Disposing || (!m_HostedByManager && !m_Host))
            {
                Dispose();
                return false;
            }

            if (inYieldUpdate != YieldPhase.None && m_YieldFrameDelay > 0)
            {
                --m_YieldFrameDelay;
                return true;
            }

            if (IsPaused() || m_UnityWait != null)
                return true;

            if (m_YieldPhase != inYieldUpdate)
                return true;

            if (inYieldUpdate != YieldPhase.None)
            {
                Manager.Fibers.RemoveFiberFromYieldList(this, m_YieldPhase);
                m_YieldPhase = YieldPhase.None;

                // If we're in a manual update, don't proceed from here
                if (m_UpdatePhase == RoutinePhase.Manual)
                    return true;
            }

            // If we're a chained routine, just accept
            // the parent's delta time.
            if (!m_Chained)
            {
                ApplyDeltaTime();
            }
            else if (inYieldUpdate != YieldPhase.None)
            {
                m_RootFiber.ApplyDeltaTime();
            }

            if (m_WaitTime > 0)
            {
                m_WaitTime -= Manager.Frame.DeltaTime;
                if (m_WaitTime > 0)
                    return true;

                // We don't modify delta time in the fixed update phase,
                // to preserve a consistent delta time within fixed update.
                if (m_UpdatePhase != RoutinePhase.FixedUpdate && inYieldUpdate != YieldPhase.WaitForFixedUpdate)
                {
                    Manager.Frame.DeltaTime += m_WaitTime;
                }

                m_WaitTime = 0;
            }

            bool bExecuteStack = true;

            while (bExecuteStack)
            {
                bExecuteStack = false;

                // Set this flag to prevent updating this routine mid-execution
                m_Executing = true;

                IEnumerator current = m_Stack[m_StackPosition];
                bool bMovedNext = false;

                if (Manager.HandleExceptions || m_HandleExceptions || (m_Chained && m_RootFiber.m_HandleExceptions))
                {
                    try
                    {
                        bMovedNext = current.MoveNext();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);

                        Routine.ExceptionHandler exceptionHandler = m_OnException;
                        if (m_Chained)
                        {
                            exceptionHandler = m_RootFiber.m_OnException;
                            m_RootFiber.m_Disposing = true;
                        }

                        if (exceptionHandler != null)
                            exceptionHandler(e);

                        m_Disposing = true;
                    }
                }
                else
                {
                    bMovedNext = current.MoveNext();
                }

                m_Executing = false;

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
                        m_WaitTime = (int) result;
                        return true;
                    }

                    if (resultType == TYPEHANDLE_FLOAT)
                    {
                        m_WaitTime = (float) result;
                        return true;
                    }

                    if (resultType == TYPEHANDLE_DOUBLE)
                    {
                        m_WaitTime = (float) (double) result;
                        return true;
                    }

                    if (resultType == TYPEHANDLE_ROUTINE)
                    {
                        IEnumerator waitSequence = ((Routine) result).Wait();
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
                        // Disable obsolete WWW warning
                        #pragma warning disable 612, 618

                        m_UnityWait = Manager.Host.StartCoroutine(UnityWait((WWW) result));
                        return true;

                        // Restore obsolete WWW warning
                        #pragma warning restore 612, 618
                    }

                    if (resultType == TYPEHANDLE_COMMAND)
                    {
                        Routine.Command c = (Routine.Command) result;
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
                                ((IDisposable) current).Dispose();
                                if (m_StackPosition < 0)
                                {
                                    Dispose();
                                    return false;
                                }
                                bExecuteStack = true;
                                break;
                            case Routine.Command.Continue:
                                bExecuteStack = true;
                                break;
                        }

                        if (!bExecuteStack)
                            return true;

                        continue;
                    }

                    if (resultType == TYPEHANDLE_DECORATOR)
                    {
                        RoutineDecorator decorator = (RoutineDecorator) result;
                        IEnumerator decoratedEnumerator = decorator.Enumerator;
                        bExecuteStack = (decorator.Flags & RoutineDecoratorFlag.Inline) != 0;

                        if (decoratedEnumerator != null)
                        {
                            // Check if we need to resize the stack
                            if (m_StackPosition == m_StackSize - 1)
                                Array.Resize(ref m_Stack, m_StackSize *= 2);
                            m_Stack[++m_StackPosition] = decorator;

                            CheckForNesting(decoratedEnumerator, false);
                        }

                        if (!bExecuteStack)
                            return true;

                        continue;
                    }

                    // Check for yield timing changes

                    if (m_UpdatePhase != RoutinePhase.Manual)
                    {
                        bool bApplyYieldDelay = !Manager.IsUpdating(RoutinePhase.Manual) && inYieldUpdate == YieldPhase.None;

                        if (resultType == TYPEHANDLE_ROUTINEPHASE)
                        {
                            RoutinePhase phase = (RoutinePhase) result;

                            switch (phase)
                            {
                                case RoutinePhase.FixedUpdate:
                                    {
                                        Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForFixedUpdate);
                                        m_YieldPhase = YieldPhase.WaitForFixedUpdate;
                                        m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.FixedUpdate && Manager.IsUpdating(RoutinePhase.FixedUpdate) ? 1 : 0;
                                        return true;
                                    }

                                case RoutinePhase.LateUpdate:
                                    {
                                        Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForLateUpdate);
                                        m_YieldPhase = YieldPhase.WaitForLateUpdate;
                                        m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.LateUpdate && Manager.IsUpdating(RoutinePhase.LateUpdate) ? 1 : 0;
                                        return true;
                                    }

                                case RoutinePhase.Update:
                                    {
                                        Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForUpdate);
                                        m_YieldPhase = YieldPhase.WaitForUpdate;
                                        m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.Update && Manager.IsUpdating(RoutinePhase.Update) ? 1 : 0;
                                        return true;
                                    }

                                case RoutinePhase.CustomUpdate:
                                    {
                                        Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForCustomUpdate);
                                        m_YieldPhase = YieldPhase.WaitForCustomUpdate;
                                        m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.CustomUpdate && Manager.IsUpdating(RoutinePhase.CustomUpdate) ? 1 : 0;
                                        return true;
                                    }

                                case RoutinePhase.ThinkUpdate:
                                    {
                                        Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForThinkUpdate);
                                        m_YieldPhase = YieldPhase.WaitForThinkUpdate;
                                        m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.ThinkUpdate && Manager.IsUpdating(RoutinePhase.ThinkUpdate) ? 1 : 0;
                                        return true;
                                    }

                                case RoutinePhase.RealtimeUpdate:
                                    {
                                        Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForRealtimeUpdate);
                                        m_YieldPhase = YieldPhase.WaitForRealtimeUpdate;
                                        m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.RealtimeUpdate && Manager.IsUpdating(RoutinePhase.RealtimeUpdate) ? 1 : 0;
                                        return true;
                                    }

                                case RoutinePhase.Manual:
                                default:
                                    {
                                        // Yielding a manual will not do anything
                                        return true;
                                    }
                            }
                        }

                        if (resultType == TYPEHANDLE_WAITFORENDOFFRAME)
                        {
                            Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForEndOfFrame);
                            m_YieldPhase = YieldPhase.WaitForEndOfFrame;
                            m_YieldFrameDelay = 0;
                            return true;
                        }

                        if (resultType == TYPEHANDLE_WAITFORFIXEDUPDATE)
                        {
                            Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForFixedUpdate);
                            m_YieldPhase = YieldPhase.WaitForFixedUpdate;
                            m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.FixedUpdate && Manager.IsUpdating(RoutinePhase.FixedUpdate) ? 1 : 0;
                            return true;
                        }

                        if (resultType == TYPEHANDLE_WAITFORLATEUPDATE)
                        {
                            Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForLateUpdate);
                            m_YieldPhase = YieldPhase.WaitForLateUpdate;
                            m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.LateUpdate && Manager.IsUpdating(RoutinePhase.LateUpdate) ? 1 : 0;
                            return true;
                        }

                        if (resultType == TYPEHANDLE_WAITFORUPDATE)
                        {
                            Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForUpdate);
                            m_YieldPhase = YieldPhase.WaitForUpdate;
                            m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.Update && Manager.IsUpdating(RoutinePhase.Update) ? 1 : 0;
                            return true;
                        }

                        if (resultType == TYPEHANDLE_WAITFORCUSTOMUPDATE)
                        {
                            Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForCustomUpdate);
                            m_YieldPhase = YieldPhase.WaitForCustomUpdate;
                            m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.CustomUpdate && Manager.IsUpdating(RoutinePhase.CustomUpdate) ? 1 : 0;
                            return true;
                        }

                        if (resultType == TYPEHANDLE_WAITFORTHINKUPDATE)
                        {
                            Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForThinkUpdate);
                            m_YieldPhase = YieldPhase.WaitForThinkUpdate;
                            m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.ThinkUpdate && Manager.IsUpdating(RoutinePhase.ThinkUpdate) ? 1 : 0;
                            return true;
                        }

                        if (resultType == TYPEHANDLE_WAITFORREALTIMEUPDATE)
                        {
                            Manager.Fibers.AddFiberToYieldList(this, YieldPhase.WaitForRealtimeUpdate);
                            m_YieldPhase = YieldPhase.WaitForRealtimeUpdate;
                            m_YieldFrameDelay = bApplyYieldDelay && m_UpdatePhase == RoutinePhase.RealtimeUpdate && Manager.IsUpdating(RoutinePhase.RealtimeUpdate) ? 1 : 0;
                            return true;
                        }
                    }

                    if (WAITFORSECONDS_BYPASS && resultType == TYPEHANDLE_WAITFORSECONDS)
                    {
                        m_WaitTime = (float) FIELD_WAITFORSECONDS_SECONDS.GetValue(result);
                        return true;
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

                        CheckForNesting(enumerator, false);

                        return true;
                    }
                }
                else
                {
                    bExecuteStack = current is RoutineDecorator && (((RoutineDecorator) current).Flags & RoutineDecoratorFlag.Inline) != 0;
                    m_Stack[m_StackPosition--] = null;
                    ((IDisposable) current).Dispose();
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
            if (m_Paused || m_LockCount > 0)
                return true;
            if (m_HostedByManager)
                return false;
            return (m_HasIdentity && (m_HostIdentity.Paused || (Manager.Frame.PauseMask & (1 << m_HostIdentity.Group)) != 0)) ||
                (!m_IgnoreObjectActive && !m_Host.isActiveAndEnabled);
        }

        // Returns if this fiber is running.
        public bool IsRunning()
        {
            return (uint) m_Handle > 0;
        }

        public bool IsWaiting(YieldPhase inYieldUpdate)
        {
            return m_YieldPhase == inYieldUpdate;
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
                m_Current = (uint) m_Fiber.m_Handle;
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
                return m_Current > 0 && (uint) m_Fiber.m_Handle == m_Current;
            }

            public void Reset()
            {
                throw new NotSupportedException();
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

        /// <summary>
        /// Registers a callback for when a routine encounters exceptions.
        /// </summary>
        public void OnException(Routine.ExceptionHandler inCallback)
        {
            m_OnException += inCallback;
            m_HandleExceptions = true;
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

        // Disable obsolete WWW warning
        #pragma warning disable 612, 618

        // Waits for the WWW to finish loading
        private IEnumerator UnityWait(WWW inWWW)
        {
            yield return inWWW;
            m_UnityWait = null;
        }

        // Restore obsolete WWW warning
        #pragma warning restore 612, 618

        #endregion

        #region Auto Naming

        private void CheckAutoName()
        {
            if (m_Name == null)
                m_Name = GetTypeName(m_RootFunction.GetType());
        }

        static private Dictionary<IntPtr, string> s_IteratorNames = new Dictionary<IntPtr, string>();

        static internal string GetTypeName(Type inType)
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
            else if (m_LockCount > 0)
                stats.State = RoutineState.Locked + ": " + m_LockCount;
            else if (m_WaitTime > 0)
                stats.State = RoutineState.WaitTime + ": " + m_WaitTime.ToString("0.000");
            else if (IsPaused())
                stats.State = RoutineState.Paused;
            else if (m_UnityWait != null)
                stats.State = RoutineState.WaitUnity;
            else if (m_YieldPhase == YieldPhase.WaitForEndOfFrame)
                stats.State = RoutineState.WaitEndOfFrame;
            else if (m_YieldPhase == YieldPhase.WaitForFixedUpdate)
                stats.State = RoutineState.WaitFixedUpdate;
            else if (m_YieldPhase == YieldPhase.WaitForLateUpdate)
                stats.State = RoutineState.WaitLateUpdate;
            else if (m_YieldPhase == YieldPhase.WaitForUpdate)
                stats.State = RoutineState.WaitUpdate;
            else if (m_YieldPhase == YieldPhase.WaitForCustomUpdate)
                stats.State = RoutineState.WaitCustomUpdate;
            else if (m_YieldPhase == YieldPhase.WaitForThinkUpdate)
                stats.State = RoutineState.WaitThinkUpdate;
            else if (m_YieldPhase == YieldPhase.WaitForRealtimeUpdate)
                stats.State = RoutineState.WaitRealtimeUpdate;
            else
                stats.State = RoutineState.Running;

            stats.TimeScale = m_TimeScale;
            stats.Name = Name;
            stats.Priority = Priority;
            stats.Phase = m_UpdatePhase;

            if (m_StackPosition >= 0)
            {
                IEnumerator current = m_Stack[m_StackPosition];
                stats.Function = CleanIteratorName(current);

                INestedFiberContainer nested = current as INestedFiberContainer;
                if (nested != null)
                    stats.Nested = nested.GetStats();
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
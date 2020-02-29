/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Jan 2020
 * 
 * File:    AsyncWorkUnit.cs
 * Purpose: Single unit of work for an async worker.
 */

#if !UNITY_WEBGL
#define SUPPORTS_THREADING
#endif // UNITY_WEBGL

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauRoutine.Internal
{
    internal sealed class AsyncWorkUnit
    {
        internal struct StepResult
        {
            internal readonly StepResultType Type;
            internal readonly long TickDelay;

            internal StepResult(StepResultType inType, long inDelay = 0)
            {
                Type = inType;
                TickDelay = inDelay;
            }

            internal bool IsIncomplete()
            {
                return Type == StepResultType.Incomplete;
            }

            internal static readonly StepResult Incomplete = new StepResult(StepResultType.Incomplete);
            internal static readonly StepResult Complete = new StepResult(StepResultType.Complete);
            internal static readonly StepResult Cancelled = new StepResult(StepResultType.Cancelled);
        }

        internal enum StepResultType : byte
        {
            Incomplete,
            Complete,
            Cancelled
        }

        private const ushort ActionCompleteFlag = 0x01;
        private const ushort EnumeratorCompleteFlag = 0x02;
        private const ushort AllCompleteFlag = ActionCompleteFlag | EnumeratorCompleteFlag;
        private const ushort CancelledFlag = 0x04;

        private readonly AsyncScheduler m_Scheduler;

        // state
        private ushort m_Status;
        private ushort m_Serial;
        private Action m_Action;
        private IEnumerator m_Enumerator;
        internal AsyncFlags AsyncFlags;

        // misc
        private string m_Name;
        private List<AsyncHandle> m_Nested = new List<AsyncHandle>();
        private Action m_OnStop;

        #if SUPPORTS_THREADING
        private readonly object m_LockContext = new object();
        #endif // SUPPORTS_THREADING

        #region Lifecycle

        internal AsyncWorkUnit(AsyncScheduler inScheduler)
        {
            m_Scheduler = inScheduler;
        }

        /// <summary>
        /// Initializes the work unit with new work.
        /// </summary>
        internal void Initialize(Action inAction, IEnumerator inEnumerator, AsyncFlags inFlags, string inName)
        {
            m_Status = AllCompleteFlag;
            m_Name = inName;

            m_Action = inAction;
            if (m_Action != null)
            {
                m_Status = (ushort) (m_Status & ~ActionCompleteFlag);
            }

            m_Enumerator = inEnumerator;
            if (m_Enumerator != null)
            {
                m_Status = (ushort) (m_Status & ~EnumeratorCompleteFlag);
            }

            AsyncFlags = inFlags;

            if (m_Serial == ushort.MaxValue)
            {
                m_Serial = 1;
            }
            else
            {
                ++m_Serial;
            }
        }

        /// <summary>
        /// Schedules a callback when the work unit is stopped.
        /// </summary>
        internal void OnStopCallback(ushort inSerial, Action inOnStop)
        {
            #if SUPPORTS_THREADING
            lock(m_LockContext)
            {
                if (m_Serial == inSerial)
                {
                    m_OnStop += inOnStop;
                }
            }
            #else
            if (m_Serial == inSerial)
            {
                m_OnStop += inOnStop;
            }
            #endif // SUPPORTS_THREADING
        }

        /// <summary>
        /// Dispatch any callbacks.
        /// </summary>
        internal void DispatchStop(AsyncDispatcher inDispatcher)
        {
            #if SUPPORTS_THREADING
            lock(m_LockContext)
            {
                if (m_OnStop != null)
                {
                    inDispatcher.EnqueueInvoke(m_OnStop);
                    m_OnStop = null;
                }
            }
            #else
            if (m_OnStop != null)
            {
                inDispatcher.EnqueueInvoke(m_OnStop);
                m_OnStop = null;
            }
            #endif // SUPPORTS_THREADING
        }

        /// <summary>
        /// Clears the work unit's contents.
        /// </summary>
        internal void Clear()
        {
            m_Status = AllCompleteFlag;
            m_Action = null;
            DisposeUtils.DisposeObject(ref m_Enumerator);
            AsyncFlags = 0;
            m_Nested.Clear();
            m_OnStop = null;
            m_Name = null;
        }

        #endregion // Lifecycle

        #region Status

        /// <summary>
        /// Returns if this work unit is running.
        /// </summary>
        internal bool IsRunning(ushort inSerial)
        {
            return m_Serial == inSerial && m_Status != AllCompleteFlag;
        }

        /// <summary>
        /// Returns if this work was cancelled.
        /// </summary>
        internal bool IsCancelled()
        {
            return (m_Status & CancelledFlag) != 0;
        }

        /// <summary>
        /// Returns a handle to the work unit.
        /// </summary>
        internal AsyncHandle GetHandle()
        {
            return new AsyncHandle(this, m_Serial);
        }

        #endregion // Status

        #region Step

        /// <summary>
        /// Performs a step (not thread-safe).
        /// Returns if work remains.
        /// </summary>
        internal StepResult Step()
        {
            if ((m_Status & CancelledFlag) != 0)
            {
                return StepResult.Cancelled;
            }

            long tickDelay = 0;

            if ((m_Status & ActionCompleteFlag) == 0)
            {
                m_Action();
                m_Action = null;
                m_Status |= ActionCompleteFlag;
            }
            else if ((m_Status & EnumeratorCompleteFlag) == 0)
            {
                if (!m_Enumerator.MoveNext())
                {
                    DisposeUtils.DisposeObject(ref m_Enumerator);
                    m_Status |= EnumeratorCompleteFlag;
                }
                else
                {
                    object result = m_Enumerator.Current;
                    if (result != null)
                    {
                        if (result is AsyncSleep)
                        {
                            AsyncSleep sleep = (AsyncSleep) result;
                            tickDelay = sleep.Ticks;
                        }
                    }
                }
            }

            if (m_Status != AllCompleteFlag)
            {
                return new StepResult(StepResultType.Incomplete, tickDelay);
            }

            return StepResult.Complete;
        }

        #if SUPPORTS_THREADING

        /// <summary>
        /// Performs a step (thread-safe).
        /// Returns if work remains.
        /// </summary>
        internal StepResult ThreadedStep()
        {
            lock(m_LockContext)
            {
                return Step();
            }
        }

        #endif // SUPPORTS_THREADING

        #endregion // Step

        #region Cancel

        // Cancels all future steps
        private void Cancel()
        {
            for (int i = m_Nested.Count - 1; i >= 0; --i)
            {
                m_Nested[i].Cancel();
            }
            m_Nested.Clear();

            if (m_Status == AllCompleteFlag)
                return;

            if ((m_Status & CancelledFlag) == 0)
            {
                m_Scheduler.Log("Cancelling {0}", m_Name);
                m_Status |= CancelledFlag;
            }
        }

        /// <summary>
        /// Attempts to cancel work.
        /// </summary>
        internal void TryCancel(ushort inSerial)
        {
            #if SUPPORTS_THREADING
            lock(m_LockContext)
            {
                if (m_Serial == inSerial)
                {
                    Cancel();
                }
            }
            #else
            if (m_Serial == inSerial)
            {
                Cancel();
            }
            #endif // SUPPORTS_THREADING
        }

        #endregion // Cancel

        #region Overrides

        public override string ToString()
        {
            return m_Name;
        }

        #endregion // Overrides
    }
}
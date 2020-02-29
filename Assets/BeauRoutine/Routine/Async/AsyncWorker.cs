/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    6 Jan 2020
 * 
 * File:    AsyncWorker.cs
 * Purpose: Worker for a specific async thread.
 */

#if !UNITY_WEBGL
#define SUPPORTS_THREADING
#endif // UNITY_WEBGL

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if SUPPORTS_THREADING
using System.Threading;
using ThreadState = System.Threading.ThreadState;
#endif // SUPPORTS_THREADING

namespace BeauRoutine.Internal
{
    internal sealed class AsyncWorker
    {
        private const double BackgroundProcessingPortion = 0.4;

        #if SUPPORTS_THREADING
        private const int PausedSleepMS = 10;
        private const int StarvationSleepMS = 5;
        private const int ThreadAbortTimeoutMS = 150;
        static private readonly string ThreadAbortWarning = string.Format("Thread did not close within {0}ms", ThreadAbortTimeoutMS);

        private Thread m_Thread;
        private readonly object m_LockObject = new object();
        #endif // SUPPORTS_THREADING

        private readonly AsyncScheduler m_Scheduler;
        private readonly Queue<AsyncWorkUnit> m_MainThreadOnlyQueue;
        private readonly Queue<AsyncWorkUnit> m_BackgroundQueue;
        private readonly AsyncFlags m_PriorityFlags;

        private long m_NextBlockingTick = -1;

        internal bool ForceSingleThreaded;
        internal bool Paused;

        #region Lifecycle

        internal AsyncWorker(AsyncScheduler inScheduler, AsyncFlags inPriorityFlags)
        {
            m_Scheduler = inScheduler;
            m_BackgroundQueue = new Queue<AsyncWorkUnit>(64);
            m_MainThreadOnlyQueue = new Queue<AsyncWorkUnit>(64);
            m_PriorityFlags = inPriorityFlags;
        }

        /// <summary>
        /// Destroys the worker.
        /// </summary>
        internal void Destroy()
        {
            ForceSingleThreaded = true;

            #if SUPPORTS_THREADING

            // if we have a running thread, join and exit
            if (m_Thread != null)
            {
                m_Thread.Join(ThreadAbortTimeoutMS);
                if (m_Thread != null)
                {
                    lock(m_LockObject)
                    {
                        if (m_Thread.ThreadState != ThreadState.Stopped)
                        {
                            Console.Error.Write(ThreadAbortWarning);
                            m_Thread.Abort();
                            m_Thread = null;
                        }
                    }
                }
            }

            lock(m_LockObject)
            {
                ClearQueues();
            }

            #else

            ClearQueues();

            #endif // SUPPORTS_THREADING
        }

        // Clears all queues
        private void ClearQueues()
        {
            while (m_BackgroundQueue.Count > 0)
            {
                m_Scheduler.FreeUnit(m_BackgroundQueue.Dequeue(), false);
            }
            m_BackgroundQueue.Clear();

            while (m_MainThreadOnlyQueue.Count > 0)
            {
                m_Scheduler.FreeUnit(m_MainThreadOnlyQueue.Dequeue(), false);
            }
            m_MainThreadOnlyQueue.Clear();
        }

        #endregion // Lifecycle

        /// <summary>
        /// Returns if there is scheduled work on the main thread.
        /// </summary>
        internal bool HasBlockingWork()
        {
            #if SUPPORTS_THREADING
            lock(m_LockObject)
            {
                if (ForceSingleThreaded)
                {
                    return m_BackgroundQueue.Count > 0 || m_MainThreadOnlyQueue.Count > 0;
                }

                return m_MainThreadOnlyQueue.Count > 0;
            }
            #else
            return m_BackgroundQueue.Count > 0 || m_MainThreadOnlyQueue.Count > 0;
            #endif // SUPPORTS_THREADING
        }

        /// <summary>
        /// Schedules work for this worker.
        /// </summary>
        internal void Schedule(AsyncWorkUnit inUnit)
        {
            #if SUPPORTS_THREADING
            lock(m_LockObject)
            {
                if ((inUnit.AsyncFlags & AsyncFlags.MainThreadOnly) != 0)
                {
                    m_Scheduler.Log("Scheduling {0} on main thread", inUnit);
                    m_MainThreadOnlyQueue.Enqueue(inUnit);
                }
                else
                {
                    m_Scheduler.Log("Scheduling {0} in background", inUnit);
                    m_BackgroundQueue.Enqueue(inUnit);
                }
            }
            #else
            if ((inUnit.AsyncFlags & AsyncFlags.MainThreadOnly) != 0)
            {
                m_Scheduler.Log("Scheduling {0} on main thread", inUnit);
                m_MainThreadOnlyQueue.Enqueue(inUnit);
            }
            else
            {
                m_Scheduler.Log("Scheduling {0} in background", inUnit);
                m_BackgroundQueue.Enqueue(inUnit);
            }
            #endif // SUPPORTS_THREADING
        }

        #region Single-Threaded

        /// <summary>
        /// Processes scheduled work by time-slicing.
        /// </summary>
        internal void ProcessBlocking(Stopwatch inStopwatch, long inTotalBudget, long inSliceBudget, bool inbProcessBackground, ref long ioTicksRemaining)
        {
            long backgroundPortion = !inbProcessBackground ? 0 : (long) (inSliceBudget * BackgroundProcessingPortion);
            long mainThreadPortion = inSliceBudget - backgroundPortion;

            ProcessBlockingQueue(inStopwatch, inTotalBudget, mainThreadPortion, m_MainThreadOnlyQueue, false, ref ioTicksRemaining);
            if (inbProcessBackground && ioTicksRemaining > 0)
            {
                ProcessBlockingQueue(inStopwatch, inTotalBudget, backgroundPortion, m_BackgroundQueue, true, ref ioTicksRemaining);
            }

            ioTicksRemaining = inTotalBudget - inStopwatch.ElapsedTicks;
        }

        // Processes scheduled work by time-slicing.
        private void ProcessBlockingQueue(Stopwatch inStopwatch, long inTotalBudget, long inSliceBudget, Queue<AsyncWorkUnit> ioQueue, bool inbCheckThread, ref long ioTicksRemaining)
        {
            #if SUPPORTS_THREADING

            if (inbCheckThread)
            {
                // if we have a running thread, don't process this frame
                lock(m_LockObject)
                {
                    if (m_Thread != null)
                    {
                        ioTicksRemaining = inTotalBudget - inStopwatch.ElapsedTicks;
                        return;
                    }
                }
            }

            #endif // SUPPORTS_THREADING

            if (m_NextBlockingTick > 0)
            {
                long current = Stopwatch.GetTimestamp();
                if (current < m_NextBlockingTick)
                {
                    ioTicksRemaining = inTotalBudget - inStopwatch.ElapsedTicks;
                    return;
                }

                m_NextBlockingTick = 0;
            }

            long timestamp = inStopwatch.ElapsedTicks;
            long cutoff = timestamp + inSliceBudget;
            while (!Paused && ioTicksRemaining > 0 && ioQueue.Count > 0 && timestamp < cutoff && m_NextBlockingTick <= 0)
            {
                AsyncWorkUnit unit = ioQueue.Peek();

                AsyncWorkUnit.StepResult result = AsyncWorkUnit.StepResult.Incomplete;
                while (result.IsIncomplete() && ioTicksRemaining > 0 && !Paused && timestamp < cutoff && m_NextBlockingTick <= 0)
                {
                    result = unit.Step();
                    if (result.TickDelay > 0)
                    {
                        m_NextBlockingTick = Stopwatch.GetTimestamp() + result.TickDelay;
                    }
                    
                    timestamp = inStopwatch.ElapsedTicks;
                    ioTicksRemaining = inTotalBudget - timestamp;
                }

                if (!result.IsIncomplete())
                {
                    if (result.Type == AsyncWorkUnit.StepResultType.Complete)
                    {
                        m_Scheduler.Log("Completed {0}", unit);
                    }

                    ioQueue.Dequeue();
                    m_Scheduler.FreeUnit(unit);

                    timestamp = inStopwatch.ElapsedTicks;
                    ioTicksRemaining = inTotalBudget - timestamp;
                }
            }
        }

        #endregion // Single-Threaded

        #region Multi-Threaded

        #if SUPPORTS_THREADING

        /// <summary>
        /// Ensures thread exists if work is scheduled.
        /// </summary>
        internal void ProcessThreaded()
        {
            lock(m_LockObject)
            {
                if (!ForceSingleThreaded && m_Thread == null && m_BackgroundQueue.Count > 0)
                {
                    m_Thread = new Thread(ProcessBackgroundQueueOnThread);
                    m_Thread.Priority = GetThreadPriority();
                    m_Thread.IsBackground = true;
                    m_Thread.Name = string.Format("BeauRoutineWorker[{0}]", m_PriorityFlags.ToString());
                    m_Thread.Start();
                }
            }
        }

        private System.Threading.ThreadPriority GetThreadPriority()
        {
            if ((m_PriorityFlags & AsyncFlags.HighPriority) != 0)
            {
                return ThreadPriority.AboveNormal;
            }
            if ((m_PriorityFlags & AsyncFlags.LowPriority) != 0)
            {
                return ThreadPriority.BelowNormal;
            }
            return ThreadPriority.Normal;
        }

        // Loop to run on thread
        private void ProcessBackgroundQueueOnThread()
        {
            while (!ForceSingleThreaded)
            {
                if (Paused)
                {
                    Thread.Sleep(PausedSleepMS);
                    continue;
                }

                AsyncWorkUnit unit = null;
                lock(m_LockObject)
                {
                    if (m_BackgroundQueue.Count > 0)
                    {
                        unit = m_BackgroundQueue.Peek();
                    }
                }

                if (unit == null)
                {
                    Thread.Sleep(StarvationSleepMS);
                    continue;
                }

                AsyncWorkUnit.StepResult result = AsyncWorkUnit.StepResult.Incomplete;
                while (result.IsIncomplete() && !ForceSingleThreaded && !Paused)
                {
                    result = unit.ThreadedStep();
                    if (result.TickDelay > 0)
                    {
                        Thread.Sleep(TimeSpan.FromTicks(result.TickDelay));
                    }
                }

                if (!result.IsIncomplete())
                {
                    if (result.Type == AsyncWorkUnit.StepResultType.Complete)
                    {
                        m_Scheduler.Log("Completed {0}", unit);
                    }

                    lock(m_LockObject)
                    {
                        m_BackgroundQueue.Dequeue();
                    }

                    m_Scheduler.FreeUnit(unit);
                }
            }

            lock(m_LockObject)
            {
                m_Thread = null;
            }
        }

        #endif // SUPPORTS_THREADING

        #endregion // Multi-Threaded
    }
}
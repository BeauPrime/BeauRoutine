/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
#endif // SUPPORTS_THREADING

namespace BeauRoutine.Internal
{
    internal sealed class AsyncWorker
    {
        #if SUPPORTS_THREADING
        private const int ThreadSleepMS = 2;

        private Thread m_Thread;
        private readonly object m_LockObject = new object();
        #endif // SUPPORTS_THREADING

        private readonly AsyncScheduler m_Scheduler;
        private readonly Queue<AsyncWorkUnit> m_Queue;
        private readonly AsyncPriority m_Priority;

        internal bool ForceSingleThreaded;

        #region Lifecycle

        internal AsyncWorker(AsyncScheduler inScheduler, AsyncPriority inPriority)
        {
            m_Scheduler = inScheduler;
            m_Queue = new Queue<AsyncWorkUnit>(64);
            m_Priority = inPriority;
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
                m_Thread.Join();
                m_Thread = null;
                return;
            }

            lock(m_LockObject)
            {
                while (m_Queue.Count > 0)
                {
                    m_Scheduler.FreeUnit(m_Queue.Dequeue());
                }
                m_Queue.Clear();
            }

            #else

            while (m_Queue.Count > 0)
            {
                m_Scheduler.FreeUnit(m_Queue.Dequeue());
            }
            m_Queue.Clear();

            #endif // SUPPORTS_THREADING
        }

        #endregion // Lifecycle

        /// <summary>
        /// Returns if there is scheduled work.
        /// </summary>
        internal bool HasWork()
        {
            #if SUPPORTS_THREADING
            lock(m_LockObject)
            {
                return m_Queue.Count > 0;
            }
            #else
            return m_Queue.Count > 0;
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
                m_Queue.Enqueue(inUnit);
            }
            #else
            m_Queue.Enqueue(inUnit);
            #endif // SUPPORTS_THREADING
        }

        #region Single-Threaded

        /// <summary>
        /// Processes scheduled work by time-slicing.
        /// </summary>
        internal void ProcessBlocking(Stopwatch inStopwatch, long inStartTimestamp, long inBudget, ref long ioTicksRemaining)
        {
            #if SUPPORTS_THREADING

            // if we have a running thread, don't process this frame
            lock(m_LockObject)
            {
                if (m_Thread != null)
                {
                    ioTicksRemaining = inStopwatch.ElapsedTicks - inStartTimestamp;
                    return;
                }
            }

            #endif // SUPPORTS_THREADING

            long cutoff = inStopwatch.ElapsedTicks + inBudget;
            while (ioTicksRemaining > 0 && m_Queue.Count > 0)
            {
                AsyncWorkUnit unit = m_Queue.Peek();

                bool bComplete = unit.Step();
                if (bComplete)
                {
                    m_Queue.Dequeue();
                    m_Scheduler.FreeUnit(unit);
                }

                long timestamp = inStopwatch.ElapsedTicks;
                ioTicksRemaining = timestamp - inStartTimestamp;

                // if we exceeded our max time, exit
                if (timestamp >= cutoff)
                    break;
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
                if (m_Thread == null && m_Queue.Count > 0)
                {
                    m_Thread = new Thread(ProcessOnThread);
                    m_Thread.Priority = GetThreadPriority();
                    m_Thread.Start();
                }
            }
        }

        private System.Threading.ThreadPriority GetThreadPriority()
        {
            switch (m_Priority)
            {
                case AsyncPriority.Low:
                    return ThreadPriority.Lowest;
                case AsyncPriority.BelowNormal:
                    return ThreadPriority.BelowNormal;
                case AsyncPriority.Normal:
                    return ThreadPriority.Normal;
                case AsyncPriority.High:
                    return ThreadPriority.AboveNormal;
                default:
                    throw new ArgumentException("Unknown priority " + m_Priority.ToString(), "inPriority");
            }
        }

        // Loop to run on thread
        private void ProcessOnThread()
        {
            while (!ForceSingleThreaded)
            {
                AsyncWorkUnit unit = null;
                lock(m_LockObject)
                {
                    if (m_Queue.Count > 0)
                    {
                        unit = m_Queue.Peek();
                    }
                }

                if (unit == null)
                {
                    Thread.Sleep(ThreadSleepMS);
                    continue;
                }

                bool bComplete = false;
                while (!bComplete && !ForceSingleThreaded)
                {
                    bComplete = unit.ThreadedStep();
                }

                if (bComplete)
                {
                    lock(m_LockObject)
                    {
                        m_Queue.Dequeue();
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
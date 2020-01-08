/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    6 Jan 2020
 * 
 * File:    AsyncScheduler.cs
 * Purpose: Manager for async utilities.
 */

#if !UNITY_WEBGL
#define SUPPORTS_THREADING
#endif // UNITY_WEBGL

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#if SUPPORTS_THREADING
using System.Threading;
#endif // SUPPORTS_THREADING

namespace BeauRoutine.Internal
{
    internal sealed class AsyncScheduler
    {
        #region Support

        // Support
        #if SUPPORTS_THREADING
        internal const bool SupportsThreading = true;
        #else
        internal const bool SupportsThreading = false;
        #endif // SUPPORTS_THREADING

        #endregion // Support

        #region Pooling

        // Pooling
        internal const int StartingPoolSize = 32;

        #if SUPPORTS_THREADING
        private readonly object m_PoolLock = new object();
        #endif // SUPPORTS_THREADING

        private readonly List<AsyncWorkUnit> m_WorkUnitPool;

        #endregion // Pooling

        #region Workers

        internal const double HighPriorityPercentage = 0.5;
        internal const double NormalPriorityPercentage = 0.35;
        internal const double LowPriorityPercentage = 0.15;

        private readonly Stopwatch m_Stopwatch;
        private bool m_ForceSingleThreaded;

        private readonly AsyncWorker m_LowPriorityWorker;
        private readonly AsyncWorker m_NormalPriorityWorker;
        private readonly AsyncWorker m_HighPriorityWorker;

        public readonly AsyncDispatcher Dispatcher;

        #endregion // Workers

        #region Lifecycle

        internal AsyncScheduler()
        {
            m_LowPriorityWorker = new AsyncWorker(this, AsyncFlags.LowPriority);
            m_NormalPriorityWorker = new AsyncWorker(this, AsyncFlags.NormalPriority);
            m_HighPriorityWorker = new AsyncWorker(this, AsyncFlags.HighPriority);

            Dispatcher = new AsyncDispatcher();

            m_WorkUnitPool = new List<AsyncWorkUnit>(StartingPoolSize);
            for (int i = 0; i < StartingPoolSize; ++i)
            {
                m_WorkUnitPool.Add(new AsyncWorkUnit());
            }

            m_Stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Destroys the scheduler.
        /// </summary>
        internal void Destroy()
        {
            m_LowPriorityWorker.Destroy();
            m_NormalPriorityWorker.Destroy();
            m_HighPriorityWorker.Destroy();
            Dispatcher.Destroy();

            m_WorkUnitPool.Clear();
        }

        #endregion // Lifecycle

        #region Processing

        /// <summary>
        /// Processes all workers and dispatches callbacks.
        /// Only call on the main thread!
        /// </summary>
        internal void Process(long inAsyncBudget)
        {
            m_Stopwatch.Reset();
            m_Stopwatch.Start();

            #if SUPPORTS_THREADING
            if (m_ForceSingleThreaded)
            {
                ProcessBlocking(inAsyncBudget, true);
            }
            else
            {
                ProcessBlocking(inAsyncBudget, false);
                ProcessThreaded();
            }
            #else
            ProcessBlocking(inAsyncBudget, true);
            #endif // SUPPORTS_THREADING

            Dispatcher.DispatchCalls();

            m_Stopwatch.Stop();
        }

        // Processes workers with time-slicing.
        private void ProcessBlocking(long inAsyncBudget, bool inbProcessBackground)
        {
            long ticksRemaining = inAsyncBudget;
            while (ticksRemaining > 0 && HasBlockingWork())
            {
                m_HighPriorityWorker.ProcessBlocking(m_Stopwatch, inAsyncBudget, (long) (inAsyncBudget * HighPriorityPercentage), inbProcessBackground, ref ticksRemaining);
                m_NormalPriorityWorker.ProcessBlocking(m_Stopwatch, inAsyncBudget, (long) (inAsyncBudget * NormalPriorityPercentage), inbProcessBackground, ref ticksRemaining);
                m_LowPriorityWorker.ProcessBlocking(m_Stopwatch, inAsyncBudget, (long) (inAsyncBudget * LowPriorityPercentage), inbProcessBackground, ref ticksRemaining);
            }
        }

        // Processes the threaded elements of the workers.
        private void ProcessThreaded()
        {
            #if SUPPORTS_THREADING
            m_HighPriorityWorker.ProcessThreaded();
            m_NormalPriorityWorker.ProcessThreaded();
            m_LowPriorityWorker.ProcessThreaded();
            #endif // SUPPORTS_THREADING
        }

        // Returns if there is work scheduled.
        private bool HasBlockingWork()
        {
            return m_HighPriorityWorker.HasBlockingWork() || m_NormalPriorityWorker.HasBlockingWork() || m_LowPriorityWorker.HasBlockingWork();
        }

        #endregion // Processing

        #region Scheduling

        /// <summary>
        /// Schedules an action.
        /// </summary>
        internal AsyncHandle Schedule(Action inAction, AsyncFlags inFlags)
        {
            AsyncWorkUnit unit = AllocUnit(inAction, inFlags);
            ScheduleImpl(unit);
            return unit.GetHandle();
        }

        /// <summary>
        /// Schedules an enumerated action.
        /// </summary>
        internal AsyncHandle Schedule(IEnumerator inEnumerator, AsyncFlags inFlags)
        {
            AsyncWorkUnit unit = AllocUnit(inEnumerator, inFlags);
            ScheduleImpl(unit);
            return unit.GetHandle();
        }

        // Schedules work to the given priority worker.
        private void ScheduleImpl(AsyncWorkUnit inUnit)
        {
            AsyncWorker worker = GetWorker(inUnit.AsyncFlags);
            worker.Schedule(inUnit);
        }

        // Returns the worker for the given priority.
        private AsyncWorker GetWorker(AsyncFlags inPriority)
        {
            if ((inPriority & AsyncFlags.HighPriority) != 0)
            {
                return m_HighPriorityWorker;
            }
            if ((inPriority & AsyncFlags.LowPriority) != 0)
            {
                return m_LowPriorityWorker;
            }
            return m_NormalPriorityWorker;
        }

        #endregion // Scheduling

        #region Work Unit Pool

        // Allocates a work unit for the given action.
        private AsyncWorkUnit AllocUnit(Action inAction, AsyncFlags inFlags)
        {
            AsyncWorkUnit unit = AllocUnitImpl();
            unit.Initialize(inAction, null, inFlags);
            return unit;
        }

        // Allocates a work unit for the given enumerator.
        private AsyncWorkUnit AllocUnit(IEnumerator inEnumerator, AsyncFlags inFlags)
        {
            AsyncWorkUnit unit = AllocUnitImpl();
            unit.Initialize(null, inEnumerator, inFlags);
            return unit;
        }

        // Allocates a work unit from the pool.
        private AsyncWorkUnit AllocUnitImpl()
        {
            #if SUPPORTS_THREADING
            lock(m_PoolLock)
            {
                #endif // SUPPORTS_THREADING

                int idx = m_WorkUnitPool.Count - 1;
                if (idx < 0)
                {
                    return new AsyncWorkUnit();
                }

                AsyncWorkUnit unit = m_WorkUnitPool[idx];
                m_WorkUnitPool.RemoveAt(idx);
                return unit;

                #if SUPPORTS_THREADING
            }
            #endif // SUPPORTS_THREADING
        }

        /// <summary>
        /// Frees a work unit and returns it to the pool.
        /// </summary>
        internal void FreeUnit(AsyncWorkUnit inWorkUnit, bool inbDispatchCallbacks = true)
        {
            if (inbDispatchCallbacks)
            {
                inWorkUnit.DispatchStop(Dispatcher);
            }

            inWorkUnit.Clear();

            #if SUPPORTS_THREADING
            lock(m_PoolLock)
            {
                m_WorkUnitPool.Add(inWorkUnit);
            }
            #else
            m_WorkUnitPool.Add(inWorkUnit);
            #endif // SUPPORTS_THREADING
        }

        #endregion // Work Unit Pool

        /// <summary>
        /// Sets the single-thread force mode.
        /// </summary>
        internal void SetForceSingleThread(bool inbForceSingleThread)
        {
            m_ForceSingleThreaded = inbForceSingleThread;
            m_LowPriorityWorker.ForceSingleThreaded = inbForceSingleThread;
            m_NormalPriorityWorker.ForceSingleThreaded = inbForceSingleThread;
            m_HighPriorityWorker.ForceSingleThreaded = inbForceSingleThread;
        }

        /// <summary>
        /// Set whether or not to pause async work.
        /// </summary>
        internal void SetPaused(bool inbPaused)
        {
            m_LowPriorityWorker.Paused = inbPaused;
            m_NormalPriorityWorker.Paused = inbPaused;
            m_HighPriorityWorker.Paused = inbPaused;
        }
    }
}
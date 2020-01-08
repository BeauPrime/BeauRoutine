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
        #if SUPPORTS_THREADING
        internal const bool SupportsThreading = true;
        #else
        internal const bool SupportsThreading = false;
        #endif // SUPPORTS_THREADING

        internal const double HighPriorityPercentage = 0.4;
        internal const double NormalPriorityPercentage = 0.3;
        internal const double BelowNormalPriorityPercentage = 0.2;
        internal const double LowPriorityPercentage = 0.1;
        internal const int StartingPoolSize = 32;

        #if SUPPORTS_THREADING
        private readonly object m_PoolLock = new object();
        #endif // SUPPORTS_THREADING

        private readonly List<AsyncWorkUnit> m_Pool;

        private bool m_ForceSingleThreaded;

        private readonly AsyncWorker m_LowPriorityWorker;
        private readonly AsyncWorker m_BelowNormalPriorityWorker;
        private readonly AsyncWorker m_NormalPriorityWorker;
        private readonly AsyncWorker m_HighPriorityWorker;

        public readonly AsyncDispatcher Dispatcher;

        #region Lifecycle

        internal AsyncScheduler()
        {
            m_LowPriorityWorker = new AsyncWorker(this, AsyncPriority.Low);
            m_BelowNormalPriorityWorker = new AsyncWorker(this, AsyncPriority.BelowNormal);
            m_NormalPriorityWorker = new AsyncWorker(this, AsyncPriority.Normal);
            m_HighPriorityWorker = new AsyncWorker(this, AsyncPriority.High);

            Dispatcher = new AsyncDispatcher();

            m_Pool = new List<AsyncWorkUnit>(StartingPoolSize);
            for (int i = 0; i < StartingPoolSize; ++i)
            {
                m_Pool.Add(new AsyncWorkUnit());
            }
        }

        /// <summary>
        /// Destroys the scheduler.
        /// </summary>
        internal void Destroy()
        {
            m_LowPriorityWorker.Destroy();
            m_BelowNormalPriorityWorker.Destroy();
            m_NormalPriorityWorker.Destroy();
            m_HighPriorityWorker.Destroy();
            Dispatcher.Destroy();

            m_Pool.Clear();
        }

        #endregion // Lifecycle

        #region Processing

        /// <summary>
        /// Processes all workers and dispatches callbacks.
        /// Only call on the main thread!
        /// </summary>
        internal void Process(Stopwatch inStopwatch, long inStartTimestamp, long inAsyncBudget)
        {
            #if SUPPORTS_THREADING
            if (m_ForceSingleThreaded)
            {
                ProcessBlocking(inStopwatch, inStartTimestamp, inAsyncBudget);
            }
            else
            {
                ProcessThreaded();
            }
            #else
            ProcessBlocking(inStopwatch, inStartTimestamp, inAsyncBudget);
            #endif // SUPPORTS_THREADING

            Dispatcher.DispatchCalls();
        }

        // Processes workers with time-slicing.
        private void ProcessBlocking(Stopwatch inStopwatch, long inStartTimestamp, long inAsyncBudget)
        {
            long ticksRemaining = inAsyncBudget;
            while (ticksRemaining > 0 && HasWork())
            {
                m_HighPriorityWorker.ProcessBlocking(inStopwatch, inStartTimestamp, inAsyncBudget, (long) (inAsyncBudget * HighPriorityPercentage), ref ticksRemaining);
                m_NormalPriorityWorker.ProcessBlocking(inStopwatch, inStartTimestamp, inAsyncBudget, (long) (inAsyncBudget * NormalPriorityPercentage), ref ticksRemaining);
                m_BelowNormalPriorityWorker.ProcessBlocking(inStopwatch, inStartTimestamp, inAsyncBudget, (long) (inAsyncBudget * BelowNormalPriorityPercentage), ref ticksRemaining);
                m_LowPriorityWorker.ProcessBlocking(inStopwatch, inStartTimestamp, inAsyncBudget, (long) (inAsyncBudget * LowPriorityPercentage), ref ticksRemaining);
            }
        }

        // Processes the threaded elements of the workers.
        private void ProcessThreaded()
        {
            #if SUPPORTS_THREADING
            m_HighPriorityWorker.ProcessThreaded();
            m_NormalPriorityWorker.ProcessThreaded();
            m_BelowNormalPriorityWorker.ProcessThreaded();
            m_LowPriorityWorker.ProcessThreaded();
            #endif // SUPPORTS_THREADING
        }

        // Returns if there is work scheduled.
        private bool HasWork()
        {
            return m_HighPriorityWorker.HasWork() || m_NormalPriorityWorker.HasWork() || m_BelowNormalPriorityWorker.HasWork() || m_LowPriorityWorker.HasWork();
        }

        #endregion // Processing

        #region Scheduling

        /// <summary>
        /// Schedules an action.
        /// </summary>
        internal AsyncHandle Schedule(Action inAction, AsyncPriority inPriority)
        {
            AsyncWorkUnit unit = AllocUnit(inAction);
            ScheduleImpl(unit, inPriority);
            return unit.GetHandle();
        }

        /// <summary>
        /// Schedules an enumerated action.
        /// </summary>
        internal AsyncHandle Schedule(IEnumerator inEnumerator, AsyncPriority inPriority)
        {
            AsyncWorkUnit unit = AllocUnit(inEnumerator);
            ScheduleImpl(unit, inPriority);
            return unit.GetHandle();
        }

        // Schedules work to the given priority worker.
        private void ScheduleImpl(AsyncWorkUnit inUnit, AsyncPriority inPriority)
        {
            AsyncWorker worker = GetWorker(inPriority);
            worker.Schedule(inUnit);
        }

        // Returns the worker for the given priority.
        private AsyncWorker GetWorker(AsyncPriority inPriority)
        {
            switch (inPriority)
            {
                case AsyncPriority.Low:
                    return m_LowPriorityWorker;
                case AsyncPriority.BelowNormal:
                    return m_BelowNormalPriorityWorker;
                case AsyncPriority.Normal:
                    return m_NormalPriorityWorker;
                case AsyncPriority.High:
                    return m_HighPriorityWorker;
                default:
                    throw new ArgumentException("Unknown priority " + inPriority.ToString(), "inPriority");
            }
        }

        #endregion // Scheduling

        #region Work Unit Pool

        // Allocates a work unit for the given action.
        private AsyncWorkUnit AllocUnit(Action inAction)
        {
            AsyncWorkUnit unit = AllocUnitImpl();
            unit.Initialize(inAction, null);
            return unit;
        }

        // Allocates a work unit for the given enumerator.
        private AsyncWorkUnit AllocUnit(IEnumerator inEnumerator)
        {
            AsyncWorkUnit unit = AllocUnitImpl();
            unit.Initialize(null, inEnumerator);
            return unit;
        }

        // Allocates a work unit from the pool.
        private AsyncWorkUnit AllocUnitImpl()
        {
            #if SUPPORTS_THREADING
            lock(m_PoolLock)
            {
                #endif // SUPPORTS_THREADING

                int idx = m_Pool.Count - 1;
                if (idx < 0)
                {
                    return new AsyncWorkUnit();
                }

                AsyncWorkUnit unit = m_Pool[idx];
                m_Pool.RemoveAt(idx);
                return unit;

                #if SUPPORTS_THREADING
            }
            #endif // SUPPORTS_THREADING
        }

        /// <summary>
        /// Frees a work unit and returns it to the pool.
        /// </summary>
        internal void FreeUnit(AsyncWorkUnit inWorkUnit)
        {
            inWorkUnit.Clear();

            #if SUPPORTS_THREADING
            lock(m_PoolLock)
            {
                m_Pool.Add(inWorkUnit);
            }
            #else
            m_Pool.Add(inWorkUnit);
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
            m_BelowNormalPriorityWorker.ForceSingleThreaded = inbForceSingleThread;
            m_NormalPriorityWorker.ForceSingleThreaded = inbForceSingleThread;
            m_HighPriorityWorker.ForceSingleThreaded = inbForceSingleThread;
        }
    }
}
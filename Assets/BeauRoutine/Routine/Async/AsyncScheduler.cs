/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    6 Jan 2020
 * 
 * File:    AsyncScheduler.cs
 * Purpose: Manager for async scheduling, processing, and dispatching.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

#if !UNITY_WEBGL
#define SUPPORTS_THREADING
#endif // UNITY_WEBGL

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
        private readonly object m_LoggerLock = new object();
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

        #region Logging

        private readonly Action<string> m_Logger;
        private readonly StringBuilder m_LogBuilder;

        public bool DebugMode;

        #endregion // Logging

        #region Lifecycle

        internal AsyncScheduler(Action<string> inLogger)
        {
            m_LowPriorityWorker = new AsyncWorker(this, AsyncFlags.LowPriority);
            m_NormalPriorityWorker = new AsyncWorker(this, AsyncFlags.NormalPriority);
            m_HighPriorityWorker = new AsyncWorker(this, AsyncFlags.HighPriority);

            Dispatcher = new AsyncDispatcher();

            m_WorkUnitPool = new List<AsyncWorkUnit>(StartingPoolSize);
            for (int i = 0; i < StartingPoolSize; ++i)
            {
                m_WorkUnitPool.Add(new AsyncWorkUnit(this));
            }

            m_Stopwatch = new Stopwatch();

            m_Logger = inLogger;
            if (m_Logger != null)
            {
                m_LogBuilder = new StringBuilder(256);
            }

            #if DEVELOPMENT
            DebugMode = true;
            #else
            DebugMode = false;
            #endif // DEVELOPMENT
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
            if (inAction == null)
            {
                return AsyncHandle.Null;
            }

            AsyncWorkUnit unit = AllocUnit(inAction, inFlags);
            ScheduleImpl(unit);
            return unit.GetHandle();
        }

        /// <summary>
        /// Schedules an enumerated action.
        /// </summary>
        internal AsyncHandle Schedule(IEnumerator inEnumerator, AsyncFlags inFlags)
        {
            if (inEnumerator == null)
            {
                return AsyncHandle.Null;
            }

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
            #if DEVELOPMENT
            string name = string.Format("{0}->{1}::{2}", inAction.Target, inAction.Method.DeclaringType.FullName, inAction.Method.Name);
            #else
            string name = "[not used]";
            #endif // DEVELOPMENT
            unit.Initialize(inAction, null, inFlags, name);
            return unit;
        }

        // Allocates a work unit for the given enumerator.
        private AsyncWorkUnit AllocUnit(IEnumerator inEnumerator, AsyncFlags inFlags)
        {
            AsyncWorkUnit unit = AllocUnitImpl();
            #if DEVELOPMENT
            string name = Fiber.GetTypeName(inEnumerator.GetType());
            #else
            string name = "[not used]";
            #endif // DEVELOPMENT
            unit.Initialize(null, inEnumerator, inFlags, name);
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
                    return new AsyncWorkUnit(this);
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

        /// <summary>
        /// Logs to the debug console.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        internal void Log(string inLog)
        {
            if (!DebugMode || m_Logger == null)
                return;

            #if SUPPORTS_THREADING
            lock(m_LoggerLock)
            {
                LogImpl(inLog);
            }
            #else
            LogImpl(inLog);
            #endif // SUPPORTS_THREADING
        }

        /// <summary>
        /// Logs to the debug console.
        /// </summary>
        [Conditional("DEVELOPMENT")]
        internal void Log(string inFormat, object inArg)
        {
            if (!DebugMode || m_Logger == null)
                return;

            string formatted = string.Format(inFormat, inArg);

            #if SUPPORTS_THREADING
            lock(m_LoggerLock)
            {
                LogImpl(formatted);
            }
            #else
            LogImpl(formatted);
            #endif // SUPPORTS_THREADING
        }

        // Logs to debug console
        private void LogImpl(string inString)
        {
            m_LogBuilder.Append(inString);
            m_LogBuilder.Insert(0, "[AsyncScheduler] ");

            string str = m_LogBuilder.ToString();
            m_LogBuilder.Length = 0;

            m_Logger(str);
        }
    }
}
/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Jan 2020
 * 
 * File:    AsyncDispatcher.cs
 * Purpose: Callback dispatcher on the main thread.
 */

#if !UNITY_WEBGL
#define SUPPORTS_THREADING
#endif // UNITY_WEBGL

using System;
using System.Collections.Generic;
#if SUPPORTS_THREADING
using System.Threading;
#endif // SUPPORTS_THREADING

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Dispatches callbacks on the main thread.
    /// </summary>
    internal sealed class AsyncDispatcher
    {
        #if SUPPORTS_THREADING

        private const int BlockingSleepMS = 5;

        private readonly Thread m_MainThread;
        private readonly object m_LockObject = new object();

        #endif // SUPPORTS_THREADING

        private readonly Queue<Action> m_Queue = new Queue<Action>(512);
        private bool m_Processing;

        #region Lifecycle

        internal AsyncDispatcher()
        {
            #if SUPPORTS_THREADING
            m_MainThread = Thread.CurrentThread;
            #endif // SUPPORTS_THREADING
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        internal void Destroy()
        {
            // threaded version
            #if SUPPORTS_THREADING
            lock(m_LockObject)
            {
                m_Queue.Clear();
                return;
            }
            #else
            m_Queue.Clear();
            #endif // SUPPORTS_THREADING
        }

        #endregion // Lifecycle

        #region Invocation

        /// <summary>
        /// Enqueues an action to occur on the main thread
        /// at the end of the frame.
        /// </summary>
        internal void EnqueueInvoke(Action inAction)
        {
            #if SUPPORTS_THREADING
            if (IsMainThread() && m_Processing)
            {
                m_Queue.Enqueue(inAction);
            }
            else
            {
                lock(m_LockObject)
                {
                    m_Queue.Enqueue(inAction);
                }
            }
            #else
            m_Queue.Enqueue(inAction);
            #endif // SUPPORTS_THREADING
        }

        /// <summary>
        /// Invokes an action on the main thread,
        /// and blocks until the action has executed.
        /// </summary>
        internal void InvokeBlocking(Action inAction)
        {
            #if SUPPORTS_THREADING
            if (IsMainThread())
            {
                inAction();
            }
            else
            {
                bool bTriggered = false;
                EnqueueInvoke(() =>
                {
                    inAction();
                    bTriggered = true;
                });

                while (!bTriggered)
                {
                    Thread.Sleep(BlockingSleepMS);
                }
            }
            #else
            inAction(); // invoke this now
            #endif // SUPPORTS_THREADING
        }

        // Returns if executing on the main thread.
        internal bool IsMainThread()
        {
            #if SUPPORTS_THREADING
            return m_MainThread == Thread.CurrentThread;
            #else
            return true;
            #endif // SUPPORTS_THREADING
        }

        /// <summary>
        /// Dispatches all calls in the queue.
        /// </summary>
        internal void DispatchCalls()
        {
            #if SUPPORTS_THREADING
            lock(m_LockObject)
            {
                m_Processing = true;
                while (m_Queue.Count > 0)
                {
                    m_Queue.Dequeue().Invoke();
                }
                m_Processing = false;
            }
            #else
            m_Processing = true;
            while (m_Queue.Count > 0)
            {
                m_Queue.Dequeue().Invoke();
            }
            m_Processing = false;
            #endif // SUPPORTS_THREADING

        }

        #endregion // Invocation
    }
}
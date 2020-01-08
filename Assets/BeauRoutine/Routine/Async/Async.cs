/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    7 Jan 2020
 * 
 * File:    Async.cs
 * Purpose: Utility functions for performing async calls.
 */

using System;
using System.Collections;
using BeauRoutine.Internal;

namespace BeauRoutine
{
    /// <summary>
    /// Asynchronous operations.
    /// </summary>
    static public class Async
    {
        #region Invoke

        /// <summary>
        /// Invokes the given function on the main thread
        /// at the end of the frame.
        /// </summary>
        static public void InvokeAsync(Action inAction)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                m.Scheduler.Dispatcher.EnqueueInvoke(inAction);
            }
        }

        /// <summary>
        /// Invokes the given function on the main thread
        /// at the end of the frame.
        /// </summary>
        static public void InvokeAsync<T>(Action<T> inAction, T inArgument)
        {
            InvokeAsync(() => inAction(inArgument));
        }

        /// <summary>
        /// Invokes the given function on the main thread.
        /// If not on the main thread, blocks until executed.
        /// </summary>
        static public void Invoke(Action inAction)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                m.Scheduler.Dispatcher.InvokeBlocking(inAction);
            }
        }

        /// <summary>
        /// Invokes the given function on the main thread.
        /// If not on the main thread, blocks until executed.
        /// </summary>
        static public void Invoke<T>(Action<T> inAction, T inArgument)
        {
            Invoke(() => inAction(inArgument));
        }

        #endregion // Invoke

        #region Schedule

        /// <summary>
        /// Schedules an action to be executed asynchronously.
        /// </summary>
        static public AsyncHandle Schedule(Action inAction, AsyncPriority inPriority)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                return m.Scheduler.Schedule(inAction, inPriority);
            }
            return AsyncHandle.Null;
        }

        /// <summary>
        /// Schedules an enumerated procedure to be executed asynchronously.
        /// </summary>
        static public AsyncHandle Schedule(IEnumerator inEnumerator, AsyncPriority inPriority)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                return m.Scheduler.Schedule(inEnumerator, inPriority);
            }
            return AsyncHandle.Null;
        }

        #endregion // Schedule

        #region For

        static public AsyncHandle For(int inFrom, int inTo, Routine.IndexOperation inOperation, AsyncPriority inPriority)
        {
            if (inOperation == null || inFrom >= inTo)
                return AsyncHandle.Null;

            return Schedule(ForImpl(inFrom, inTo, inOperation), inPriority);
        }

        static private IEnumerator ForImpl(int inFrom, int inTo, Routine.IndexOperation inOperation)
        {
            for (int i = inFrom; i < inTo; ++i)
            {
                inOperation(i);
                yield return null;
            }
        }

        #endregion // For
    }
}
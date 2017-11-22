/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 Oct 2017
 * 
 * File:    Future.cs
 * Purpose: Single-assignment Future value. Can be used to return values
            from a coroutine.
*/

using System;
using System.Collections;

namespace BeauRoutine
{
    /// <summary>
    /// Interface for a Future value.
    /// </summary>
    public interface IFuture : IDisposable
    {
        bool IsDone();
        bool IsComplete();
        bool IsFailed();
        bool IsCancelled();
        IEnumerator Wait();
    }

    /// <summary>
    /// Static methods for creating Futures.
    /// </summary>
    static public partial class Future
    {
        /// <summary>
        /// Failure state for a future.
        /// </summary>
        public struct Failure
        {
            /// <summary>
            /// How a future has failed.
            /// </summary>
            public FailureType Type;

            /// <summary>
            /// Failure details.
            /// </summary>
            public object Object;

            public override string ToString()
            {
                return string.Format("[Failure {0} ({1})]", Type, Object);
            }
        }

        /// <summary>
        /// Common error codes for Fail calls.
        /// </summary>
        public enum FailureType
        {
            Unknown,

            Exception,
            RoutineStopped,
            NullReference
        }

        /// <summary>
        /// Creates a Future.
        /// </summary>
        static public Future<T> Create<T>()
        {
            return new Future<T>();
        }

        /// <summary>
        /// Creates a Future with a completion callback.
        /// </summary>
        static public Future<T> Create<T>(Action<T> inCompleteCallback)
        {
            return new Future<T>(inCompleteCallback);
        }

        /// <summary>
        /// Creates a Future with a completion callback and failure callback.
        /// </summary>
        static public Future<T> Create<T>(Action<T> inCompleteCallback, Action inFailureCallback)
        {
            return new Future<T>(inCompleteCallback, inFailureCallback);
        }

        /// <summary>
        /// Creates a Future with a completion callback and failure callback.
        /// </summary>
        static public Future<T> Create<T>(Action<T> inCompleteCallback, Action<Failure> inFailureCallback)
        {
            return new Future<T>(inCompleteCallback, inFailureCallback);
        }

        /// <summary>
        /// Creates an already-completed Future. 
        /// </summary>
        static public Future<T> Completed<T>(T inValue)
        {
            Future<T> future = new Future<T>();
            future.Complete(inValue);
            return future;
        }

        /// <summary>
        /// Creates an already-failed Future. 
        /// </summary>
        static public Future<T> Failed<T>()
        {
            Future<T> future = new Future<T>();
            future.Fail();
            return future;
        }

        /// <summary>
        /// Creates an already-failed Future. 
        /// </summary>
        static public Future<T> Failed<T>(FailureType inType)
        {
            Future<T> future = new Future<T>();
            future.Fail(inType);
            return future;
        }

        /// <summary>
        /// Creates an already-failed Future. 
        /// </summary>
        static public Future<T> Failed<T>(FailureType inType, object inArg)
        {
            Future<T> future = new Future<T>();
            future.Fail(inType, inArg);
            return future;
        }
    }

    /// <summary>
    /// Represents a value that will be set at some point
    /// in the future. Can be either completed or failed.
    /// </summary>
    public class Future<T> : IFuture
    {
        private enum State { Uninitialized, Completed, Failed, Cancelled }

        private State m_State;

        private T m_Value;
        private Action<T> m_CallbackComplete;

        private Future.Failure m_Failure;
        private Action m_CallbackFail;
        private Action<Future.Failure> m_CallbackFailWithArgs;

        private Routine m_Prophet;

        public Future()
        {
            m_State = State.Uninitialized;
            m_Value = default(T);
            m_CallbackComplete = null;
            m_CallbackFail = null;
        }

        public Future(Action<T> inCompleteCallback)
        {
            m_State = State.Uninitialized;
            m_Value = default(T);
            m_CallbackComplete = inCompleteCallback;
            m_CallbackFail = null;
        }

        public Future(Action<T> inCompleteCallback, Action inFailureCallback)
        {
            m_State = State.Uninitialized;
            m_Value = default(T);
            m_CallbackComplete = inCompleteCallback;
            m_CallbackFail = inFailureCallback;
        }

        public Future(Action<T> inCompleteCallback, Action<Future.Failure> inFailureCallback)
        {
            m_State = State.Uninitialized;
            m_Value = default(T);
            m_CallbackComplete = inCompleteCallback;
            m_CallbackFailWithArgs = inFailureCallback;
        }

        /// <summary>
        /// Cancels the Future if not already completed or failed,
        /// and cleans up references.
        /// </summary>
        public void Dispose()
        {
            Cancel();

            m_Value = default(T);
            m_CallbackComplete = null;
            m_Failure.Object = null;
            m_Failure.Type = Future.FailureType.Unknown;
            m_CallbackFail = null;
            m_CallbackFailWithArgs = null;
        }

        /// <summary>
        /// Returns if the Future has been completed or failed.
        /// </summary>
        public bool IsDone() { return m_State != State.Uninitialized; }

        #region Completion

        /// <summary>
        /// Returns if the Future has been completed.
        /// </summary>
        public bool IsComplete() { return m_State == State.Completed; }

        /// <summary>
        /// Returns the value, or throws an exception
        /// if the Future has not been completed.
        /// </summary>
        public T Get()
        {
            if (m_State != State.Completed)
                throw new InvalidOperationException("Cannot get value of Future<" + typeof(T).Name + "> before it is completed!");
            return m_Value;
        }

        /// <summary>
        /// Attempts to return the value.
        /// </summary>
        public bool TryGet(out T outValue)
        {
            outValue = m_Value;
            return m_State == State.Completed;
        }

        /// <summary>
        /// Sets the value, or throws an exception
        /// if the Future has already been completed or failed.
        /// </summary>
        public void Complete(T inValue)
        {
            if (m_State == State.Cancelled)
                return;

            if (m_State != State.Uninitialized)
                throw new InvalidOperationException("Cannot set value of Future<" + typeof(T).Name + "> once Future has completed or failed!");
            m_State = State.Completed;
            m_Value = inValue;

            if (m_CallbackComplete != null)
            {
                Routine.StartCall(m_CallbackComplete, m_Value);
                m_CallbackComplete = null;
            }

            m_CallbackFail = null;
            m_CallbackFailWithArgs = null;
        }

        /// <summary>
        /// Adds a callback for when the Future is completed.
        /// Will call immediately if the Future has already completed.
        /// </summary>
        public Future<T> OnComplete(Action<T> inCallback)
        {
            if (inCallback == null)
                return this;

            if (m_State == State.Completed)
                inCallback(m_Value);
            else if (m_State == State.Uninitialized)
                m_CallbackComplete += inCallback;

            return this;
        }

        #endregion

        #region Failure

        /// <summary>
        /// Returns if the Future has failed.
        /// </summary>
        public bool IsFailed() { return m_State == State.Failed; }

        /// <summary>
        /// Returns the failure object, or throws an exception
        /// if the Future has not failed.
        /// </summary>
        public Future.Failure GetFailure()
        {
            if (m_State != State.Failed)
                throw new InvalidOperationException("Cannot get error of Future<" + typeof(T).Name + "> before it has failed!");
            return m_Failure;
        }

        /// <summary>
        /// Attempts to return the failure object.
        /// </summary>
        public bool TryGetFailure(out Future.Failure outFailure)
        {
            outFailure = m_Failure;
            return m_State == State.Failed;
        }

        /// <summary>
        /// Fails the Future, or throws an exception
        /// if the Future has already been set or failed.
        /// </summary>
        public void Fail()
        {
            Fail(Future.FailureType.Unknown, null);
        }

        /// <summary>
        /// Fails the Future, or throws an exception
        /// if the Future has already been set or failed.
        /// </summary>
        public void Fail(Future.FailureType inType)
        {
            Fail(inType, null);
        }

        /// <summary>
        /// Fails the Future, or throws an exception
        /// if the Future has already been set or failed.
        /// </summary>
        public void Fail(Future.FailureType inType, object inArg)
        {
            if (m_State == State.Cancelled)
                return;
            
            if (m_State != State.Uninitialized)
                throw new InvalidOperationException("Cannot fail Future<" + typeof(T).Name + "> once Future has completed or failed!");
            m_State = State.Failed;
            m_Failure.Type = inType;
            m_Failure.Object = inArg;

            if (m_CallbackFail != null)
            {
                Routine.StartCall(m_CallbackFail);
                m_CallbackFail = null;
            }

            if (m_CallbackFailWithArgs != null)
            {
                Routine.StartCall(InvokeFailure, m_CallbackFailWithArgs);
                m_CallbackFailWithArgs = null;
            }

            m_CallbackComplete = null;
        }

        /// <summary>
        /// Adds a callback for when the Future fails.
        /// Will call immediately if the Future has already failed.
        /// </summary>
        public Future<T> OnFail(Action inCallback)
        {
            if (inCallback == null)
                return this;

            if (m_State == State.Failed)
                inCallback();
            else if (m_State == State.Uninitialized)
                m_CallbackFail += inCallback;

            return this;
        }

        /// <summary>
        /// Adds a callback for when the Future fails.
        /// Will call immediately if the Future has already failed.
        /// </summary>
        public Future<T> OnFail(Action<Future.Failure> inCallback)
        {
            if (inCallback == null)
                return this;

            if (m_State == State.Failed)
                inCallback(m_Failure);
            else if (m_State == State.Uninitialized)
                m_CallbackFailWithArgs += inCallback;

            return this;
        }

        #endregion

        #region Cancellation

        /// <summary>
        /// Returns if the Future has been cancelled.
        /// </summary>
        public bool IsCancelled() { return m_State == State.Cancelled; }

        /// <summary>
        /// Cancels the Future. It will no longer receive Complete or Fail calls.
        /// </summary>
        public void Cancel()
        {
            if (m_State == State.Uninitialized)
            {
                m_State = State.Cancelled;
                m_Prophet.Stop();
            }
        }

        #endregion

        /// <summary>
        /// Links a Routine to the Future.
        /// If the Routine stops, the Future will fail.
        /// If the Future is cancelled, the Routine will Stop.
        /// </summary>
        public Future<T> LinkTo(Routine inRoutine)
        {
            if (!m_Prophet && m_State == State.Uninitialized)
            {
                m_Prophet = inRoutine;
                m_Prophet.OnStop(OnProphetStopped);
            }
            return this;
        }

        /// <summary>
        /// Waits for the Future to be completed or failed.
        /// </summary>
        public IEnumerator Wait()
        {
            if (m_State == State.Uninitialized)
                return WaitInternal();
            return null;
        }

        private IEnumerator WaitInternal()
        {
            while(m_State == State.Uninitialized)
                yield return null;
        }

        private void OnProphetStopped()
        {
            Fail(Future.FailureType.RoutineStopped);
        }

        private void InvokeFailure(Action<Future.Failure> inFailure)
        {
            inFailure(m_Failure);
        }

        static public implicit operator T(Future<T> inValue)
        {
            return inValue.Get();
        }
    }
}
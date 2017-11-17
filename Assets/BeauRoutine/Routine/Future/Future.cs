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
        static public Future<T> Create<T>(Action<T> inCompleteCallback, Action<object> inFailureCallback)
        {
            return new Future<T>(inCompleteCallback, inFailureCallback);
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

        private object m_FailObject;
        private Action m_CallbackFail;
        private Action<object> m_CallbackFailWithArgs;

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

        public Future(Action<T> inCompleteCallback, Action<object> inFailureCallback)
        {
            m_State = State.Uninitialized;
            m_Value = default(T);
            m_CallbackComplete = inCompleteCallback;
            m_CallbackFailWithArgs = inFailureCallback;
        }

        public void Dispose()
        {
            m_State = State.Cancelled;
            m_Value = default(T);
            m_CallbackComplete = null;
            m_FailObject = null;
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
        public object GetFailure()
        {
            if (m_State != State.Failed)
                throw new InvalidOperationException("Cannot get error of Future<" + typeof(T).Name + "> before it has failed!");
            return m_FailObject;
        }

        /// <summary>
        /// Attempts to return the failure object.
        /// </summary>
        public bool TryGetFailure(out object outError)
        {
            outError = m_FailObject;
            return m_State == State.Failed;
        }

        /// <summary>
        /// Fails the Future, or throws an exception
        /// if the Future has already been set or failed.
        /// </summary>
        public void Fail()
        {
            Fail(null);
        }

        /// <summary>
        /// Fails the Future, or throws an exception
        /// if the Future has already been set or failed.
        /// </summary>
        public void Fail(object inArgument)
        {
            if (m_State == State.Cancelled)
                return;
            
            if (m_State != State.Uninitialized)
                throw new InvalidOperationException("Cannot fail Future<" + typeof(T).Name + "> once Future has completed or failed!");
            m_State = State.Failed;
            m_FailObject = inArgument;

            if (m_CallbackFail != null)
            {
                Routine.StartCall(m_CallbackFail);
                m_CallbackFail = null;
            }

            if (m_CallbackFailWithArgs != null)
            {
                Routine.StartCall(m_CallbackFailWithArgs, m_FailObject);
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
        public Future<T> OnFail(Action<object> inCallback)
        {
            if (inCallback == null)
                return this;

            if (m_State == State.Failed)
                inCallback(m_FailObject);
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
            }
        } 

        #endregion

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

        static public implicit operator T(Future<T> inValue)
        {
            return inValue.Get();
        }
    }
}
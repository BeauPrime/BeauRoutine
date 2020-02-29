/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Oct 2017
 * 
 * File:    Future.cs
 * Purpose: Single-assignment Future value. Can be used to return values
            from a coroutine.
*/

using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Interface for a Future value.
    /// </summary>
    public interface IFuture : IDisposable
    {
        bool IsDone();

        // Progress
        bool IsInProgress();
        float GetProgress();
        void SetProgress(float inProgress);
        IFuture OnProgress(Action<float> inProgressCallback);

        // Completion
        bool IsComplete();

        // Failure
        bool IsFailed();
        Future.Failure GetFailure();
        bool TryGetFailure(out Future.Failure outFailure);
        void Fail();
        void Fail(object inArg);
        void Fail(Exception inException);
        void Fail(Future.Failure inFailure);
        void Fail(Future.FailureType inType);
        void Fail(Future.FailureType inType, object inArg);
        IFuture OnFail(Action inFailureCallback);
        IFuture OnFail(Action<Future.Failure> inFailureCallback);

        // Cancellation
        bool IsCancelled();
        void Cancel();

        // Coroutines
        IFuture LinkTo(Routine inRoutine);
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

        #region Linked

        public delegate IEnumerator LinkedFutureDelegate<T>(Future<T> inFuture);
        public delegate IEnumerator LinkedFutureDelegate<T, A1>(Future<T> inFuture, A1 inArg1);
        public delegate IEnumerator LinkedFutureDelegate<T, A1, A2>(Future<T> inFuture, A1 inArg1, A2 inArg2);
        public delegate IEnumerator LinkedFutureDelegate<T, A1, A2, A3>(Future<T> inFuture, A1 inArg1, A2 inArg2, A3 inArg3);
        public delegate IEnumerator LinkedFutureDelegate<T, A1, A2, A3, A4>(Future<T> inFuture, A1 inArg1, A2 inArg2, A3 inArg3, A4 inArg4);

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future<T> CreateLinked<T>(LinkedFutureDelegate<T> inFunction, MonoBehaviour inHost = null)
        {
            var future = Future.Create<T>();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future))
            );
            return future;
        }

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future<T> CreateLinked<T, A1>(LinkedFutureDelegate<T, A1> inFunction, A1 inArg1, MonoBehaviour inHost = null)
        {
            var future = Future.Create<T>();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future, inArg1))
            );
            return future;
        }

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future<T> CreateLinked<T, A1, A2>(LinkedFutureDelegate<T, A1, A2> inFunction, A1 inArg1, A2 inArg2, MonoBehaviour inHost = null)
        {
            var future = Future.Create<T>();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future, inArg1, inArg2))
            );
            return future;
        }

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future<T> CreateLinked<T, A1, A2, A3>(LinkedFutureDelegate<T, A1, A2, A3> inFunction, A1 inArg1, A2 inArg2, A3 inArg3, MonoBehaviour inHost = null)
        {
            var future = Future.Create<T>();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future, inArg1, inArg2, inArg3))
            );
            return future;
        }

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future<T> CreateLinked<T, A1, A2, A3, A4>(LinkedFutureDelegate<T, A1, A2, A3, A4> inFunction, A1 inArg1, A2 inArg2, A3 inArg3, A4 inArg4, MonoBehaviour inHost = null)
        {
            var future = Future.Create<T>();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future, inArg1, inArg2, inArg3, inArg4))
            );
            return future;
        }

        #endregion // Linked
    }

    /// <summary>
    /// Represents a value that will be set at some point
    /// in the future. Can be either completed or failed.
    /// </summary>
    public class Future<T> : IFuture
    {
        private enum State { InProgress, Completed, Failed, Cancelled }

        private State m_State;

        private float m_Progress = 0;
        private Action<float> m_CallbackProgress;

        private T m_Value;
        private Action<T> m_CallbackComplete;

        private Future.Failure m_Failure;
        private Action m_CallbackFail;
        private Action<Future.Failure> m_CallbackFailWithArgs;

        private Routine m_Prophet;
        private AsyncHandle m_Async;

        public Future()
        {
            m_State = State.InProgress;
            m_Value = default(T);
            m_CallbackComplete = null;
            m_CallbackFail = null;
        }

        public Future(Action<T> inCompleteCallback)
        {
            m_State = State.InProgress;
            m_Value = default(T);
            m_CallbackComplete = inCompleteCallback;
            m_CallbackFail = null;
        }

        public Future(Action<T> inCompleteCallback, Action inFailureCallback)
        {
            m_State = State.InProgress;
            m_Value = default(T);
            m_CallbackComplete = inCompleteCallback;
            m_CallbackFail = inFailureCallback;
        }

        public Future(Action<T> inCompleteCallback, Action<Future.Failure> inFailureCallback)
        {
            m_State = State.InProgress;
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

            m_Progress = 0;
            m_Value = default(T);
            m_CallbackComplete = null;
            m_Failure.Object = null;
            m_Failure.Type = Future.FailureType.Unknown;
            m_CallbackFail = null;
            m_CallbackFailWithArgs = null;
            m_CallbackProgress = null;
        }

        /// <summary>
        /// Returns if the Future has been completed, failed, or cancelled.
        /// </summary>
        public bool IsDone() { return m_State != State.InProgress; }

        #region Progress

        /// <summary>
        /// Returns if a Future is in progress.
        /// </summary>
        public bool IsInProgress() { return m_State == State.InProgress; }

        /// <summary>
        /// Returns the future's progress towards completion. [0, 1]
        /// </summary>
        public float GetProgress()
        {
            return m_Progress;
        }

        /// <summary>
        /// Sets the future's progress towards completion. [0, 1]
        /// </summary>
        public void SetProgress(float inProgress)
        {
            if (m_State != State.InProgress)
                return;

            inProgress = inProgress < 0 ? 0 : (inProgress > 1 ? 1 : inProgress);
            if (m_Progress != inProgress)
            {
                m_Progress = inProgress;
                if (m_CallbackProgress != null)
                    Async.InvokeAsync(m_CallbackProgress, m_Progress);
            }
        }

        IFuture IFuture.OnProgress(Action<float> inProgressCallback)
        {
            return OnProgress(inProgressCallback);
        }

        /// <summary>
        /// Adds a callback for when the Future progresses.
        /// </summary>
        public Future<T> OnProgress(Action<float> inProgressCallback)
        {
            if (inProgressCallback == null)
                return this;

            if (m_State == State.Completed)
                inProgressCallback(m_Progress);
            else if (m_State == State.InProgress)
            {
                m_CallbackProgress += inProgressCallback;
                if (m_Progress > 0)
                    inProgressCallback(m_Progress);
            }

            return this;
        }

        #endregion

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

            if (m_State != State.InProgress)
                throw new InvalidOperationException("Cannot set value of Future<" + typeof(T).Name + "> once Future has completed or failed!");
            m_State = State.Completed;
            m_Value = inValue;

            // Force progress to 1.
            if (m_Progress < 1)
            {
                m_Progress = 1;
                if (m_CallbackProgress != null)
                    Async.InvokeAsync(m_CallbackProgress, m_Progress);
                m_CallbackProgress = null;
            }

            if (m_CallbackComplete != null)
            {
                Async.InvokeAsync(m_CallbackComplete, m_Value);
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
            else if (m_State == State.InProgress)
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
        public void Fail(object inArg)
        {
            Fail(Future.FailureType.Unknown, inArg);
        }

        /// <summary>
        /// Fails the Future, or throws an exception
        /// if the Future has already been set or failed.
        /// </summary>
        public void Fail(Exception inException)
        {
            Fail(Future.FailureType.Exception, inException);
        }

        /// <summary>
        /// Fails the Future, or throws an exception
        /// if the Future has already been set or failed.
        /// </summary>
        public void Fail(Future.Failure inFailure)
        {
            Fail(inFailure.Type, inFailure.Object);
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

            if (m_State != State.InProgress)
                throw new InvalidOperationException("Cannot fail Future<" + typeof(T).Name + "> once Future has completed or failed!");
            m_State = State.Failed;
            m_Failure.Type = inType;
            m_Failure.Object = inArg;

            if (m_CallbackFail != null)
            {
                Async.InvokeAsync(m_CallbackFail);
                m_CallbackFail = null;
            }

            if (m_CallbackFailWithArgs != null)
            {
                Async.InvokeAsync(InvokeFailure, m_CallbackFailWithArgs);
                m_CallbackFailWithArgs = null;
            }

            m_CallbackComplete = null;
            m_CallbackProgress = null;
        }

        IFuture IFuture.OnFail(Action inFailureCallback)
        {
            return OnFail(inFailureCallback);
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
            else if (m_State == State.InProgress)
                m_CallbackFail += inCallback;

            return this;
        }

        IFuture IFuture.OnFail(Action<Future.Failure> inFailureCallback)
        {
            return OnFail(inFailureCallback);
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
            else if (m_State == State.InProgress)
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
            if (m_State == State.InProgress)
            {
                m_State = State.Cancelled;
                m_Prophet.Stop();
                m_Async.Cancel();
            }
        }

        #endregion

        IFuture IFuture.LinkTo(Routine inRoutine)
        {
            return LinkTo(inRoutine);
        }

        /// <summary>
        /// Links a Routine to the Future.
        /// If the Routine stops, the Future will fail.
        /// If the Future is cancelled, the Routine will Stop.
        /// </summary>
        public Future<T> LinkTo(Routine inRoutine)
        {
            if (!m_Prophet && m_State == State.InProgress)
            {
                m_Prophet = inRoutine;
                m_Prophet.OnStop(OnLinkedStopped);
            }
            return this;
        }

        /// <summary>
        /// Links an async handle to the Future.
        /// If the async operation stops, the Future will fail.
        /// If the Future is cancelled, the operation will Stop.
        /// </summary>
        public Future<T> LinkTo(AsyncHandle inAsync)
        {
            if (m_Async == AsyncHandle.Null && m_State == State.InProgress)
            {
                m_Async = inAsync;
                m_Async.OnStop(OnLinkedStopped);
            }
            return this;
        }

        /// <summary>
        /// Waits for the Future to be completed or failed.
        /// </summary>
        public IEnumerator Wait()
        {
            if (m_State == State.InProgress)
                return WaitInternal();
            return null;
        }

        private IEnumerator WaitInternal()
        {
            while (m_State == State.InProgress)
                yield return null;
        }

        private void OnLinkedStopped()
        {
            if (m_State == State.InProgress)
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

        public override string ToString()
        {
            string prophetName = m_Prophet.GetName();
            if (prophetName != null)
            {
                return string.Format("[Future<{0}>; State={1}; Progress={2:0}%; LinkedTo={3}]", typeof(T).FullName, m_State, m_Progress * 100f, prophetName);
            }
            return string.Format("[Future<{0}>; State={1}; Progress={2:0}%]", typeof(T).FullName, m_State, m_Progress * 100);
        }
    }
}
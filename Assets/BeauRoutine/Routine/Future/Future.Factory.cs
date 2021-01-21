/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Jan 2021
 * 
 * File:    Future.Factory.cs
 * Purpose: Factory methods for constructing futures.
*/

using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Static methods for creating Futures.
    /// </summary>
    public partial class Future
    {
        /// <summary>
        /// Creates a Future.
        /// </summary>
        static public Future<T> Create<T>()
        {
            return new Future<T>();
        }

        /// <summary>
        /// Creates a Future.
        /// </summary>
        static public Future Create()
        {
            return new Future();
        }

        /// <summary>
        /// Creates a Future with a completion callback.
        /// </summary>
        static public Future<T> Create<T>(Action<T> inCompleteCallback)
        {
            return new Future<T>(inCompleteCallback);
        }

        /// <summary>
        /// Creates a Future with a completion callback.
        /// </summary>
        static public Future Create(Action inCompleteCallback)
        {
            return new Future(inCompleteCallback);
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
        static public Future Create(Action inCompleteCallback, Action inFailureCallback)
        {
            return new Future(inCompleteCallback, inFailureCallback);
        }

        /// <summary>
        /// Creates a Future with a completion callback and failure callback.
        /// </summary>
        static public Future<T> Create<T>(Action<T> inCompleteCallback, Action<Failure> inFailureCallback)
        {
            return new Future<T>(inCompleteCallback, inFailureCallback);
        }

        /// <summary>
        /// Creates a Future with a completion callback and failure callback.
        /// </summary>
        static public Future Create(Action inCompleteCallback, Action<Failure> inFailureCallback)
        {
            return new Future(inCompleteCallback, inFailureCallback);
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
        /// Creates an already-completed Future. 
        /// </summary>
        static public Future Completed()
        {
            Future future = new Future();
            future.Complete();
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
        static public Future Failed()
        {
            Future future = new Future();
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
        static public Future Failed(FailureType inType)
        {
            Future future = new Future();
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

        /// <summary>
        /// Creates an already-failed Future. 
        /// </summary>
        static public Future Failed(FailureType inType, object inArg)
        {
            Future future = new Future();
            future.Fail(inType, inArg);
            return future;
        }

        #region Linked

        public delegate IEnumerator LinkedFutureDelegate(Future inFuture);
        public delegate IEnumerator LinkedFutureDelegate<A1>(Future inFuture, A1 inArg1);
        public delegate IEnumerator LinkedFutureDelegate<A1, A2>(Future inFuture, A1 inArg1, A2 inArg2);
        public delegate IEnumerator LinkedFutureDelegate<A1, A2, A3>(Future inFuture, A1 inArg1, A2 inArg2, A3 inArg3);
        public delegate IEnumerator LinkedFutureDelegate<A1, A2, A3, A4>(Future inFuture, A1 inArg1, A2 inArg2, A3 inArg3, A4 inArg4);

        public delegate IEnumerator GenericLinkedFutureDelegate<T>(Future<T> inFuture);
        public delegate IEnumerator GenericLinkedFutureDelegate<T, A1>(Future<T> inFuture, A1 inArg1);
        public delegate IEnumerator GenericLinkedFutureDelegate<T, A1, A2>(Future<T> inFuture, A1 inArg1, A2 inArg2);
        public delegate IEnumerator GenericLinkedFutureDelegate<T, A1, A2, A3>(Future<T> inFuture, A1 inArg1, A2 inArg2, A3 inArg3);
        public delegate IEnumerator GenericLinkedFutureDelegate<T, A1, A2, A3, A4>(Future<T> inFuture, A1 inArg1, A2 inArg2, A3 inArg3, A4 inArg4);

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future<T> CreateLinked<T>(GenericLinkedFutureDelegate<T> inFunction, MonoBehaviour inHost = null)
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
        static public Future<T> CreateLinked<T, A1>(GenericLinkedFutureDelegate<T, A1> inFunction, A1 inArg1, MonoBehaviour inHost = null)
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
        static public Future<T> CreateLinked<T, A1, A2>(GenericLinkedFutureDelegate<T, A1, A2> inFunction, A1 inArg1, A2 inArg2, MonoBehaviour inHost = null)
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
        static public Future<T> CreateLinked<T, A1, A2, A3>(GenericLinkedFutureDelegate<T, A1, A2, A3> inFunction, A1 inArg1, A2 inArg2, A3 inArg3, MonoBehaviour inHost = null)
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
        static public Future<T> CreateLinked<T, A1, A2, A3, A4>(GenericLinkedFutureDelegate<T, A1, A2, A3, A4> inFunction, A1 inArg1, A2 inArg2, A3 inArg3, A4 inArg4, MonoBehaviour inHost = null)
        {
            var future = Future.Create<T>();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future, inArg1, inArg2, inArg3, inArg4))
            );
            return future;
        }

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future CreateLinked(LinkedFutureDelegate inFunction, MonoBehaviour inHost = null)
        {
            var future = Future.Create();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future))
            );
            return future;
        }

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future CreateLinked<A1>(LinkedFutureDelegate<A1> inFunction, A1 inArg1, MonoBehaviour inHost = null)
        {
            var future = Future.Create();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future, inArg1))
            );
            return future;
        }

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future CreateLinked<A1, A2>(LinkedFutureDelegate<A1, A2> inFunction, A1 inArg1, A2 inArg2, MonoBehaviour inHost = null)
        {
            var future = Future.Create();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future, inArg1, inArg2))
            );
            return future;
        }

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future CreateLinked<A1, A2, A3>(LinkedFutureDelegate<A1, A2, A3> inFunction, A1 inArg1, A2 inArg2, A3 inArg3, MonoBehaviour inHost = null)
        {
            var future = Future.Create();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future, inArg1, inArg2, inArg3))
            );
            return future;
        }

        /// <summary>
        /// Creates a future and runs a BeauRoutine with the future as its first argument.
        /// </summary>
        static public Future CreateLinked<A1, A2, A3, A4>(LinkedFutureDelegate<A1, A2, A3, A4> inFunction, A1 inArg1, A2 inArg2, A3 inArg3, A4 inArg4, MonoBehaviour inHost = null)
        {
            var future = Future.Create();
            future.LinkTo(
                Routine.Start(inHost, inFunction(future, inArg1, inArg2, inArg3, inArg4))
            );
            return future;
        }

        #endregion // Linked
    }
}
/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Oct 2017
 * 
 * File:    Future.Shortcuts.cs
 * Purpose: Shortcut methods for creating Futures for specific tasks.
*/

using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine
{
    static public partial class Future
    {
        /// <summary>
        /// Asynchronous resource loading methods. These will create Routines to load resources
        /// asynchronously and return a Future for the result.
        /// </summary>
        static public class Resources
        {
            /// <summary>
            /// Loads an asset from the Resources folder asynchronously
            /// and returns a Future for the loaded asset.
            /// </summary>
            static public Future<T> LoadAsync<T>(string inPath, MonoBehaviour inRoutineHost = null) where T : UnityEngine.Object
            {
                var future = Future.Create<T>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadResource<T>(future, inPath))
                    );
                return future;
            }

            static private IEnumerator DownloadResource<T>(Future<T> inFuture, string inPath) where T : UnityEngine.Object
            {
                var resourceRequest = UnityEngine.Resources.LoadAsync<T>(inPath);
                while(!resourceRequest.isDone)
                {
                    yield return null;
                    inFuture.SetProgress(resourceRequest.progress);
                }
                if (resourceRequest.asset == null)
                    inFuture.Fail(Future.FailureType.Unknown, "No resource found");
                else
                    inFuture.Complete((T)resourceRequest.asset);
            }
        }

        /// <summary>
        /// Creates Futures from function calls.
        /// </summary>
        static public class Call
        {
            /// <summary>
            /// Creates a Future that will be completed with the return value of the given function.
            /// </summary>
            static public Future<T> Func<T>(Func<T> inFunction, MonoBehaviour inRoutineHost = null) where T : UnityEngine.Object
            {
                var future = Future.Create<T>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, FuncRoutine(future, inFunction))
                );
                return future;
            }

            static private IEnumerator FuncRoutine<T>(Future<T> inFuture, Func<T> inFunction)
            {
                try
                {
                    inFuture.Complete(inFunction());
                }
                catch(Exception e)
                {
                    inFuture.Fail(FailureType.Exception, e);
                }
                yield break;
            }

            /// <summary>
            /// Creates a Future that will be resolved by the given function.
            /// </summary>
            static public Future<T> Resolve<T>(Action<Future<T>> inFunction, MonoBehaviour inRoutineHost = null) where T : UnityEngine.Object
            {
                var future = Future.Create<T>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, ResolveRoutine(future, inFunction))
                );
                return future;
            }

            static private IEnumerator ResolveRoutine<T>(Future<T> inFuture, Action<Future<T>> inFunction)
            {
                try
                {
                    inFunction(inFuture);
                }
                catch(Exception e)
                {
                    inFuture.Fail(FailureType.Exception, e);
                }

                if (!inFuture.IsDone())
                    inFuture.Fail(FailureType.RoutineStopped);
                yield break;
            }
        }
    }
}
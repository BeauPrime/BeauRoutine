/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 Oct 2017
 * 
 * File:    Future.Shortcuts.cs
 * Purpose: Shortcut methods for creating Futures for specific tasks.
*/

using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Static methods for dealing with Futures.
    /// </summary>
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
                Routine.Start(inRoutineHost, DownloadResource<T>(future, inPath));
                return future;
            }

            static private IEnumerator DownloadResource<T>(Future<T> inFuture, string inPath) where T : UnityEngine.Object
            {
                var resourceRequest = UnityEngine.Resources.LoadAsync<T>(inPath);
                yield return resourceRequest;
                if (resourceRequest.asset == null)
                    inFuture.Fail("No resource found");
                else
                    inFuture.Complete((T)resourceRequest.asset);
            }
        }
    }
}
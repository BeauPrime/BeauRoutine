/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Apr 2018
 * 
 * File:    Utils.cs
 * Purpose: Contains utility functions for disposing of objects.
*/

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    #define DEVELOPMENT
#endif

using System;
using System.Collections;

namespace BeauRoutine.Internal
{
    static internal class DisposeUtils
    {
        /// <summary>
        /// Disposes of an object if it is an IDisposable
        /// and nulls out the reference.
        /// </summary>
        static public void DisposeObject<T>(ref T ioObject) where T : class
        {
            if (ioObject != null)
            {
                if (ioObject is IDisposable)
                    ((IDisposable)ioObject).Dispose();
                ioObject = null;
            }
        }

        /// <summary>
        /// Disposes of an IEnumerator and nulls out the reference.
        /// </summary>
        static public void DisposeEnumerator<T>(ref T ioEnumerator) where T : IEnumerator
        {
            if (ioEnumerator != null)
            {
                ((IDisposable)ioEnumerator).Dispose();
                ioEnumerator = default(T);
            }
        }
    }
}

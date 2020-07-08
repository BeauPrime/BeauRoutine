/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 May 2018
 * 
 * File:    TweenPool.cs
 * Purpose: Provides creation and pooling for tweens.
*/

using System;
using UnityEngine;

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Controls pooling and tween creation.
    /// </summary>
    internal sealed class TweenPool
    {
        static private int s_InUse = 0;

        static private bool s_PoolEnabled = false;
        static private Tween[] s_Pool = null;
        static private int s_Available = 0;
        static private int s_Instantiated = 0;

        /// <summary>
        /// Returns a Tween from the pool,
        /// or allocates a new Tween.
        /// </summary>
        static public Tween Alloc()
        {
            ++s_InUse;

            if (s_PoolEnabled)
            {
                if (s_Available > 0)
                {
                    Tween t = s_Pool[--s_Available];
                    s_Pool[s_Available] = null;
                    return t;
                }

                ++s_Instantiated;
            }

            return new Tween();
        }

        /// <summary>
        /// Adds the given Tween to the pool, if pooling is enabled.
        /// </summary>
        static public void Free(Tween inTween)
        {
            --s_InUse;

            if (s_PoolEnabled)
            {
                if (s_Available >= s_Pool.Length)
                    Array.Resize(ref s_Pool, Mathf.NextPowerOfTwo(s_Pool.Length * 2));
                s_Pool[s_Available++] = inTween;
            }
        }

        /// <summary>
        /// Resizes to accommodate the given number of pooled tweens.
        /// </summary>
        static public void Resize(int inCapacity)
        {
            int desiredPoolSize = Mathf.NextPowerOfTwo(inCapacity + s_InUse);

            if (!s_PoolEnabled || (s_Pool != null && s_Pool.Length >= desiredPoolSize))
                return;

            Array.Resize(ref s_Pool, desiredPoolSize);

            int newRequired = inCapacity - s_Instantiated;
            if (newRequired > 0)
            {
                for (int i = 0; i < newRequired; ++i)
                    s_Pool[s_Available + i] = new Tween();

                s_Available += newRequired;
                s_Instantiated += newRequired;
            }
        }

        /// <summary>
        /// Starts pooling Tweens.
        /// </summary>
        static public void StartPooling(int inCapacity)
        {
            s_PoolEnabled = true;
            Resize(inCapacity);
        }

        /// <summary>
        /// Stops pooling Tweens.
        /// </summary>
        static public void StopPooling()
        {
            if (s_PoolEnabled)
            {
                s_PoolEnabled = false;

                for (int i = s_Available - 1; i >= 0; --i)
                    s_Pool[i] = null;
                s_Pool = null;

                s_Available = 0;
                s_Instantiated = 0;
            }
        }
    }
}
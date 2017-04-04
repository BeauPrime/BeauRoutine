/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 Apr 2017
 * 
 * File:    Frame.cs
 * Purpose: Data needed for an individual frame of routine execution.
*/

using UnityEngine;

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Contains settings for an individual frame.
    /// </summary>
    public struct Frame
    {
        /// <summary>
        /// Current delta time.
        /// </summary>
        public float DeltaTime;
        
        /// <summary>
        /// Unscaled delta time.
        /// </summary>
        public float UnscaledDeltaTime;

        /// <summary>
        /// Which groups are paused this frame.
        /// </summary>
        public int PauseMask;

        /// <summary>
        /// Time scale for each group.
        /// </summary>
        public float[] GroupTimeScale;

        /// <summary>
        /// Resets delta time.
        /// </summary>
        public void ResetTime()
        {
            DeltaTime = UnscaledDeltaTime = Time.deltaTime;
        }

        /// <summary>
        /// Scales delta time.
        /// </summary>
        public void SetTimeScale(float inScale)
        {
            DeltaTime = UnscaledDeltaTime * inScale;
        }

        /// <summary>
        /// Resets delta time scale.
        /// </summary>
        public void ResetTimeScale()
        {
            DeltaTime = UnscaledDeltaTime;
        }
    }
}

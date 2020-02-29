/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Apr 2017
 * 
 * File:    Frame.cs
 * Purpose: Data needed for an individual frame of routine execution.
 */

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Contains settings for an individual frame.
    /// </summary>
    internal struct Frame
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
        /// Time scale factor.
        /// </summary>
        public float TimeScale;

        /// <summary>
        /// Which groups are paused this frame.
        /// </summary>
        public int PauseMask;

        /// <summary>
        /// Time scale for each group.
        /// </summary>
        public float[] GroupTimeScale;

        /// <summary>
        /// Serial number.
        /// </summary>
        public byte Serial;

        /// <summary>
        /// Increments the serial counter.
        /// </summary>
        public void IncrementSerial()
        {
            if (Serial == 255)
                Serial = 0;
            else
                ++Serial;
        }

        /// <summary>
        /// Decrements the serial counter.
        /// </summary>
        public void DecrementSerial()
        {
            if (Serial == 0)
                Serial = 255;
            else
                --Serial;
        }

        /// <summary>
        /// Resets delta time.
        /// </summary>
        public void ResetTime(float inDeltaTime, float inGlobalTimescale)
        {
            TimeScale = 1;
            DeltaTime = UnscaledDeltaTime = inDeltaTime * inGlobalTimescale;
        }

        /// <summary>
        /// Scales delta time.
        /// </summary>
        public void SetTimeScale(float inScale)
        {
            TimeScale = inScale;
            DeltaTime = UnscaledDeltaTime * inScale;
        }

        /// <summary>
        /// Refreshes delta time to scaled value.
        /// </summary>
        public void RefreshTimeScale()
        {
            DeltaTime = UnscaledDeltaTime * TimeScale;
        }

        /// <summary>
        /// Resets delta time scale.
        /// </summary>
        public void ResetTimeScale()
        {
            TimeScale = 1;
            DeltaTime = UnscaledDeltaTime;
        }
    }
}
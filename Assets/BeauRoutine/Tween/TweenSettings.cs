/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenSettings.cs
 * Purpose: Tuple of time to curve. Used in overloaded Tween
 *          shortcuts as a means of setting both time and easing
 *          curve simultaneously.
*/

using System;

namespace BeauRoutine
{
    /// <summary>
    /// Basic tween settings in a struct.
    /// Useful for grouping animations in the inspector.
    /// </summary>
    [Serializable]
    public struct TweenSettings
    {
        /// <summary>
        /// Duration of the tween.
        /// </summary>
        public float Time;

        /// <summary>
        /// Easing function for the value.
        /// </summary>
        public Curve Curve;

        public TweenSettings(float inTime, Curve inCurve = default(Curve))
        {
            Time = inTime;
            Curve = inCurve;
        }
    }
}

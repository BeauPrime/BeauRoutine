/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 May 2018
 * 
 * File:    SplineOffset.cs
 * Purpose: Enum describing how Spline positions should be applied.
*/

namespace BeauRoutine.Splines
{
    /// <summary>
    /// How a spline should affect transform positioning.
    /// </summary>
    public enum SplineOffset : byte
    {
        /// <summary>
        /// Move relative to starting position.
        /// </summary>
        Relative,

        /// <summary>
        /// Move directly along spline.
        /// </summary>
        Absolute
    }
}

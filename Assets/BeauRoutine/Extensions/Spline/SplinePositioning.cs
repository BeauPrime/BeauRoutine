/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 May 2018
 * 
 * File:    SplineMode.cs
 * Purpose: Enum describing how Spline positions should be applied.
*/

namespace BeauRoutine.Splines
{
    /// <summary>
    /// How a spline should affect transform positioning.
    /// </summary>
    public enum SplinePositioning : byte
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

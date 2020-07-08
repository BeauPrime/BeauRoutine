/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    AngleMode.cs
 * Purpose: Enum describing how Euler interoplation
 *          should be carried out.
*/

namespace BeauRoutine
{
    /// <summary>
    /// How an euler rotation should be tweened.
    /// </summary>
    public enum AngleMode : byte
    {
        /// <summary>
        /// Rotate along the shortest path.
        /// </summary>
        Shortest,

        /// <summary>
        /// Rotate directly to the value.
        /// Helpful for rotating more than 180 degrees.
        /// </summary>
        Absolute
    }
}

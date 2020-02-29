/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    11 May 2018
 * 
 * File:    SplineType.cs
 * Purpose: Enum for the type of spline.
*/

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Type of spline.
    /// </summary>
    public enum SplineType
    {
        /// <summary>
        /// Quadratic bezier. Two points, one control point.
        /// </summary>
        SimpleSpline,

        /// <summary>
        /// Polygonal spline. Multiple points, no controls.
        /// </summary>
        LinearSpline,

        /// <summary>
        /// Cubic hermite spline. Multiple points with tangents.
        /// </summary>
        CSpline,

        /// <summary>
        /// Catmull-Rom/Cardinal spline. Multiple points, with tension parameter and optional control points for non-looped splines.
        /// </summary>
        Cardinal
    }
}

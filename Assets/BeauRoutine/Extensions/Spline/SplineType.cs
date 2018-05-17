/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
        // Simple spline. Two points, one control point.
        SimpleSpline,

        // Polygonal "spline". Multiple points, no controls.
        VertexSpline,

        // Cubic spline. Multiple points w/ tangents.
        CSpline
    }
}

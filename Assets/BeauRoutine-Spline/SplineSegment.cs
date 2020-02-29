/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    11 May 2018
 * 
 * File:    SplineSegment.cs
 * Purpose: Enum describing how Spline positions should be applied.
*/

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Info about a segment of a spline.
    /// </summary>
    public struct SplineSegment
    {
        /// <summary>
        /// Index of the first vertex.
        /// </summary>
        public int VertexA;

        /// <summary>
        /// Index of the second vertex.
        /// </summary>
        public int VertexB;

        /// <summary>
        /// Interpolation between these two vertices.
        /// </summary>
        public float Interpolation;
    }
}

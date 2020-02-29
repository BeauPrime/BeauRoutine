/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 June 2018
 * 
 * File:    SplineUpdateInfo.cs
 * Purpose: Struct containing info about a Spine update.
*/

using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Information about a Spline update.
    /// </summary>
    public struct SplineUpdateInfo
    {
        /// <summary>
        /// Spline.
        /// </summary>
        public ISpline Spline;

        /// <summary>
        /// Percentage along the spline.
        /// </summary>
        public float Percent;

        /// <summary>
        /// Point along the spline.
        /// </summary>
        public Vector3 Point;

        /// <summary>
        /// Direction at this point on the spline.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Returns the current segment on the spline.
        /// </summary>
        public SplineSegment GetSegment()
        {
            return Spline.GetSegment(Percent);
        }
    }

    /// <summary>
    /// Callback for spline updates.
    /// </summary>
    public delegate void SplineUpdateDelegate(SplineUpdateInfo inUpdateInfo);
}

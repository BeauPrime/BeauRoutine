/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    11 May 2018
 * 
 * File:    ISpline.cs
 * Purpose: Defines a common interface for splines.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Interface for a spline.
    /// </summary>
    public interface ISpline
    {
        /// <summary>
        /// Returns the type of spline.
        /// </summary>
        SplineType GetSplineType();

        /// <summary>
        /// Returns the total distance between vertices. 
        /// </summary>
        float GetDistance();

        /// <summary>
        /// Returns the direct distance between vertices.
        /// </summary>
        float GetDirectDistance();

        /// <summary>
        /// Returns the number of vertices.
        /// </summary>
        int GetVertexCount();

        /// <summary>
        /// Returns the vertex at the given index.
        /// </summary>
        Vector3 GetVertex(int inIndex);

        /// <summary>
        /// Returns if the spline is a closed loop.
        /// </summary>
        bool IsLooped();

        /// <summary>
        /// Returns the vertex interpolation percentage for the given percentage and method.
        /// </summary>
        float TransformPercent(float inPercent, SplineLerp inLerpMethod);

        /// <summary>
        /// Returns the method interpolation percentage corresponding to the given percentage and method.
        /// </summary>
        float InvTransformPercent(float inPercent, SplineLerp inLerpMethod);

        /// <summary>
        /// Performs any processing necessary to perform interpolations.
        /// </summary>
        void Process();

        /// <summary>
        /// Interpolates along the spline.
        /// </summary>
        Vector3 GetPoint(float inPercent, Curve inSegmentCurve = Curve.Linear);

        /// <summary>
        /// Returns the direction along the spline.
        /// </summary>
        Vector3 GetDirection(float inPercent, Curve inSegmentCurve = Curve.Linear);

        /// <summary>
        /// Retrieves info about the segment the given percentage of the way through the spline.
        /// </summary>
        void GetSegment(float inPercent, out SplineSegment outSegment);
        
        // /// <summary>
        // /// Returns the closest point along the spline to the given point.
        // /// Also outputs the percentage distance along the spline.
        // /// </summary>
        // Vector3 GetClosestPoint(Vector3 inPoint, out float outPercentage);
    }
}
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
        bool IsClosed();

        /// <summary>
        /// Returns the correct interpolation percentage for the given percentage and method.
        /// </summary>
        float CorrectPercent(float inPercent, SplineLerp inLerpMethod);

        /// <summary>
        /// Performs any processing necessary to perform interpolations.
        /// </summary>
        void Process();

        /// <summary>
        /// Interpolates along the spline.
        /// </summary>
        Vector3 Lerp(float inPercent, Curve inSegmentCurve = Curve.Linear);
        
        // /// <summary>
        // /// Returns the closest point along the spline to the given point.
        // /// Also outputs how far along the path this is in the given lerp method space.
        // /// </summary>
        // Vector3 GetClosestPoint(Vector3 inPoint, SplineLerpSpace inLerpMethod, out float outPercentage);

        // /// <summary>
        // /// Retrieves info about the segment the given percentage of the way through the spline.
        // /// </summary>
        // void GetSegment(float inPercent, out SplineSegment outSegment);

        // /// <summary>
        // /// Populates the given Vector3 array with vertices along the spline.
        // /// Returns the number of vertices added.
        // /// </summary>
        // int Sample(Vector3[] outPoints, float inStart, float inEnd, int inStartIdx, float inNumSamples);

        // /// <summary>
        // /// Populates the given Vector3 list with vertices along the spline.
        // /// Returns the number of vertices added.
        // /// </summary>
        // int Sample(List<Vector3> outPoints, float inStart, float inEnd, float inNumSamples);
    }
}
/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    11 May 2018
 * 
 * File:    ISpline.cs
 * Purpose: Defines a common interface for splines.
*/

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Interface for a spline.
    /// </summary>
    public interface ISpline
    {
        #region Basic Info

        /// <summary>
        /// Returns the type of spline.
        /// </summary>
        SplineType GetSplineType();

        /// <summary>
        /// Returns if the spline is a closed loop.
        /// </summary>
        bool IsLooped();

        /// <summary>
        /// Returns the total distance between vertices. 
        /// </summary>
        float GetDistance();

        /// <summary>
        /// Returns the direct distance between vertices.
        /// </summary>
        float GetDirectDistance();

        #endregion // Basic Info

        #region Vertex Info

        /// <summary>
        /// Returns the number of vertices.
        /// </summary>
        int GetVertexCount();

        /// <summary>
        /// Returns the vertex at the given index.
        /// </summary>
        Vector3 GetVertex(int inIndex);

        /// <summary>
        /// Sets the vertex at the given index.
        /// </summary>
        void SetVertex(int inIndex, Vector3 inVertex);

        /// <summary>
        /// Gets user data for the given vertex.
        /// </summary>
        object GetVertexUserData(int inIndex);

        /// <summary>
        /// Sets user data for the given vertex.
        /// </summary>
        void SetVertexUserData(int inIndex, object inUserData);

        /// <summary>
        /// Returns the number of control points.
        /// </summary>
        int GetControlCount();

        /// <summary>
        /// Returns the control point at the given index.
        /// </summary>
        Vector3 GetControlPoint(int inIndex);

        /// <summary>
        /// Sets the control point at the given index.
        /// </summary>
        void SetControlPoint(int inIndex, Vector3 inVertex);

        #endregion // Vertex Info

        #region Evaluation

        /// <summary>
        /// Returns the vertex interpolation percentage for the given percentage and method.
        /// </summary>
        float TransformPercent(float inPercent, SplineLerp inLerpMethod);

        /// <summary>
        /// Returns the method interpolation percentage corresponding to the given percentage and method.
        /// </summary>
        float InvTransformPercent(float inPercent, SplineLerp inLerpMethod);

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

        #endregion // Evaluation

        #region Operations

        /// <summary>
        /// Performs any processing necessary to perform interpolations.
        /// </summary>
        bool Process();

        /// <summary>
        /// Callback for when a Spline is updated.
        /// </summary>
        Action<ISpline> OnUpdated { get; set; }

        #endregion // Operations
    }
}
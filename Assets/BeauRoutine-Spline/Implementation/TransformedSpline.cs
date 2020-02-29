/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 June 2018
 * 
 * File:    TransformedSpline.cs
 * Purpose: Wrapper around a Spline. Transforms vertices and directions.
*/

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Transforms vertices, control points, and directions for the given spline.
    /// </summary>
    public class TransformedSpline<T> : ISpline where T : ISpline
    {
        public readonly Transform Transform;
        public readonly T Spline;

        public TransformedSpline(Transform inTransform, T inSpline)
        {
            Transform = inTransform;
            Spline = inSpline;
        }

        public Action<ISpline> OnUpdated
        {
            get { return Spline.OnUpdated; }
            set { Spline.OnUpdated = value; }
        }

        public int GetControlCount()
        {
            return Spline.GetControlCount();
        }

        public Vector3 GetControlPoint(int inIndex)
        {
            return Transform.TransformPoint(Spline.GetControlPoint(inIndex));
        }

        public float GetDirectDistance()
        {
            return Spline.GetDirectDistance();
        }

        public Vector3 GetDirection(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            return Transform.TransformVector(Spline.GetDirection(inPercent, inSegmentCurve));
        }

        public float GetDistance()
        {
            return Spline.GetDistance();
        }

        public Vector3 GetPoint(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            return Transform.TransformPoint(Spline.GetPoint(inPercent, inSegmentCurve));
        }

        public void GetSegment(float inPercent, out SplineSegment outSegment)
        {
            Spline.GetSegment(inPercent, out outSegment);
        }

        public SplineType GetSplineType()
        {
            return Spline.GetSplineType();
        }

        public Vector3 GetVertex(int inIndex)
        {
            return Transform.TransformPoint(Spline.GetVertex(inIndex));
        }

        public int GetVertexCount()
        {
            return Spline.GetVertexCount();
        }

        public object GetVertexUserData(int inIndex)
        {
            return Spline.GetVertexUserData(inIndex);
        }

        public float InvTransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            return Spline.InvTransformPercent(inPercent, inLerpMethod);
        }

        public bool IsLooped()
        {
            return Spline.IsLooped();
        }

        public bool Process()
        {
            return Spline.Process();
        }

        public void SetControlPoint(int inIndex, Vector3 inVertex)
        {
            Spline.SetControlPoint(inIndex, Transform.InverseTransformPoint(inVertex));
        }

        public void SetVertex(int inIndex, Vector3 inVertex)
        {
            Spline.SetVertex(inIndex, Transform.InverseTransformPoint(inVertex));
        }

        public void SetVertexUserData(int inIndex, object inUserData)
        {
            Spline.SetVertexUserData(inIndex, inUserData);
        }

        public float TransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            return Spline.TransformPercent(inPercent, inLerpMethod);
        }

        static public explicit operator T(TransformedSpline<T> inSpline)
        {
            return inSpline.Spline;
        }
    }
}
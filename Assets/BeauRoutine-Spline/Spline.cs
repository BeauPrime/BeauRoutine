/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 May 2018
 * 
 * File:    Spline.cs
 * Purpose: Defines a common interface for splines, along with factory methods.
 */

using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine.Splines
{
    static public partial class Spline
    {
        #region Simple Spline

        /// <summary>
        /// Returns a new quadratic bezier spline.
        /// </summary>
        static public SimpleSpline Simple(Vector3 inStart, Vector3 inEnd, Vector3 inControl)
        {
            return new SimpleSpline(inStart, inEnd, inControl);
        }

        /// <summary>
        /// Returns a new quadtratic bezier spline,
        /// setting the control point as an offset from a point along the line.
        /// </summary>
        static public SimpleSpline Simple(Vector3 inStart, Vector3 inEnd, float inControlPercent, Vector3 inControlOffset)
        {
            return new SimpleSpline(inStart, inEnd, inStart + ((inEnd - inStart) * inControlPercent) + inControlOffset);
        }

        /// <summary>
        /// Returns a new quadratic bezier spline.
        /// </summary>
        static public SimpleSpline Simple(Vector2 inStart, Vector2 inEnd, Vector2 inControl)
        {
            return new SimpleSpline(inStart, inEnd, inControl);
        }

        /// <summary>
        /// Returns a new quadratic bezier spline.
        /// </summary>
        static public SimpleSpline Simple(Vector2 inStart, Vector2 inEnd, float inControlPercent, Vector2 inControlOffset)
        {
            return new SimpleSpline(inStart, inEnd, inStart + ((inEnd - inStart) * inControlPercent) + inControlOffset);
        }

        #endregion // Simple

        #region Vertex Spline

        /// <summary>
        /// Creates a new vertex spline.
        /// </summary>
        static public LinearSpline Linear(bool inbLooped, params CSplineVertex[] inVertices)
        {
            LinearSpline spline = new LinearSpline(inVertices.Length);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
            return spline;
        }

        /// <summary>
        /// Creates a new vertex spline.
        /// </summary>
        static public LinearSpline Linear(bool inbLooped, List<CSplineVertex> inVertices)
        {
            LinearSpline spline = new LinearSpline(inVertices.Count);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
            return spline;
        }

        /// <summary>
        /// Creates a new vertex spline.
        /// </summary>
        static public LinearSpline Linear(bool inbLooped, params Vector3[] inVertices)
        {
            LinearSpline spline = new LinearSpline(inVertices.Length);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
            return spline;
        }

        /// <summary>
        /// Creates a new vertex spline.
        /// </summary>
        static public LinearSpline Linear(bool inbLooped, List<Vector3> inVertices)
        {
            LinearSpline spline = new LinearSpline(inVertices.Count);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
            return spline;
        }

        /// <summary>
        /// Creates a new vertex spline.
        /// </summary>
        static public LinearSpline Linear(bool inbLooped, params Vector2[] inVertices)
        {
            LinearSpline spline = new LinearSpline(inVertices.Length);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
            return spline;
        }

        /// <summary>
        /// Creates a new vertex spline.
        /// </summary>
        static public LinearSpline Linear(bool inbLooped, List<Vector2> inVertices)
        {
            LinearSpline spline = new LinearSpline(inVertices.Count);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
            return spline;
        }

        /// <summary>
        /// Creates a new vertex spline from the given transforms.
        /// </summary>
        static public LinearSpline Linear(bool inbLooped, Space inSpace, params Transform[] inVertices)
        {
            LinearSpline spline = new LinearSpline(inVertices.Length);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices, inSpace);
            return spline;
        }

        /// <summary>
        /// Creates a new vertex spline from the given transforms.
        /// </summary>
        static public LinearSpline Linear(bool inbLooped, Space inSpace, List<Transform> inVertices)
        {
            LinearSpline spline = new LinearSpline(inVertices.Count);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices, inSpace);
            return spline;
        }

        #endregion // Vertex

        #region CSpline

        /// <summary>
        /// Creates a new CSpline.
        /// </summary>
        static public CSpline CSpline(bool inbLooped, params CSplineVertex[] inVertices)
        {
            CSpline spline = new CSpline(inVertices.Length);
            spline.SetAsCSpline();
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline.
        /// </summary>
        static public CSpline CSpline(bool inbLooped, List<CSplineVertex> inVertices)
        {
            CSpline spline = new CSpline(inVertices.Count);
            spline.SetAsCSpline();
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
            return spline;
        }

        #endregion // CSpline

        #region Cardinal

        /// <summary>
        /// Creates a new CSpline using the Catmull-Rom algorithm.
        /// </summary>
        static public CSpline CatmullRom(bool inbLooped, params Vector3[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetAsCatmullRom();
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Catmull-Rom algorithm.
        /// </summary>
        static public CSpline CatmullRom(bool inbLooped, List<Vector3> inPoints)
        {
            CSpline spline = new CSpline(inPoints.Count);
            spline.SetAsCatmullRom();
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Cardinal algorithm.
        /// </summary>
        static public CSpline Cardinal(bool inbLooped, float inTension, params Vector3[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetAsCardinal(inTension);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Cardinal algorithm.
        /// </summary>
        static public CSpline Cardinal(bool inbLooped, float inTension, List<Vector3> inPoints)
        {
            CSpline spline = new CSpline(inPoints.Count);
            spline.SetAsCardinal(inTension);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Catmull-Rom algorithm.
        /// </summary>
        static public CSpline CatmullRom(bool inbLooped, params Vector2[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetAsCatmullRom();
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Catmull-Rom algorithm.
        /// </summary>
        static public CSpline CatmullRom(bool inbLooped, List<Vector2> inPoints)
        {
            CSpline spline = new CSpline(inPoints.Count);
            spline.SetAsCatmullRom();
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Cardinal algorithm.
        /// </summary>
        static public CSpline Cardinal(bool inbLooped, float inTension, params Vector2[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetAsCardinal(inTension);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Cardinal algorithm.
        /// </summary>
        static public CSpline Cardinal(bool inbLooped, float inTension, List<Vector2> inPoints)
        {
            CSpline spline = new CSpline(inPoints.Count);
            spline.SetAsCardinal(inTension);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline from the given transforms using the Catmull-Rom algorithm.
        /// </summary>
        static public CSpline CatmullRom(bool inbLooped, Space inSpace, params Transform[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetAsCatmullRom();
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints, inSpace);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline from the given transforms using the Catmull-Rom algorithm.
        /// </summary>
        static public CSpline CatmullRom(bool inbLooped, Space inSpace, List<Transform> inPoints)
        {
            CSpline spline = new CSpline(inPoints.Count);
            spline.SetAsCatmullRom();
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints, inSpace);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline from the given transforms using the Cardinal algorithm.
        /// </summary>
        static public CSpline Cardinal(bool inbLooped, float inTension, Space inSpace, params Transform[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetAsCardinal(inTension);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints, inSpace);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline from the given transforms using the Cardinal algorithm.
        /// </summary>
        static public CSpline Cardinal(bool inbLooped, float inTension, Space inSpace, List<Transform> inPoints)
        {
            CSpline spline = new CSpline(inPoints.Count);
            spline.SetAsCardinal(inTension);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inPoints, inSpace);
            if (!inbLooped)
                spline.ResetControlPoints();
            return spline;
        }

        #endregion // Cardinal

        #region Extension Methods

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector3[] outPoints, float inStart, float inEnd, int inStartIdx, int inNumSamples, SplineLerp inLerp = SplineLerp.Vertex)
        {
            inNumSamples = Mathf.Min(outPoints.Length - inStartIdx, inNumSamples);

            float delta = inEnd - inStart;
            for (int i = 0; i < inNumSamples; ++i)
            {
                float t = (float) i / (inNumSamples - 1);
                outPoints[inStartIdx + i] = inSpline.GetPoint(inSpline.TransformPercent(inStart + t * delta, inLerp));
            }

            return inNumSamples;
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector2[] outPoints, float inStart, float inEnd, int inStartIdx, int inNumSamples, SplineLerp inLerp = SplineLerp.Vertex)
        {
            inNumSamples = Mathf.Min(outPoints.Length - inStartIdx, inNumSamples);

            float delta = inEnd - inStart;
            for (int i = 0; i < inNumSamples; ++i)
            {
                float t = (float) i / (inNumSamples - 1);
                outPoints[inStartIdx + i] = inSpline.GetPoint(inSpline.TransformPercent(inStart + t * delta, inLerp));
            }

            return inNumSamples;
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to a list.
        /// </summary>
        static public void Sample(this ISpline inSpline, List<Vector3> outPoints, float inStart, float inEnd, int inNumSamples, SplineLerp inLerp = SplineLerp.Vertex)
        {
            float delta = inEnd - inStart;
            for (int i = 0; i < inNumSamples; ++i)
            {
                float t = (float) i / (inNumSamples - 1);
                outPoints.Add(inSpline.GetPoint(inSpline.TransformPercent(inStart + t * delta, inLerp)));
            }
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to a list.
        /// </summary>
        static public void Sample(this ISpline inSpline, List<Vector2> outPoints, float inStart, float inEnd, int inNumSamples, SplineLerp inLerp = SplineLerp.Vertex)
        {
            float delta = inEnd - inStart;
            for (int i = 0; i < inNumSamples; ++i)
            {
                float t = (float) i / (inNumSamples - 1);
                outPoints.Add(inSpline.GetPoint(inSpline.TransformPercent(inStart + t * delta, inLerp)));
            }
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector3[] outPoints, float inStart, float inEnd, int inStartIdx, SplineLerp inLerp = SplineLerp.Vertex)
        {
            return Sample(inSpline, outPoints, inStart, inEnd, inStartIdx, outPoints.Length - inStartIdx, inLerp);
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector2[] outPoints, float inStart, float inEnd, int inStartIdx, SplineLerp inLerp = SplineLerp.Vertex)
        {
            return Sample(inSpline, outPoints, inStart, inEnd, inStartIdx, outPoints.Length - inStartIdx, inLerp);
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector3[] outPoints, float inStart, float inEnd, SplineLerp inLerp = SplineLerp.Vertex)
        {
            return Sample(inSpline, outPoints, inStart, inEnd, 0, outPoints.Length, inLerp);
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector2[] outPoints, float inStart, float inEnd, SplineLerp inLerp = SplineLerp.Vertex)
        {
            return Sample(inSpline, outPoints, inStart, inEnd, 0, outPoints.Length, inLerp);
        }

        /// <summary>
        /// Returns the userdata for the given vertex, casted as a Transform.
        /// </summary>
        static public Transform GetVertexTransform(this ISpline inSpline, int inIndex)
        {
            return inSpline.GetVertexUserData(inIndex) as Transform;
        }

        /// <summary>
        /// Returns info about a segment on the spline.
        /// </summary>
        static public SplineSegment GetSegment(this ISpline inSpline, float inPercent)
        {
            SplineSegment seg;
            inSpline.GetSegment(inPercent, out seg);
            return seg;
        }

        /// <summary>
        /// Generates info about an interpolation along the given spline.
        /// </summary>
        static public void GetUpdateInfo(this ISpline inSpline, float inPercent, SplineTweenSettings inTweenSettings, out SplineUpdateInfo outInfo)
        {
            GetUpdateInfo(inSpline, inPercent, inTweenSettings.LerpMethod, inTweenSettings.SegmentEase, out outInfo);
        }

        /// <summary>
        /// Generates info about an interpolation along the given spline.
        /// </summary>
        static public void GetUpdateInfo(this ISpline inSpline, float inPercent, SplineLerp inLerp, out SplineUpdateInfo outInfo)
        {
            GetUpdateInfo(inSpline, inPercent, inLerp, Curve.Linear, out outInfo);
        }

        /// <summary>
        /// Generates info about an interpolation along the given spline.
        /// </summary>
        static public void GetUpdateInfo(this ISpline inSpline, float inPercent, out SplineUpdateInfo outInfo)
        {
            GetUpdateInfo(inSpline, inPercent, SplineLerp.Vertex, Curve.Linear, out outInfo);
        }

        /// <summary>
        /// Generates info about an interpolation along the given spline.
        /// </summary>
        static public void GetUpdateInfo(this ISpline inSpline, float inPercent, SplineLerp inLerpMethod, Curve inSegmentEase, out SplineUpdateInfo outInfo)
        {
            outInfo.Spline = inSpline;
            outInfo.Percent = inSpline.TransformPercent(inPercent, inLerpMethod);
            outInfo.Point = inSpline.GetPoint(outInfo.Percent, inSegmentEase);
            outInfo.Direction = inSpline.GetDirection(outInfo.Percent, inSegmentEase);
        }

        #endregion // Extension Methods

        #region Alignment

        /// <summary>
        /// Aligns a Transform to a point along the spline, using either localPosition or position.
        /// </summary>
        static public void Align(ISpline inSpline, Transform inTransform, float inPercent, Axis inAxis, Space inSpace, SplineLerp inLerpMethod, Curve inSegmentEase, SplineOrientationSettings inOrientation)
        {
            SplineUpdateInfo info;
            GetUpdateInfo(inSpline, inPercent, inLerpMethod, inSegmentEase, out info);
            inTransform.SetPosition(info.Point, inAxis, inSpace);
            if (inOrientation != null)
            {
                inOrientation.Apply(ref info, inTransform, inSpace);
            }
        }

        /// <summary>
        /// Aligns a RectTransform to a point along the spline, using anchoredPosition.
        /// </summary>
        static public void AlignAnchorPos(ISpline inSpline, RectTransform inTransform, float inPercent, Axis inAxis, SplineLerp inLerpMethod, Curve inSegmentEase, SplineOrientationSettings inOrientation)
        {
            SplineUpdateInfo info;
            GetUpdateInfo(inSpline, inPercent, inLerpMethod, inSegmentEase, out info);
            inTransform.SetAnchorPos(info.Point, inAxis);
            if (inOrientation != null)
            {
                inOrientation.Apply(ref info, inTransform, Space.Self);
            }
        }

        #endregion // Alignment
    }
}
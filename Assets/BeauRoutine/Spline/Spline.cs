/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 May 2018
 * 
 * File:    Spline.cs
 * Purpose: Defines a common interface for splines, along with factory methods.
*/

using System;
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
        static public LinearSpline Linear(bool inbLooped, params Vector2[] inVertices)
        {
            LinearSpline spline = new LinearSpline(inVertices.Length);
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
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
            spline.SetLooped(inbLooped);
            spline.SetVertices(inVertices);
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Catmull-Rom algorithm.
        /// </summary>
        static public CSpline CatmullRom(bool inbLooped, params Vector3[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetLooped(inbLooped);
            spline.SetCatmullRom(inPoints);
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Cardinal algorithm.
        /// </summary>
        static public CSpline Cardinal(bool inbLooped, float inTension, params Vector3[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetLooped(inbLooped);
            spline.SetCardinal(inTension, inPoints);
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Catmull-Rom algorithm.
        /// </summary>
        static public CSpline CatmullRom(bool inbLooped, params Vector2[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetLooped(inbLooped);
            spline.SetCatmullRom(inPoints);
            return spline;
        }

        /// <summary>
        /// Creates a new CSpline using the Cardinal algorithm.
        /// </summary>
        static public CSpline Cardinal(bool inbLooped, float inTension, params Vector2[] inPoints)
        {
            CSpline spline = new CSpline(inPoints.Length);
            spline.SetLooped(inbLooped);
            spline.SetCardinal(inTension, inPoints);
            return spline;
        }

        #endregion // CSpline

        #region Extension Methods

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector3[] outPoints, float inStart, float inEnd, int inStartIdx, int inNumSamples)
        {
            inNumSamples = Mathf.Min(outPoints.Length - inStartIdx, inNumSamples);

            float delta = inEnd - inStart;
            for (int i = 0; i < inNumSamples; ++i)
            {
                float t = (float)i / (inNumSamples - 1);
                outPoints[inStartIdx + i] = inSpline.GetPoint(inStart + t * delta);
            }

            return inNumSamples;
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector2[] outPoints, float inStart, float inEnd, int inStartIdx, int inNumSamples)
        {
            inNumSamples = Mathf.Min(outPoints.Length - inStartIdx, inNumSamples);

            float delta = inEnd - inStart;
            for (int i = 0; i < inNumSamples; ++i)
            {
                float t = (float)i / (inNumSamples - 1);
                outPoints[inStartIdx + i] = inSpline.GetPoint(inStart + t * delta);
            }

            return inNumSamples;
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to a list.
        /// </summary>
        static public void Sample(this ISpline inSpline, List<Vector3> outPoints, float inStart, float inEnd, int inNumSamples)
        {
            float delta = inEnd - inStart;
            for (int i = 0; i < inNumSamples; ++i)
            {
                float t = (float)i / (inNumSamples - 1);
                outPoints.Add(inSpline.GetPoint(inStart + t * delta));
            }
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to a list.
        /// </summary>
        static public void Sample(this ISpline inSpline, List<Vector2> outPoints, float inStart, float inEnd, int inNumSamples)
        {
            float delta = inEnd - inStart;
            for (int i = 0; i < inNumSamples; ++i)
            {
                float t = (float)i / (inNumSamples - 1);
                outPoints.Add(inSpline.GetPoint(inStart + t * delta));
            }
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector3[] outPoints, float inStart, float inEnd, int inStartIdx)
        {
            return inSpline.Sample(outPoints, inStart, inEnd, inStartIdx, outPoints.Length - inStartIdx);
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector2[] outPoints, float inStart, float inEnd, int inStartIdx)
        {
            return inSpline.Sample(outPoints, inStart, inEnd, inStartIdx, outPoints.Length - inStartIdx);
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector3[] outPoints, float inStart, float inEnd)
        {
            return inSpline.Sample(outPoints, inStart, inEnd, 0, outPoints.Length);
        }

        /// <summary>
        /// Samples the spline for the given range and outputs to an array.
        /// </summary>
        static public int Sample(this ISpline inSpline, Vector2[] outPoints, float inStart, float inEnd)
        {
            return inSpline.Sample(outPoints, inStart, inEnd, 0, outPoints.Length);
        }

        #endregion // Extension Methods
    }
}
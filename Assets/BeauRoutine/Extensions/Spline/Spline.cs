/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 May 2018
 * 
 * File:    Spline.cs
 * Purpose: Defines a common interface for splines, along with factory methods.
*/

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    static public class Spline
    {
        // /// <summary>
        // /// Returns a new quadratic bezier spline.
        // /// </summary>
        // static public SimpleSpline Simple(Vector3 inStart, Vector3 inControl, Vector3 inEnd)
        // {
        //     return new SimpleSpline(inStart, inControl, inEnd);
        // }

        // /// <summary>
        // /// Returns a new quadratic bezier spline.
        // /// </summary>
        // static public SimpleSpline Simple(Vector2 inStart, Vector2 inControl, Vector2 inEnd)
        // {
        //     return new SimpleSpline(inStart, inControl, inEnd);
        // }

        // /// <summary>
        // /// Returns a new closed cubic hermite spline.
        // /// </summary>
        // static public CSpline CSpline(params CSplinePoint[] inPoints)
        // {
        //     return new CSpline(inPoints, true);
        // }

        // /// <summary>
        // /// Returns a new closed cubic hermite spline.
        // /// </summary>
        // static public CSpline CSplineClosed(params CSplinePoint[] inPoints)
        // {
        //     return new CSpline(inPoints, true);
        // }

        #region Catmull-Rom

        // Uncomment once methods are introduced to convert from CatmullRom to CSpline
        // /// <summary>
        // /// Returns a new unclosed catmull-rom spline.
        // /// </summary>
        // static public CatmullRomSpline CatmullRom(params Vector3[] inPoints)
        // {
        //     return new CatmullRomSpline(inPoints, false);
        // }

        // /// <summary>
        // /// Returns a new unclosed catmull-rom spline.
        // /// </summary>
        // static public CatmullRomSpline CatmullRom(params Vector2[] inPoints)
        // {
        //     return new CatmullRomSpline(inPoints, false);
        // }

        // /// <summary>
        // /// Returns a new closed catmull-rom spline.
        // /// </summary>
        // static public CatmullRomSpline CatmullRomClosed(params Vector3[] inPoints)
        // {
        //     return new CatmullRomSpline(inPoints, true);
        // }

        // /// <summary>
        // /// Returns a new closed catmull-rom spline.
        // /// </summary>
        // static public CatmullRomSpline CatmullRomClosed(params Vector2[] inPoints)
        // {
        //     return new CatmullRomSpline(inPoints, true);
        // }

        #endregion // Catmull-Rom

        #region Extension Methods

        // static public void Sample(this ISpline inSpline, Vector3[] outPoints, float inStart, float inEnd)
        // {
        //     inSpline.Sample(outPoints, inStart, inEnd, 0, outPoints.Length);
        // }

        #endregion // Extension Methods
    }
}
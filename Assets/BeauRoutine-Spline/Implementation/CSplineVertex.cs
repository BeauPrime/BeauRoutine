/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    11 May 2018
 * 
 * File:    CSplineVertex.cs
 * Purpose: Vertex for a CSpline.
*/

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Spline vertex, with position and tangents.
    /// </summary>
    [Serializable]
    public struct CSplineVertex
    {
        public Vector3 Point;
        public Vector3 InTangent;
        public Vector3 OutTangent;

        public CSplineVertex(Vector3 inPoint)
        {
            Point = inPoint;
            InTangent = OutTangent = s_Zero;
        }

        public CSplineVertex(Vector2 inPoint)
        {
            Point = inPoint;
            InTangent = OutTangent = s_Zero;
        }

        public CSplineVertex(Vector3 inPoint, Vector3 inTangent)
        {
            Point = inPoint;
            InTangent = OutTangent = inTangent;
        }

        public CSplineVertex(Vector2 inPoint, Vector2 inTangent)
        {
            Point = inPoint;
            InTangent = OutTangent = inTangent;
        }

        public CSplineVertex(Vector3 inPoint, Vector3 inTangentIn, Vector3 inTangentOut)
        {
            Point = inPoint;
            InTangent = inTangentIn;
            OutTangent = inTangentOut;
        }

        public CSplineVertex(Vector2 inPoint, Vector2 inTangentIn, Vector2 inTangentOut)
        {
            Point = inPoint;
            InTangent = inTangentIn;
            OutTangent = inTangentOut;
        }

        static private readonly Vector3 s_Zero = new Vector3(0, 0, 0);
    }
}
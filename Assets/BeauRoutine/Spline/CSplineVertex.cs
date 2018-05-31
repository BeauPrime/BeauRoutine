/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
    }
}
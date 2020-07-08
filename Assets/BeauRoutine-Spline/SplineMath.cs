/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 May 2018
 * 
 * File:    SplineMath.cs
 * Purpose: Spline interpolation algorithms and utilities.
*/

using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Contains spline algorithms.
    /// </summary>
    static public class SplineMath
    {
        /// <summary>
        /// Number of increments per segment for distance calculations.
        /// </summary>
        static public int DistancePrecision = 16;

        /// <summary>
        /// Look-ahead percent to use for direction calculations.
        /// </summary>
        static public float LookAhead = 0.001f;

        /// <summary>
        /// Quadratic bezier spline calculation.
        /// </summary>
        static public Vector3 Quadratic(Vector3 inP0, Vector3 inC0, Vector3 inP1, float inT)
        {
            float tReverse = 1 - inT;
            float a = tReverse * tReverse;
            float b = tReverse * inT * 2;
            float c = inT * inT;

            return new Vector3(
                (inP0.x * a) + (inC0.x * b) + (inP1.x * c),
                (inP0.y * a) + (inC0.y * b) + (inP1.y * c),
                (inP0.z * a) + (inC0.z * b) + (inP1.z * c)
            );
        }

        /// <summary>
        /// First derivative of a quadratic bezier spline calculation.
        /// </summary>
        static public Vector3 QuadraticDerivative(Vector3 inP0, Vector3 inC0, Vector3 inP1, float inT)
        {
            float tReverse = 1 - inT;
            float a = 2f * tReverse;
            float b = 2f * inT;

            return new Vector3(
                a * (inC0.x - inP0.x) + b * (inP1.x - inC0.x),
                a * (inC0.y - inP0.y) + b * (inP1.y - inC0.y),
                a * (inC0.z - inP0.z) + b * (inP1.z - inC0.z)
            );
        }

        /// <summary>
        /// Cubic hermite spline calculation.
        /// </summary>
        static public Vector3 Hermite(Vector3 inT0, Vector3 inP0, Vector3 inP1, Vector3 inT1, float inT)
        {
            float t2 = inT * inT;
            float t3 = t2 * inT;

            float a = 2 * t3 - 3 * t2 + 1;
            float b = t3 - 2 * t2 + inT;
            float c = t3 - t2;
            float d = -2 * t3 + 3 * t2;

            return new Vector3(
                inP0.x * a + inT0.x * b + inT1.x * c + inP1.x * d,
                inP0.y * a + inT0.y * b + inT1.y * c + inP1.y * d,
                inP0.z * a + inT0.z * b + inT1.z * c + inP1.z * d
            );
        }
    }
}
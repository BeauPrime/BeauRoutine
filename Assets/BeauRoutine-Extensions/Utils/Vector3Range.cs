/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Nov 2018
 * 
 * File:    Vector3Range.cs
 * Purpose: Simple serialized range for a Vector3 value.
 *          Composed of a base value plus min and max modifiers
 */

using System;
using UnityEngine;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Vector3 range.
    /// Starts with Base, adds random value between ModifierMin and ModifierMax.
    /// </summary>
    [Serializable]
    public struct Vector3Range
    {
        public Vector3 Base;
        public Vector3 ModifierMin;
        public Vector3 ModifierMax;

        public Vector3Range(Vector3 inBase, Vector3 inModMin = default(Vector3), Vector3 inModMax =  default(Vector3))
        {
            Base = inBase;
            ModifierMin = inModMin;
            ModifierMax = inModMax;
        }

        public Vector3 Generate()
        {
            Vector3 result = Base;
            result.x += UnityEngine.Random.Range(ModifierMin.x, ModifierMax.x);
            result.y += UnityEngine.Random.Range(ModifierMin.y, ModifierMax.y);
            result.z += UnityEngine.Random.Range(ModifierMin.z, ModifierMax.z);
            return result;
        }

        public Vector3 Generate(System.Random inRandom)
        {
            Vector3 result = Base;
            result.x += NextFloat(inRandom, ModifierMin.x, ModifierMax.x);
            result.y += NextFloat(inRandom, ModifierMin.y, ModifierMax.y);
            result.z += NextFloat(inRandom, ModifierMin.z, ModifierMax.z);
            return result;
        }

        static private float NextFloat(System.Random inRandom, float inMin, float inMax)
        {
            return (float) inRandom.NextDouble() * (inMax - inMin) + inMin;
        }
    }
}
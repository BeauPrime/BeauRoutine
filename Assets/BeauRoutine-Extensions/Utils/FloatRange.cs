/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 Jan 2019
 * 
 * File:    FloatRange.cs
 * Purpose: Simple serialized range for a float value.
 *          Composed of a base value plus min and max modifiers
 */

using System;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Float range.
    /// Starts with Base, adds random value between ModifierMin and ModifierMax.
    /// </summary>
    [Serializable]
    public struct FloatRange
    {
        public float Base;
        public float ModifierMin;
        public float ModifierMax;

        public FloatRange(float inBase, float inModMin = 0, float inModMax = 0)
        {
            Base = inBase;
            ModifierMin = inModMin;
            ModifierMax = inModMax;
        }

        public float Generate()
        {
            return Base + UnityEngine.Random.Range(ModifierMin, ModifierMax);
        }

        public float Generate(System.Random inRandom)
        {
            return Base + NextFloat(inRandom, ModifierMin, ModifierMax);
        }

        static private float NextFloat(System.Random inRandom, float inMin, float inMax)
        {
            return (float) inRandom.NextDouble() * (inMax - inMin) + inMin;
        }
    }
}
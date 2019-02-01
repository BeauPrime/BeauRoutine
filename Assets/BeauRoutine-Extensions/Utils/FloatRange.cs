/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 Jan 2019
 * 
 * File:    FloatRange.cs
 * Purpose: Simple serialized range for a float value.
 *          Composed of a base value plus min and max modifiers
 */

using System;
using UnityEngine;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Float range.
    /// Starts with Base, adds random value between ModifierMin and ModifierMax.
    /// </summary>
    [Serializable]
    public class FloatRange
    {
        public float Base;
        public float ModifierMin;
        public float ModifierMax;

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
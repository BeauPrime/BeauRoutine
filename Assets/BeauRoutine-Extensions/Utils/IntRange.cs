/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    14 March 2019
 * 
 * File:    IntRange.cs
 * Purpose: Simple serialized range for an int value.
 *          Composed of a base value plus min and max modifiers
 */

using System;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Int range.
    /// Starts with Base, adds random value between ModifierMin and ModifierMax.
    /// </summary>
    [Serializable]
    public struct IntRange
    {
        public int Base;
        public int ModifierMin;
        public int ModifierMax;

        public IntRange(int inBase, int inModMin = 0, int inModMax = 0)
        {
            Base = inBase;
            ModifierMin = inModMin;
            ModifierMax = inModMax;
        }

        public int Generate()
        {
            return Base + UnityEngine.Random.Range(ModifierMin, ModifierMax + 1);
        }

        public int Generate(System.Random inRandom)
        {
            return Base + NextInt(inRandom, ModifierMin, ModifierMax);
        }

        static private int NextInt(System.Random inRandom, int inMin, int inMax)
        {
            return inRandom.Next(inMin, inMax + 1);
        }
    }
}
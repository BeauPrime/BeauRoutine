/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    19 May 2018
 * 
 * File:    SplineTweenSettings.cs
 * Purpose: Settings for a spline tween.
*/

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Settings for a Spline tween.
    /// </summary>
    [Serializable]
    public struct SplineTweenSettings
    {
        public SplineOffset Offset;
        public SplineLerp LerpMethod;
        public Curve SegmentEase;

        static private SplineTweenSettings s_Default = new SplineTweenSettings()
        {
            Offset = SplineOffset.Absolute,
            LerpMethod = SplineLerp.Vertex,
            SegmentEase = Curve.Linear
        };

        /// <summary>
        /// Gets/sets the default spline tween settings.
        /// </summary>
        static public SplineTweenSettings Default
        {
            get { return s_Default; }
            set { s_Default = value; }
        }
    }
}
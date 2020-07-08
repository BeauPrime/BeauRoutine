/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
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
    public class SplineTweenSettings
    {
        #region Interpolation

        /// <summary>
        /// Interpolation space for a Spline tween.
        /// </summary>
        public SplineLerp LerpMethod = SplineLerp.Vertex;

        /// <summary>
        /// Easing function to apply per-segment.
        /// </summary>
        public Curve SegmentEase = Curve.Linear;

        /// <summary>
        /// Callback for detailed info on progress through a spline tween.
        /// </summary>
        public SplineUpdateDelegate UpdateCallback = null;

        #endregion // Interpolation

        #region Position

        [Header("Position")]

        /// <summary>
        /// How a Spline tween influences position.
        /// </summary>
        public SplineOffset Offset = SplineOffset.Absolute;

        #endregion // Position

        #region Orientation

        [Header("Orientation")]

        /// <summary>
        /// How a Spline tween influences orientation.
        /// </summary>
        public SplineOrientationSettings Orient = new SplineOrientationSettings();

        #endregion // Orientation

        public SplineTweenSettings() { }

        public SplineTweenSettings(SplineTweenSettings inSource)
        {
            LerpMethod = inSource.LerpMethod;
            SegmentEase = inSource.SegmentEase;
            UpdateCallback = inSource.UpdateCallback;

            Offset = inSource.Offset;

            Orient = new SplineOrientationSettings(inSource.Orient);
        }

        #region Building

        public SplineTweenSettings WithOrientation(SplineOrientationMode inOrientation, Axis inAxis = Axis.XYZ)
        {
            Orient.Mode = inOrientation;
            Orient.DirectionMask = inAxis;
            Orient.UpVector = Vector3.up;

            return this;
        }

        public SplineTweenSettings WithOrientation(SplineOrientationMode inOrientation, Axis inAxis, Vector3 inUp)
        {
            Orient.Mode = inOrientation;
            Orient.DirectionMask = inAxis;
            Orient.UpVector = inUp;

            return this;
        }

        #endregion // Building

        static private SplineTweenSettings s_Default = new SplineTweenSettings();

        /// <summary>
        /// Gets/sets the default Spline tween settings.
        /// </summary>
        static public SplineTweenSettings Default
        {
            get { return s_Default; }
            set { s_Default = value; }
        }
    }
}
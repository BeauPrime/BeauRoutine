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
        public SplineOrientation Orient = SplineOrientation.Ignore;

        /// <summary>
        /// Axis to use for rotation calculations.
        /// </summary>
        public Axis OrientAxis = Axis.XYZ;

        /// <summary>
        /// Up vector for rotations.
        /// </summary>
        public Vector3 OrientUp = Vector3.up;

        /// <summary>
        /// Callback for modifying orientation.
        /// </summary>
        public SplineOrientationCallback OrientCallback = null;

        #endregion // Orientation

        public SplineTweenSettings() { }

        public SplineTweenSettings(SplineTweenSettings inSource)
        {
            LerpMethod = inSource.LerpMethod;
            SegmentEase = inSource.SegmentEase;

            Offset = inSource.Offset;

            Orient = inSource.Orient;
            OrientAxis = inSource.OrientAxis;
            OrientUp = inSource.OrientUp;
            OrientCallback = inSource.OrientCallback;
        }

        #region Building

        public SplineTweenSettings WithOrientation(SplineOrientation inOrientation, Axis inAxis = Axis.XYZ)
        {
            Orient = inOrientation;
            OrientAxis = inAxis;
            OrientUp = Vector3.up;

            return this;
        }

        public SplineTweenSettings WithOrientation(SplineOrientation inOrientation, Axis inAxis, Vector3 inUp)
        {
            Orient = inOrientation;
            OrientAxis = inAxis;
            OrientUp = inUp;

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
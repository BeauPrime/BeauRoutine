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
    /// Settings for orienting objects to a spline.
    /// </summary>
    [Serializable]
    public class SplineOrientationSettings
    {
        /// <summary>
        /// How a Spline tween influences orientation.
        /// </summary>
        public SplineOrientationMode Mode = SplineOrientationMode.Ignore;

        /// <summary>
        /// Axis to use for rotation calculations.
        /// </summary>
        public Axis DirectionMask = Axis.XYZ;

        /// <summary>
        /// Up vector for rotations.
        /// </summary>
        public Vector3 UpVector = Vector3.up;

        /// <summary>
        /// Callback for modifying orientation.
        /// </summary>
        public SplineOrientationCallback CustomCallback = null;

        public SplineOrientationSettings() { }

        public SplineOrientationSettings(SplineOrientationSettings inSource)
        {
            Mode = inSource.Mode;
            DirectionMask = inSource.DirectionMask;
            UpVector = inSource.UpVector;
            CustomCallback = inSource.CustomCallback;
        }

        public void Apply(ref SplineUpdateInfo inInfo, Transform inTransform, Space inSpace)
        {
            if (Mode == SplineOrientationMode.Ignore)
                return;

            Axis maskedAxis = DirectionMask & Axis.XYZ;
            if (maskedAxis == 0)
                return;

            Vector3 direction = inInfo.Direction;
            if (maskedAxis != Axis.XYZ)
            {
                if ((DirectionMask & Axis.X) == 0)
                    direction.x = 0;
                if ((DirectionMask & Axis.Y) == 0)
                    direction.y = 0;
                if ((DirectionMask & Axis.Z) == 0)
                    direction.z = 0;
            }

            if (Mode == SplineOrientationMode.ThreeD)
            {
                Quaternion dirRot = Quaternion.LookRotation(direction, UpVector);
                if (inSpace == Space.Self)
                    inTransform.localRotation = dirRot;
                else
                    inTransform.rotation = dirRot;
            }
            else if (Mode == SplineOrientationMode.TwoD)
            {
                float dir = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                if (dir < -180)
                    dir += 360f;

                inTransform.SetRotation(dir, Axis.Z, inSpace);
            }
            else if (CustomCallback != null)
            {
                CustomCallback(inTransform, direction, inSpace);
            }
        }
    }
}
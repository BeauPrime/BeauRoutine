/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 May 2018
 * 
 * File:    SplineOrientation.cs
 * Purpose: Enum describing how Spline orientations should be applied.
*/

using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// How a spline should affect transform orientation.
    /// </summary>
    public enum SplineOrientationMode : byte
    {
        /// <summary>
        /// Ignore rotation changes
        /// </summary>
        Ignore,

        /// <summary>
        /// Rotate to face path direction.
        /// <summary>
        ThreeD,

        /// <summary>
        /// Rotate to face path direction, only rotating on Z axis.
        /// </summary>
        TwoD,

        /// <summary>
        /// Executes a callback to rotate the transform.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Callback to execute to modify a transform's orientation during a Spline tween.
    /// </summary>
    public delegate void SplineOrientationCallback(Transform inTransform, Vector3 inDirection, Space inSpace);
}

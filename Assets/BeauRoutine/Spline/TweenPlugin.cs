/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    24 May 2018
 * 
 * File:    TweenPlugin.cs
 * Purpose: Adds generic Spline tweens to the Tween class.
*/

using System;
using UnityEngine;
using BeauRoutine.Splines;

namespace BeauRoutine
{
    /// <summary>
    /// Contains tweens related to splines.
    /// </summary>
    public partial class Tween
    {
        #region Value

        private sealed class TweenData_Value_Spline : ITweenData
        {
            private ISpline m_Spline;
            private Action<Vector3> m_Setter;
            private SplineTweenSettings m_SplineSettings;

            public TweenData_Value_Spline(ISpline inSpline, Action<Vector3> inSetter, SplineTweenSettings inSettings)
            {
                m_Spline = inSpline;
                m_Setter = inSetter;
                m_SplineSettings = inSettings;
            }

            public void OnTweenStart()
            {
                m_Spline.Process();
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Setter(m_Spline.GetPoint(m_Spline.TransformPercent(inPercent, m_SplineSettings.LerpMethod), m_SplineSettings.SegmentEase));
            }

            public override string ToString()
            {
                return "Value<Spline>";
            }
        }

        /// <summary>
        /// Tweens over the given spline over time.
        /// </summary>
        static public Tween Vector(ISpline inSpline, Action<Vector3> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Spline(inSpline, inSetter, SplineTweenSettings.Default), inTime);
        }

        /// <summary>
        /// Tweens over the given spline over time.
        /// </summary>
        static public Tween Vector(ISpline inSpline, Action<Vector3> inSetter, float inTime, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_Value_Spline(inSpline, inSetter, inSplineSettings), inTime);
        }

        /// <summary>
        /// Tweens over the given spline over time.
        /// </summary>
        static public Tween Vector(ISpline inSpline, Action<Vector3> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Spline(inSpline, inSetter, SplineTweenSettings.Default), inSettings);
        }

        /// <summary>
        /// Tweens over the given spline over time.
        /// </summary>
        static public Tween Vector(ISpline inSpline, Action<Vector3> inSetter, TweenSettings inSettings, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_Value_Spline(inSpline, inSetter, inSplineSettings), inSettings);
        }

        #endregion // Value
    }
}
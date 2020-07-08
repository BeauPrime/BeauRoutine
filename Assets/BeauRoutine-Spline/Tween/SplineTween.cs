/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 May 2018
 * 
 * File:    SplineTween.cs
 * Purpose: Implements generic spline tweens.
*/

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Contains tweens related to splines.
    /// </summary>
    public partial class SplineTween
    {
        #region Vector

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
                if (m_SplineSettings.UpdateCallback != null)
                {
                    SplineUpdateInfo info;
                    Splines.Spline.GetUpdateInfo(m_Spline, inPercent, m_SplineSettings, out info);
                    
                    m_Setter(info.Point);
                    m_SplineSettings.UpdateCallback(info);
                }
                else
                {
                    m_Setter(m_Spline.GetPoint(m_Spline.TransformPercent(inPercent, m_SplineSettings.LerpMethod), m_SplineSettings.SegmentEase));
                }
            }

            public override string ToString()
            {
                return "Value<Vector3> (Spline)";
            }
        }

        /// <summary>
        /// Tweens a vector over the given spline over time.
        /// </summary>
        static public Tween Vector(ISpline inSpline, Action<Vector3> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Spline(inSpline, inSetter, SplineTweenSettings.Default), inTime);
        }

        /// <summary>
        /// Tweens a vector over the given spline over time.
        /// </summary>
        static public Tween Vector(ISpline inSpline, Action<Vector3> inSetter, float inTime, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_Value_Spline(inSpline, inSetter, inSplineSettings), inTime);
        }

        /// <summary>
        /// Tweens a vector over the given spline over time.
        /// </summary>
        static public Tween Vector(ISpline inSpline, Action<Vector3> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Spline(inSpline, inSetter, SplineTweenSettings.Default), inSettings);
        }

        /// <summary>
        /// Tweens a vector over the given spline over time.
        /// </summary>
        static public Tween Vector(ISpline inSpline, Action<Vector3> inSetter, TweenSettings inSettings, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_Value_Spline(inSpline, inSetter, inSplineSettings), inSettings);
        }

        #endregion // Vector

        #region Info

        private sealed class TweenData_Value_SplineInfo : ITweenData
        {
            private ISpline m_Spline;
            private SplineUpdateDelegate m_Delegate;
            private SplineTweenSettings m_SplineSettings;

            public TweenData_Value_SplineInfo(ISpline inSpline, SplineUpdateDelegate inSetter, SplineTweenSettings inSettings)
            {
                m_Spline = inSpline;
                m_Delegate = inSetter;
                m_SplineSettings = inSettings;
            }

            public void OnTweenStart()
            {
                m_Spline.Process();
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                SplineUpdateInfo info;
                Splines.Spline.GetUpdateInfo(m_Spline, inPercent, m_SplineSettings, out info);
                m_Delegate(info);

                if (m_SplineSettings.UpdateCallback != null)
                    m_SplineSettings.UpdateCallback(info);
            }

            public override string ToString()
            {
                return "Value<Spline> (Info)";
            }
        }

        /// <summary>
        /// Tweens over the given spline over time.
        /// </summary>
        static public Tween Spline(ISpline inSpline, SplineUpdateDelegate inUpdate, float inTime)
        {
            return Tween.Create(new TweenData_Value_SplineInfo(inSpline, inUpdate, SplineTweenSettings.Default), inTime);
        }

        /// <summary>
        /// Tweens over the given spline over time.
        /// </summary>
        static public Tween Spline(ISpline inSpline, SplineUpdateDelegate inUpdate, float inTime, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_Value_SplineInfo(inSpline, inUpdate, inSplineSettings), inTime);
        }

        /// <summary>
        /// Tweens over the given spline over time.
        /// </summary>
        static public Tween Spline(ISpline inSpline, SplineUpdateDelegate inUpdate, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_SplineInfo(inSpline, inUpdate, SplineTweenSettings.Default), inSettings);
        }

        /// <summary>
        /// Tweens over the given spline over time.
        /// </summary>
        static public Tween Spline(ISpline inSpline, SplineUpdateDelegate inUpdate, TweenSettings inSettings, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_Value_SplineInfo(inSpline, inUpdate, inSplineSettings), inSettings);
        }

        #endregion // Detailed
    }
}
/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    24 May 2018
 * 
 * File:    SplineTween.cs
 * Purpose: Contains tweens related to splines.
*/

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Contains tweens related to splines.
    /// </summary>
    static public class SplineTweenShortcuts
    {
        #region Transform

        private sealed class TweenData_Transform_PositionSpline : ITweenData
        {
            private Transform m_Transform;
            private ISpline m_Spline;
            private Space m_Space;
            private Axis m_Axis;

            private SplineTweenSettings m_SplineSettings;

            private Vector3 m_Start;

            public TweenData_Transform_PositionSpline(Transform inTransform, ISpline inSpline, Space inSpace, Axis inAxis, SplineTweenSettings inSettings)
            {
                m_Transform = inTransform;
                m_Spline = inSpline;
                m_Space = inSpace;
                m_Axis = inAxis;
                m_SplineSettings = inSettings;
            }

            public void OnTweenStart()
            {
                m_Start = m_SplineSettings.Offset == SplineOffset.Relative ? (m_Space == Space.World ? m_Transform.position : m_Transform.localPosition) : Vector3.zero;
                m_Spline.Process();
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                inPercent = m_Spline.TransformPercent(inPercent, m_SplineSettings.LerpMethod);
                Vector3 final = m_Spline.GetPoint(inPercent, m_SplineSettings.SegmentEase);
                final.x += m_Start.x;
                final.y += m_Start.y;
                final.z += m_Start.z;
                m_Transform.SetPosition(final, m_Axis, m_Space);

                if (m_SplineSettings.Orient != SplineOrientation.Ignore)
                {
                    Axis maskedAxis = m_SplineSettings.OrientAxis & Axis.XYZ;
                    if (maskedAxis != 0)
                    {
                        Vector3 direction = m_Spline.GetDirection(inPercent, m_SplineSettings.SegmentEase);
                        if (maskedAxis != Axis.XYZ)
                        {
                            if ((m_SplineSettings.OrientAxis & Axis.X) == 0)
                                direction.x = 0;
                            if ((m_SplineSettings.OrientAxis & Axis.Y) == 0)
                                direction.y = 0;
                            if ((m_SplineSettings.OrientAxis & Axis.Z) == 0)
                                direction.z = 0;
                        }

                        if (m_SplineSettings.Orient == SplineOrientation.ThreeD)
                        {
                            Quaternion dirRot = Quaternion.LookRotation(direction, m_SplineSettings.OrientUp);
                            if (m_Space == Space.Self)
                                m_Transform.localRotation = dirRot;
                            else
                                m_Transform.rotation = dirRot;
                        }
                        else if (m_SplineSettings.Orient == SplineOrientation.TwoD)
                        {
                            float dir = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                            if (dir < -180)
                                dir += 360f;

                            m_Transform.SetRotation(dir, Axis.Z, m_Space);
                        }

                        if (m_SplineSettings.OrientCallback != null)
                        {
                            m_SplineSettings.OrientCallback(m_Transform, direction, m_Space);
                        }
                    }
                }
            }

            public override string ToString()
            {
                return "Transform: Position (Spline)";
            }
        }

        /// <summary>
        /// Moves the Transform along a spline over time.
        /// </summary>
        static public Tween MoveAlong(this Transform inTransform, ISpline inSpline, float inTime, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, SplineTweenSettings.Default), inTime);
        }

        /// <summary>
        /// Moves the Transform along a spline over time.
        /// </summary>
        static public Tween MoveAlong(this Transform inTransform, ISpline inSpline, float inTime, Axis inAxis, Space inSpace, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, inSplineSettings), inTime);
        }

        /// <summary>
        /// Moves the Transform along a spline over time.
        /// </summary>
        static public Tween MoveAlong(this Transform inTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, SplineTweenSettings.Default), inSettings);
        }

        /// <summary>
        /// Moves the Transform along a spline over time.
        /// </summary>
        static public Tween MoveAlong(this Transform inTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis, Space inSpace, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, inSplineSettings), inSettings);
        }

        /// <summary>
        /// Moves the Transform along a spline with the given average speed.
        /// </summary>
        static public Tween MoveAlongWithSpeed(this Transform inTransform, ISpline inSpline, float inSpeed, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return MoveAlongWithSpeed(inTransform, inSpline, inSpeed, inAxis, inSpace, SplineTweenSettings.Default);
        }

        /// <summary>
        /// Moves the Transform along a spline with the given average speed.
        /// </summary>
        static public Tween MoveAlongWithSpeed(this Transform inTransform, ISpline inSpline, float inSpeed, Axis inAxis, Space inSpace, SplineTweenSettings inSplineSettings)
        {
            float time;
            switch(inSplineSettings.LerpMethod)
            {
                case SplineLerp.Direct:
                case SplineLerp.Vertex:
                    time = inSpline.GetDirectDistance() / inSpeed;
                    break;

                case SplineLerp.Precise:
                default:
                    time = inSpline.GetDistance() / inSpeed;
                    break;
            }

            return Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, inSplineSettings), time);
        }

        #endregion // Transform
    
        #region RectTransform

        private sealed class TweenData_RectTransform_AnchorPosSpline : ITweenData
        {
            private RectTransform m_RectTransform;
            private ISpline m_Spline;
            private Axis m_Axis;

            private SplineTweenSettings m_SplineSettings;

            private Vector2 m_Start;

            public TweenData_RectTransform_AnchorPosSpline(RectTransform inRectTransform, ISpline inSpline, Axis inAxis, SplineTweenSettings inSettings)
            {
                m_RectTransform = inRectTransform;
                m_Spline = inSpline;
                m_Axis = inAxis;
                m_SplineSettings = inSettings;
            }

            public void OnTweenStart()
            {
                m_Start = m_SplineSettings.Offset == SplineOffset.Relative ? m_RectTransform.anchoredPosition : Vector2.zero;
                m_Spline.Process();
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector2 final = (Vector2)m_Spline.GetPoint(m_Spline.TransformPercent(inPercent, m_SplineSettings.LerpMethod), m_SplineSettings.SegmentEase);
                final.x += m_Start.x;
                final.y += m_Start.y;
                m_RectTransform.SetAnchorPos(final, m_Axis);
            }

            public override string ToString()
            {
                return "RectTransform: AnchorPos (Spline)";
            }
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline over time.
        /// </summary>
        static public Tween AnchorPosAlong(this RectTransform inRectTransform, ISpline inSpline, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, SplineTweenSettings.Default), inTime);
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline over time.
        /// </summary>
        static public Tween AnchorPosAlong(this RectTransform inRectTransform, ISpline inSpline, float inTime, Axis inAxis, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, inSplineSettings), inTime);
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline over time.
        /// </summary>
        static public Tween AnchorPosAlong(this RectTransform inRectTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, SplineTweenSettings.Default), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline over time.
        /// </summary>
        static public Tween AnchorPosAlong(this RectTransform inRectTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis, SplineTweenSettings inSplineSettings)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, inSplineSettings), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline with the given average speed.
        /// </summary>
        static public Tween AnchorPosAlongWithSpeed(this RectTransform inRectTransform, ISpline inSpline, float inSpeed, Axis inAxis = Axis.XY)
        {
            return AnchorPosAlongWithSpeed(inRectTransform, inSpline, inSpeed, inAxis, SplineTweenSettings.Default);
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline with the given average speed.
        /// </summary>
        static public Tween AnchorPosAlongWithSpeed(this RectTransform inRectTransform, ISpline inSpline, float inSpeed, Axis inAxis, SplineTweenSettings inSplineSettings)
        {
            float time;
            switch(inSplineSettings.LerpMethod)
            {
                case SplineLerp.Direct:
                case SplineLerp.Vertex:
                    time = inSpline.GetDirectDistance() / inSpeed;
                    break;

                case SplineLerp.Precise:
                default:
                    time = inSpline.GetDistance() / inSpeed;
                    break;
            }

            return Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, inSplineSettings), time);
        }

        #endregion // RectTransform
    }
}
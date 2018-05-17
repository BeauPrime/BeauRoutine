/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 May 2018
 * 
 * File:    Spline.cs
 * Purpose: Defines a common interface for splines, along with factory methods.
*/

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    static public class SplineTweens
    {
        #region Transform

        private sealed class TweenData_Transform_PositionSpline : ITweenData
        {
            private Transform m_Transform;
            private ISpline m_Spline;
            private Space m_Space;
            private Axis m_Axis;
            private SplinePositioning m_Positioning;
            private SplineLerpSpace m_Lerp;

            private Vector3 m_Start;

            public TweenData_Transform_PositionSpline(Transform inTransform, ISpline inSpline, Space inSpace, Axis inAxis, SplinePositioning inPositioning, SplineLerpSpace inLerpMethod)
            {
                m_Transform = inTransform;
                m_Spline = inSpline;
                m_Space = inSpace;
                m_Axis = inAxis;
                m_Positioning = inPositioning;
                m_Lerp = inLerpMethod;
            }

            public void OnTweenStart()
            {
                m_Start = m_Positioning == SplinePositioning.Relative ? (m_Space == Space.World ? m_Transform.position : m_Transform.localPosition) : Vector3.zero;
                m_Spline.Process();
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 final = m_Start + m_Spline.Lerp(m_Spline.CorrectPercent(inPercent, m_Lerp));
                m_Transform.SetPosition(final, m_Axis, m_Space);
            }

            public override string ToString()
            {
                return "Transform: Position (Spline)";
            }
        }

        /// <summary>
        /// Moves the Transform along a spline over time.
        /// </summary>
        static public Tween MoveAlong(this Transform inTransform, ISpline inSpline, float inTime, Axis inAxis = Axis.XYZ, Space inSpace = Space.World, SplinePositioning inPositioning = SplinePositioning.Relative, SplineLerpSpace inLerpMethod = SplineLerpSpace.Direct)
        {
            return Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, inPositioning, inLerpMethod), inTime);
        }

        /// <summary>
        /// Moves the Transform along a spline over time.
        /// </summary>
        static public Tween MoveAlong(this Transform inTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis = Axis.XYZ, Space inSpace = Space.World, SplinePositioning inPositioning = SplinePositioning.Relative, SplineLerpSpace inLerpMethod = SplineLerpSpace.Direct)
        {
            return Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, inPositioning, inLerpMethod), inSettings);
        }

        #endregion // Transform
    
        #region RectTransform

        private sealed class TweenData_RectTransform_AnchorPosSpline : ITweenData
        {
            private RectTransform m_RectTransform;
            private ISpline m_Spline;
            private Axis m_Axis;
            private SplinePositioning m_Positioning;
            private SplineLerpSpace m_Lerp;

            private Vector2 m_Start;

            public TweenData_RectTransform_AnchorPosSpline(RectTransform inRectTransform, ISpline inSpline, Axis inAxis, SplinePositioning inPositioning, SplineLerpSpace inLerpMethod)
            {
                m_RectTransform = inRectTransform;
                m_Spline = inSpline;
                m_Axis = inAxis;
                m_Positioning = inPositioning;
                m_Lerp = inLerpMethod;
            }

            public void OnTweenStart()
            {
                m_Start = m_Positioning == SplinePositioning.Relative ? m_RectTransform.anchoredPosition : Vector2.zero;
                m_Spline.Process();
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector2 final = m_Start + (Vector2)m_Spline.Lerp(m_Spline.CorrectPercent(inPercent, m_Lerp));
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
        static public Tween AnchorPosAlong(this RectTransform inRectTransform, ISpline inSpline, float inTime, Axis inAxis = Axis.XY, SplinePositioning inPositioning = SplinePositioning.Relative, SplineLerpSpace inLerpMethod = SplineLerpSpace.Direct)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, inPositioning, inLerpMethod), inTime);
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline over time.
        /// </summary>
        static public Tween AnchorPosAlong(this RectTransform inRectTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis = Axis.XY, SplinePositioning inPositioning = SplinePositioning.Relative, SplineLerpSpace inLerpMethod = SplineLerpSpace.Direct)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, inPositioning, inLerpMethod), inSettings);
        }

        #endregion // Splines
    }
}
/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    24 May 2018
 * 
 * File:    SplineTweenShortcuts.cs
 * Purpose: Contains shortcuts for splines.
 */

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
                SplineUpdateInfo info;
                Splines.Spline.GetUpdateInfo(m_Spline, inPercent, m_SplineSettings, out info);
                info.Point.x += m_Start.x;
                info.Point.y += m_Start.y;
                info.Point.z += m_Start.z;

                m_Transform.SetPosition(info.Point, m_Axis, m_Space);
                m_SplineSettings.Orient.Apply(ref info, m_Transform, m_Space);

                if (m_SplineSettings.UpdateCallback != null)
                {
                    m_SplineSettings.UpdateCallback(info);
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
            return BeauRoutine.Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, SplineTweenSettings.Default), inTime);
        }

        /// <summary>
        /// Moves the Transform along a spline over time.
        /// </summary>
        static public Tween MoveAlong(this Transform inTransform, ISpline inSpline, float inTime, Axis inAxis, Space inSpace, SplineTweenSettings inSplineSettings)
        {
            return BeauRoutine.Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, inSplineSettings), inTime);
        }

        /// <summary>
        /// Moves the Transform along a spline over time.
        /// </summary>
        static public Tween MoveAlong(this Transform inTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return BeauRoutine.Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, SplineTweenSettings.Default), inSettings);
        }

        /// <summary>
        /// Moves the Transform along a spline over time.
        /// </summary>
        static public Tween MoveAlong(this Transform inTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis, Space inSpace, SplineTweenSettings inSplineSettings)
        {
            return BeauRoutine.Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, inSplineSettings), inSettings);
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
            switch (inSplineSettings.LerpMethod)
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

            return BeauRoutine.Tween.Create(new TweenData_Transform_PositionSpline(inTransform, inSpline, inSpace, inAxis, inSplineSettings), time);
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
                SplineUpdateInfo info;
                Splines.Spline.GetUpdateInfo(m_Spline, inPercent, m_SplineSettings, out info);
                info.Point.x += m_Start.x;
                info.Point.y += m_Start.y;

                m_RectTransform.SetAnchorPos(info.Point, m_Axis);
                m_SplineSettings.Orient.Apply(ref info, m_RectTransform, Space.Self);

                if (m_SplineSettings.UpdateCallback != null)
                {
                    m_SplineSettings.UpdateCallback(info);
                }
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
            return BeauRoutine.Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, SplineTweenSettings.Default), inTime);
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline over time.
        /// </summary>
        static public Tween AnchorPosAlong(this RectTransform inRectTransform, ISpline inSpline, float inTime, Axis inAxis, SplineTweenSettings inSplineSettings)
        {
            return BeauRoutine.Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, inSplineSettings), inTime);
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline over time.
        /// </summary>
        static public Tween AnchorPosAlong(this RectTransform inRectTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return BeauRoutine.Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, SplineTweenSettings.Default), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform's anchorPosition along a spline over time.
        /// </summary>
        static public Tween AnchorPosAlong(this RectTransform inRectTransform, ISpline inSpline, TweenSettings inSettings, Axis inAxis, SplineTweenSettings inSplineSettings)
        {
            return BeauRoutine.Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, inSplineSettings), inSettings);
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
            switch (inSplineSettings.LerpMethod)
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

            return BeauRoutine.Tween.Create(new TweenData_RectTransform_AnchorPosSpline(inRectTransform, inSpline, inAxis, inSplineSettings), time);
        }

        #endregion // RectTransform

        #region Spline Vertices

        private sealed class TweenData_SplineVertex_Position : ITweenData
        {
            private ISpline m_Spline;
            private int m_Index;
            private Vector3 m_Target;
            private Axis m_Axis;

            private Vector3 m_Start;
            private Vector3 m_Delta;

            public TweenData_SplineVertex_Position(ISpline inSpline, int inIndex, Vector3 inTarget, Axis inAxis)
            {
                m_Spline = inSpline;
                m_Index = inIndex;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Spline.GetVertex(m_Index);
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 final = new Vector3(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent,
                    m_Start.z + m_Delta.z * inPercent);
                Vector3 current;
                if ((m_Axis & Axis.XYZ) == Axis.XYZ)
                {
                    current = final;
                }
                else
                {
                    current = m_Spline.GetVertex(m_Index);
                    VectorUtil.CopyFrom(ref current, final, m_Axis);
                }

                m_Spline.SetVertex(m_Index, current);
            }

            public override string ToString()
            {
                return "Spline: Vertex Position";
            }
        }

        private sealed class TweenData_SplineControl_Position : ITweenData
        {
            private ISpline m_Spline;
            private int m_Index;
            private Vector3 m_Target;
            private Axis m_Axis;

            private Vector3 m_Start;
            private Vector3 m_Delta;

            public TweenData_SplineControl_Position(ISpline inSpline, int inIndex, Vector3 inTarget, Axis inAxis)
            {
                m_Spline = inSpline;
                m_Index = inIndex;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Spline.GetControlPoint(m_Index);
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 final = new Vector3(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent,
                    m_Start.z + m_Delta.z * inPercent);
                Vector3 current;
                if ((m_Axis & Axis.XYZ) == Axis.XYZ)
                {
                    current = final;
                }
                else
                {
                    current = m_Spline.GetControlPoint(m_Index);
                    VectorUtil.CopyFrom(ref current, final, m_Axis);
                }

                m_Spline.SetControlPoint(m_Index, current);
            }

            public override string ToString()
            {
                return "Spline: Control Position";
            }
        }

        /// <summary>
        /// Tween a spline vertex position to the given position over time.
        /// </summary>
        static public Tween VertexPosTo(this ISpline inSpline, int inIndex, Vector3 inTarget, float inTime, Axis inAxis = Axis.XYZ)
        {
            return BeauRoutine.Tween.Create(new TweenData_SplineVertex_Position(inSpline, inIndex, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Tween a spline vertex position to the given position over time.
        /// </summary>
        static public Tween VertexPosTo(this ISpline inSpline, int inIndex, Vector3 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ)
        {
            return BeauRoutine.Tween.Create(new TweenData_SplineVertex_Position(inSpline, inIndex, inTarget, inAxis), inSettings);
        }

        /// <summary>
        /// Tween a spline control position to the given position over time.
        /// </summary>
        static public Tween ControlPosTo(this ISpline inSpline, int inIndex, Vector3 inTarget, float inTime, Axis inAxis = Axis.XYZ)
        {
            return BeauRoutine.Tween.Create(new TweenData_SplineControl_Position(inSpline, inIndex, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Tween a spline control position to the given position over time.
        /// </summary>
        static public Tween ControlPosTo(this ISpline inSpline, int inIndex, Vector3 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ)
        {
            return BeauRoutine.Tween.Create(new TweenData_SplineControl_Position(inSpline, inIndex, inTarget, inAxis), inSettings);
        }

        #endregion // Spline Vertices
    }
}
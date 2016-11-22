/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenShortcuts.RectTransform.cs
 * Purpose: Extension methods for creating Tweens affecting
 *          properties on a RectTransform.
*/

using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating tweens.
    /// </summary>
    static public partial class TweenShortcuts
    {
        #region Anchor

        private sealed class TweenData_RectTransform_AnchorFixed : ITweenData
        {
            private RectTransform m_Transform;
            private Vector2 m_Target;
            private Axis m_Axis;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_RectTransform_AnchorFixed(RectTransform inTransform, Vector2 inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.anchoredPosition;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 final = new Vector2(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent);

                m_Transform.SetAnchorPos(final, m_Axis);
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "RectTransform: Anchor (Fixed)";
            }
        }

        private sealed class TweenData_RectTransform_AnchorDynamic : ITweenData
        {
            private RectTransform m_Transform;
            private RectTransform m_Target;
            private Axis m_Axis;
            
            private Vector2 m_Start;

            public TweenData_RectTransform_AnchorDynamic(RectTransform inTransform, RectTransform inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.anchoredPosition;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 target = m_Target.anchoredPosition;
                Vector2 delta = target - m_Start;
                Vector2 final = new Vector3(
                    m_Start.x + delta.x * inPercent,
                    m_Start.y + delta.y * inPercent);

                m_Transform.SetAnchorPos(final, m_Axis);
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "RectTransform: Anchor (Dynamic)";
            }
        }

        /// <summary>
        /// Moves the RectTransform to another anchor over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, Vector2 inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorFixed(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Moves the RectTransform to another anchor over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, Vector2 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorFixed(inTransform, inTarget, inAxis), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform to another anchor over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, float inTarget, float inTime, Axis inAxis)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorFixed(inTransform, new Vector2(inTarget, inTarget), inAxis), inTime);
        }

        /// <summary>
        /// Moves the RectTransform to another anchor over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, float inTarget, TweenSettings inSettings, Axis inAxis)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorFixed(inTransform, new Vector2(inTarget, inTarget), inAxis), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform to the anchor of another RectTransform over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, RectTransform inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorDynamic(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Moves the RectTransform to the anchor of another RectTransform over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, RectTransform inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorDynamic(inTransform, inTarget, inAxis), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform to another anchor with the given average speed.
        /// </summary>
        static public Tween AnchorToWithSpeed(this RectTransform inTransform, Vector2 inTarget, float inSpeed, Axis inAxis = Axis.XY)
        {
            float distance = (inTarget - inTransform.anchoredPosition).magnitude;
            return Tween.Create(new TweenData_RectTransform_AnchorFixed(inTransform, inTarget, inAxis), distance / inSpeed);
        }

        #endregion

        #region Size Delta

        private sealed class TweenData_RectTransform_SizeDeltaFixed : ITweenData
        {
            private RectTransform m_Transform;
            private Vector2 m_Target;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_RectTransform_SizeDeltaFixed(RectTransform inTransform, Vector2 inTarget)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.sizeDelta;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 final = new Vector2(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent);

                m_Transform.sizeDelta = final;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "RectTransform: SizeDelta (Fixed)";
            }
        }

        private sealed class TweenData_RectTransform_SizeDeltaDynamic : ITweenData
        {
            private RectTransform m_Transform;
            private RectTransform m_Target;

            private Vector2 m_Start;

            public TweenData_RectTransform_SizeDeltaDynamic(RectTransform inTransform, RectTransform inTarget)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.sizeDelta;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 target = m_Target.sizeDelta;
                Vector2 delta = target - m_Start;
                Vector2 final = new Vector2(
                    m_Start.x + delta.x * inPercent,
                    m_Start.y + delta.y * inPercent);

                m_Transform.sizeDelta = final;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "RectTransform: SizeDelta (Dynamic)";
            }
        }

        /// <summary>
        /// Scales the given RectTransform to another sizeDelta over time.
        /// </summary>
        static public Tween SizeDeltaTo(this RectTransform inTransform, Vector2 inTarget, float inTime)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaFixed(inTransform, inTarget), inTime);
        }

        /// <summary>
        /// Scales the given RectTransform to another sizeDelta over time.
        /// </summary>
        static public Tween SizeDeltaTo(this RectTransform inTransform, Vector2 inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaFixed(inTransform, inTarget), inSettings);
        }

        /// <summary>
        /// Scales the given RectTransform to the sizeDelta of another RectTransform over time.
        /// </summary>
        static public Tween SizeDeltaTo(this RectTransform inTransform, RectTransform inTarget, float inTime)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaDynamic(inTransform, inTarget), inTime);
        }

        /// <summary>
        /// Scales the given RectTransform to the sizeDelta of another RectTransform over time.
        /// </summary>
        static public Tween SizeDeltaTo(this RectTransform inTransform, RectTransform inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaDynamic(inTransform, inTarget), inSettings);
        }

        #endregion
    }
}

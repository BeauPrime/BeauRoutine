/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
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
        #region Anchor Position

        private sealed class TweenData_RectTransform_AnchorPosFixed : ITweenData
        {
            private RectTransform m_Transform;
            private Vector2 m_Target;
            private Axis m_Axis;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_RectTransform_AnchorPosFixed(RectTransform inTransform, Vector2 inTarget, Axis inAxis)
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
                return "RectTransform: Anchored Position (Fixed)";
            }
        }

        private sealed class TweenData_RectTransform_AnchorPosDynamic : ITweenData
        {
            private RectTransform m_Transform;
            private RectTransform m_Target;
            private Axis m_Axis;
            
            private Vector2 m_Start;

            public TweenData_RectTransform_AnchorPosDynamic(RectTransform inTransform, RectTransform inTarget, Axis inAxis)
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
                return "RectTransform: Anchored Position (Dynamic)";
            }
        }

        /// <summary>
        /// Moves the RectTransform to another anchored position over time.
        /// </summary>
        static public Tween AnchorPosTo(this RectTransform inTransform, Vector2 inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosFixed(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Moves the RectTransform to another anchored position over time.
        /// </summary>
        static public Tween AnchorPosTo(this RectTransform inTransform, Vector2 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosFixed(inTransform, inTarget, inAxis), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform to another anchored position over time.
        /// </summary>
        static public Tween AnchorPosTo(this RectTransform inTransform, float inTarget, float inTime, Axis inAxis)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosFixed(inTransform, new Vector2(inTarget, inTarget), inAxis), inTime);
        }

        /// <summary>
        /// Moves the RectTransform to another anchored position over time.
        /// </summary>
        static public Tween AnchorPosTo(this RectTransform inTransform, float inTarget, TweenSettings inSettings, Axis inAxis)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosFixed(inTransform, new Vector2(inTarget, inTarget), inAxis), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform to the anchor of another RectTransform over time.
        /// </summary>
        static public Tween AnchorPosTo(this RectTransform inTransform, RectTransform inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosDynamic(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Moves the RectTransform to the anchor of another RectTransform over time.
        /// </summary>
        static public Tween AnchorPosTo(this RectTransform inTransform, RectTransform inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorPosDynamic(inTransform, inTarget, inAxis), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform to another anchor with the given average speed.
        /// </summary>
        static public Tween AnchorPosToWithSpeed(this RectTransform inTransform, Vector2 inTarget, float inSpeed, Axis inAxis = Axis.XY)
        {
            float distance = (inTarget - inTransform.anchoredPosition).magnitude;
            return Tween.Create(new TweenData_RectTransform_AnchorPosFixed(inTransform, inTarget, inAxis), distance / inSpeed);
        }

        #endregion

        #region Anchor

        private sealed class TweenData_RectTransform_AnchorFixed : ITweenData
        {
            private RectTransform m_Transform;
            private Vector4 m_Target;
            private Axis m_Axis;

            private Vector4 m_Start;
            private Vector4 m_Delta;

            public TweenData_RectTransform_AnchorFixed(RectTransform inTransform, Vector4 inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = new Vector4(m_Transform.anchorMin.x, m_Transform.anchorMin.y, m_Transform.anchorMax.x, m_Transform.anchorMax.y);
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                Vector4 final = new Vector4(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent,
                    m_Start.z + m_Delta.z * inPercent,
                    m_Start.w + m_Delta.w * inPercent);

                if ((m_Axis & Axis.X) == 0)
                {
                    final.x = m_Transform.anchorMin.x;
                    final.z = m_Transform.anchorMax.x;
                }
                if ((m_Axis & Axis.Y) == 0)
                {
                    final.y = m_Transform.anchorMin.y;
                    final.w = m_Transform.anchorMax.y;
                }

                m_Transform.anchorMin = new Vector2(final.x, final.y);
                m_Transform.anchorMax = new Vector2(final.z, final.w);
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

            private Vector4 m_Start;

            public TweenData_RectTransform_AnchorDynamic(RectTransform inTransform, RectTransform inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = new Vector4(m_Transform.anchorMin.x, m_Transform.anchorMin.y, m_Transform.anchorMax.x, m_Transform.anchorMax.y);
            }

            public void ApplyTween(float inPercent)
            {
                Vector4 delta = new Vector4(m_Target.anchorMin.x, m_Target.anchorMin.y, m_Target.anchorMax.x, m_Target.anchorMax.y) - m_Start;

                Vector4 final = new Vector4(
                    m_Start.x + delta.x * inPercent,
                    m_Start.y + delta.y * inPercent,
                    m_Start.z + delta.z * inPercent,
                    m_Start.w + delta.w * inPercent);

                if ((m_Axis & Axis.X) == 0)
                {
                    final.x = m_Transform.anchorMin.x;
                    final.z = m_Transform.anchorMax.x;
                }
                if ((m_Axis & Axis.Y) == 0)
                {
                    final.y = m_Transform.anchorMin.y;
                    final.w = m_Transform.anchorMax.y;
                }

                m_Transform.anchorMin = new Vector2(final.x, final.y);
                m_Transform.anchorMax = new Vector2(final.z, final.w);
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "RectTransform: Anchor (Dynamic)";
            }
        }

        /// <summary>
        /// Moves the RectTransform's anchors to a single point over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, Vector2 inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorFixed(inTransform, new Vector4(inTarget.x, inTarget.y, inTarget.x, inTarget.y), inAxis), inTime);
        }

        /// <summary>
        /// Moves the RectTransform's anchors to a single point over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, Vector2 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorFixed(inTransform, new Vector4(inTarget.x, inTarget.y, inTarget.x, inTarget.y), inAxis), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform's anchors to a set of points over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, Vector4 inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorFixed(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Moves the RectTransform's anchors to a set of points over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, Vector4 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorFixed(inTransform, inTarget, inAxis), inSettings);
        }

        /// <summary>
        /// Moves the RectTransform's anchors to match the given RectTransform's anchors over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, RectTransform inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorDynamic(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Moves the RectTransform's anchors to match the given RectTransform's anchors over time.
        /// </summary>
        static public Tween AnchorTo(this RectTransform inTransform, RectTransform inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_AnchorDynamic(inTransform, inTarget, inAxis), inSettings);
        }

        #endregion

        #region Size Delta

        private sealed class TweenData_RectTransform_SizeDeltaFixed : ITweenData
        {
            private RectTransform m_Transform;
            private Vector2 m_Target;
            private Axis m_Axis;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_RectTransform_SizeDeltaFixed(RectTransform inTransform, Vector2 inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
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

                m_Transform.SetSizeDelta(final, m_Axis);
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
            private Axis m_Axis;

            private Vector2 m_Start;

            public TweenData_RectTransform_SizeDeltaDynamic(RectTransform inTransform, RectTransform inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
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

                m_Transform.SetSizeDelta(final, m_Axis);
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
        static public Tween SizeDeltaTo(this RectTransform inTransform, Vector2 inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaFixed(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Scales the given RectTransform to another sizeDelta over time.
        /// </summary>
        static public Tween SizeDeltaTo(this RectTransform inTransform, Vector2 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaFixed(inTransform, inTarget, inAxis), inSettings);
        }

        /// <summary>
        /// Scales the given RectTransform to another sizeDelta over time.
        /// </summary>
        static public Tween SizeDeltaTo(this RectTransform inTransform, float inTarget, float inTime, Axis inAxis)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaFixed(inTransform, new Vector2(inTarget, inTarget), inAxis), inTime);
        }

        /// <summary>
        /// Scales the given RectTransform to another sizeDelta over time.
        /// </summary>
        static public Tween SizeDeltaTo(this RectTransform inTransform, float inTarget, TweenSettings inSettings, Axis inAxis)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaFixed(inTransform, new Vector2(inTarget, inTarget), inAxis), inSettings);
        }

        /// <summary>
        /// Scales the given RectTransform to the sizeDelta of another RectTransform over time.
        /// </summary>
        static public Tween SizeDeltaTo(this RectTransform inTransform, RectTransform inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaDynamic(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Scales the given RectTransform to the sizeDelta of another RectTransform over time.
        /// </summary>
        static public Tween SizeDeltaTo(this RectTransform inTransform, RectTransform inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_SizeDeltaDynamic(inTransform, inTarget, inAxis), inSettings);
        }

        #endregion

        #region Pivot

        private sealed class TweenData_RectTransform_PivotFixed : ITweenData
        {
            private RectTransform m_Transform;
            private Vector2 m_Target;
            private Axis m_Axis;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_RectTransform_PivotFixed(RectTransform inTransform, Vector2 inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.pivot;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 final = new Vector2(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent);

                if ((m_Axis & Axis.X) == 0)
                    final.x = m_Transform.pivot.x;
                if ((m_Axis & Axis.Y) == 0)
                    final.y = m_Transform.pivot.y;

                m_Transform.pivot = final;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "RectTransform: Pivot (Fixed)";
            }
        }

        private sealed class TweenData_RectTransform_PivotDynamic : ITweenData
        {
            private RectTransform m_Transform;
            private RectTransform m_Target;
            private Axis m_Axis;

            private Vector2 m_Start;

            public TweenData_RectTransform_PivotDynamic(RectTransform inTransform, RectTransform inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.pivot;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 target = m_Target.pivot;
                Vector2 delta = target - m_Start;
                Vector2 final = new Vector2(
                    m_Start.x + delta.x * inPercent,
                    m_Start.y + delta.y * inPercent);

                if ((m_Axis & Axis.X) == 0)
                    final.x = m_Transform.pivot.x;
                if ((m_Axis & Axis.Y) == 0)
                    final.y = m_Transform.pivot.y;

                m_Transform.pivot = final;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "RectTransform: Pivot (Dynamic)";
            }
        }

        /// <summary>
        /// Moves the given RectTransform's pivot to another point over time.
        /// </summary>
        static public Tween PivotTo(this RectTransform inTransform, Vector2 inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_PivotFixed(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Moves the given RectTransform's pivot to another point over time.
        /// </summary>
        static public Tween PivotTo(this RectTransform inTransform, Vector2 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_PivotFixed(inTransform, inTarget, inAxis), inSettings);
        }

        /// <summary>
        /// Moves the given RectTransform's pivot to another point over time.
        /// </summary>
        static public Tween PivotTo(this RectTransform inTransform, float inTarget, float inTime, Axis inAxis)
        {
            return Tween.Create(new TweenData_RectTransform_PivotFixed(inTransform, new Vector2(inTarget, inTarget), inAxis), inTime);
        }

        /// <summary>
        /// Moves the given RectTransform's pivot to another point over time.
        /// </summary>
        static public Tween PivotTo(this RectTransform inTransform, float inTarget, TweenSettings inSettings, Axis inAxis)
        {
            return Tween.Create(new TweenData_RectTransform_PivotFixed(inTransform, new Vector2(inTarget, inTarget), inAxis), inSettings);
        }

        /// <summary>
        /// Moves the given RectTransform's pivot to the pivot of another RectTransform over time.
        /// </summary>
        static public Tween PivotTo(this RectTransform inTransform, RectTransform inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_PivotDynamic(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Moves the given RectTransform's pivot to the pivot of another RectTransform over time.
        /// </summary>
        static public Tween PivotTo(this RectTransform inTransform, RectTransform inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RectTransform_PivotDynamic(inTransform, inTarget, inAxis), inSettings);
        }

        #endregion
    }
}

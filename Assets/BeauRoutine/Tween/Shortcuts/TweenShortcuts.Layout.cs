/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenShortcuts.Layout.cs
 * Purpose: Extension methods for creating Tweens affecting
 *          properties on LayoutElements and LayoutGroups.
*/

using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating tweens.
    /// </summary>
    static public partial class TweenShortcuts
    {
        #region LayoutElement

        private sealed class TweenData_LayoutElement_MinSize : ITweenData
        {
            private LayoutElement m_Layout;
            private Vector2 m_Target;
            private Axis m_Axis;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_LayoutElement_MinSize(LayoutElement inLayout, Vector2 inTarget, Axis inAxis)
            {
                m_Layout = inLayout;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = new Vector2(m_Layout.minWidth, m_Layout.minHeight);
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 newValue = m_Start + m_Delta * inPercent;

                if ((m_Axis & Axis.X) != 0)
                    m_Layout.minWidth = newValue.x;
                if ((m_Axis & Axis.Y) != 0)
                    m_Layout.minHeight = newValue.y;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "LayoutElement: Min Size";
            }
        }

        private sealed class TweenData_LayoutElement_PreferredSize : ITweenData
        {
            private LayoutElement m_Layout;
            private Vector2 m_Target;
            private Axis m_Axis;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_LayoutElement_PreferredSize(LayoutElement inLayout, Vector2 inTarget, Axis inAxis)
            {
                m_Layout = inLayout;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = new Vector2(m_Layout.preferredWidth, m_Layout.preferredHeight);
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 newValue = m_Start + m_Delta * inPercent;

                if ((m_Axis & Axis.X) != 0)
                    m_Layout.preferredWidth = newValue.x;
                if ((m_Axis & Axis.Y) != 0)
                    m_Layout.preferredHeight = newValue.y;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "LayoutElement: Perferred Size";
            }
        }

        private sealed class TweenData_LayoutElement_FlexibleSize : ITweenData
        {
            private LayoutElement m_Layout;
            private Vector2 m_Target;
            private Axis m_Axis;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_LayoutElement_FlexibleSize(LayoutElement inLayout, Vector2 inTarget, Axis inAxis)
            {
                m_Layout = inLayout;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = new Vector2(m_Layout.flexibleWidth, m_Layout.flexibleHeight);
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 newValue = m_Start + m_Delta * inPercent;

                if ((m_Axis & Axis.X) != 0)
                    m_Layout.flexibleWidth = newValue.x;
                if ((m_Axis & Axis.Y) != 0)
                    m_Layout.flexibleHeight = newValue.y;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "LayoutElement: Flexible Size";
            }
        }

        /// <summary>
        /// Tweens the minimum size on a LayoutElement over time.
        /// </summary>
        static public Tween MinSizeTo(this LayoutElement inLayout, Vector2 inValue, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_LayoutElement_MinSize(inLayout, inValue, inAxis), inTime);
        }

        /// <summary>
        /// Tweens the minimum size on a LayoutElement over time.
        /// </summary>
        static public Tween MinSizeTo(this LayoutElement inLayout, Vector2 inValue, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_LayoutElement_MinSize(inLayout, inValue, inAxis), inSettings);
        }

        /// <summary>
        /// Tweens the preferred size on a LayoutElement over time.
        /// </summary>
        static public Tween PreferredSizeTo(this LayoutElement inLayout, Vector2 inValue, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_LayoutElement_PreferredSize(inLayout, inValue, inAxis), inTime);
        }

        /// <summary>
        /// Tweens the preferred size on a LayoutElement over time.
        /// </summary>
        static public Tween PreferredSizeTo(this LayoutElement inLayout, Vector2 inValue, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_LayoutElement_PreferredSize(inLayout, inValue, inAxis), inSettings);
        }

        /// <summary>
        /// Tweens the flexible size on a LayoutElement over time.
        /// </summary>
        static public Tween FlexibleSizeTo(this LayoutElement inLayout, Vector2 inValue, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_LayoutElement_FlexibleSize(inLayout, inValue, inAxis), inTime);
        }

        /// <summary>
        /// Tweens the flexible size on a LayoutElement over time.
        /// </summary>
        static public Tween FlexibleSizeTo(this LayoutElement inLayout, Vector2 inValue, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_LayoutElement_FlexibleSize(inLayout, inValue, inAxis), inSettings);
        }

        #endregion

        #region Padding

        private sealed class TweenData_LayoutGroup_Padding : ITweenData
        {
            private LayoutGroup m_Layout;
            private RectOffset m_Target;

            private RectOffset m_Start;

            public TweenData_LayoutGroup_Padding(LayoutGroup inRenderer, RectOffset inTarget)
            {
                m_Layout = inRenderer;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Layout.padding;
            }

            public void ApplyTween(float inPercent)
            {
                RectOffset final = new UnityEngine.RectOffset(
                    (int)(m_Start.left + (m_Target.left - m_Start.left) * inPercent),
                    (int)(m_Start.right + (m_Target.right - m_Start.right) * inPercent),
                    (int)(m_Start.top + (m_Target.top - m_Start.top) * inPercent),
                    (int)(m_Start.bottom + (m_Target.bottom - m_Start.bottom) * inPercent));
                m_Layout.padding = final;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "LayoutGroup: Padding";
            }
        }

        /// <summary>
        /// Tweens the padding on a LayoutGroup to another value over time.
        /// </summary>
        static public Tween PaddingTo(this LayoutGroup inLayout, RectOffset inValue, float inTime)
        {
            return Tween.Create(new TweenData_LayoutGroup_Padding(inLayout, inValue), inTime);
        }

        /// <summary>
        /// Tweens the padding on a LayoutGroup to another value over time.
        /// </summary>
        static public Tween PaddingTo(this LayoutGroup inLayout, int inValue, float inTime)
        {
            return Tween.Create(new TweenData_LayoutGroup_Padding(inLayout, new RectOffset(inValue, inValue, inValue, inValue)), inTime);
        }

        /// <summary>
        /// Tweens the padding on a LayoutGroup to another value over time.
        /// </summary>
        static public Tween PaddingTo(this LayoutGroup inLayout, RectOffset inValue, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_LayoutGroup_Padding(inLayout, inValue), inSettings);
        }

        /// <summary>
        /// Tweens the padding on a LayoutGroup to another value over time.
        /// </summary>
        static public Tween PaddingTo(this LayoutGroup inLayout, int inValue, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_LayoutGroup_Padding(inLayout, new RectOffset(inValue, inValue, inValue, inValue)), inSettings);
        }

        #endregion

        #region Spacing

        private sealed class TweenData_DirectionLayoutGroup_Spacing : ITweenData
        {
            private HorizontalOrVerticalLayoutGroup m_Layout;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_DirectionLayoutGroup_Spacing(HorizontalOrVerticalLayoutGroup inLayout, float inTarget)
            {
                m_Layout = inLayout;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Layout.spacing;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Layout.spacing = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "HorizontalOrVerticalLayoutGroup: Spacing";
            }
        }

        /// <summary>
        /// Tweens the spacing on a HorizontalOrVerticalLayoutGroup to another value over time.
        /// </summary>
        static public Tween SpacingTo(this HorizontalOrVerticalLayoutGroup inRenderer, float inSpacing, float inTime)
        {
            return Tween.Create(new TweenData_DirectionLayoutGroup_Spacing(inRenderer, inSpacing), inTime);
        }

        /// <summary>
        /// Tweens the spacing on a HorizontalOrVerticalLayoutGroup to another value over time.
        /// </summary>
        static public Tween SpacingTo(this HorizontalOrVerticalLayoutGroup inRenderer, float inSpacing, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_DirectionLayoutGroup_Spacing(inRenderer, inSpacing), inSettings);
        }

        #endregion

        #region GridLayoutGroup

        #region Spacing

        private sealed class TweenData_GridLayoutGroup_Spacing : ITweenData
        {
            private GridLayoutGroup m_Layout;
            private Vector2 m_Target;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_GridLayoutGroup_Spacing(GridLayoutGroup inLayout, Vector2 inTarget)
            {
                m_Layout = inLayout;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Layout.spacing;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Layout.spacing = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "GridLayoutGroup: Spacing";
            }
        }

        /// <summary>
        /// Tweens the spacing on a GridLayoutGroup to another value over time.
        /// </summary>
        static public Tween SpacingTo(this GridLayoutGroup inRenderer, Vector2 inSpacing, float inTime)
        {
            return Tween.Create(new TweenData_GridLayoutGroup_Spacing(inRenderer, inSpacing), inTime);
        }

        /// <summary>
        /// Tweens the spacing on a GridLayoutGroup to another value over time.
        /// </summary>
        static public Tween SpacingTo(this GridLayoutGroup inRenderer, float inSpacing, float inTime)
        {
            return Tween.Create(new TweenData_GridLayoutGroup_Spacing(inRenderer, new Vector2(inSpacing, inSpacing)), inTime);
        }

        /// <summary>
        /// Tweens the spacing on a GridLayoutGroup to another value over time.
        /// </summary>
        static public Tween SpacingTo(this GridLayoutGroup inRenderer, Vector2 inSpacing, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_GridLayoutGroup_Spacing(inRenderer, inSpacing), inSettings);
        }

        /// <summary>
        /// Tweens the spacing on a GridLayoutGroup to another value over time.
        /// </summary>
        static public Tween SpacingTo(this GridLayoutGroup inRenderer, float inSpacing, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_GridLayoutGroup_Spacing(inRenderer, new Vector2(inSpacing, inSpacing)), inSettings);
        }

        #endregion

        #region Cell Size

        private sealed class TweenData_GridLayoutGroup_CellSize : ITweenData
        {
            private GridLayoutGroup m_Layout;
            private Vector2 m_Target;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_GridLayoutGroup_CellSize(GridLayoutGroup inLayout, Vector2 inTarget)
            {
                m_Layout = inLayout;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Layout.cellSize;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Layout.cellSize = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "GridLayoutGroup: Cell Size";
            }
        }

        /// <summary>
        /// Tweens the cell size on a GridLayoutGroup to another value over time.
        /// </summary>
        static public Tween CellSizeTo(this GridLayoutGroup inRenderer, Vector2 inSize, float inTime)
        {
            return Tween.Create(new TweenData_GridLayoutGroup_CellSize(inRenderer, inSize), inTime);
        }

        /// <summary>
        /// Tweens the cell size on a GridLayoutGroup to another value over time.
        /// </summary>
        static public Tween CellSizeTo(this GridLayoutGroup inRenderer, float inSize, float inTime)
        {
            return Tween.Create(new TweenData_GridLayoutGroup_CellSize(inRenderer, new Vector2(inSize, inSize)), inTime);
        }

        /// <summary>
        /// Tweens the cell size on a GridLayoutGroup to another value over time.
        /// </summary>
        static public Tween CellSizeTo(this GridLayoutGroup inRenderer, Vector2 inSize, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_GridLayoutGroup_CellSize(inRenderer, inSize), inSettings);
        }

        /// <summary>
        /// Tweens the cell size on a GridLayoutGroup to another value over time.
        /// </summary>
        static public Tween CellSizeTo(this GridLayoutGroup inRenderer, float inSize, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_GridLayoutGroup_CellSize(inRenderer, new Vector2(inSize, inSize)), inSettings);
        }

        #endregion

        #endregion
    }
}

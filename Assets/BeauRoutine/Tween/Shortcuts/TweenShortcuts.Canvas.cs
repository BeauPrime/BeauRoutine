/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 May 2018
 * 
 * File:    TweenShortcuts.Canvas.cs
 * Purpose: Extension methods for creating Tweens affecting
 *          properties on Canvas renderers and UI elements.
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
        #region CanvasGroup

        private sealed class TweenData_CanvasGroup_Alpha : ITweenData
        {
            private CanvasGroup m_Renderer;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_CanvasGroup_Alpha(CanvasGroup inRenderer, float inTarget)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.alpha;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Renderer.alpha = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "CanvasGroup: Alpha";
            }
        }

        /// <summary>
        /// Fades the CanvasGroup to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this CanvasGroup inRenderer, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_CanvasGroup_Alpha(inRenderer, inAlpha), inTime);
        }

        /// <summary>
        /// Fades the CanvasGroup to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this CanvasGroup inRenderer, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_CanvasGroup_Alpha(inRenderer, inAlpha), inSettings);
        }

        #endregion

        #region Graphic

        private sealed class TweenData_Graphic_Alpha : ITweenData
        {
            private Graphic m_Renderer;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Graphic_Alpha(Graphic inRenderer, float inTarget)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.color.a;
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Renderer.color;
                final.a = m_Start + m_Delta * inPercent;
                m_Renderer.color = final;
            }

            public override string ToString()
            {
                return "Graphic: Alpha";
            }
        }

        private sealed class TweenData_Graphic_Color : ITweenData
        {
            private Graphic m_Renderer;
            private Color m_Target;
            private ColorUpdate m_Update;

            private Color m_Start;

            public TweenData_Graphic_Color(Graphic inRenderer, Color inTarget, ColorUpdate inUpdate)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.color;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = UnityEngine.Color.LerpUnclamped(m_Start, m_Target, inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Renderer.color.a;
                m_Renderer.color = final;
            }

            public override string ToString()
            {
                return "Graphic: Color";
            }
        }

        private sealed class TweenData_Graphic_Gradient : ITweenData
        {
            private Graphic m_Renderer;
            private Gradient m_Gradient;
            private ColorUpdate m_Update;

            public TweenData_Graphic_Gradient(Graphic inRenderer, Gradient inTarget, ColorUpdate inUpdate)
            {
                m_Renderer = inRenderer;
                m_Gradient = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Gradient.Evaluate(inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Renderer.color.a;
                m_Renderer.color = final;
            }

            public override string ToString()
            {
                return "Graphic: Gradient";
            }
        }

        /// <summary>
        /// Fades the Graphic to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Graphic inRenderer, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_Graphic_Alpha(inRenderer, inAlpha), inTime);
        }

        /// <summary>
        /// Fades the Graphic to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Graphic inRenderer, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Graphic_Alpha(inRenderer, inAlpha), inSettings);
        }

        /// <summary>
        /// Fades the Graphic to another color over time.
        /// </summary>
        static public Tween ColorTo(this Graphic inRenderer, Color inTarget, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Graphic_Color(inRenderer, inTarget, inUpdate), inTime);
        }

        /// <summary>
        /// Fades the Graphic to another color over time.
        /// </summary>
        static public Tween ColorTo(this Graphic inRenderer, Color inTarget, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Graphic_Color(inRenderer, inTarget, inUpdate), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to the Graphic over time.
        /// </summary>
        static public Tween Gradient(this Graphic inRenderer, Gradient inGradient, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Graphic_Gradient(inRenderer, inGradient, inUpdate), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to the Graphic over time.
        /// </summary>
        static public Tween Gradient(this Graphic inRenderer, Gradient inGradient, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Graphic_Gradient(inRenderer, inGradient, inUpdate), inSettings);
        }

        #endregion

        #region Image

        private sealed class TweenData_Image_FillAmount : ITweenData
        {
            private Image m_Renderer;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Image_FillAmount(Image inRenderer, float inTarget)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.fillAmount;
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Renderer.fillAmount = m_Start + m_Delta * inPercent;
            }

            public override string ToString()
            {
                return "Image: FillAmount";
            }
        }

        /// <summary>
        /// Shifts the Image's fillAmount over time.
        /// </summary>
        static public Tween FillTo(this Image inRenderer, float inFillAmount, float inTime)
        {
            return Tween.Create(new TweenData_Image_FillAmount(inRenderer, inFillAmount), inTime);
        }

        /// <summary>
        /// Shifts the Image's fillAmount over time.
        /// </summary>
        static public Tween FillTo(this Image inRenderer, float inFillAmount, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Image_FillAmount(inRenderer, inFillAmount), inSettings);
        }

        #endregion

        #region RawImage

        private sealed class TweenData_RawImage_UVRect : ITweenData
        {
            private RawImage m_Renderer;
            private Rect m_Target;
            private Axis m_Axis;

            private Rect m_Start;
            private Vector4 m_Delta;

            public TweenData_RawImage_UVRect(RawImage inRenderer, Rect inTarget, Axis inAxis)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.uvRect;
                m_Delta = new Vector4(m_Target.x - m_Start.x, m_Target.y - m_Start.y, m_Target.width - m_Start.width, m_Target.height - m_Start.height);
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Rect final = new Rect(m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent,
                    m_Start.width + m_Delta.z * inPercent,
                    m_Start.height + m_Delta.w * inPercent);

                Rect rendererRect = m_Renderer.uvRect;

                if ((m_Axis & Axis.X) == 0)
                {
                    final.xMin = rendererRect.xMin;
                    final.xMax = rendererRect.xMax;
                }
                if ((m_Axis & Axis.Y) == 0)
                {
                    final.yMin = rendererRect.yMin;
                    final.yMax = rendererRect.yMax;
                }

                m_Renderer.uvRect = final;
            }

            public override string ToString()
            {
                return "RawImage: UVRect";
            }
        }

        /// <summary>
        /// Shifts the RawImage's uvRect over time.
        /// </summary>
        static public Tween UVRectShift(this RawImage inRenderer, Vector2 inShift, float inTime, Axis inAxis = Axis.XY)
        {
            Rect shiftedRect = inRenderer.uvRect;
            shiftedRect.x += inShift.x;
            shiftedRect.y += inShift.y;
            return Tween.Create(new TweenData_RawImage_UVRect(inRenderer, shiftedRect, inAxis), inTime);
        }

        /// <summary>
        /// Shifts the RawImage's uvRect over time.
        /// </summary>
        static public Tween UVRectShift(this RawImage inRenderer, float inShift, float inTime, Axis inAxis)
        {
            return UVRectShift(inRenderer, new Vector2(inShift, inShift), inTime, inAxis);
        }

        /// <summary>
        /// Shifts the RawImage's uvRect over time.
        /// </summary>
        static public Tween UVRectShift(this RawImage inRenderer, Vector2 inShift, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            Rect shiftedRect = inRenderer.uvRect;
            shiftedRect.x += inShift.x;
            shiftedRect.y += inShift.y;
            return Tween.Create(new TweenData_RawImage_UVRect(inRenderer, shiftedRect, inAxis), inSettings);
        }

        /// <summary>
        /// Shifts the RawImage's uvRect over time.
        /// </summary>
        static public Tween UVRectShift(this RawImage inRenderer, float inShift, TweenSettings inSettings, Axis inAxis)
        {
            return UVRectShift(inRenderer, new Vector2(inShift, inShift), inSettings, inAxis);
        }

        /// <summary>
        /// Shifts the RawImage's uvRect over time.
        /// </summary>
        static public Tween UVRectTo(this RawImage inRenderer, Rect inTarget, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RawImage_UVRect(inRenderer, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Shifts the RawImage's uvRect over time.
        /// </summary>
        static public Tween UVRectTo(this RawImage inRenderer, Rect inTarget, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_RawImage_UVRect(inRenderer, inTarget, inAxis), inSettings);
        }

        #endregion

        #region CanvasRenderer

        private sealed class TweenData_CanvasRenderer_Alpha : ITweenData
        {
            private CanvasRenderer m_Renderer;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_CanvasRenderer_Alpha(CanvasRenderer inRenderer, float inTarget)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.GetAlpha();
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Renderer.SetAlpha(m_Start + m_Delta * inPercent);
            }

            public override string ToString()
            {
                return "CanvasRenderer: Alpha";
            }
        }

        private sealed class TweenData_CanvasRenderer_Color : ITweenData
        {
            private CanvasRenderer m_Renderer;
            private Color m_Target;
            private ColorUpdate m_Update;

            private Color m_Start;

            public TweenData_CanvasRenderer_Color(CanvasRenderer inRenderer, Color inTarget, ColorUpdate inUpdate)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.GetColor();
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = UnityEngine.Color.LerpUnclamped(m_Start, m_Target, inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Renderer.GetAlpha();
                m_Renderer.SetColor(final);
            }

            public override string ToString()
            {
                return "CanvasRenderer: Color";
            }
        }

        private sealed class TweenData_CanvasRenderer_Gradient : ITweenData
        {
            private CanvasRenderer m_Renderer;
            private Gradient m_Gradient;
            private ColorUpdate m_Update;

            public TweenData_CanvasRenderer_Gradient(CanvasRenderer inRenderer, Gradient inTarget, ColorUpdate inUpdate)
            {
                m_Renderer = inRenderer;
                m_Gradient = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Gradient.Evaluate(inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Renderer.GetAlpha();
                m_Renderer.SetColor(final);
            }

            public override string ToString()
            {
                return "CanvasRenderer: Gradient";
            }
        }

        /// <summary>
        /// Fades the Graphic to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this CanvasRenderer inRenderer, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_CanvasRenderer_Alpha(inRenderer, inAlpha), inTime);
        }

        /// <summary>
        /// Fades the Graphic to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this CanvasRenderer inRenderer, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_CanvasRenderer_Alpha(inRenderer, inAlpha), inSettings);
        }

        /// <summary>
        /// Fades the Graphic to another color over time.
        /// </summary>
        static public Tween ColorTo(this CanvasRenderer inRenderer, Color inTarget, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_CanvasRenderer_Color(inRenderer, inTarget, inUpdate), inTime);
        }

        /// <summary>
        /// Fades the Graphic to another color over time.
        /// </summary>
        static public Tween ColorTo(this CanvasRenderer inRenderer, Color inTarget, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_CanvasRenderer_Color(inRenderer, inTarget, inUpdate), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to the Graphic over time.
        /// </summary>
        static public Tween Gradient(this CanvasRenderer inRenderer, Gradient inGradient, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_CanvasRenderer_Gradient(inRenderer, inGradient, inUpdate), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to the Graphic over time.
        /// </summary>
        static public Tween Gradient(this CanvasRenderer inRenderer, Gradient inGradient, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_CanvasRenderer_Gradient(inRenderer, inGradient, inUpdate), inSettings);
        }

        #endregion
    
        #region ScrollRect

        private sealed class TweenData_ScrollRect_NormalizedPos : ITweenData
        {
            private ScrollRect m_ScrollRect;
            private Vector2 m_Target;
            private Axis m_Axis;

            private Vector2 m_Start;
            private Vector2 m_Delta;

            public TweenData_ScrollRect_NormalizedPos(ScrollRect inScrollRect, Vector2 inTarget, Axis inAxis)
            {
                m_ScrollRect = inScrollRect;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_ScrollRect.normalizedPosition;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                Vector2 final = new Vector2(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent);
                if ((m_Axis & Axis.XY) != Axis.XY)
                {
                    Vector2 current = m_ScrollRect.normalizedPosition;
                    if ((m_Axis & Axis.X) == 0)
                        final.x = current.x;
                    if ((m_Axis & Axis.Y) == 0)
                        final.y = current.y;
                }

                m_ScrollRect.normalizedPosition = final;;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "ScrollRect: Normalized Position";
            }
        }

        /// <summary>
        /// Changes the ScrollRect's normalized position to another value over time.
        /// </summary>
        static public Tween NormalizedPosTo(this ScrollRect inScrollRect, Vector2 inPosition, float inTime, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_ScrollRect_NormalizedPos(inScrollRect, inPosition, inAxis), inTime);
        }

        /// <summary>
        /// Changes the ScrollRect's normalized position to another value over time.
        /// </summary>
        static public Tween NormalizedPosTo(this ScrollRect inScrollRect, Vector2 inPosition, TweenSettings inSettings, Axis inAxis = Axis.XY)
        {
            return Tween.Create(new TweenData_ScrollRect_NormalizedPos(inScrollRect, inPosition, inAxis), inSettings);
        }

        /// <summary>
        /// Changes the ScrollRect's normalized position to another value over time.
        /// </summary>
        static public Tween NormalizedPosTo(this ScrollRect inScrollRect, float inPositionAxis, float inTime, Axis inAxis)
        {
            return Tween.Create(new TweenData_ScrollRect_NormalizedPos(inScrollRect, new Vector2(inPositionAxis, inPositionAxis), inAxis), inTime);
        }

        /// <summary>
        /// Changes the ScrollRect's normalized position to another value over time.
        /// </summary>
        static public Tween NormalizedPosTo(this ScrollRect inScrollRect, float inPositionAxis, TweenSettings inSettings, Axis inAxis)
        {
            return Tween.Create(new TweenData_ScrollRect_NormalizedPos(inScrollRect, new Vector2(inPositionAxis, inPositionAxis), inAxis), inSettings);
        }

        #endregion // ScrollRect

        #region Slider

        private sealed class TweenData_Slider_Value : ITweenData
        {
            private Slider m_Slider;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Slider_Value(Slider inSlider, float inTarget)
            {
                m_Slider = inSlider;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Slider.value;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Slider.value = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Slider: Value";
            }
        }

        private sealed class TweenData_Slider_NormalizedValue : ITweenData
        {
            private Slider m_Slider;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Slider_NormalizedValue(Slider inSlider, float inTarget)
            {
                m_Slider = inSlider;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Slider.normalizedValue;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Slider.normalizedValue = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Slider: Normalized Value";
            }
        }

        /// <summary>
        /// Changes the Slider's value to another value over time.
        /// </summary>
        static public Tween ValueTo(this Slider inSlider, float inValue, float inTime)
        {
            return Tween.Create(new TweenData_Slider_Value(inSlider, inValue), inTime);
        }

        /// <summary>
        /// Changes the Slider's value to another value over time.
        /// </summary>
        static public Tween ValueTo(this Slider inSlider, float inValue, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Slider_Value(inSlider, inValue), inSettings);
        }

        /// <summary>
        /// Changes the Slider's normalized value to another value over time.
        /// </summary>
        static public Tween NormalizedValueTo(this Slider inSlider, float inNormalizedValue, float inTime)
        {
            return Tween.Create(new TweenData_Slider_NormalizedValue(inSlider, inNormalizedValue), inTime);
        }

        /// <summary>
        /// Changes the Slider's normalized value to another value over time.
        /// </summary>
        static public Tween NormalizedValueTo(this Slider inSlider, float inNormalizedValue, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Slider_NormalizedValue(inSlider, inNormalizedValue), inSettings);
        }

        #endregion // Slider

        #region Scrollbar

        private sealed class TweenData_Scrollbar_Value : ITweenData
        {
            private Scrollbar m_Scrollbar;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Scrollbar_Value(Scrollbar inScrollbar, float inTarget)
            {
                m_Scrollbar = inScrollbar;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Scrollbar.value;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Scrollbar.value = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Scrollbar: Value";
            }
        }

        private sealed class TweenData_Scrollbar_Size : ITweenData
        {
            private Scrollbar m_Scrollbar;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Scrollbar_Size(Scrollbar inScrollbar, float inTarget)
            {
                m_Scrollbar = inScrollbar;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Scrollbar.size;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Scrollbar.size = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Scrollbar: Size";
            }
        }

        /// <summary>
        /// Changes the Scrollbar's value to another value over time.
        /// </summary>
        static public Tween ValueTo(this Scrollbar inScrollbar, float inValue, float inTime)
        {
            return Tween.Create(new TweenData_Scrollbar_Value(inScrollbar, inValue), inTime);
        }

        /// <summary>
        /// Changes the Scrollbar's value to another value over time.
        /// </summary>
        static public Tween ValueTo(this Scrollbar inScrollbar, float inValue, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Scrollbar_Value(inScrollbar, inValue), inSettings);
        }

        /// <summary>
        /// Changes the Scrollbar's size to another size over time.
        /// </summary>
        static public Tween SizeTo(this Scrollbar inScrollbar, float inSize, float inTime)
        {
            return Tween.Create(new TweenData_Scrollbar_Size(inScrollbar, inSize), inTime);
        }

        /// <summary>
        /// Changes the Scrollbar's size to another size over time.
        /// </summary>
        static public Tween SizeTo(this Scrollbar inScrollbar, float inSize, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Scrollbar_Size(inScrollbar, inSize), inSettings);
        }

        #endregion // Scrollbar
    }
}

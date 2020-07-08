/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenShortcuts.Renderer.cs
 * Purpose: Extension methods for creating Tweens affecting world renderers.
*/

using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating tweens.
    /// </summary>
    static public partial class TweenShortcuts
    {
        #region SpriteRenderer

        private sealed class TweenData_SpriteRenderer_Alpha : ITweenData
        {
            private SpriteRenderer m_Renderer;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_SpriteRenderer_Alpha(SpriteRenderer inRenderer, float inTarget)
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
                return "SpriteRenderer: Alpha";
            }
        }

        private sealed class TweenData_SpriteRenderer_Color : ITweenData
        {
            private SpriteRenderer m_Renderer;
            private Color m_Target;
            private ColorUpdate m_Update;

            private Color m_Start;

            public TweenData_SpriteRenderer_Color(SpriteRenderer inRenderer, Color inTarget, ColorUpdate inUpdate)
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
                return "SpriteRenderer: Color";
            }
        }

        private sealed class TweenData_SpriteRenderer_Gradient : ITweenData
        {
            private SpriteRenderer m_Renderer;
            private Gradient m_Gradient;
            private ColorUpdate m_Update;

            public TweenData_SpriteRenderer_Gradient(SpriteRenderer inRenderer, Gradient inTarget, ColorUpdate inUpdate)
            {
                m_Renderer = inRenderer;
                m_Gradient = inTarget;
                m_Update = inUpdate;
            }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Gradient.Evaluate(inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Renderer.color.a;
                m_Renderer.color = final;
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "SpriteRenderer: Gradient";
            }
        }

        /// <summary>
        /// Fades the SpriteRenderer to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this SpriteRenderer inRenderer, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_SpriteRenderer_Alpha(inRenderer, inAlpha), inTime);
        }

        /// <summary>
        /// Fades the SpriteRenderer to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this SpriteRenderer inRenderer, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_SpriteRenderer_Alpha(inRenderer, inAlpha), inSettings);
        }

        /// <summary>
        /// Fades the SpriteRenderer to another color over time.
        /// </summary>
        static public Tween ColorTo(this SpriteRenderer inRenderer, Color inTarget, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_SpriteRenderer_Color(inRenderer, inTarget, inUpdate), inTime);
        }

        /// <summary>
        /// Fades the SpriteRenderer to another color over time.
        /// </summary>
        static public Tween ColorTo(this SpriteRenderer inRenderer, Color inTarget, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_SpriteRenderer_Color(inRenderer, inTarget, inUpdate), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to the SpriteRenderer over time.
        /// </summary>
        static public Tween Gradient(this SpriteRenderer inRenderer, Gradient inGradient, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_SpriteRenderer_Gradient(inRenderer, inGradient, inUpdate), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to the SpriteRenderer over time.
        /// </summary>
        static public Tween Gradient(this SpriteRenderer inRenderer, Gradient inGradient, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_SpriteRenderer_Gradient(inRenderer, inGradient, inUpdate), inSettings);
        }

        #endregion // SpriteRenderer

        #region TextMesh

        private sealed class TweenData_TextMesh_Alpha : ITweenData
        {
            private TextMesh m_Renderer;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_TextMesh_Alpha(TextMesh inRenderer, float inTarget)
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
                return "TextMesh: Alpha";
            }
        }

        private sealed class TweenData_TextMesh_Color : ITweenData
        {
            private TextMesh m_Renderer;
            private Color m_Target;
            private ColorUpdate m_Update;

            private Color m_Start;

            public TweenData_TextMesh_Color(TextMesh inRenderer, Color inTarget, ColorUpdate inUpdate)
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
                return "TextMesh: Color";
            }
        }

        private sealed class TweenData_TextMesh_Gradient : ITweenData
        {
            private TextMesh m_Renderer;
            private Gradient m_Gradient;
            private ColorUpdate m_Update;

            public TweenData_TextMesh_Gradient(TextMesh inRenderer, Gradient inTarget, ColorUpdate inUpdate)
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
                return "TextMesh: Gradient";
            }
        }

        /// <summary>
        /// Fades the TextMesh to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this TextMesh inRenderer, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_TextMesh_Alpha(inRenderer, inAlpha), inTime);
        }

        /// <summary>
        /// Fades the TextMesh to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this TextMesh inRenderer, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_TextMesh_Alpha(inRenderer, inAlpha), inSettings);
        }

        /// <summary>
        /// Fades the TextMesh to another color over time.
        /// </summary>
        static public Tween ColorTo(this TextMesh inRenderer, Color inTarget, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_TextMesh_Color(inRenderer, inTarget, inUpdate), inTime);
        }

        /// <summary>
        /// Fades the TextMesh to another color over time.
        /// </summary>
        static public Tween ColorTo(this TextMesh inRenderer, Color inTarget, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_TextMesh_Color(inRenderer, inTarget, inUpdate), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to the TextMesh over time.
        /// </summary>
        static public Tween Gradient(this TextMesh inRenderer, Gradient inGradient, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_TextMesh_Gradient(inRenderer, inGradient, inUpdate), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to the TextMesh over time.
        /// </summary>
        static public Tween Gradient(this TextMesh inRenderer, Gradient inGradient, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_TextMesh_Gradient(inRenderer, inGradient, inUpdate), inSettings);
        }

        #endregion // TextMesh

        #region SkinnedMeshRenderer

        private sealed class TweenData_SkinnedMeshRenderer_BlendWeight : ITweenData
        {
            private SkinnedMeshRenderer m_Renderer;
            private int m_Index;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_SkinnedMeshRenderer_BlendWeight(SkinnedMeshRenderer inRenderer, int inIndex, float inTarget)
            {
                m_Renderer = inRenderer;
                m_Index = inIndex;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.GetBlendShapeWeight(m_Index);
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Renderer.SetBlendShapeWeight(m_Index, m_Start + m_Delta * inPercent);
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "SkinnedMeshRenderer: Blend Shape Weight";
            }
        }

        /// <summary>
        /// Tweens the weight for a blend shape on a SkinnedMeshRenderer to another value over time.
        /// </summary>
        static public Tween BlendShapeTo(this SkinnedMeshRenderer inRenderer, int inIndex, float inTarget, float inTime)
        {
            return Tween.Create(new TweenData_SkinnedMeshRenderer_BlendWeight(inRenderer, inIndex, inTarget), inTime);
        }

        /// <summary>
        /// Tweens the weight for a blend shape on a SkinnedMeshRenderer to another value over time.
        /// </summary>
        static public Tween BlendShapeTo(this SkinnedMeshRenderer inRenderer, int inIndex, float inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_SkinnedMeshRenderer_BlendWeight(inRenderer, inIndex, inTarget), inSettings);
        }

        #endregion // SkinnedMeshRenderer
    }
}

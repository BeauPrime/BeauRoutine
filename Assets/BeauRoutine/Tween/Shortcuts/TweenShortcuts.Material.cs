/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenShortcuts.Material.cs
 * Purpose: Extension methods for creating Tweens affecting
 *          properties on a Material.
 *          
 * Notes:   Ensure you're not accidentally leaking a cloned
 *          material when you use these!
*/

using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating tweens.
    /// </summary>
    static public partial class TweenShortcuts
    {
        #region Color/Alpha

        private sealed class TweenData_Material_Alpha : ITweenData
        {
            private Material m_Material;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Material_Alpha(Material inMaterial, float inTarget)
            {
                m_Material = inMaterial;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Material.color.a;
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Material.color;
                final.a = m_Start + m_Delta * inPercent;
                m_Material.color = final;
            }

            public override string ToString()
            {
                return "Material: Alpha";
            }
        }

        private sealed class TweenData_Material_Color : ITweenData
        {
            private Material m_Material;
            private Color m_Target;
            private ColorUpdate m_Update;

            private Color m_Start;

            public TweenData_Material_Color(Material inMaterial, Color inTarget, ColorUpdate inUpdate)
            {
                m_Material = inMaterial;
                m_Target = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart()
            {
                m_Start = m_Material.color;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = UnityEngine.Color.LerpUnclamped(m_Start, m_Target, inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Material.color.a;
                m_Material.color = final;
            }

            public override string ToString()
            {
                return "Material: Color";
            }
        }

        private sealed class TweenData_Material_Gradient : ITweenData
        {
            private Material m_Material;
            private Gradient m_Gradient;
            private ColorUpdate m_Update;

            public TweenData_Material_Gradient(Material inMaterial, Gradient inTarget, ColorUpdate inUpdate)
            {
                m_Material = inMaterial;
                m_Gradient = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Gradient.Evaluate(inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Material.color.a;
                m_Material.color = final;
            }

            public override string ToString()
            {
                return "Material: Gradient";
            }
        }

        /// <summary>
        /// Fades the Material to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Material inMaterial, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_Material_Alpha(inMaterial, inAlpha), inTime);
        }

        /// <summary>
        /// Fades the Material to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Material inMaterial, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Material_Alpha(inMaterial, inAlpha), inSettings);
        }

        /// <summary>
        /// Fades the Material to another color over time.
        /// </summary>
        static public Tween ColorTo(this Material inMaterial, Color inTarget, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_Color(inMaterial, inTarget, inUpdate), inTime);
        }

        /// <summary>
        /// Fades the Material to another color over time.
        /// </summary>
        static public Tween ColorTo(this Material inMaterial, Color inTarget, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_Color(inMaterial, inTarget, inUpdate), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to the Material over time.
        /// </summary>
        static public Tween Gradient(this Material inMaterial, Gradient inGradient, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_Gradient(inMaterial, inGradient, inUpdate), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to the Material over time.
        /// </summary>
        static public Tween Gradient(this Material inMaterial, Gradient inGradient, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_Gradient(inMaterial, inGradient, inUpdate), inSettings);
        }

        #endregion
    }
}

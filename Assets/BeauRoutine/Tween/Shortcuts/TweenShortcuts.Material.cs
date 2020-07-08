/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
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
        #region Alpha

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

        private sealed class TweenData_Material_AlphaProperty : ITweenData
        {
            private Material m_Material;
            private int m_Property;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Material_AlphaProperty(Material inMaterial, int inProperty, float inTarget)
            {
                m_Material = inMaterial;
                m_Property = inProperty;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Material.GetColor(m_Property).a;
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Material.GetColor(m_Property);
                final.a = m_Start + m_Delta * inPercent;
                m_Material.SetColor(m_Property, final);
            }

            public override string ToString()
            {
                return "Material: Alpha (Property)";
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
        /// Fades a Material color property to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Material inMaterial, string inProperty, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_Material_AlphaProperty(inMaterial, Shader.PropertyToID(inProperty), inAlpha), inTime);
        }

        /// <summary>
        /// Fades a Material color property to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Material inMaterial, string inProperty, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Material_AlphaProperty(inMaterial, Shader.PropertyToID(inProperty), inAlpha), inSettings);
        }

        /// <summary>
        /// Fades a Material color property to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Material inMaterial, int inPropertyID, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_Material_AlphaProperty(inMaterial, inPropertyID, inAlpha), inTime);
        }

        /// <summary>
        /// Fades a Material color property to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Material inMaterial, int inPropertyID, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Material_AlphaProperty(inMaterial, inPropertyID, inAlpha), inSettings);
        }

        #endregion //Alpha
    
        #region Color

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

        private sealed class TweenData_Material_ColorProperty : ITweenData
        {
            private Material m_Material;
            private int m_Property;
            private Color m_Target;
            private ColorUpdate m_Update;

            private Color m_Start;

            public TweenData_Material_ColorProperty(Material inMaterial, int inProperty, Color inTarget, ColorUpdate inUpdate)
            {
                m_Material = inMaterial;
                m_Property = inProperty;
                m_Target = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart()
            {
                m_Start = m_Material.GetColor(m_Property);
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = UnityEngine.Color.LerpUnclamped(m_Start, m_Target, inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Material.GetColor(m_Property).a;
                m_Material.color = final;
            }

            public override string ToString()
            {
                return "Material: Color (Property)";
            }
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
        /// Fades a Material color property to another color over time.
        /// </summary>
        static public Tween ColorTo(this Material inMaterial, string inProperty, Color inTarget, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_ColorProperty(inMaterial, Shader.PropertyToID(inProperty), inTarget, inUpdate), inTime);
        }

        /// <summary>
        /// Fades a Material color property to another color over time
        /// </summary>
        static public Tween ColorTo(this Material inMaterial, string inProperty, Color inTarget, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_ColorProperty(inMaterial, Shader.PropertyToID(inProperty), inTarget, inUpdate), inSettings);
        }

        /// <summary>
        /// Fades a Material color property to another color over time.
        /// </summary>
        static public Tween ColorTo(this Material inMaterial, int inPropertyID, Color inTarget, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_ColorProperty(inMaterial, inPropertyID, inTarget, inUpdate), inTime);
        }

        /// <summary>
        /// Fades a Material color property to another color over time
        /// </summary>
        static public Tween ColorTo(this Material inMaterial, int inPropertyID, Color inTarget, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_ColorProperty(inMaterial, inPropertyID, inTarget, inUpdate), inSettings);
        }

        #endregion // Color

        #region Gradient

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

        private sealed class TweenData_Material_GradientProperty : ITweenData
        {
            private Material m_Material;
            private int m_Property;
            private Gradient m_Gradient;
            private ColorUpdate m_Update;

            public TweenData_Material_GradientProperty(Material inMaterial, int inProperty, Gradient inTarget, ColorUpdate inUpdate)
            {
                m_Material = inMaterial;
                m_Property = inProperty;
                m_Gradient = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Gradient.Evaluate(inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Material.GetColor(m_Property).a;
                m_Material.SetColor(m_Property, final);
            }

            public override string ToString()
            {
                return "Material: Gradient (Property)";
            }
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

        /// <summary>
        /// Applies a gradient of colors to a Material color property over time.
        /// </summary>
        static public Tween Gradient(this Material inMaterial, string inProperty, Gradient inGradient, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_GradientProperty(inMaterial, Shader.PropertyToID(inProperty), inGradient, inUpdate), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to a Material color property over time.
        /// </summary>
        static public Tween Gradient(this Material inMaterial, string inProperty, Gradient inGradient, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_GradientProperty(inMaterial, Shader.PropertyToID(inProperty), inGradient, inUpdate), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to a Material color property over time.
        /// </summary>
        static public Tween Gradient(this Material inMaterial, int inPropertyID, Gradient inGradient, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_GradientProperty(inMaterial, inPropertyID, inGradient, inUpdate), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to a Material color property over time.
        /// </summary>
        static public Tween Gradient(this Material inMaterial, int inPropertyID, Gradient inGradient, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Material_GradientProperty(inMaterial, inPropertyID, inGradient, inUpdate), inSettings);
        }

        #endregion // Gradient

        #region Float

        private sealed class TweenData_Material_FloatProperty : ITweenData
        {
            private Material m_Material;
            private int m_Property;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Material_FloatProperty(Material inMaterial, int inProperty, float inTarget)
            {
                m_Material = inMaterial;
                m_Property = inProperty;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Material.GetFloat(m_Property);
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Material.SetFloat(m_Property, m_Start + m_Delta * inPercent);
            }

            public override string ToString()
            {
                return "Material: Float (Property)";
            }
        }

        /// <summary>
        /// Fades a Material float property to another value over time.
        /// </summary>
        static public Tween FloatTo(this Material inMaterial, string inProperty, float inValue, float inTime)
        {
            return Tween.Create(new TweenData_Material_FloatProperty(inMaterial, Shader.PropertyToID(inProperty), inValue), inTime);
        }

        /// <summary>
        /// Fades a Material float property to another value over time.
        /// </summary>
        static public Tween FloatTo(this Material inMaterial, string inProperty, float inValue, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Material_FloatProperty(inMaterial, Shader.PropertyToID(inProperty), inValue), inSettings);
        }

        /// <summary>
        /// Fades a Material float property to another value over time.
        /// </summary>
        static public Tween FloatTo(this Material inMaterial, int inPropertyID, float inValue, float inTime)
        {
            return Tween.Create(new TweenData_Material_FloatProperty(inMaterial, inPropertyID, inValue), inTime);
        }

        /// <summary>
        /// Fades a Material float property to another value over time.
        /// </summary>
        static public Tween FloatTo(this Material inMaterial, int inPropertyID, float inValue, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Material_FloatProperty(inMaterial, inPropertyID, inValue), inSettings);
        }

        #endregion // Float

        #region Vector

        private sealed class TweenData_Material_VectorProperty : ITweenData
        {
            private Material m_Material;
            private int m_Property;
            private Vector4 m_Target;
            private Axis m_Axis;

            private Vector4 m_Start;
            private Vector4 m_Delta;

            public TweenData_Material_VectorProperty(Material inMaterial, int inProperty, Vector4 inTarget, Axis inAxis)
            {
                m_Material = inMaterial;
                m_Property = inProperty;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Material.GetVector(m_Property);
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector4 final = new Vector4(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent,
                    m_Start.z + m_Delta.z * inPercent,
                    m_Start.w + m_Delta.w * inPercent
                );
                if ((m_Axis & Axis.XYZW) != Axis.XYZW)
                {
                    Vector4 currentValue = m_Material.GetVector(m_Property);
                    VectorUtil.CopyFrom(ref currentValue, final, m_Axis);
                    final = currentValue;
                }
                m_Material.SetVector(m_Property, final);
            }

            public override string ToString()
            {
                return "Material: Vector (Property)";
            }
        }

        /// <summary>
        /// Fades a Material vector property to another value over time.
        /// </summary>
        static public Tween VectorTo(this Material inMaterial, string inProperty, Vector4 inValue, float inTime, Axis inAxis = Axis.XYZW)
        {
            return Tween.Create(new TweenData_Material_VectorProperty(inMaterial, Shader.PropertyToID(inProperty), inValue, inAxis), inTime);
        }

        /// <summary>
        /// Fades a Material vector property to another value over time.
        /// </summary>
        static public Tween VectorTo(this Material inMaterial, string inProperty, Vector4 inValue, TweenSettings inSettings, Axis inAxis = Axis.XYZW)
        {
            return Tween.Create(new TweenData_Material_VectorProperty(inMaterial, Shader.PropertyToID(inProperty), inValue, inAxis), inSettings);
        }

        /// <summary>
        /// Fades a Material vector property to another value over time.
        /// </summary>
        static public Tween VectorTo(this Material inMaterial, int inPropertyID, Vector4 inValue, float inTime, Axis inAxis = Axis.XYZW)
        {
            return Tween.Create(new TweenData_Material_VectorProperty(inMaterial, inPropertyID, inValue, inAxis), inTime);
        }

        /// <summary>
        /// Fades a Material vector property to another value over time.
        /// </summary>
        static public Tween VectorTo(this Material inMaterial, int inPropertyID, Vector4 inValue, TweenSettings inSettings, Axis inAxis = Axis.XYZW)
        {
            return Tween.Create(new TweenData_Material_VectorProperty(inMaterial, inPropertyID, inValue, inAxis), inSettings);
        }

        #endregion // Vector
    }
}
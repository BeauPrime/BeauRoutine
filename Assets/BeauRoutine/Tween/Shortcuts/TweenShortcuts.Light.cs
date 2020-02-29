/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    23 May 2018
 * 
 * File:    TweenShortcuts.Light.cs
 * Purpose: Extension methods for creating Tweens affecting Light objects.
*/

using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating tweens.
    /// </summary>
    static public partial class TweenShortcuts
    {
        #region Light-Specific

        private sealed class TweenData_Light_Range : ITweenData
        {
            private Light m_Light;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Light_Range(Light inLight, float inTarget)
            {
                m_Light = inLight;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Light.range;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Light.range = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Light: Range";
            }
        }

        private sealed class TweenData_Light_Intensity : ITweenData
        {
            private Light m_Light;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Light_Intensity(Light inLight, float inTarget)
            {
                m_Light = inLight;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Light.intensity;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Light.intensity = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Light: Intensity";
            }
        }

        private sealed class TweenData_Light_SpotAngle : ITweenData
        {
            private Light m_Light;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Light_SpotAngle(Light inLight, float inTarget)
            {
                m_Light = inLight;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Light.spotAngle;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Light.spotAngle = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Light: Spot Angle";
            }
        }

        private sealed class TweenData_Light_ShadowStrength : ITweenData
        {
            private Light m_Light;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Light_ShadowStrength(Light inLight, float inTarget)
            {
                m_Light = inLight;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Light.shadowStrength;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Light.shadowStrength = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Light: Shadow Strength";
            }
        }

        /// <summary>
        /// Tweens the Light's range to another value over time.
        /// </summary>
        static public Tween RangeTo(this Light inLight, float inRange, float inTime)
        {
            return Tween.Create(new TweenData_Light_Range(inLight, inRange), inTime);
        }

        /// <summary>
        /// Tweens the Light's range to another value over time.
        /// </summary>
        static public Tween RangeTo(this Light inLight, float inRange, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Light_Range(inLight, inRange), inSettings);
        }

        /// <summary>
        /// Tweens the Light's intensity to another value over time.
        /// </summary>
        static public Tween IntensityTo(this Light inLight, float inIntensity, float inTime)
        {
            return Tween.Create(new TweenData_Light_Intensity(inLight, inIntensity), inTime);
        }

        /// <summary>
        /// Tweens the Light's intensity to another value over time.
        /// </summary>
        static public Tween IntensityTo(this Light inLight, float inIntensity, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Light_Intensity(inLight, inIntensity), inSettings);
        }

        /// <summary>
        /// Tweens the Light's spotlight angle to another value over time.
        /// </summary>
        static public Tween SpotAngleTo(this Light inLight, float inSpotAngle, float inTime)
        {
            return Tween.Create(new TweenData_Light_SpotAngle(inLight, inSpotAngle), inTime);
        }

        /// <summary>
        /// Tweens the Light's spotlight angle to another value over time.
        /// </summary>
        static public Tween SpotAngleTo(this Light inLight, float inSpotAngle, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Light_SpotAngle(inLight, inSpotAngle), inSettings);
        }

        /// <summary>
        /// Tweens the Light's shadow strength to another value over time.
        /// </summary>
        static public Tween ShadowStrengthTo(this Light inLight, float inShadowStrength, float inTime)
        {
            return Tween.Create(new TweenData_Light_ShadowStrength(inLight, inShadowStrength), inTime);
        }

        /// <summary>
        /// Tweens the Light's shadow strength to another value over time.
        /// </summary>
        static public Tween ShadowStrengthTo(this Light inLight, float inShadowStrength, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Light_ShadowStrength(inLight, inShadowStrength), inSettings);
        }

        #endregion // Light-specific

        #region Colors

        private sealed class TweenData_Light_Color : ITweenData
        {
            private Light m_Light;
            private Color m_Target;

            private Color m_Start;

            public TweenData_Light_Color(Light inLight, Color inTarget)
            {
                m_Light = inLight;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Light.color;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Light.color = UnityEngine.Color.LerpUnclamped(m_Start, m_Target, inPercent);
            }

            public override string ToString()
            {
                return "Light: Color";
            }
        }

        private sealed class TweenData_Light_Gradient : ITweenData
        {
            private Light m_Light;
            private Gradient m_Gradient;

            public TweenData_Light_Gradient(Light inLight, Gradient inTarget)
            {
                m_Light = inLight;
                m_Gradient = inTarget;
            }

            public void ApplyTween(float inPercent)
            {
                m_Light.color = m_Gradient.Evaluate(inPercent);
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Light: Gradient";
            }
        }

        /// <summary>
        /// Fades the Light to another color over time.
        /// </summary>
        static public Tween ColorTo(this Light inLight, Color inTarget, float inTime)
        {
            return Tween.Create(new TweenData_Light_Color(inLight, inTarget), inTime);
        }

        /// <summary>
        /// Fades the Light to another color over time.
        /// </summary>
        static public Tween ColorTo(this Light inLight, Color inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Light_Color(inLight, inTarget), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to the Light over time.
        /// </summary>
        static public Tween Gradient(this Light inLight, Gradient inGradient, float inTime)
        {
            return Tween.Create(new TweenData_Light_Gradient(inLight, inGradient), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to the Light over time.
        /// </summary>
        static public Tween Gradient(this Light inLight, Gradient inGradient, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Light_Gradient(inLight, inGradient), inSettings);
        }

        #endregion // Colors
    }
}
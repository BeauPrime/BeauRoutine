/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
                return "Light: Spotlight Angle";
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

        private sealed class TweenData_Light_Alpha : ITweenData
        {
            private Light m_Light;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Light_Alpha(Light inLight, float inTarget)
            {
                m_Light = inLight;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Light.color.a;
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Light.color;
                final.a = m_Start + m_Delta * inPercent;
                m_Light.color = final;
            }

            public override string ToString()
            {
                return "Light: Alpha";
            }
        }

        private sealed class TweenData_Light_Color : ITweenData
        {
            private Light m_Light;
            private Color m_Target;
            private ColorUpdate m_Update;

            private Color m_Start;

            public TweenData_Light_Color(Light inLight, Color inTarget, ColorUpdate inUpdate)
            {
                m_Light = inLight;
                m_Target = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart()
            {
                m_Start = m_Light.color;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = UnityEngine.Color.LerpUnclamped(m_Start, m_Target, inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Light.color.a;
                m_Light.color = final;
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
            private ColorUpdate m_Update;

            public TweenData_Light_Gradient(Light inLight, Gradient inTarget, ColorUpdate inUpdate)
            {
                m_Light = inLight;
                m_Gradient = inTarget;
                m_Update = inUpdate;
            }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Gradient.Evaluate(inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Light.color.a;
                m_Light.color = final;
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Light: Gradient";
            }
        }

        /// <summary>
        /// Fades the Light to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Light inLight, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_Light_Alpha(inLight, inAlpha), inTime);
        }

        /// <summary>
        /// Fades the Light to another alpha over time.
        /// </summary>
        static public Tween FadeTo(this Light inLight, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Light_Alpha(inLight, inAlpha), inSettings);
        }

        /// <summary>
        /// Fades the Light to another color over time.
        /// </summary>
        static public Tween ColorTo(this Light inLight, Color inTarget, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Light_Color(inLight, inTarget, inUpdate), inTime);
        }

        /// <summary>
        /// Fades the Light to another color over time.
        /// </summary>
        static public Tween ColorTo(this Light inLight, Color inTarget, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Light_Color(inLight, inTarget, inUpdate), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to the Light over time.
        /// </summary>
        static public Tween Gradient(this Light inLight, Gradient inGradient, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Light_Gradient(inLight, inGradient, inUpdate), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to the Light over time.
        /// </summary>
        static public Tween Gradient(this Light inLight, Gradient inGradient, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Light_Gradient(inLight, inGradient, inUpdate), inSettings);
        }

        #endregion // Colors
    }
}
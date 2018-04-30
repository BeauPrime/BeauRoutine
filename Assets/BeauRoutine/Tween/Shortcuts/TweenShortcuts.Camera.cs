/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenShortcuts.Camera.cs
 * Purpose: Extension methods for creating Tweens affecting
 *          properties on a Camera.
*/

using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating tweens.
    /// </summary>
    static public partial class TweenShortcuts
    {
        #region Orthographic Size

        private sealed class TweenData_Camera_OrthographicSize : ITweenData
        {
            private Camera m_Camera;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Camera_OrthographicSize(Camera inCamera, float inTarget)
            {
                m_Camera = inCamera;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Camera.orthographicSize;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Camera.orthographicSize = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Camera: Orthographic Size";
            }
        }

        /// <summary>
        /// Changes the orthographic size of the camera over time.
        /// </summary>
        static public Tween OrthoSizeTo(this Camera inCamera, float inTarget, float inTime)
        {
            return Tween.Create(new TweenData_Camera_OrthographicSize(inCamera, inTarget), inTime);
        }

        /// <summary>
        /// Changes the orthographic size of the camera over time.
        /// </summary>
        static public Tween OrthoSizeTo(this Camera inCamera, float inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Camera_OrthographicSize(inCamera, inTarget), inSettings);
        }

        #endregion // Orthographic Size

        #region Field of View

        private sealed class TweenData_Camera_FOV : ITweenData
        {
            private Camera m_Camera;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Camera_FOV(Camera inCamera, float inTarget)
            {
                m_Camera = inCamera;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Camera.fieldOfView;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Camera.fieldOfView = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "Camera: FOV";
            }
        }

        /// <summary>
        /// Changes the FOV of the camera over time.
        /// </summary>
        static public Tween FieldOfViewTo(this Camera inCamera, float inTarget, float inTime)
        {
            return Tween.Create(new TweenData_Camera_FOV(inCamera, inTarget), inTime);
        }

        /// <summary>
        /// Changes the FOV of the camera over time.
        /// </summary>
        static public Tween FieldOfViewTo(this Camera inCamera, float inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Camera_FOV(inCamera, inTarget), inSettings);
        }

        #endregion // Field of view
    
        #region Background Color

        private sealed class TweenData_Camera_Alpha : ITweenData
        {
            private Camera m_Renderer;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Camera_Alpha(Camera inRenderer, float inTarget)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.backgroundColor.a;
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = m_Renderer.backgroundColor;
                final.a = m_Start + m_Delta * inPercent;
                m_Renderer.backgroundColor = final;
            }

            public override string ToString()
            {
                return "Camera: Background Color Alpha";
            }
        }

        private sealed class TweenData_Camera_Color : ITweenData
        {
            private Camera m_Renderer;
            private Color m_Target;
            private ColorUpdate m_Update;

            private Color m_Start;

            public TweenData_Camera_Color(Camera inRenderer, Color inTarget, ColorUpdate inUpdate)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
                m_Update = inUpdate;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.backgroundColor;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Color final = UnityEngine.Color.LerpUnclamped(m_Start, m_Target, inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Renderer.backgroundColor.a;
                m_Renderer.backgroundColor = final;
            }

            public override string ToString()
            {
                return "Camera: Background Color";
            }
        }

        private sealed class TweenData_Camera_Gradient : ITweenData
        {
            private Camera m_Renderer;
            private Gradient m_Gradient;
            private ColorUpdate m_Update;

            public TweenData_Camera_Gradient(Camera inRenderer, Gradient inTarget, ColorUpdate inUpdate)
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
                    final.a = m_Renderer.backgroundColor.a;
                m_Renderer.backgroundColor = final;
            }

            public override string ToString()
            {
                return "Camera: Background Color Gradient";
            }
        }

        /// <summary>
        /// Fades the Camera to another alpha over time.
        /// </summary>
        static public Tween BackgroundFadeTo(this Camera inRenderer, float inAlpha, float inTime)
        {
            return Tween.Create(new TweenData_Camera_Alpha(inRenderer, inAlpha), inTime);
        }

        /// <summary>
        /// Fades the Camera to another alpha over time.
        /// </summary>
        static public Tween BackgroundFadeTo(this Camera inRenderer, float inAlpha, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Camera_Alpha(inRenderer, inAlpha), inSettings);
        }

        /// <summary>
        /// Fades the Camera to another color over time.
        /// </summary>
        static public Tween BackgroundColorTo(this Camera inRenderer, Color inTarget, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Camera_Color(inRenderer, inTarget, inUpdate), inTime);
        }

        /// <summary>
        /// Fades the Camera to another color over time.
        /// </summary>
        static public Tween BackgroundColorTo(this Camera inRenderer, Color inTarget, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Camera_Color(inRenderer, inTarget, inUpdate), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to the Camera over time.
        /// </summary>
        static public Tween BackgroundGradient(this Camera inRenderer, Gradient inGradient, float inTime, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Camera_Gradient(inRenderer, inGradient, inUpdate), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to the Camera over time.
        /// </summary>
        static public Tween BackgroundGradient(this Camera inRenderer, Gradient inGradient, TweenSettings inSettings, ColorUpdate inUpdate = ColorUpdate.PreserveAlpha)
        {
            return Tween.Create(new TweenData_Camera_Gradient(inRenderer, inGradient, inUpdate), inSettings);
        }

        #endregion // Background Color
    }
}

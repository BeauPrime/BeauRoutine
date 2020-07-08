/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
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

        private sealed class TweenData_Camera_Color : ITweenData
        {
            private Camera m_Renderer;
            private Color m_Target;

            private Color m_Start;

            public TweenData_Camera_Color(Camera inRenderer, Color inTarget)
            {
                m_Renderer = inRenderer;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Renderer.backgroundColor;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Renderer.backgroundColor = UnityEngine.Color.LerpUnclamped(m_Start, m_Target, inPercent);
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

            public TweenData_Camera_Gradient(Camera inRenderer, Gradient inTarget)
            {
                m_Renderer = inRenderer;
                m_Gradient = inTarget;
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Renderer.backgroundColor = m_Gradient.Evaluate(inPercent);
            }

            public override string ToString()
            {
                return "Camera: Background Color Gradient";
            }
        }

        /// <summary>
        /// Fades the Camera to another color over time.
        /// </summary>
        static public Tween BackgroundColorTo(this Camera inRenderer, Color inTarget, float inTime)
        {
            return Tween.Create(new TweenData_Camera_Color(inRenderer, inTarget), inTime);
        }

        /// <summary>
        /// Fades the Camera to another color over time.
        /// </summary>
        static public Tween BackgroundColorTo(this Camera inRenderer, Color inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Camera_Color(inRenderer, inTarget), inSettings);
        }

        /// <summary>
        /// Applies a gradient of colors to the Camera over time.
        /// </summary>
        static public Tween BackgroundGradient(this Camera inRenderer, Gradient inGradient, float inTime)
        {
            return Tween.Create(new TweenData_Camera_Gradient(inRenderer, inGradient), inTime);
        }

        /// <summary>
        /// Applies a gradient of colors to the Camera over time.
        /// </summary>
        static public Tween BackgroundGradient(this Camera inRenderer, Gradient inGradient, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Camera_Gradient(inRenderer, inGradient), inSettings);
        }

        #endregion // Background Color
    }
}

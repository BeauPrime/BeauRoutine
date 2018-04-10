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

        #endregion

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

        #endregion
    }
}

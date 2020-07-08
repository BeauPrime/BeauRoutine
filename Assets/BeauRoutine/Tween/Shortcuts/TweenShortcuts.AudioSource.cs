/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenShortcuts.AudioSource.cs
 * Purpose: Extension methods for creating Tweens affecting
 *          properties on an AudioSource.
*/

using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating tweens.
    /// </summary>
    static public partial class TweenShortcuts
    {
        #region Volume

        private sealed class TweenData_AudioSource_Volume : ITweenData
        {
            private AudioSource m_Audio;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_AudioSource_Volume(AudioSource inSource, float inTarget)
            {
                m_Audio = inSource;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Audio.volume;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Audio.volume = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "AudioSource: Volume";
            }
        }

        /// <summary>
        /// Changes the volume of the AudioSource over time.
        /// </summary>
        static public Tween VolumeTo(this AudioSource inSource, float inTarget, float inTime)
        {
            return Tween.Create(new TweenData_AudioSource_Volume(inSource, inTarget), inTime);
        }

        /// <summary>
        /// Changes the volume of the AudioSource over time.
        /// </summary>
        static public Tween VolumeTo(this AudioSource inSource, float inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_AudioSource_Volume(inSource, inTarget), inSettings);
        }

        #endregion

        #region Pitch

        private sealed class TweenData_AudioSource_Pitch : ITweenData
        {
            private AudioSource m_Audio;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_AudioSource_Pitch(AudioSource inSource, float inTarget)
            {
                m_Audio = inSource;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Audio.pitch;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Audio.pitch = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "AudioSource: Pitch";
            }
        }

        /// <summary>
        /// Changes the pitch of the AudioSource over time.
        /// </summary>
        static public Tween PitchTo(this AudioSource inSource, float inTarget, float inTime)
        {
            return Tween.Create(new TweenData_AudioSource_Pitch(inSource, inTarget), inTime);
        }

        /// <summary>
        /// Changes the pitch of the AudioSource over time.
        /// </summary>
        static public Tween PitchTo(this AudioSource inSource, float inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_AudioSource_Pitch(inSource, inTarget), inSettings);
        }

        #endregion

        #region Pan

        private sealed class TweenData_AudioSource_Pan : ITweenData
        {
            private AudioSource m_Audio;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_AudioSource_Pan(AudioSource inSource, float inTarget)
            {
                m_Audio = inSource;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Audio.panStereo;
                m_Delta = m_Target - m_Start;
            }

            public void ApplyTween(float inPercent)
            {
                m_Audio.panStereo = m_Start + m_Delta * inPercent;
            }

            public void OnTweenEnd() { }

            public override string ToString()
            {
                return "AudioSource: Pan";
            }
        }

        /// <summary>
        /// Changes the panning of the AudioSource over time.
        /// </summary>
        static public Tween PanTo(this AudioSource inSource, float inTarget, float inTime)
        {
            return Tween.Create(new TweenData_AudioSource_Pan(inSource, inTarget), inTime);
        }

        /// <summary>
        /// Changes the panning of the AudioSource over time.
        /// </summary>
        static public Tween PanTo(this AudioSource inSource, float inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_AudioSource_Pan(inSource, inTarget), inSettings);
        }

        #endregion
    }
}

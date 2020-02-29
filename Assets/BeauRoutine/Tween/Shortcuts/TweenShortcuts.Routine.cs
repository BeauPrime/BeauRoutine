/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenShortcuts.Routine.cs
 * Purpose: Extension methods for creating Tweens affecting
 *          properties on a Routine.
*/

using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating tweens.
    /// </summary>
    static public partial class TweenShortcuts
    {
        #region Routine

        private sealed class TweenData_Routine_TimeScale : ITweenData
        {
            private Routine m_Routine;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_Routine_TimeScale(Routine inRoutine, float inTarget)
            {
                m_Routine = inRoutine;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Routine.GetTimeScale();
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Routine.SetTimeScale(m_Start + m_Delta * inPercent);
            }

            public override string ToString()
            {
                return "Routine: TimeScale";
            }
        }

        /// <summary>
        /// Changes the TimeScale of the routine.
        /// </summary>
        static public Tween TimeScaleTo(this Routine inRoutine, float inTarget, float inTime)
        {
            return Tween.Create(new TweenData_Routine_TimeScale(inRoutine, inTarget), inTime);
        }

        /// <summary>
        /// Changes the TimeScale of the routine.
        /// </summary>
        static public Tween TimeScaleTo(this Routine inRoutine, float inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Routine_TimeScale(inRoutine, inTarget), inSettings);
        }

        #endregion

        #region RoutineIdentity

        private sealed class TweenData_RoutineIdentity_TimeScale : ITweenData
        {
            private RoutineIdentity m_Identity;
            private float m_Target;

            private float m_Start;
            private float m_Delta;

            public TweenData_RoutineIdentity_TimeScale(RoutineIdentity inIdentity, float inTarget)
            {
                m_Identity = inIdentity;
                m_Target = inTarget;
            }

            public void OnTweenStart()
            {
                m_Start = m_Identity.TimeScale;
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Identity.TimeScale = m_Start + m_Delta * inPercent;
            }

            public override string ToString()
            {
                return "RoutineIdentity: TimeScale";
            }
        }

        /// <summary>
        /// Changes the TimeScale of the RoutineIdentity.
        /// </summary>
        static public Tween TimeScaleTo(this RoutineIdentity inIdentity, float inTarget, float inTime)
        {
            return Tween.Create(new TweenData_RoutineIdentity_TimeScale(inIdentity, inTarget), inTime);
        }

        /// <summary>
        /// Changes the TimeScale of the RoutineIdentity.
        /// </summary>
        static public Tween TimeScaleTo(this RoutineIdentity inIdentity, float inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_RoutineIdentity_TimeScale(inIdentity, inTarget), inSettings);
        }

        /// <summary>
        /// Changes the TimeScale of the RoutineIdentity on this GameObject.
        /// </summary>
        static public Tween TimeScaleTo(this GameObject inGameObject, float inTarget, float inTime)
        {
            return Tween.Create(new TweenData_RoutineIdentity_TimeScale(RoutineIdentity.Require(inGameObject), inTarget), inTime);
        }

        /// <summary>
        /// Changes the TimeScale of the RoutineIdentity on this GameObject.
        /// </summary>
        static public Tween TimeScaleTo(this GameObject inGameObject, float inTarget, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_RoutineIdentity_TimeScale(RoutineIdentity.Require(inGameObject), inTarget), inSettings);
        }

        #endregion
    }
}

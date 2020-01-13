/*
 * Copyright (C) 2016-2020. Filament Games, LLC. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Nov 2018
 * 
 * File:    RoutineButton.cs
 * Purpose: Button replacement, animated through BeauRoutines.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Button with additional routine-driven animation states.
    /// </summary>
    [AddComponentMenu("BeauRoutine/Routine Button")]
    public class RoutineButton : Button
    {
        /// <summary>
        /// Button transition states.
        /// </summary>
        public enum State { Normal = 0, Highlighted = 1, Pressed = 2, Disabled = 3 }

        /// <summary>
        /// Interface for performing transition animations.
        /// </summary>
        public interface IAnimator
        {
            /// <summary>
            /// Performs an instant transition to the given state.
            /// </summary>
            void InstantTransitionTo(State inState);
            
            /// <summary>
            /// Returns an IEnumerator for transitioning to the given state.
            /// </summary>
            IEnumerator TransitionTo(State inState);
        }

        #region Inspector

        [SerializeField]
        protected RoutineButtonAnimator m_RoutineAnimator = null;

        #endregion // Inspector

        private IAnimator m_CurrentAnimator;
        private Routine m_CurrentAnimation;

        /// <summary>
        /// Source for performing state transitions.
        /// </summary>
        public IAnimator AnimationSource
        {
            get { return m_CurrentAnimator; }
            set
            {
                if (m_CurrentAnimator != value)
                {
                    bool bPerformTransitions = isActiveAndEnabled;

                    m_CurrentAnimation.Stop();

                    if (bPerformTransitions && m_CurrentAnimator != null)
                        m_CurrentAnimator.InstantTransitionTo(State.Normal);
                    m_CurrentAnimator = value;
                    if (bPerformTransitions && m_CurrentAnimator != null)
                        m_CurrentAnimator.InstantTransitionTo((State) currentSelectionState);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (m_CurrentAnimator == null)
            {
                if (!m_RoutineAnimator)
                    m_RoutineAnimator = GetComponent<RoutineButtonAnimator>();
                if (m_RoutineAnimator)
                    m_CurrentAnimator = m_RoutineAnimator;
            }
        }

        protected override void InstantClearState()
        {
            base.InstantClearState();

            m_CurrentAnimation.Stop();

            if (m_CurrentAnimator == null)
                return;

            m_CurrentAnimator.InstantTransitionTo(State.Normal);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (m_CurrentAnimator == null)
                return;

            if (instant)
            {
                m_CurrentAnimation.Stop();
                m_CurrentAnimator.InstantTransitionTo((State) state);
            }
            else
            {
                IEnumerator anim = m_CurrentAnimator.TransitionTo((State) state);
                m_CurrentAnimation.Replace(this, anim);
            }
        }

        #if UNITY_EDITOR

        protected override void Reset()
        {
            base.Reset();

            m_RoutineAnimator = GetComponent<RoutineButtonAnimator>();
        }

        [UnityEditor.CustomEditor(typeof(RoutineButton), true)]
        [UnityEditor.CanEditMultipleObjects]
        private class RoutineButtonEditor : UnityEditor.UI.ButtonEditor
        {
            private UnityEditor.SerializedProperty m_RoutineAnimatorProperty;

            protected override void OnEnable()
            {
                base.OnEnable();
                m_RoutineAnimatorProperty = serializedObject.FindProperty("m_RoutineAnimator");
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                UnityEditor.EditorGUILayout.Space();
                serializedObject.Update();
                UnityEditor.EditorGUILayout.PropertyField(m_RoutineAnimatorProperty);
                this.serializedObject.ApplyModifiedProperties();
            }
        }

        #endif // UNITY_EDITOR
    }
}
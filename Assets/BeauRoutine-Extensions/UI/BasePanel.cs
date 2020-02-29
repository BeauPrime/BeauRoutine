/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Nov 2018
 * 
 * File:    BasePanel.cs
 * Purpose: Abstract two-state UI panel.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Base class for panels. Can be shown or hidden.
    /// </summary>
    public abstract class BasePanel : MonoBehaviour
    {
        protected enum InputAnimationBehavior
        {
            AlwaysOn = 0,
            DisableInteract = 1,
            DisableRaycasts = 2
        }

        /// <summary>
        /// Interface for performing transition animations.
        /// </summary>
        public interface IAnimator
        {
            /// <summary>
            /// Performs an instant transition to the given state.
            /// </summary>
            void InstantTransitionTo(bool inbOn);

            /// <summary>
            /// Returns an IEnumerator for transitioning to the given state.
            /// </summary>
            IEnumerator TransitionTo(bool inbOn);
        }

        #region Inspector

        [SerializeField]
        protected RectTransform m_RootTransform = null;

        [SerializeField]
        protected CanvasGroup m_RootGroup = null;

        [SerializeField]
        private InputAnimationBehavior m_InputBehavior = InputAnimationBehavior.DisableInteract;

        [SerializeField]
        private bool m_HideOnStart = true;

        [SerializeField]
        private BasePanelAnimator[] m_PanelAnimators = null;

        #endregion // Inspector

        private Routine m_ShowHideAnim;
        [NonSerialized] private bool m_Showing;
        [NonSerialized] private bool m_StateInitialized;

        public RectTransform Root { get { return m_RootTransform; } }
        public CanvasGroup CanvasGroup { get { return m_RootGroup; } }

        #region Unity Events

        protected virtual void Awake() { }

        protected virtual void Start()
        {
            if (m_StateInitialized)
                return;

            if (m_HideOnStart)
            {
                InstantHide();
            }
            else
            {
                InstantShow();
            }
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable()
        {
            InstantHide();
        }

        #endregion // Unity Events

        #region Show/Hide

        public bool IsShowing()
        {
            return m_Showing;
        }

        public bool IsTransitioning()
        {
            return m_ShowHideAnim;
        }

        public IEnumerator Show(float inDelay = 0)
        {
            if (!m_StateInitialized)
            {
                InstantShow();
                return null;
            }

            if (!m_Showing)
            {
                m_Showing = true;
                m_ShowHideAnim.Replace(this, ShowImpl(inDelay)).ExecuteWhileDisabled();
            }

            return m_ShowHideAnim.Wait();
        }

        public void InstantShow()
        {
            m_ShowHideAnim.Stop();
            m_Showing = true;
            m_StateInitialized = true;

            OnShow(true);
            SubAnimatorInstantTransition(true);
            InstantTransitionToShow();
            SetInputState(true);
            OnShowComplete(true);
        }

        public IEnumerator Hide(float inDelay = 0)
        {
            if (!m_StateInitialized)
            {
                InstantHide();
                return null;
            }

            if (m_Showing)
            {
                m_Showing = false;
                SetInputState(false);
                m_ShowHideAnim.Replace(this, HideImpl(inDelay)).ExecuteWhileDisabled();
            }

            return m_ShowHideAnim.Wait();
        }

        public void InstantHide()
        {
            m_ShowHideAnim.Stop();
            m_Showing = false;
            m_StateInitialized = true;

            OnHide(true);
            SubAnimatorInstantTransition(false);
            InstantTransitionToHide();
            SetInputState(false);

            if (m_RootTransform)
                m_RootTransform.gameObject.SetActive(false);

            OnHideComplete(true);
        }

        #endregion // Show/Hide

        #region Animations

        private IEnumerator ShowImpl(float inDelay)
        {
            SetInputState(false);

            if (inDelay > 0)
                yield return inDelay;

            OnShow(false);
            yield return Routine.Inline(Routine.Combine(
                TransitionToShow(),
                SubAnimatorTransition(true)
            ));
            SetInputState(true);
            OnShowComplete(false);
        }

        private IEnumerator HideImpl(float inDelay)
        {
            SetInputState(false);

            if (inDelay > 0)
                yield return inDelay;

            OnHide(false);
            yield return Routine.Inline(Routine.Combine(
                TransitionToHide(),
                SubAnimatorTransition(false)
            ));

            if (m_RootTransform)
                m_RootTransform.gameObject.SetActive(false);

            OnHideComplete(false);
        }

        protected virtual void SetInputState(bool inbEnabled)
        {
            if (!m_RootGroup || m_InputBehavior == InputAnimationBehavior.AlwaysOn)
                return;

            switch (m_InputBehavior)
            {
                case InputAnimationBehavior.DisableInteract:
                    m_RootGroup.interactable = inbEnabled;
                    break;

                case InputAnimationBehavior.DisableRaycasts:
                    m_RootGroup.blocksRaycasts = inbEnabled;
                    break;
            }
        }

        private IEnumerator SubAnimatorTransition(bool inbOn)
        {
            if (m_PanelAnimators == null || m_PanelAnimators.Length == 0)
                return null;

            if (m_PanelAnimators.Length == 1)
                return m_PanelAnimators[0].TransitionTo(inbOn);

            IEnumerator[] children = new IEnumerator[m_PanelAnimators.Length];
            for (int i = 0; i < m_PanelAnimators.Length; ++i)
            {
                children[i] = Routine.Inline(m_PanelAnimators[i].TransitionTo(inbOn));
            }
            return Routine.Inline(Routine.Combine(children));
        }

        private void SubAnimatorInstantTransition(bool inbOn)
        {
            if (m_PanelAnimators != null)
            {
                for (int i = 0; i < m_PanelAnimators.Length; ++i)
                {
                    m_PanelAnimators[i].InstantTransitionTo(inbOn);
                }
            }
        }

        #endregion // Animations

        #region Abstract

        protected virtual void OnShow(bool inbInstant) { }
        protected virtual void OnShowComplete(bool inbInstant) { }

        protected virtual IEnumerator TransitionToShow()
        {
            InstantTransitionToShow();
            return null;
        }

        protected virtual void InstantTransitionToShow()
        {
            if (m_RootTransform)
                m_RootTransform.gameObject.SetActive(true);
        }

        protected virtual void OnHide(bool inbInstant) { }
        protected virtual void OnHideComplete(bool inbInstant) { }

        protected virtual IEnumerator TransitionToHide()
        {
            InstantTransitionToHide();
            return null;
        }

        protected virtual void InstantTransitionToHide()
        {
            if (m_RootTransform)
                m_RootTransform.gameObject.SetActive(false);
        }

        #endregion // Abstract

        #region Debug

        #if UNITY_EDITOR

        [ContextMenu("Show Panel")]
        private void ShowDebug()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                Show();
            }
            else
            {
                UnityEditor.Undo.RegisterFullObjectHierarchyUndo(this, "Showing Panel");
                InstantTransitionToShow();
            }
        }

        [ContextMenu("Hide Panel")]
        private void HideDebug()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                Hide();
            }
            else
            {
                UnityEditor.Undo.RegisterFullObjectHierarchyUndo(this, "Hiding Panel");
                InstantTransitionToHide();
            }
        }

        #endif // UNITY_EDITOR

        #endregion // Debug
    }
}
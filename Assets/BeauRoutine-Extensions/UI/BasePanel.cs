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
using UnityEngine.Events;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Base class for panels. Can be shown or hidden.
    /// </summary>
    public abstract class BasePanel : MonoBehaviour
    {
        /// <summary>
        /// Animation type.
        /// </summary>
        public enum TransitionType : byte
        {
            Animated,
            Instant
        }

        /// <summary>
        /// Event for transitions.
        /// </summary>
        public class TransitionEvent : UnityEvent<TransitionType> { }

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

        [Header("Components")]
        
        [SerializeField] protected RectTransform m_RootTransform = null;
        [SerializeField] protected CanvasGroup m_RootGroup = null;
        [SerializeField] private BasePanelAnimator[] m_PanelAnimators = null;

        [Header("Behavior")]
        [SerializeField] private InputAnimationBehavior m_InputBehavior = InputAnimationBehavior.DisableInteract;
        [SerializeField] private bool m_HideOnStart = true;

        [Header("Events")]
        [SerializeField] private TransitionEvent m_OnShowEvent = new TransitionEvent();
        [SerializeField] private TransitionEvent m_OnShowCompleteEvent = new TransitionEvent();
        [SerializeField] private TransitionEvent m_OnHideEvent = new TransitionEvent();
        [SerializeField] private TransitionEvent m_OnHideCompleteEvent = new TransitionEvent();

        #endregion // Inspector

        [NonSerialized] private Routine m_ShowHideAnim;
        [NonSerialized] private bool m_Showing;
        [NonSerialized] private bool m_StateInitialized;

        public RectTransform Root { get { return m_RootTransform; } }
        public CanvasGroup CanvasGroup { get { return m_RootGroup; } }

        public TransitionEvent OnShowEvent { get { return m_OnShowEvent; } }
        public TransitionEvent OnShowCompleteEvent { get { return m_OnShowCompleteEvent; } }

        public TransitionEvent OnHideEvent { get { return m_OnHideEvent; } }
        public TransitionEvent OnHideCompleteEvent { get { return m_OnHideCompleteEvent; } }

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

            InvokeOnShow(TransitionType.Instant);
            SubAnimatorInstantTransition(true);
            InstantTransitionToShow();
            SetInputState(true);
            InvokeOnShowComplete(TransitionType.Instant);
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

            InvokeOnHide(TransitionType.Instant);
            SubAnimatorInstantTransition(false);
            InstantTransitionToHide();
            SetInputState(false);

            if (m_RootTransform)
                m_RootTransform.gameObject.SetActive(false);

            InvokeOnHideComplete(TransitionType.Instant);
        }

        #endregion // Show/Hide

        #region Animations

        private IEnumerator ShowImpl(float inDelay)
        {
            SetInputState(false);

            if (inDelay > 0)
                yield return inDelay;

            InvokeOnShow(TransitionType.Animated);
            yield return Routine.Inline(Routine.Combine(
                TransitionToShow(),
                SubAnimatorTransition(true)
            ));
            SetInputState(true);
            InvokeOnShowComplete(TransitionType.Animated);
        }

        private IEnumerator HideImpl(float inDelay)
        {
            SetInputState(false);

            if (inDelay > 0)
                yield return inDelay;

            InvokeOnHide(TransitionType.Animated);
            yield return Routine.Inline(Routine.Combine(
                TransitionToHide(),
                SubAnimatorTransition(false)
            ));

            if (m_RootTransform)
                m_RootTransform.gameObject.SetActive(false);

            InvokeOnHideComplete(TransitionType.Animated);
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

        private void InvokeOnShow(TransitionType inType)
        {
            m_OnShowEvent.Invoke(inType);
            OnShow(inType == TransitionType.Instant);
        }

        private void InvokeOnShowComplete(TransitionType inType)
        {
            m_OnShowCompleteEvent.Invoke(inType);
            OnShowComplete(inType == TransitionType.Instant);
        }

        private void InvokeOnHide(TransitionType inType)
        {
            m_OnHideEvent.Invoke(inType);
            OnHide(inType == TransitionType.Instant);
        }

        private void InvokeOnHideComplete(TransitionType inType)
        {
            m_OnHideCompleteEvent.Invoke(inType);
            OnHideComplete(inType == TransitionType.Instant);
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
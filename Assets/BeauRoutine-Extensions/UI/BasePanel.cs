/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    7 Nov 2018
 * 
 * File:    BasePanel.cs
 * Purpose: Abstract two-state UI panel.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

        #region Inspector

        [SerializeField]
        protected RectTransform m_RootTransform = null;

        [SerializeField]
        protected CanvasGroup m_RootGroup = null;

        [SerializeField]
        private InputAnimationBehavior m_InputBehavior = InputAnimationBehavior.DisableInteract;

        [SerializeField]
        private bool m_HideOnStart = true;

        #endregion // Inspector

        protected Routine m_ShowHideAnim;
        protected bool m_Showing;

        private bool m_StateInitialized;

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

        public IEnumerator Show(float inDelay = 0)
        {
            if (!m_Showing || !m_StateInitialized)
            {
                m_StateInitialized = true;
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

            InstantTransitionToShow();
            SetInputState(true);
        }

        public IEnumerator Hide(float inDelay = 0)
        {
            if (m_Showing || !m_StateInitialized)
            {
                m_StateInitialized = true;
                m_Showing = false;
                m_ShowHideAnim.Replace(this, HideImpl(inDelay)).ExecuteWhileDisabled();
            }

            return m_ShowHideAnim.Wait();
        }

        public void InstantHide()
        {
            m_ShowHideAnim.Stop();
            m_Showing = false;
            m_StateInitialized = true;

            InstantTransitionToHide();
            SetInputState(false);

            if (m_RootTransform)
                m_RootTransform.gameObject.SetActive(false);
        }

        #endregion // Show/Hide

        #region Animations

        private IEnumerator ShowImpl(float inDelay)
        {
            SetInputState(false);

            if (inDelay > 0)
                yield return inDelay;

            yield return Routine.Inline(TransitionToShow());
            SetInputState(true);
        }

        private IEnumerator HideImpl(float inDelay)
        {
            SetInputState(false);

            if (inDelay > 0)
                yield return inDelay;

            yield return Routine.Inline(TransitionToHide());

            if (m_RootTransform)
                m_RootTransform.gameObject.SetActive(false);
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

        #endregion // Animations

        #region Abstract

        protected abstract IEnumerator TransitionToShow();
        protected abstract void InstantTransitionToShow();
        protected abstract IEnumerator TransitionToHide();
        protected abstract void InstantTransitionToHide();

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
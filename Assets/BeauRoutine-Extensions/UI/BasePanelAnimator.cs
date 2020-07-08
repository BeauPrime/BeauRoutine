/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 May 2019
 * 
 * File:    BasePanelAnimator.cs
 * Purpose: Abstract component for hooking up BasePanel transitions.
 */

using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Abstract MonoBehaviour implementation of BasePanel.IAnimator
    /// Able to be linked up to a BasePanelAnimator via the inspector
    /// </summary>
    public abstract class BasePanelAnimator : MonoBehaviour, BasePanel.IAnimator
    {
        [NonSerialized] protected bool m_IsShowing;

        public void InstantTransitionTo(bool inbOn)
        {
            m_IsShowing = inbOn;

            if (inbOn)
            {
                InstantShow();
            }
            else
            {
                InstantHide();
            }
        }

        public IEnumerator TransitionTo(bool inbOn)
        {
            if (m_IsShowing == inbOn)
                return null;

            m_IsShowing = inbOn;
            if (inbOn)
            {
                return Show();
            }
            else
            {
                return Hide();
            }
        }

        protected abstract void InstantShow();
        protected abstract IEnumerator Show();

        protected abstract void InstantHide();
        protected abstract IEnumerator Hide();
    }
}
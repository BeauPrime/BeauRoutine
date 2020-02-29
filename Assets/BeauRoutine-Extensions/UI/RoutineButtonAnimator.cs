/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Nov 2018
 * 
 * File:    RoutineButtonAnimator.cs
 * Purpose: Abstract component for hooking up RoutineButton transitions.
 */

using System.Collections;
using UnityEngine;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Abstract MonoBehaviour implementation of RoutineButton.IAnimator
    /// Able to be linked up to a RoutineButton via the inspector
    /// </summary>
    public abstract class RoutineButtonAnimator : MonoBehaviour, RoutineButton.IAnimator
    {
        public abstract void InstantTransitionTo(RoutineButton.State inState);
        public abstract IEnumerator TransitionTo(RoutineButton.State inState);
    }
}
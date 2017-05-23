/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    RoutineShortcuts.cs
 * Purpose: Extension methods for creating routine functions
 *          from a set of Unity objects.
*/

using System.Collections;
using System.Threading;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating routines.
    /// </summary>
    static public class RoutineShortcuts
    {
        #region Animator

        /// <summary>
        /// Waits for the animator to either reach the end of the current state,
        /// loop back around to the start of the state, or switch to a different state.
        /// </summary>
        static public IEnumerator WaitToCompleteAnimation(this Animator inAnimator, int inLayer = 0)
        {
            AnimatorStateInfo stateInfo = inAnimator.GetCurrentAnimatorStateInfo(inLayer);
            int initialHash = stateInfo.fullPathHash;
            float initialNormalizedTime = stateInfo.normalizedTime;
            while(true)
            {
                yield return null;
                stateInfo = inAnimator.GetCurrentAnimatorStateInfo(inLayer);
                if (stateInfo.fullPathHash != initialHash
                    || stateInfo.normalizedTime >= 1 || stateInfo.normalizedTime < initialNormalizedTime)
                    break;
            }
        }

        /// <summary>
        /// Waits for the animator to play and exit the given state.
        /// </summary>
        static public IEnumerator WaitToCompleteState(this Animator inAnimator, string inStateName, int inLayer = 0)
        {
            yield return WaitForState(inAnimator, inStateName, inLayer);
            yield return WaitForNotState(inAnimator, inStateName, inLayer);
        }

        /// <summary>
        /// Waits for the animator to be in the given state.
        /// </summary>
        static public IEnumerator WaitForState(this Animator inAnimator, string inStateName, int inLayer = 0)
        {
            while (true)
            {
                AnimatorStateInfo stateInfo = inAnimator.GetCurrentAnimatorStateInfo(inLayer);
                if (stateInfo.IsName(inStateName))
                    yield break;
                yield return null;
            }
        }

        /// <summary>
        /// Waits for the animator to stop being in the given state.
        /// </summary>
        static public IEnumerator WaitForNotState(this Animator inAnimator, string inStateName, int inLayer = 0)
        {
            while (true)
            {
                AnimatorStateInfo stateInfo = inAnimator.GetCurrentAnimatorStateInfo(inLayer);
                if (!stateInfo.IsName(inStateName))
                    yield break;
                yield return null;
            }
        }

        #endregion

        #region AudioSource

        /// <summary>
        /// Waits for the AudioSource to stop playing.
        /// Make sure it's not looping.
        /// </summary>
        static public IEnumerator WaitToComplete(this AudioSource inAudioSource)
        {
            while (inAudioSource.isPlaying)
                yield return null;
        }

        #endregion

        #region Threading

        /// <summary>
        /// Waits for the given thread to finish running.
        /// </summary>
        static public IEnumerator WaitToComplete(this Thread inThread)
        {
            while (inThread.IsAlive)
                yield return null;
        }

        #endregion
    }
}

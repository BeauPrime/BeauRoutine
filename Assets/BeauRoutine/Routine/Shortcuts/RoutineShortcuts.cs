/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    RoutineShortcuts.cs
 * Purpose: Extension methods for creating routine functions
 *          from a set of Unity objects.
 */

#if UNITY_WEBGL
#define DISABLE_THREADING
#endif // UNITY_WEBGL

#if UNITY_5_5_OR_NEWER
#define SUPPORTS_PARTICLESYSTEM_ISEMITTING
#endif // UNITY_5_5_OR_NEWER

#if UNITY_2017_3_OR_NEWER
#define SUPPORTS_IPLAYABLE
#endif // UNITY_2017_3_OR_NEWER

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if !DISABLE_THREADING
using System.Threading;
#endif // DISABLE_THREADING

#if SUPPORTS_IPLAYABLE
using UnityEngine.Playables;
#endif // SUPPORTS_IPLAYABLE

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
            while (true)
            {
                yield return null;
                stateInfo = inAnimator.GetCurrentAnimatorStateInfo(inLayer);
                if (stateInfo.fullPathHash != initialHash ||
                    stateInfo.normalizedTime >= 1 || stateInfo.normalizedTime < initialNormalizedTime)
                    break;
            }
            yield return Routine.Command.BreakAndResume;
        }

        /// <summary>
        /// Waits for the animator to play and exit the given state.
        /// </summary>
        static public IEnumerator WaitToCompleteState(this Animator inAnimator, string inStateName, int inLayer = 0)
        {
            yield return Routine.Inline(WaitForState(inAnimator, inStateName, inLayer));
            yield return Routine.Inline(WaitForNotState(inAnimator, inStateName, inLayer));
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
                    yield return Routine.Command.BreakAndResume;
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
                    yield return Routine.Command.BreakAndResume;
                yield return null;
            }
        }

        #endregion // Animator

        #region AudioSource

        /// <summary>
        /// Waits for the AudioSource to stop playing.
        /// Make sure it's not looping.
        /// </summary>
        static public IEnumerator WaitToComplete(this AudioSource inAudioSource)
        {
            while (inAudioSource.isPlaying)
                yield return null;

            yield return Routine.Command.BreakAndResume;
        }

        #endregion // AudioSource

        #region SpriteRenderer

        /// <summary>
        /// Displays a sequence of sprites on SpriteRenderer.
        /// </summary>
        static public IEnumerator PlaySequence(this SpriteRenderer inSpriteRenderer, Sprite[] inSprites, float inDurationPerFrame, bool inbHoldLastFrame = true, Action<int, Sprite> inCallback = null)
        {
            return PlaySequence(inSpriteRenderer, (IList<Sprite>) inSprites, inDurationPerFrame, inbHoldLastFrame, inCallback);
        }

        /// <summary>
        /// Displays a sequence of sprites on SpriteRenderer.
        /// </summary>
        static public IEnumerator PlaySequence(this SpriteRenderer inSpriteRenderer, IList<Sprite> inSprites, float inDurationPerFrame, bool inbHoldLastFrame = true, Action<int, Sprite> inCallback = null)
        {
            if (inSprites.Count < 0)
                yield break;

            if (inDurationPerFrame <= 0)
                throw new ArgumentOutOfRangeException("inDurationPerFrame", "Duration must be greater than 0");

            int currentFrame = 0;
            float accum = 0;

            Sprite originalSprite = inSpriteRenderer.sprite;
            inSpriteRenderer.sprite = inSprites[0];

            if (inCallback != null)
                inCallback(0, inSpriteRenderer.sprite);

            while (true)
            {
                yield return null;
                accum += Routine.DeltaTime;

                while (accum >= inDurationPerFrame)
                {
                    ++currentFrame;
                    accum -= inDurationPerFrame;
                    if (currentFrame >= inSprites.Count)
                    {
                        if (!inbHoldLastFrame)
                            inSpriteRenderer.sprite = originalSprite;
                        yield break;
                    }

                    inSpriteRenderer.sprite = inSprites[currentFrame];
                    if (inCallback != null)
                        inCallback(currentFrame, inSpriteRenderer.sprite);
                }
            }
        }

        #endregion // SpriteRenderer

        #region ParticleSystem

        /// <summary>
        /// Waits for the ParticleSystem to stop emitting and for its particles to die.
        /// </summary>
        static public IEnumerator WaitToComplete(this ParticleSystem inParticleSystem)
        {
            #if SUPPORTS_PARTICLESYSTEM_ISEMITTING
            while (!inParticleSystem.isStopped || inParticleSystem.isEmitting || inParticleSystem.particleCount > 0)
                yield return null;
            #else
            while (!inParticleSystem.isStopped || inParticleSystem.particleCount > 0)
                yield return null;
            #endif

            yield return Routine.Command.BreakAndResume;
        }

        #endregion // ParticleSystem

        #region Unity Events

        #region Zero Args

        /// <summary>
        /// Waits for the UnityEvent to be invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke(this UnityEvent inEvent)
        {
            return new WaitForUnityEventListener(inEvent);
        }

        /// <summary>
        /// Waits for the UnityEvent to be invoked,
        /// and invokes an additional callback when invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke(this UnityEvent inEvent, UnityAction inCallback)
        {
            return new WaitForUnityEventListener(inEvent, inCallback);
        }

        // Implements the event listener
        private sealed class WaitForUnityEventListener : IEnumerator, IDisposable
        {
            private UnityEvent m_Event;
            private UnityAction m_Listener;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent inEvent, UnityAction inListener = null)
            {
                m_Event = inEvent;
                m_Phase = 0;
                m_Listener = inListener;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                {
                    m_Event.RemoveListener(OnInvoke);
                    if (m_Listener != null)
                        m_Event.RemoveListener(m_Listener);
                }

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch (m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
                        if (m_Listener != null)
                            m_Event.AddListener(m_Listener);
                        return true;

                    case 2:
                        return false;

                    case 1:
                    default:
                        return true;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            private void OnInvoke()
            {
                m_Phase = 2;
            }
        }

        #endregion

        #region One Arg

        /// <summary>
        /// Waits for the UnityEvent to be invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke<T0>(this UnityEvent<T0> inEvent)
        {
            return new WaitForUnityEventListener<T0>(inEvent);
        }

        /// <summary>
        /// Waits for the UnityEvent to be invoked,
        /// and invokes an additional callback when invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke<T0>(this UnityEvent<T0> inEvent, UnityAction<T0> inCallback)
        {
            return new WaitForUnityEventListener<T0>(inEvent, inCallback);
        }

        // Implements the event listener
        private sealed class WaitForUnityEventListener<T0> : IEnumerator, IDisposable
        {
            private UnityEvent<T0> m_Event;
            private UnityAction<T0> m_Listener;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent<T0> inEvent, UnityAction<T0> inListener = null)
            {
                m_Event = inEvent;
                m_Phase = 0;
                m_Listener = inListener;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                {
                    m_Event.RemoveListener(OnInvoke);
                    if (m_Listener != null)
                        m_Event.RemoveListener(m_Listener);
                }

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch (m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
                        if (m_Listener != null)
                            m_Event.AddListener(m_Listener);
                        return true;

                    case 2:
                        return false;

                    case 1:
                    default:
                        return true;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            private void OnInvoke(T0 inArg0)
            {
                m_Phase = 2;
            }
        }

        #endregion

        #region Two Args

        /// <summary>
        /// Waits for the UnityEvent to be invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke<T0, T1>(this UnityEvent<T0, T1> inEvent)
        {
            return new WaitForUnityEventListener<T0, T1>(inEvent);
        }

        /// <summary>
        /// Waits for the UnityEvent to be invoked,
        /// and invokes an additional callback when invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke<T0, T1>(this UnityEvent<T0, T1> inEvent, UnityAction<T0, T1> inCallback)
        {
            return new WaitForUnityEventListener<T0, T1>(inEvent, inCallback);
        }

        // Implements the event listener
        private sealed class WaitForUnityEventListener<T0, T1> : IEnumerator, IDisposable
        {
            private UnityEvent<T0, T1> m_Event;
            private UnityAction<T0, T1> m_Listener;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent<T0, T1> inEvent, UnityAction<T0, T1> inListener = null)
            {
                m_Event = inEvent;
                m_Phase = 0;
                m_Listener = inListener;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                {
                    m_Event.RemoveListener(OnInvoke);
                    if (m_Listener != null)
                        m_Event.RemoveListener(m_Listener);
                }

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch (m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
                        if (m_Listener != null)
                            m_Event.AddListener(m_Listener);
                        return true;

                    case 2:
                        return false;

                    case 1:
                    default:
                        return true;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            private void OnInvoke(T0 inArg0, T1 inArg1)
            {
                m_Phase = 2;
            }
        }

        #endregion

        #region Three Args

        /// <summary>
        /// Waits for the UnityEvent to be invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke<T0, T1, T2>(this UnityEvent<T0, T1, T2> inEvent)
        {
            return new WaitForUnityEventListener<T0, T1, T2>(inEvent);
        }

        /// <summary>
        /// Waits for the UnityEvent to be invoked,
        /// and invokes an additional callback when invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke<T0, T1, T2>(this UnityEvent<T0, T1, T2> inEvent, UnityAction<T0, T1, T2> inCallback)
        {
            return new WaitForUnityEventListener<T0, T1, T2>(inEvent, inCallback);
        }

        // Implements the event listener
        private sealed class WaitForUnityEventListener<T0, T1, T2> : IEnumerator, IDisposable
        {
            private UnityEvent<T0, T1, T2> m_Event;
            private UnityAction<T0, T1, T2> m_Listener;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent<T0, T1, T2> inEvent, UnityAction<T0, T1, T2> inListener = null)
            {
                m_Event = inEvent;
                m_Phase = 0;
                m_Listener = inListener;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                {
                    m_Event.RemoveListener(OnInvoke);
                    if (m_Listener != null)
                        m_Event.RemoveListener(m_Listener);
                }

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch (m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
                        if (m_Listener != null)
                            m_Event.AddListener(m_Listener);
                        return true;

                    case 2:
                        return false;

                    case 1:
                    default:
                        return true;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            private void OnInvoke(T0 inArg0, T1 inArg1, T2 inArg2)
            {
                m_Phase = 2;
            }
        }

        #endregion

        #region Four Args

        /// <summary>
        /// Waits for the UnityEvent to be invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> inEvent)
        {
            return new WaitForUnityEventListener<T0, T1, T2, T3>(inEvent);
        }

        /// <summary>
        /// Waits for the UnityEvent to be invoked,
        /// and invokes an additional callback when invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> inEvent, UnityAction<T0, T1, T2, T3> inCallback)
        {
            return new WaitForUnityEventListener<T0, T1, T2, T3>(inEvent, inCallback);
        }

        // Implements the event listener
        private sealed class WaitForUnityEventListener<T0, T1, T2, T3> : IEnumerator, IDisposable
        {
            private UnityEvent<T0, T1, T2, T3> m_Event;
            private UnityAction<T0, T1, T2, T3> m_Listener;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent<T0, T1, T2, T3> inEvent, UnityAction<T0, T1, T2, T3> inListener = null)
            {
                m_Event = inEvent;
                m_Phase = 0;
                m_Listener = inListener;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                {
                    m_Event.RemoveListener(OnInvoke);
                    if (m_Listener != null)
                        m_Event.RemoveListener(m_Listener);
                }

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch (m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
                        if (m_Listener != null)
                            m_Event.AddListener(m_Listener);
                        return true;

                    case 2:
                        return false;

                    case 1:
                    default:
                        return true;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            private void OnInvoke(T0 inArg0, T1 inArg1, T2 inArg2, T3 inArg3)
            {
                m_Phase = 2;
            }
        }

        #endregion

        #endregion // Unity Events

        #region Threading

        #if !DISABLE_THREADING
        /// <summary>
        /// Waits for the given thread to finish running.
        /// </summary>
        static public IEnumerator WaitToComplete(this Thread inThread)
        {
            while (inThread.IsAlive)
                yield return null;
            yield return Routine.Command.BreakAndResume;
        }
        #endif // !DISABLE_THREADING

        #endregion // Threading

        #region IPlayable

        #if SUPPORTS_IPLAYABLE

        /// <summary>
        /// Waits for the given IPlayable to be done.
        /// </summary>
        static public IEnumerator WaitToComplete<U>(this U inPlayable) where U : struct, IPlayable
        {
            while (!PlayableExtensions.IsDone(inPlayable))
                yield return null;
        }

        /// <summary>
        /// Waits for the given PlayableGraph to be done.
        /// </summary>
        static public IEnumerator WaitToComplete(this PlayableGraph inGraph)
        {
            while (!inGraph.IsDone())
                yield return null;
        }

        #endif // SUPPORTS_IPLAYABLE

        #endregion // IPlayable
    }
}
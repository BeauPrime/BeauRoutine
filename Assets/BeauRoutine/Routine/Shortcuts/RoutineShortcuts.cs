/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    RoutineShortcuts.cs
 * Purpose: Extension methods for creating routine functions
 *          from a set of Unity objects.
*/

#if UNITY_WEBGL
    #define DISABLE_THREADING
#endif

#if UNITY_5_5_OR_NEWER
    #define SUPPORTS_PARTICLESYSTEM_ISEMITTING
#endif

using System;
using System.Collections;
#if !DISABLE_THREADING
using System.Threading;
#endif
using UnityEngine;
using UnityEngine.Events;

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
            
            yield return Routine.Command.BreakAndResume;
        }

        #endregion

        #region ParticleSystem

        /// <summary>
        /// Waits for the ParticleSystem to stop emitting and for its particles to die.
        /// </summary>
        static public IEnumerator WaitToComplete(this ParticleSystem inParticleSystem)
        {
            #if SUPPORTS_PARTICLESYSTEM_ISEMITTING
            while(!inParticleSystem.isStopped || inParticleSystem.isEmitting || inParticleSystem.particleCount > 0)
                yield return null;
            #else
            while(!inParticleSystem.isStopped || inParticleSystem.particleCount > 0)
                yield return null;
            #endif

            yield return Routine.Command.BreakAndResume;
        }

        #endregion

        #region Unity Events

        #region Zero Args

        /// <summary>
        /// Waits for the UnityEvent to be invoked.
        /// </summary>
        static public IEnumerator WaitForInvoke(this UnityEvent inEvent)
        {
            return new WaitForUnityEventListener(inEvent);
        }

        // Implements the event listener
        private sealed class WaitForUnityEventListener : IEnumerator, IDisposable
        {
            private UnityEvent m_Event;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent inEvent)
            {
                m_Event = inEvent;
                m_Phase = 0;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                    m_Event.RemoveListener(OnInvoke);

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch(m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
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

        // Implements the event listener
        private sealed class WaitForUnityEventListener<T0> : IEnumerator, IDisposable
        {
            private UnityEvent<T0> m_Event;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent<T0> inEvent)
            {
                m_Event = inEvent;
                m_Phase = 0;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                    m_Event.RemoveListener(OnInvoke);

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch(m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
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

        // Implements the event listener
        private sealed class WaitForUnityEventListener<T0, T1> : IEnumerator, IDisposable
        {
            private UnityEvent<T0, T1> m_Event;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent<T0, T1> inEvent)
            {
                m_Event = inEvent;
                m_Phase = 0;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                    m_Event.RemoveListener(OnInvoke);

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch(m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
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

        // Implements the event listener
        private sealed class WaitForUnityEventListener<T0, T1, T2> : IEnumerator, IDisposable
        {
            private UnityEvent<T0, T1, T2> m_Event;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent<T0, T1, T2> inEvent)
            {
                m_Event = inEvent;
                m_Phase = 0;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                    m_Event.RemoveListener(OnInvoke);

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch(m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
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

        // Implements the event listener
        private sealed class WaitForUnityEventListener<T0, T1, T2, T3> : IEnumerator, IDisposable
        {
            private UnityEvent<T0, T1, T2, T3> m_Event;
            private int m_Phase = 0; // 0 uninitialized 1 waiting 2 done

            public WaitForUnityEventListener(UnityEvent<T0, T1, T2, T3> inEvent)
            {
                m_Event = inEvent;
                m_Phase = 0;
            }

            public object Current { get { return null; } }

            public void Dispose()
            {
                if (m_Phase > 0)
                    m_Event.RemoveListener(OnInvoke);

                m_Phase = 0;
                m_Event = null;
            }

            public bool MoveNext()
            {
                switch(m_Phase)
                {
                    case 0:
                        m_Phase = 1;
                        m_Event.AddListener(OnInvoke);
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

        #endregion

        #region Threading

#if !DISABLE_THREADING
        /// <summary>
        /// Waits for the given thread to finish running.
        /// </summary>
        static public IEnumerator WaitToComplete(this Thread inThread)
        {
            while (inThread.IsAlive)
                yield return null;
        }
#endif

        #endregion
    }
}

/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Utils.cs
 * Purpose: Set of utility functions for both running Routines
 *          and creating routine functions for common tasks.
 *          
 * Note:    Combine and Race are slightly hacky, but incredibly
 *          useful.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine
{
    public partial struct Routine
    {
        #region Special Commands

        /// <summary>
        /// Special yield values.
        /// </summary>
        public enum Command : byte
        {
            /// <summary>
            /// Pauses the current routine.
            /// </summary>
            Pause,

            /// <summary>
            /// Stops the current routine.
            /// </summary>
            Stop
        }

        #endregion

        #region Waiting

        /// <summary>
        /// Returns an IEnumerator that waits for the given number of frames.
        /// </summary>
        static public IEnumerator WaitFrames(int inFrames)
        {
            while (inFrames-- > 0)
                yield return null;
        }

        /// <summary>
        /// Returns an IEnumerator that waits for the given number of scaled seconds.
        /// </summary>
        static public IEnumerator WaitSeconds(float inSeconds)
        {
            yield return inSeconds;
        }

        /// <summary>
        /// Returns an IEnumerator that waits for the given number of real seconds.
        /// </summary>
        static public IEnumerator WaitRealSeconds(float inSeconds)
        {
            float endTime = Time.unscaledTime + inSeconds;
            while (Time.unscaledTime < endTime)
                yield return null;
        }

        /// <summary>
        /// Returns an IEnumerator that waits for the given condition to evaluate to true.
        /// </summary>
        static public IEnumerator WaitCondition(Func<bool> inCondition, float inInterval = 0)
        {
            object boxedTime = (inInterval > 0 ? (object)inInterval : null);
            while (!inCondition())
                yield return boxedTime;
        }

        /// <summary>
        /// Returns an IEnumerator that waits forever.
        /// </summary>
        static public IEnumerator WaitForever()
        {
            while (true)
                yield return null;
        }

        /// <summary>
        /// Returns an IEnumerator that waits for all the given routines to end.
        /// </summary>
        static public IEnumerator WaitRoutines(params Routine[] inRoutines)
        {
            for (int i = 0; i < inRoutines.Length; ++i)
            {
                IEnumerator wait = inRoutines[i].Wait();
                if (wait != null)
                    yield return wait;
            }
        }

        /// <summary>
        /// Returns an IEnumerator that waits for all the given routines to end.
        /// </summary>
        static public IEnumerator WaitRoutines(List<Routine> inRoutines)
        {
            for (int i = 0; i < inRoutines.Count; ++i)
            {
                IEnumerator wait = inRoutines[i].Wait();
                if (wait != null)
                    yield return wait;
            }
        }

        #endregion

        #region Combine

        /// <summary>
        /// Returns an IEnumerator that runs the given routines concurrently
        /// and waits for them all to end.
        /// </summary>
        static public IEnumerator Combine(params IEnumerator[] inRoutines)
        {
            if (inRoutines.Length == 0)
                return null;
            if (inRoutines.Length == 1)
                return inRoutines[0];

            int nonNullCount = 0;
            for(int i = 0; i < inRoutines.Length; ++i)
            {
                if (inRoutines[i] != null)
                    ++nonNullCount;
            }

            return nonNullCount > 0 ? CreateCombine(inRoutines, false) : null;
        }

        /// <summary>
        /// Returns an IEnumerator that runs the given routines concurrently
        /// and waits for them all to end.
        /// </summary>
        static public IEnumerator Combine(List<IEnumerator> inRoutines)
        {
            if (inRoutines.Count == 0)
                return null;
            if (inRoutines.Count == 1)
                return inRoutines[0];

            int nonNullCount = 0;
            for (int i = 0; i < inRoutines.Count; ++i)
            {
                if (inRoutines[i] != null)
                    ++nonNullCount;
            }

            return nonNullCount > 0 ? CreateCombine(inRoutines.ToArray(), false) : null;
        }

        /// <summary>
        /// Returns an IEnumerator that runs the given routines concurrently
        /// and waits for one to finish.
        /// </summary>
        static public IEnumerator Race(params IEnumerator[] inRoutines)
        {
            if (inRoutines.Length == 0)
                return null;
            if (inRoutines.Length == 1)
                return inRoutines[0];

            int nonNullCount = 0;
            for (int i = 0; i < inRoutines.Length; ++i)
            {
                if (inRoutines[i] != null)
                    ++nonNullCount;
            }

            return nonNullCount > 0 ? CreateCombine(inRoutines, true) : null;
        }

        /// <summary>
        /// Returns an IEnumerator that runs the given routines concurrently
        /// and waits for one to finish.
        /// </summary>
        static public IEnumerator Race(List<IEnumerator> inRoutines)
        {
            if (inRoutines.Count == 0)
                return null;
            if (inRoutines.Count == 1)
                return inRoutines[0];

            int nonNullCount = 0;
            for (int i = 0; i < inRoutines.Count; ++i)
            {
                if (inRoutines[i] != null)
                    ++nonNullCount;
            }

            return nonNullCount > 0 ? CreateCombine(inRoutines.ToArray(), true) : null;
        }

        /// <summary>
        /// Runs all the given IEnumerators
        /// </summary>
        static private IEnumerator CreateCombine(IEnumerator[] inEnumerators, bool inbRace)
        {
            return new CombineIterator(inEnumerators, inbRace);
        }

        private sealed class CombineIterator : IRoutineEnumerator
        {
            private IEnumerator[] m_Enumerators;
            private List<Fiber> m_Fibers;
            private bool m_Race;

            public CombineIterator(IEnumerator[] inEnumerators, bool inbRace)
            {
                m_Enumerators = inEnumerators;
                m_Race = inbRace;
                m_Fibers = new List<Fiber>(inEnumerators.Length);
            }

            public void Dispose()
            {
                if (m_Enumerators != null)
                {
                    for(int i = 0; i < m_Enumerators.Length; ++i)
                    {
                        ((IDisposable)m_Enumerators[i]).Dispose();
                        m_Enumerators[i] = null;
                    }
                    m_Enumerators = null;
                }

                for(int i = 0; i < m_Fibers.Count; ++i)
                {
                    m_Fibers[i].Stop();
                    m_Fibers[i].Run();
                }

                m_Fibers.Clear();
            }

            public object Current
            {
                get { return null; }
            }

            public bool MoveNext()
            {
                if (m_Fibers.Count > 0)
                {
                    for(int i = 0; i < m_Fibers.Count; ++i)
                    {
                        Fiber myFiber = m_Fibers[i];
                        if (!myFiber.Run())
                        {
                            m_Fibers.RemoveAt(i--);
                            if (m_Race)
                                return false;
                        }
                    }
                }

                return m_Fibers.Count > 0;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public bool OnRoutineStart()
            {
                for (int i = 0; i < m_Enumerators.Length; ++i)
                {
                    if (m_Enumerators[i] != null)
                        m_Fibers.Add(ChainFiber(m_Enumerators[i]));
                    m_Enumerators[i] = null;
                }

                m_Enumerators = null;
                return m_Fibers.Count > 0;
            }

            public override string ToString()
            {
                return m_Race ? "Routine::Race()" : "Routine::Combine()";
            }

#if UNITY_EDITOR
            public Editor.RoutineStats[] GetStats()
            {
                Editor.RoutineStats[] stats = new Editor.RoutineStats[m_Fibers.Count];
                for (int i = 0; i < stats.Length; ++i)
                    stats[i] = m_Fibers[i].GetStats();
                return stats;
            }
#endif
        }

        #endregion

        #region Delay

        /// <summary>
        /// Executes an action after the given number of seconds.
        /// </summary>
        static public Routine StartDelay(Action inAction, float inSeconds)
        {
            return Start(Delay(inAction, inSeconds));
        }

        /// <summary>
        /// Executes an action with an argument after the given number of seconds.
        /// </summary>
        static public Routine StartDelay<T>(Action<T> inAction, T inArg, float inSeconds)
        {
            return Start(Delay(inAction, inArg, inSeconds));
        }

        /// <summary>
        /// Executes after the given number of seconds.
        /// </summary>
        static public Routine StartDelay(IEnumerator inRoutine, float inSeconds)
        {
            return Start(Delay(inRoutine, inSeconds));
        }

        /// <summary>
        /// Executes an action after the given number of seconds.
        /// </summary>
        static public Routine StartDelay(MonoBehaviour inHost, Action inAction, float inSeconds)
        {
            return Start(inHost, Delay(inAction, inSeconds));
        }

        /// <summary>
        /// Executes an action with an argument after the given number of seconds.
        /// </summary>
        static public Routine StartDelay<T>(MonoBehaviour inHost, Action<T> inAction, T inArg, float inSeconds)
        {
            return Start(inHost, Delay(inAction, inArg, inSeconds));
        }

        /// <summary>
        /// Executes after the given number of seconds.
        /// </summary>
        static public Routine StartDelay(MonoBehaviour inHost, IEnumerator inRoutine, float inSeconds)
        {
            return Start(inHost, Delay(inRoutine, inSeconds));
        }

        /// <summary>
        /// Returns an IEnumerator that executes an action after the given number of seconds.
        /// </summary>
        static public IEnumerator Delay(Action inAction, float inSeconds)
        {
            if (inSeconds > 0)
                yield return inSeconds;
            inAction();
        }

        /// <summary>
        /// Returns an IEnumerator that executes an action after the given number of seconds.
        /// </summary>
        static public IEnumerator Delay<T>(Action<T> inAction, T inArg, float inSeconds)
        {
            if (inSeconds > 0)
                yield return inSeconds;
            inAction(inArg);
        }

        /// <summary>
        /// Returns an IEnumerator that executes a routine after the given number of seconds.
        /// </summary>
        static public IEnumerator Delay(IEnumerator inRoutine, float inSeconds)
        {
            if (inSeconds > 0)
                yield return inSeconds;
            yield return inRoutine;
        }

        #endregion

        #region Call

        /// <summary>
        /// Executes an action at the end of the frame.
        /// </summary>
        static public Routine StartCall(Action inAction)
        {
            return Start(CallRoutine(inAction));
        }

        /// <summary>
        /// Executes an action with an argument at the end of the frame.
        /// </summary>
        static public Routine StartCall<T>(Action<T> inAction, T inArg)
        {
            return Start(CallRoutine(inAction, inArg));
        }

        /// <summary>
        /// Executes an action at the end of the frame.
        /// </summary>
        static public Routine StartCall(MonoBehaviour inHost, Action inAction)
        {
            return Start(inHost, CallRoutine(inAction));
        }

        /// <summary>
        /// Executes an action with an argument at the end of the frame.
        /// </summary>
        static public Routine StartCall<T>(MonoBehaviour inHost, Action<T> inAction, T inArg)
        {
            return Start(inHost, CallRoutine(inAction, inArg));
        }

        #region Internal

        static private IEnumerator CallRoutine(Action inAction)
        {
            if (inAction != null)
                inAction();
            yield break;
        }

        static private IEnumerator CallRoutine<T>(Action<T> inAction, T inArg)
        {
            if (inAction != null)
                inAction(inArg);
            yield break;
        }

        #endregion

        #endregion

        #region Loop

        /// <summary>
        /// Executes an action once per frame indefinitely.
        /// </summary>
        static public Routine StartLoop(Action inAction)
        {
            return Start(LoopedRoutine(inAction));
        }

        /// <summary>
        /// Executes an action with an argument once per frame indefinitely.
        /// </summary>
        static public Routine StartLoop<T>(Action<T> inAction, T inArg)
        {
            return Start(LoopedRoutine(inAction, inArg));
        }

        /// <summary>
        /// Executes a routine indefinitely.
        /// </summary>
        static public Routine StartLoopRoutine(Func<IEnumerator> inEnumerator)
        {
            return Start(LoopedRoutine(inEnumerator));
        }

        /// <summary>
        /// Executes an action once per frame indefinitely.
        /// </summary>
        static public Routine StartLoop(MonoBehaviour inHost, Action inAction)
        {
            return Start(inHost, LoopedRoutine(inAction));
        }

        /// <summary>
        /// Executes an action with an argument once per frame indefinitely.
        /// </summary>
        static public Routine StartLoop<T>(MonoBehaviour inHost, Action<T> inAction, T inArg)
        {
            return Start(inHost, LoopedRoutine(inAction, inArg));
        }

        /// <summary>
        /// Executes a routine indefinitely.
        /// </summary>
        static public Routine StartLoopRoutine(MonoBehaviour inHost, Func<IEnumerator> inEnumerator)
        {
            return Start(inHost, LoopedRoutine(inEnumerator));
        }

        #region Internal

        static private IEnumerator LoopedRoutine(Action inAction)
        {
            while (true)
            {
                inAction();
                yield return null;
            }
        }

        static private IEnumerator LoopedRoutine<T>(Action<T> inAction, T inArg)
        {
            while (true)
            {
                inAction(inArg);
                yield return null;
            }
        }

        static private IEnumerator LoopedRoutine(Func<IEnumerator> inRoutine)
        {
            while(true)
                yield return inRoutine();
        }

        #endregion

        #endregion

        #region Per Second

        /// <summary>
        /// Returns an IEnumerator that executes an action a certain number of times per second.
        /// </summary>
        static public IEnumerator PerSecond(Action inAction, float inTimesPerSecond)
        {
            float accumulation = 0;
            while (true)
            {
                accumulation += Routine.DeltaTime * inTimesPerSecond;
                while (accumulation > 1)
                {
                    inAction();
                    --accumulation;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Returns an IEnumerator that executes an action with an argument a certain number of times per second.
        /// </summary>
        static public IEnumerator PerSecond<T>(Action<T> inAction, T inArg, float inTimesPerSecond)
        {
            float accumulation = 0;
            while (true)
            {
                accumulation += Routine.DeltaTime * inTimesPerSecond;
                while (accumulation > 1)
                {
                    inAction(inArg);
                    --accumulation;
                }
                yield return null;
            }
        }

        #endregion
    }
}

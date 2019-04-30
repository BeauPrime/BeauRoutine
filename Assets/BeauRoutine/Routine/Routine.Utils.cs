/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Utils.cs
 * Purpose: Set of utility functions for both running Routines
 *          and creating routine functions for common tasks.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using BeauRoutine.Internal;
using UnityEngine;

namespace BeauRoutine
{
    public partial struct Routine
    {
        /// <summary>
        /// Delegate used for exception handling within a routine.
        /// </summary>
        public delegate void ExceptionHandler(Exception inException);

        #region Time

        /// <summary>
        /// Global time scale.
        /// </summary>
        static public float TimeScale
        {
            get
            {
                Manager m = Manager.Get();
                if (m != null)
                    return m.TimeScale;
                return 1.0f;
            }
            set
            {
                Manager m = Manager.Get();
                if (m != null)
                    m.TimeScale = value;
            }
        }

        /// <summary>
        /// Current delta time.
        /// </summary>
        static public float DeltaTime
        {
            get
            {
                Manager m = Manager.Get();
                if (m != null)
                    return m.Frame.DeltaTime;
                return 0.0f;
            }
        }

        /// <summary>
        /// Raw delta time.
        /// </summary>
        static public float UnscaledDeltaTime
        {
            get
            {
                Manager m = Manager.Get();
                if (m != null)
                    return m.Frame.UnscaledDeltaTime;
                return 0.0f;
            }
        }

        /// <summary>
        /// Calculates the time scale for the given object.
        /// </summary>
        static public float CalculateTimeScale(GameObject inObject)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                RoutineIdentity identity = RoutineIdentity.Find(inObject);
                if (identity != null)
                    return m.GetTimescale(identity.Group) * m.TimeScale * identity.TimeScale;
                return m.TimeScale;
            }
            return 1.0f;
        }

        /// <summary>
        /// Calculates the time scale for the given object.
        /// </summary>
        static public float CalculateTimeScale(RoutineIdentity inIdentity)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                if (inIdentity != null)
                    return m.GetTimescale(inIdentity.Group) * m.TimeScale * inIdentity.TimeScale;
                return m.TimeScale;
            }
            return 1.0f;
        }

        /// <summary>
        /// Calculates the time scale for the given object.
        /// </summary>
        static public float CalculateTimeScale(MonoBehaviour inBehaviour)
        {
            return CalculateTimeScale(inBehaviour.gameObject);
        }

        #endregion

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
            Stop,

            /// <summary>
            /// Exits the current enumerator and executes
            /// its caller immediately
            /// </summary>
            BreakAndResume,
        }

        static private readonly WaitForFixedUpdate s_CachedWaitForFixedUpdate = new WaitForFixedUpdate();
        static private readonly WaitForEndOfFrame s_CachedWaitForEndOfFrame = new WaitForEndOfFrame();
        static private readonly WaitForLateUpdate s_CachedWaitForLateUpdate = new WaitForLateUpdate();
        static private readonly WaitForUpdate s_CachedWaitForUpdate = new WaitForUpdate();
        static private readonly WaitForCustomUpdate s_CachedWaitForCustomUpdate = new WaitForCustomUpdate();
        static private readonly WaitForThinkUpdate s_CachedWaitForThinkUpdate = new WaitForThinkUpdate();
        static private readonly WaitForRealtimeUpdate s_CachedWaitForRealtimeUpdate = new WaitForRealtimeUpdate();

        /// <summary>
        /// Waits for a FixedUpdate to occur.
        /// </summary>
        static public WaitForFixedUpdate WaitForFixedUpdate()
        {
            return s_CachedWaitForFixedUpdate;
        }

        /// <summary>
        /// Waits until rendering is complete.
        /// </summary>
        static public WaitForEndOfFrame WaitForEndOfFrame()
        {
            return s_CachedWaitForEndOfFrame;
        }

        /// <summary>
        /// Waits until after the LateUpdate phase.
        /// </summary>
        static public IEnumerator WaitForLateUpdate()
        {
            return s_CachedWaitForLateUpdate;
        }

        /// <summary>
        /// Waits until after the Update phase.
        /// </summary>
        static public IEnumerator WaitForUpdate()
        {
            return s_CachedWaitForUpdate;
        }

        /// <summary>
        /// Waits until after the CustomUpdate phase.
        /// </summary>
        static public IEnumerator WaitForCustomUpdate()
        {
            return s_CachedWaitForCustomUpdate;
        }

        /// <summary>
        /// Waits until after the ThinkUpdate phase.
        /// </summary>
        static public IEnumerator WaitForThinkUpdate()
        {
            return s_CachedWaitForThinkUpdate;
        }

        /// <summary>
        /// Waits until after the RealtimeUpdate phase.
        /// </summary>
        /// <returns></returns>
        static public IEnumerator WaitForRealtimeUpdate()
        {
            return s_CachedWaitForRealtimeUpdate;
        }

        /// <summary>
        /// Executes the enclosed coroutine immediately when yielded.
        /// Will resume its caller immediately once complete.
        /// </summary>
        static public IEnumerator Inline(IEnumerator inEnumerator)
        {
            RoutineDecorator decorator;
            if (inEnumerator is RoutineDecorator)
            {
                decorator = (RoutineDecorator) inEnumerator;
            }
            else
            {
                decorator = new RoutineDecorator();
                decorator.Enumerator = inEnumerator;
            }

            decorator.Flags |= RoutineDecoratorFlag.Inline;
            return decorator;
        }

        #endregion

        #region Waiting

        /// <summary>
        /// Returns an IEnumerator that waits for the given number of frames.
        /// </summary>
        static public IEnumerator WaitFrames(int inFrames)
        {
            while (--inFrames > 0)
                yield return null;
        }

        /// <summary>
        /// Returns an IEnumerator that waits for the given number of scaled seconds.
        /// </summary>
        static public IEnumerator WaitSeconds(float inSeconds)
        {
            return Yield(inSeconds);
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
            object boxedTime = (inInterval > 0 ? (object) inInterval : null);
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

        /// <summary>
        /// Returns an IEnumerator that counts down from a specific time.
        /// </summary>
        static public IEnumerator Timer(float inSeconds, Action<float> inOnUpdate)
        {
            while (true)
            {
                inSeconds -= Routine.DeltaTime;
                if (inSeconds < 0)
                    inSeconds = 0;

                if (inOnUpdate != null)
                    inOnUpdate(inSeconds);
                if (inSeconds > 0)
                    yield return null;
                else
                    break;
            }
        }

        /// <summary>
        /// Returns an IEnumerator that counts up to the specific time.
        /// </summary>
        static public IEnumerator Accumulate(float inSeconds, Action<float> inOnUpdate)
        {
            float acc = 0;
            while (true)
            {
                acc += Routine.DeltaTime;
                if (acc > inSeconds)
                    acc = inSeconds;

                if (inOnUpdate != null)
                    inOnUpdate(acc);
                if (acc < inSeconds)
                    yield return null;
                else
                    break;
            }
        }

        #endregion

        #region Combine/Race

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
            int firstNonNull = -1;
            for (int i = 0; i < inRoutines.Length; ++i)
            {
                if (inRoutines[i] != null)
                {
                    ++nonNullCount;
                    if (nonNullCount == 1)
                        firstNonNull = i;
                }
            }

            if (nonNullCount == 0)
                return null;
            if (nonNullCount == 1)
                return inRoutines[firstNonNull];
            return CreateParallel(new List<IEnumerator>(inRoutines), false);
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
            int firstNonNull = -1;
            for (int i = 0; i < inRoutines.Count; ++i)
            {
                if (inRoutines[i] != null)
                {
                    ++nonNullCount;
                    if (nonNullCount == 1)
                        firstNonNull = i;
                }
            }

            if (nonNullCount == 0)
                return null;
            if (nonNullCount == 1)
                return inRoutines[firstNonNull];
            return CreateParallel(new List<IEnumerator>(inRoutines), false);
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
            int firstNonNull = -1;
            for (int i = 0; i < inRoutines.Length; ++i)
            {
                if (inRoutines[i] != null)
                {
                    ++nonNullCount;
                    if (nonNullCount == 1)
                        firstNonNull = i;
                }
            }

            if (nonNullCount == 0)
                return null;
            if (nonNullCount == 1)
                return inRoutines[firstNonNull];
            return CreateParallel(new List<IEnumerator>(inRoutines), true);
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
            int firstNonNull = -1;
            for (int i = 0; i < inRoutines.Count; ++i)
            {
                if (inRoutines[i] != null)
                {
                    ++nonNullCount;
                    if (nonNullCount == 1)
                        firstNonNull = i;
                }
            }

            if (nonNullCount == 0)
                return null;
            if (nonNullCount == 1)
                return inRoutines[firstNonNull];
            return CreateParallel(new List<IEnumerator>(inRoutines), true);
        }

        // Runs all the given IEnumerators
        static private ParallelFibers CreateParallel(List<IEnumerator> inEnumerators, bool inbRace)
        {
            Manager m = Manager.Get();
            if (m != null)
                return new ParallelFibers(m, inEnumerators, inbRace);
            return null;
        }

        static private ParallelFibers CreateEmptyParallel(bool inbRace)
        {
            Manager m = Manager.Get();
            if (m != null)
                return new ParallelFibers(m, new List<IEnumerator>(), inbRace);
            return null;
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
            return new DelayedIEnumerator(inRoutine, inSeconds);
        }

        // Delays executing an IEnumerator by a certain amount of seconds
        private class DelayedIEnumerator : IEnumerator, IDisposable
        {
            private IEnumerator m_Enumerator;
            private float m_Delay;
            private object m_Yielding = false;

            public DelayedIEnumerator(IEnumerator inRoutine, float inSeconds)
            {
                m_Enumerator = inRoutine;
                m_Delay = inSeconds;
            }

            public object Current
            {
                get { return m_Yielding; }
            }

            public void Dispose()
            {
                DisposeUtils.DisposeEnumerator(ref m_Enumerator);
                m_Delay = 0;
            }

            public bool MoveNext()
            {
                if (m_Enumerator == null)
                    return false;

                if (m_Delay > 0)
                {
                    m_Yielding = m_Delay;
                    m_Delay = 0;
                    return true;
                }

                m_Yielding = Routine.Inline(m_Enumerator);
                m_Enumerator = null;
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public override string ToString()
            {
                return "Routine::Delay()";
            }
        }

        #endregion

        #region Call

        /// <summary>
        /// Executes an action at the end of the frame.
        /// </summary>
        static public Routine StartCall(Action inAction)
        {
            return Start(Call(inAction));
        }

        /// <summary>
        /// Executes an action with an argument at the end of the frame.
        /// </summary>
        static public Routine StartCall<T>(Action<T> inAction, T inArg)
        {
            return Start(Call(inAction, inArg));
        }

        /// <summary>
        /// Executes an action at the end of the frame.
        /// </summary>
        static public Routine StartCall(MonoBehaviour inHost, Action inAction)
        {
            return Start(inHost, Call(inAction));
        }

        /// <summary>
        /// Executes an action with an argument at the end of the frame.
        /// </summary>
        static public Routine StartCall<T>(MonoBehaviour inHost, Action<T> inAction, T inArg)
        {
            return Start(inHost, Call(inAction, inArg));
        }

        /// <summary>
        /// Returns an IEnumerator that executes the given action.
        /// </summary>
        static public IEnumerator Call(Action inAction)
        {
            if (inAction != null)
                inAction();
            yield break;
        }

        /// <summary>
        /// Returns an IEnumerator that executes the given action.
        /// </summary>
        static public IEnumerator Call<T>(Action<T> inAction, T inArg)
        {
            if (inAction != null)
                inAction(inArg);
            yield break;
        }

        #endregion

        #region Loop

        /// <summary>
        /// Executes an action once per frame indefinitely.
        /// </summary>
        static public Routine StartLoop(Action inAction)
        {
            if (inAction == null)
                return Routine.Null;
            return Start(LoopedRoutine(inAction));
        }

        /// <summary>
        /// Executes an action with an argument once per frame indefinitely.
        /// </summary>
        static public Routine StartLoop<T>(Action<T> inAction, T inArg)
        {
            if (inAction == null)
                return Routine.Null;
            return Start(LoopedRoutine(inAction, inArg));
        }

        /// <summary>
        /// Executes a routine indefinitely.
        /// </summary>
        static public Routine StartLoopRoutine(Func<IEnumerator> inEnumerator)
        {
            if (inEnumerator == null)
                return Routine.Null;
            return Start(LoopedRoutine(inEnumerator));
        }

        /// <summary>
        /// Executes an action once per frame indefinitely.
        /// </summary>
        static public Routine StartLoop(MonoBehaviour inHost, Action inAction)
        {
            if (inAction == null)
                return Routine.Null;
            return Start(inHost, LoopedRoutine(inAction));
        }

        /// <summary>
        /// Executes an action with an argument once per frame indefinitely.
        /// </summary>
        static public Routine StartLoop<T>(MonoBehaviour inHost, Action<T> inAction, T inArg)
        {
            if (inAction == null)
                return Routine.Null;
            return Start(inHost, LoopedRoutine(inAction, inArg));
        }

        /// <summary>
        /// Executes a routine indefinitely.
        /// </summary>
        static public Routine StartLoopRoutine(MonoBehaviour inHost, Func<IEnumerator> inEnumerator)
        {
            if (inEnumerator == null)
                return Routine.Null;
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
            while (true)
                yield return Routine.Inline(inRoutine());
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
                if (accumulation >= 1)
                {
                    // Ensure DeltaTime reflects this interval
                    Manager m = Manager.Get();
                    float oldUnscaledDeltaTime = m.Frame.UnscaledDeltaTime;
                    m.Frame.UnscaledDeltaTime = 1f / inTimesPerSecond;
                    m.Frame.RefreshTimeScale();

                    while (accumulation >= 1)
                    {
                        inAction();
                        --accumulation;
                    }

                    // Reset DeltaTime
                    m.Frame.UnscaledDeltaTime = oldUnscaledDeltaTime;
                    m.Frame.RefreshTimeScale();
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
                if (accumulation >= 1)
                {
                    // Ensure DeltaTime reflects this interval
                    Manager m = Manager.Get();
                    float oldUnscaledDeltaTime = m.Frame.UnscaledDeltaTime;
                    m.Frame.UnscaledDeltaTime = 1f / inTimesPerSecond;
                    m.Frame.RefreshTimeScale();

                    while (accumulation >= 1)
                    {
                        inAction(inArg);
                        --accumulation;
                    }

                    // Reset DeltaTime
                    m.Frame.UnscaledDeltaTime = oldUnscaledDeltaTime;
                    m.Frame.RefreshTimeScale();
                }
                yield return null;
            }
        }

        /// <summary>
        /// Returns an IEnumerator that attempts to update the given routine a certain number of times per frame.
        /// </summary>
        static public IEnumerator PerSecond(IEnumerator inRoutine, float inTimesPerSecond)
        {
            return new IntervalFiber(Manager.Get(), inRoutine, 1f / inTimesPerSecond);
        }

        #endregion

        #region For Each

        /// <summary>
        /// Executes, in order, an enumerator function for every element in the given enumerable.
        /// </summary>
        static public IEnumerator ForEach<T>(IEnumerable<T> inEnumerable, Func<T, IEnumerator> inOperation)
        {
            if (inEnumerable != null && inOperation != null)
            {
                foreach (var obj in inEnumerable)
                    yield return inOperation(obj);
            }
        }

        /// <summary>
        /// Executes, in order, an enumerator function for every element in the given enumerable.
        /// </summary>
        static public IEnumerator ForEach<T>(IEnumerable<T> inEnumerable, Func<int, T, IEnumerator> inOperation)
        {
            if (inEnumerable != null && inOperation != null)
            {
                int idx = 0;
                foreach (var obj in inEnumerable)
                    yield return inOperation(idx++, obj);
            }
        }

        /// <summary>
        /// Executes, in parallel, an enumerator function for every element in the given enumerable.
        /// </summary>
        static public IEnumerator ForEachParallel<T>(IEnumerable<T> inEnumerable, Func<T, IEnumerator> inOperation)
        {
            if (inEnumerable != null && inOperation != null)
            {
                ParallelFibers combine = CreateEmptyParallel(false);
                foreach (var obj in inEnumerable)
                {
                    combine.AddEnumerator(inOperation(obj));
                }
                yield return Routine.Inline(combine);
            }
        }

        /// <summary>
        /// Executes, in parallel, an enumerator function for every element in the given enumerable.
        /// </summary>
        static public IEnumerator ForEachParallel<T>(IEnumerable<T> inEnumerable, Func<int, T, IEnumerator> inOperation)
        {
            if (inEnumerable != null && inOperation != null)
            {
                ParallelFibers combine = CreateEmptyParallel(false);
                int idx = 0;
                foreach (var obj in inEnumerable)
                {
                    combine.AddEnumerator(inOperation(idx++, obj));
                }
                yield return Routine.Inline(combine);
            }
        }

        /// <summary>
        /// Executes, in parallel chunks, an enumerator function for every element in the given list.
        /// </summary>
        static public IEnumerator ForEachParallel<T>(IEnumerable<T> inEnumerable, Func<T, IEnumerator> inOperation, int inChunkSize)
        {
            if (inEnumerable != null && inOperation != null)
            {
                IEnumerator[] block = new IEnumerator[inChunkSize];
                int numItems = 0;
                foreach (var obj in inEnumerable)
                {
                    IEnumerator operation = inOperation(obj);
                    if (operation != null)
                    {
                        block[numItems++] = operation;

                        if (numItems == inChunkSize)
                        {
                            yield return Routine.Inline(
                                Routine.Combine(block)
                            );

                            for (int i = numItems - 1; i >= 0; --i)
                                block[i] = null;
                            numItems = 0;
                        }
                    }
                }

                if (numItems > 0)
                {
                    yield return Routine.Inline(
                        Routine.Combine(block)
                    );

                    for (int i = numItems - 1; i >= 0; --i)
                        block[i] = null;
                    numItems = 0;
                }
            }
        }

        /// <summary>
        /// Executes, in parallel chunks, an enumerator function for every element in the given enumerable.
        /// </summary>
        static public IEnumerator ForEachParallel<T>(IEnumerable<T> inEnumerable, Func<int, T, IEnumerator> inOperation, int inChunkSize)
        {
            if (inEnumerable != null && inOperation != null)
            {
                IEnumerator[] block = new IEnumerator[inChunkSize];
                int numItems = 0;
                int idx = 0;
                foreach (var obj in inEnumerable)
                {
                    IEnumerator operation = inOperation(idx++, obj);
                    if (operation != null)
                    {
                        block[numItems++] = operation;

                        if (numItems == inChunkSize)
                        {
                            yield return Routine.Inline(
                                Routine.Combine(block)
                            );

                            for (int i = numItems - 1; i >= 0; --i)
                                block[i] = null;
                            numItems = 0;
                        }
                    }
                }

                if (numItems > 0)
                {
                    yield return Routine.Inline(
                        Routine.Combine(block)
                    );

                    for (int i = numItems - 1; i >= 0; --i)
                        block[i] = null;
                    numItems = 0;
                }
            }
        }

        #endregion

        #region Yield

        /// <summary>
        /// Wraps an object into a yieldable IEnumerator.
        /// This will yield the given object once.
        /// </summary>
        static public IEnumerator Yield(object inObject)
        {
            if (inObject == null)
                return null;

            IEnumerator enumerator = inObject as IEnumerator;
            if (enumerator != null)
                return Routine.Inline(enumerator);
            return Routine.Inline(new YieldableObject(inObject));
        }

        /// <summary>
        /// Wraps a sequence of objects into a yieldable IEnumerator.
        /// This will yield each given object once.
        /// </summary>
        static public IEnumerator Yield(params object[] inObjects)
        {
            if (inObjects == null || inObjects.Length == 0)
                return null;

            if (inObjects.Length == 1)
                return Yield(inObjects[0]);

            return Routine.Inline(new YieldableArray((object[]) inObjects.Clone()));
        }

        /// <summary>
        /// Wraps a sequence of objects into a yieldable IEnumerator.
        /// This will yield each given object once.
        /// </summary>
        static public IEnumerator Yield(IList<object> inObjects)
        {
            if (inObjects == null || inObjects.Count == 0)
                return null;

            if (inObjects.Count == 1)
                return Yield(inObjects[0]);

            object[] objects = new object[inObjects.Count];
            inObjects.CopyTo(objects, 0);
            return Routine.Inline(new YieldableArray(objects));
        }

        // Yields a value
        private class YieldableObject : IEnumerator, IDisposable
        {
            private object m_Object;
            private bool m_Finished;

            public object Current { get { return m_Object; } }

            public YieldableObject(object inObject)
            {
                m_Object = inObject;
                m_Finished = false;
            }

            public void Dispose()
            {
                DisposeUtils.DisposeObject(ref m_Object);
                m_Finished = true;
            }

            public bool MoveNext()
            {
                if (!m_Finished)
                {
                    m_Finished = true;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public override string ToString()
            {
                return "Routine::Yield()";
            }
        }

        // Yields a set of values in sequence
        private class YieldableArray : IEnumerator, IDisposable
        {
            private object[] m_Objects;
            private int m_Index = -1;

            public object Current { get { return m_Objects[m_Index]; } }

            public YieldableArray(object[] inObjects)
            {
                m_Objects = inObjects;
            }

            public void Dispose()
            {
                if (m_Objects != null)
                {
                    for (int i = m_Objects.Length - 1; i >= 0; --i)
                        DisposeUtils.DisposeObject(ref m_Objects[i]);
                    m_Objects = null;
                }

                m_Index = -1;
            }

            public bool MoveNext()
            {
                return (++m_Index >= m_Objects.Length);
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public override string ToString()
            {
                return "Routine::Yield()";
            }
        }

        #endregion
    }
}
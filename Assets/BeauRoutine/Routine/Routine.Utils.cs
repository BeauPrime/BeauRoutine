/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Utils.cs
 * Purpose: Set of utility functions for both running Routines
 *          and creating routine functions for common tasks.
*/

using BeauRoutine.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine
{
    public partial struct Routine
    {
        #region Time

        /// <summary>
        /// Global time scale.
        /// </summary>
        static public float TimeScale
        {
            get
            {
                Manager m = GetManager();
                if (m != null)
                    return m.TimeScale;
                return 1.0f;
            }
            set
            {
                Manager m = GetManager();
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
                Manager m = GetManager();
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
                Manager m = GetManager();
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
            Manager m = GetManager();
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
            Manager m = GetManager();
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

        /// <summary>
        /// Executes the enclosed coroutine immediately when yielded.
        /// Will resume its caller immediately once complete.
        /// </summary>
        static public IEnumerator Inline(IEnumerator inEnumerator)
        {
            RoutineDecorator decorator;
            if (inEnumerator is RoutineDecorator)
            {
                decorator = (RoutineDecorator)inEnumerator;
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
            return Routine.Inline(WaitSecondsImpl(inSeconds));
        }

        // Implements waiting for a given number of seconds
        // This is more accurate than just yielding those seconds in WaitSeconds,
        // since otherwise there is a frame delay
        static private IEnumerator WaitSecondsImpl(float inSeconds)
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

        /// <summary>
        /// Returns an IEnumerator that counts down from a specific time.
        /// </summary>
        static public IEnumerator Timer(float inSeconds, Action<float> inOnUpdate)
        {
            while(true)
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
            for(int i = 0; i < inRoutines.Length; ++i)
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

        /// <summary>
        /// Runs all the given IEnumerators
        /// </summary>
        static private IEnumerator CreateParallel(List<IEnumerator> inEnumerators, bool inbRace)
        {
            Manager m = GetManager();
            if (m != null)
                return new ParallelFibers(m, inEnumerators, inbRace);
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
            if (inSeconds > 0)
                yield return inSeconds;
            yield return Routine.Inline(inRoutine);
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

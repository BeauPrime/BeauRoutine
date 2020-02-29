/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.cs
 * Purpose: A safe handle pointing at an executing Routine Fiber.
 *          Helps prevent memory leaks and dangling references.
*/

using BeauRoutine.Internal;
using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Custom coroutines.
    /// </summary>
    public partial struct Routine : IEquatable<Routine>, IDisposable
    {
        private uint m_Value;

        /// <summary>
        /// Generates the Routine handle with the given ID.
        /// </summary>
        private Routine(uint inValue)
        {
            m_Value = inValue;
        }

        /// <summary>
        /// Default Routine. Points to no routine.
        /// </summary>
        static public Routine Null
        {
            get { return s_Null; }
        }

        static private Routine s_Null = default(Routine);

        #region Status

        /// <summary>
        /// Returns an IEnumerator that waits for the routine to finish.
        /// </summary>
        public IEnumerator Wait()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    return fiber.Wait();
            }
            return null;
        }

        /// <summary>
        /// Returns if the routine exists.
        /// </summary>
        public bool Exists()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                return m_Value > 0 && m.Fibers[this] != null;
            }
            return false;
        }

        #endregion

        #region Flow Control

        /// <summary>
        /// Pauses the routine.
        /// </summary>
        public Routine Pause()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.Pause();
            }
            return this;
        }

        /// <summary>
        /// Resumes the routine.
        /// </summary>
        public Routine Resume()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.Resume();
            }
            return this;
        }

        /// <summary>
        /// Returns if the routine is paused.
        /// </summary>
        public bool GetPaused()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    return fiber.GetPaused();
            }
            return false;
        }

        /// <summary>
        /// Stops the routine and clears the pointer.
        /// </summary>
        public Routine Stop()
        {
            Manager m = Manager.Get();
            if (m != null && m_Value != 0)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.Stop();
                m_Value = 0;
            }
            return this;
        }

        /// <summary>
        /// Delays the routine by the given number of seconds.
        /// </summary>
        public Routine DelayBy(float inSeconds)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.AddDelay(inSeconds);
            }
            return this;
        }

        /// <summary>
        /// Returns the time scaling on the routine.
        /// </summary>
        public float GetTimeScale()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                return fiber == null ? 1.0f : fiber.TimeScale;
            }
            return 1.0f;
        }

        /// <summary>
        /// Sets the time scaling on the routine.
        /// </summary>
        public Routine SetTimeScale(float inValue)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.TimeScale = inValue;
            }
            return this;
        }

        /// <summary>
        /// Disables per-object time scaling
        /// for this routine.
        /// </summary>
        public Routine DisableObjectTimeScale()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.IgnoreObjectTimeScale();
            }
            return this;
        }

        /// <summary>
        /// Re-enables per-object time scaling
        /// for this routine.
        /// </summary>
        public Routine EnableObjectTimeScale()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.UseObjectTimeScale();
            }
            return this;
        }

        /// <summary>
        /// The Routine will execute even while the object is disabled.
        /// </summary>
        public Routine ExecuteWhileDisabled()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.IgnoreObjectActive();
            }
            return this;
        }

        /// <summary>
        /// The Routine will execute only while the object is enabled.
        /// This is default behavior.
        /// </summary>
        public Routine ExecuteWhileEnabled()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.UseObjectActive();
            }
            return this;
        }

        /// <summary>
        /// Returns the optional name
        /// for this routine.
        /// </summary>
        public string GetName()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    return fiber.Name;
            }
            return null;
        }

        /// <summary>
        /// Sets the optional name
        /// for this routine.
        /// </summary>
        public Routine SetName(string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.Name = inName;
            }
            return this;
        }

        /// <summary>
        /// Returns the execution priority
        /// for this routine.
        /// </summary>
        public int GetPriority()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    return fiber.Priority;
            }
            return 0;
        }

        /// <summary>
        /// Sets the execution priority for this routine.
        /// Routines with a greater priority are executed
        /// first.
        /// </summary>
        public Routine SetPriority(int inPriority)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.SetPriority(inPriority);
            }
            return this;
        }

        /// <summary>
        /// Gets the update timing for the routine.
        /// </summary>
        public RoutinePhase GetPhase()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    return fiber.GetPhase();
                return m.DefaultPhase;
            }
            return RoutinePhase.LateUpdate;
        }

        /// <summary>
        /// Sets the update timing for the routine.
        /// Note that if this update is currently running,
        /// this routine will not execute until the next update.
        /// </summary>
        public Routine SetPhase(RoutinePhase inUpdate)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.SetPhase(inUpdate);
            }
            return this;
        }

        /// <summary>
        /// Attempts to manually update the given routine.
        /// </summary>
        public bool TryManuallyUpdate()
        {
            return TryManuallyUpdate(Time.deltaTime);
        }

        /// <summary>
        /// Attempts to manually update the given routine
        /// by the given delta time.
        /// </summary>
        public bool TryManuallyUpdate(float inDeltaTime)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                {
                    return m.ManualUpdate(fiber, inDeltaTime);
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a lock on the routine.
        /// Locked routines will not execute while locked.
        /// Dispose or release the RoutineLock to unlock.
        /// </summary>
        public RoutineLock GetLock()
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                {
                    fiber.AddLock();
                    return new RoutineLock(this);
                }
            }
            return default(RoutineLock);
        }

        #endregion

        #region Replace

        /// <summary>
        /// Stops the current routine and runs another routine.
        /// </summary>
        public Routine Replace(IEnumerator inNewRoutine)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.Stop();
                m_Value = m.RunFiber(null, inNewRoutine).m_Value;
            }
            return this;
        }

        /// <summary>
        /// Stops the current routine and runs another routine.
        /// </summary>
        public Routine Replace(MonoBehaviour inHost, IEnumerator inNewRoutine)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.Stop();
                m_Value = m.RunFiber(inHost, inNewRoutine).m_Value;
            }
            return this;
        }

        /// <summary>
        /// Stops the current routine and points to the given routine.
        /// </summary>
        public Routine Replace(Routine inRoutine)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.Stop();
                m_Value = inRoutine.m_Value;
            }
            return this;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Registers a function to be called when the
        /// routine completes successfully.
        /// </summary>
        public Routine OnComplete(Action inCallback)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.OnComplete(inCallback);
            }
            return this;
        }

        /// <summary>
        /// Registers a function to be called when the
        /// routine exits prematurely.
        /// </summary>
        public Routine OnStop(Action inCallback)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.OnStop(inCallback);
            }
            return this;
        }

        /// <summary>
        /// Registers a function to be called if the
        /// routine encounters an exception.
        /// </summary>
        public Routine OnException(ExceptionHandler inCallback)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.OnException(inCallback);
            }
            return this;
        }

        #endregion

        #region Overrides

        public void Dispose()
        {
            Stop();
        }

        public bool Equals(Routine other)
        {
            return m_Value == other.m_Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Routine)
                return Equals((Routine)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return (int)m_Value;
        }

        static public implicit operator bool(Routine inHandle)
        {
            return inHandle.Exists();
        }

        static public explicit operator uint(Routine inHandle)
        {
            return inHandle.m_Value;
        }

        static public explicit operator Routine(uint inValue)
        {
            return new Routine(inValue);
        }

        static public bool operator==(Routine first, Routine second)
        {
            return first.m_Value == second.m_Value;
        }

        static public bool operator !=(Routine first, Routine second)
        {
            return first.m_Value != second.m_Value;
        }

        #endregion
    }
}

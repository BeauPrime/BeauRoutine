/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
    /// Custom Coroutine implementation.
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
            Manager m = GetManager();
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
            Manager m = GetManager();
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
            Manager m = GetManager();
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
            Manager m = GetManager();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.Resume();
            }
            return this;
        }

        /// <summary>
        /// Stops the routine and clears the pointer.
        /// </summary>
        public Routine Stop()
        {
            Manager m = GetManager();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.Stop();
                m_Value = 0;
            }
            return this;
        }

        /// <summary>
        /// Gets the time scaling on the routine.
        /// </summary>
        public float GetTimeScale()
        {
            Manager m = GetManager();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                return fiber == null ? 1.0f : fiber.TimeScale;
            }
            return 1.0f;
        }

        /// <summary>
        /// Gets the time scaling on the routine.
        /// </summary>
        public Routine SetTimeScale(float inValue)
        {
            Manager m = GetManager();
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
            Manager m = GetManager();
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
            Manager m = GetManager();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.UseObjectTimeScale();
            }
            return this;
        }

        /// <summary>
        /// Returns the optional name
        /// for this routine.
        /// </summary>
        public string GetName()
        {
            Manager m = GetManager();
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
            Manager m = GetManager();
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
            Manager m = GetManager();
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
            Manager m = GetManager();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.SetPriority(inPriority);
            }
            return this;
        }

        #endregion

        #region Replace

        /// <summary>
        /// Stops the current routine and runs another routine.
        /// </summary>
        public Routine Replace(IEnumerator inNewRoutine)
        {
            Manager m = GetManager();
            if (m != null)
            {
                Stop();
                m_Value = m.RunFiber(null, inNewRoutine).m_Value;
            }
            return this;
        }

        /// <summary>
        /// Stops the current routine and runs another routine.
        /// </summary>
        public Routine Replace(MonoBehaviour inHost, IEnumerator inNewRoutine)
        {
            Manager m = GetManager();
            if (m != null)
            {
                Stop();
                m_Value = m.RunFiber(inHost, inNewRoutine).m_Value;
            }
            return this;
        }

        /// <summary>
        /// Stops the current routine and points to the given routine.
        /// </summary>
        public Routine Replace(Routine inRoutine)
        {
            Manager m = GetManager();
            if (m != null)
            {
                Stop();
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
            Manager m = GetManager();
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
            Manager m = GetManager();
            if (m != null)
            {
                Fiber fiber = m.Fibers[this];
                if (fiber != null)
                    fiber.OnStop(inCallback);
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

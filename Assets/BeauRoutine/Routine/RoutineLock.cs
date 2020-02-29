/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Apr 2018
 * 
 * File:    RoutineLock.cs
 * Purpose: Handle that represents a lock on a Fiber.
            Disposing or releasing will unlock on the Fiber.
*/

using System;
using System.Collections;
using BeauRoutine.Internal;

namespace BeauRoutine
{
    /// <summary>
    /// Represents a lock on a Routine.
    /// Locked Routines will suspend execution
    /// until all locks are removed.
    /// </summary>
    public class RoutineLock : IDisposable, IEnumerator
    {
        private Routine m_Locked;

        internal RoutineLock(Routine inRoutine)
        {
            m_Locked = inRoutine;
        }

        /// <summary>
        /// Releases the lock on the Routine.
        /// </summary>
        public void Release()
        {
            if (m_Locked == Routine.Null)
                return;

            Manager m = Manager.Get();
            if (m != null)
            {
                Fiber f = m.Fibers[m_Locked];
                if (f != null)
                    f.ReleaseLock();
            }

            m_Locked = Routine.Null;
        }

        static public implicit operator bool(RoutineLock inLock)
        {
            return inLock.m_Locked;
        }

        static public implicit operator Routine(RoutineLock inLock)
        {
            return inLock.m_Locked;
        }

        public override string ToString()
        {
            return "RoutineLock";
        }

        #region IDisposable

        void IDisposable.Dispose()
        {
            Release();
        }

        #endregion // IDisposable

        #region IEnumerator

        object IEnumerator.Current { get { return null; } }

        bool IEnumerator.MoveNext()
        {
            return m_Locked;
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        #endregion // IEnumerator
    }
}

/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    6 Jan 2020
 * 
 * File:    AsyncScheduler.cs
 * Purpose: Manager for async utilities.
 */

#if !UNITY_WEBGL
#define SUPPORTS_THREADING
#endif // UNITY_WEBGL

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
#if SUPPORTS_THREADING
using System.Threading;
#endif // SUPPORTS_THREADING

namespace BeauRoutine.Internal
{
    internal sealed class AsyncWorkUnit
    {
        private const ushort ActionCompleteFlag = 0x01;
        private const ushort EnumeratorCompleteFlag = 0x02;
        private const ushort AllCompleteFlag = ActionCompleteFlag | EnumeratorCompleteFlag;

        // state
        private ushort m_Status;
        private ushort m_Serial;
        private Action m_Action;
        private IEnumerator m_Enumerator;

        #if SUPPORTS_THREADING
        private readonly object m_LockContext = null;
        #endif // SUPPORTS_THREADING

        #region Lifecycle

        /// <summary>
        /// Initializes the work unit with new work.
        /// </summary>
        internal void Initialize(Action inAction, IEnumerator inEnumerator)
        {
            m_Status = AllCompleteFlag;

            m_Action = inAction;
            if (m_Action != null)
                m_Status = (ushort) (m_Status & ~ActionCompleteFlag);

            m_Enumerator = inEnumerator;
            if (m_Enumerator != null)
                m_Status = (ushort) (m_Status & ~EnumeratorCompleteFlag);

            if (m_Serial == ushort.MaxValue)
            {
                m_Serial = 1;
            }
            else
            {
                ++m_Serial;
            }
        }

        /// <summary>
        /// Clears the work unit's contents.
        /// </summary>
        internal void Clear()
        {
            m_Status = AllCompleteFlag;
            m_Action = null;
            DisposeUtils.DisposeObject(ref m_Enumerator);
        }

        #endregion // Lifecycle

        #region Status

        /// <summary>
        /// Returns if this work unit is running.
        /// </summary>
        internal bool IsRunning(ushort inSerial)
        {
            return m_Serial == inSerial && m_Status != AllCompleteFlag;
        }

        /// <summary>
        /// Returns a handle to the work unit.
        /// </summary>
        internal AsyncHandle GetHandle()
        {
            return new AsyncHandle(this, m_Serial);
        }

        #endregion // Status

        #region Step

        /// <summary>
        /// Performs a step (not thread-safe).
        /// Returns if work remains.
        /// </summary>
        internal bool Step()
        {
            if ((m_Status & ActionCompleteFlag) == 0)
            {
                m_Action();
                m_Action = null;
                m_Status |= ActionCompleteFlag;
            }
            else if ((m_Status & EnumeratorCompleteFlag) == 0)
            {
                if (!m_Enumerator.MoveNext())
                {
                    DisposeUtils.DisposeObject(ref m_Enumerator);
                    m_Status |= EnumeratorCompleteFlag;
                }
            }

            return m_Status != AllCompleteFlag;
        }

        #if SUPPORTS_THREADING

        /// <summary>
        /// Performs a step (thread-safe).
        /// Returns if work remains.
        /// </summary>
        internal bool ThreadedStep()
        {
            lock(m_LockContext)
            {
                return Step();
            }
        }

        #endif // SUPPORTS_THREADING

        #endregion // Step

        #region Cancel

        // Cancels all future steps
        private void Cancel()
        {
            m_Status = AllCompleteFlag;
        }

        /// <summary>
        /// Attempts to cancel work.
        /// </summary>
        internal void TryCancel(ushort inSerial)
        {
            #if SUPPORTS_THREADING
            lock(m_LockContext)
            {
                if (m_Serial == inSerial)
                {
                    Cancel();
                }
            }
            #else
            if (m_Serial == inSerial)
            {
                Cancel();
            }
            #endif // SUPPORTS_THREADING
        }

        #endregion // Cancel
    }
}
/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Jan 2020
 * 
 * File:    AsyncHandle.cs
 * Purpose: Handle to an async operation.
 */

using System;
using System.Collections;
using BeauRoutine.Internal;

namespace BeauRoutine
{
    /// <summary>
    /// Handle to an asynchronous operation.
    /// </summary>
    public struct AsyncHandle : IEnumerator, IDisposable, IEquatable<AsyncHandle>
    {
        private AsyncWorkUnit m_Unit;
        private readonly ushort m_Serial;

        internal AsyncHandle(AsyncWorkUnit inWork, ushort inSerial)
        {
            m_Unit = inWork;
            m_Serial = inSerial;
        }

        internal AsyncWorkUnit Unit { get { return m_Unit; } }

        /// <summary>
        /// Returns if this handle does not reference an async operation.
        /// </summary>
        public bool IsNull()
        {
            return m_Unit == null;
        }

        /// <summary>
        /// Returns if the async operation is still running.
        /// </summary>
        public bool IsRunning()
        {
            return m_Unit != null && m_Unit.IsRunning(m_Serial);
        }

        /// <summary>
        /// Cancels the async operation.
        /// </summary>
        public void Cancel()
        {
            if (m_Unit != null)
            {
                m_Unit.TryCancel(m_Serial);
                m_Unit = null;
            }
        }

        /// <summary>
        /// Schedules a callback to occur when the async operation completes.
        /// </summary>
        public void OnStop(Action inOnStopCallback)
        {
            if (m_Unit != null)
            {
                m_Unit.OnStopCallback(m_Serial, inOnStopCallback);
            }
        }

        #region Static

        static private AsyncHandle s_Default = default(AsyncHandle);

        /// <summary>
        /// Empty async handle.
        /// </summary>
        static public AsyncHandle Null
        {
            get { return s_Default; }
        }

        #endregion // Static

        #region IEnumerator

        object IEnumerator.Current { get { return null; } }

        bool IEnumerator.MoveNext()
        {
            return IsRunning();
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        #endregion // IEnumerator

        #region IDisposable

        void IDisposable.Dispose()
        {
            Cancel();
        }

        #endregion // IDisposable

        #region IEquatable

        public bool Equals(AsyncHandle other)
        {
            return m_Unit == other.m_Unit &&
                m_Serial == other.m_Serial;
        }

        #endregion // IEquatable

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is AsyncHandle)
            {
                return Equals((AsyncHandle) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = m_Serial.GetHashCode();
            if (m_Unit != null)
            {
                hash = (hash << 6) ^ m_Unit.GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            if (m_Unit != null)
            {
                if (IsRunning())
                {
                    return "[Running]";
                }

                return "[Not Running]";
            }
            if (m_Serial != 0)
            {
                return "[Cancelled]";
            }
            return "[Null]";
        }

        static public bool operator ==(AsyncHandle first, AsyncHandle second)
        {
            return first.Equals(second);
        }

        static public bool operator !=(AsyncHandle first, AsyncHandle second)
        {
            return !first.Equals(second);
        }

        #endregion // Overrides
    }
}
/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Jan 2020
 * 
 * File:    AsyncFlags.cs
 * Purpose: Priority and additional flags for async operations.
 */

using System;

namespace BeauRoutine
{
    /// <summary>
    /// Additional flags for async work
    /// </summary>
    [Flags]
    public enum AsyncFlags : ushort
    {
        /// <summary>
        /// Normal priority work.
        /// </summary>
        NormalPriority = 0x00,

        /// <summary>
        /// Low priority work.
        /// </summary>
        LowPriority = 0x01,

        /// <summary>
        /// High priority work.
        /// </summary>
        HighPriority = 0x02,

        /// <summary>
        /// Only execute this operation on the main thread.
        /// </summary>
        MainThreadOnly = 0x04,

        Default = NormalPriority
    }
}
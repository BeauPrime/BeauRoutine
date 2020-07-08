/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    16 Jan 2020
 * 
 * File:    AsyncSleep.cs
 * Purpose: Command for an async operation to sleep.
 */

using System;

namespace BeauRoutine
{
    /// <summary>
    /// Command for an async operation to sleep.
    /// </summary>
    public struct AsyncSleep
    {
        internal readonly long Ticks;

        public AsyncSleep(long inTicks)
        {
            Ticks = inTicks;
        }

        public AsyncSleep(TimeSpan inTimespan)
        {
            Ticks = inTimespan.Ticks;
        }
    }
}
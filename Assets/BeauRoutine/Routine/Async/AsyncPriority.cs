/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    7 Jan 2020
 * 
 * File:    AsyncPriority.cs
 * Purpose: Priority for async operations.
 */

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Priority for async work.
    /// </summary>
    public enum AsyncPriority
    {
        Low,
        BelowNormal,
        Normal,
        High
    }
}
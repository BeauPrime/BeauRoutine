/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    IRoutineEnumerator.cs
 * Purpose: Interface for custom routines that provides a callback
 *          when a routine is queued for execution.
*/

using System;
using System.Collections;

namespace BeauRoutine
{
    /// <summary>
    /// Specialization of IEnumerator.
    /// Has specific callbacks for interacting with Routines.
    /// </summary>
    public interface IRoutineEnumerator : IEnumerator, IDisposable
    {
        /// <summary>
        /// Called when the routine is added to the stack.
        /// Returns whether to continue.
        /// </summary>
        bool OnRoutineStart();
    }
}

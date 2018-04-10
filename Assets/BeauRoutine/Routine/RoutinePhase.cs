﻿/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    1 March 2018
 * 
 * File:    RoutinePhase.cs
 * Purpose: Identifies when a Routine will be updated.
*/

namespace BeauRoutine
{
    /// <summary>
    /// Identifies when a Routine will be updated.
    /// </summary>
    public enum RoutinePhase
    {
        /// <summary>
        /// Routines will update during LateUpdate.
        /// </summary>
        LateUpdate,

        /// <summary>
        /// Routines will update during FixedUpdate.
        /// </summary>
        FixedUpdate,

        /// <summary>
        /// Routines will update during Update.
        /// </summary>
        Update,

        /// <summary>
        /// Routines must be updated manually.
        /// </summary>
        Manual,

        /// <summary>
        /// Routines will be updated at a configurable rate.
        /// </summary>
        ThinkUpdate,

        /// <summary>
        /// Routines will be updated at a configurable rate.
        /// </summary>
        CustomUpdate,
    }
}

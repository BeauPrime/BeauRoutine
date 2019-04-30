/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    11 Apr 2018
 * 
 * File:    INestedFiberContainer.cs
 * Purpose: Interface for a nested fiber container.
*/

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Container for nested fibers.
    /// </summary>
    internal interface INestedFiberContainer
    {
        void RemoveFiber(Fiber inFiber);
        void SetParentFiber(Fiber inFiber);
        RoutineStats[] GetStats();
    }
}

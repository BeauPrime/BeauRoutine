/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    1 March 2018
 * 
 * File:    YieldPhase.cs
 * Purpose: Identifies Routine updates caused by a YieldInstruction.
*/

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Identifies Routine updates caused by a YieldInstruction.
    /// </summary>
    internal enum YieldPhase
    {
        None,

        WaitForFixedUpdate,
        WaitForEndOfFrame,
        WaitForLateUpdate,
        WaitForUpdate,
        WaitForThinkUpdate,
        WaitForCustomUpdate,
        WaitForRealtimeUpdate
    }
}

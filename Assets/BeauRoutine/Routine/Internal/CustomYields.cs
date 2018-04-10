/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 March 2018
 * 
 * File:    CustomYields.cs
 * Purpose: Contains custom yield types.
*/

using UnityEngine;

namespace BeauRoutine.Internal
{
    // Waits until after LateUpdate
    public class WaitForLateUpdate : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }
    }

    // Waits until after Update
    public class WaitForUpdate : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }
    }

    // Waits until after ThinkUpdate
    public class WaitForThinkUpdate : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }
    }

    // Waits until after CustomUpdate
    public class WaitForCustomUpdate : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }
    }
}

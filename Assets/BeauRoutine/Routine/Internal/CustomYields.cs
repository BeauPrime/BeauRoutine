/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 March 2018
 * 
 * File:    CustomYields.cs
 * Purpose: Contains custom yield types.
*/

using UnityEngine;

namespace BeauRoutine.Internal
{
    // Waits until after LateUpdate
    internal class WaitForLateUpdate : CustomYieldInstruction
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
    internal class WaitForUpdate : CustomYieldInstruction
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
    internal class WaitForThinkUpdate : CustomYieldInstruction
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
    internal class WaitForCustomUpdate : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                return false;
            }
        }
    }

    // Waits until after RealtimeUpdate
    internal class WaitForRealtimeUpdate : CustomYieldInstruction
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

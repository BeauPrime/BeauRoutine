/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Groups.cs
 * Purpose: API for pausing, resuming, and adjusting time scale
 *          for routine groups.
*/

using System;

namespace BeauRoutine
{
    public partial struct Routine
    {
        /// <summary>
        /// Maximum number of groups allowed.
        /// </summary>
        public const int MAX_GROUPS = 32;

        /// <summary>
        /// Pauses routines in the given groups.
        /// </summary>
        static public void PauseGroups(int inGroupMask)
        {
            GetManager().PauseGroups(inGroupMask);
        }

        /// <summary>
        /// Resumes routines in the given groups.
        /// </summary>
        static public void ResumeGroups(int inGroupMask)
        {
            GetManager().ResumeGroups(inGroupMask);
        }
        
        /// <summary>
        /// Gets the time scale for the given group.
        /// </summary>
        static public float GetGroupTimeScale(int inGroup)
        {
            if (inGroup >= MAX_GROUPS)
                throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
            return GetManager().GetTimescale(inGroup);
        }

        /// <summary>
        /// Sets the time scale for the given group.
        /// </summary>
        static public void SetGroupTimeScale(int inGroup, float inScale)
        {
            if (inGroup >= MAX_GROUPS)
                throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
            GetManager().SetTimescale(inGroup, inScale);
        }

        /// <summary>
        /// Resets the time scale for the given group.
        /// </summary>
        static public void ResetGroupTimeScale(int inGroup)
        {
            if (inGroup >= MAX_GROUPS)
                throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
            GetManager().SetTimescale(inGroup, 1.0f);
        }
    }
}

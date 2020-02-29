/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Groups.cs
 * Purpose: API for pausing, resuming, and adjusting time scale
 *          for routine groups.
*/

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    #define DEVELOPMENT
#endif

using System;
using BeauRoutine.Internal;

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
            Manager m = Manager.Get();
            if (m != null)
                m.PauseGroups(inGroupMask);
        }

        /// <summary>
        /// Resumes routines in the given groups.
        /// </summary>
        static public void ResumeGroups(int inGroupMask)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.ResumeGroups(inGroupMask);
        }

        /// <summary>
        /// Returns the pause state of the given group.
        /// </summary>
        static public bool GetGroupPaused(int inGroup)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
#if DEVELOPMENT
                if (inGroup >= MAX_GROUPS)
                    throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
#endif // DEVELOPMENT
                return m.GetPaused(inGroup);
            }
            return false;
        }

        /// <summary>
        /// Returns the pause state of the given group.
        /// </summary>
        static public bool GetGroupPaused(Enum inGroup)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                int group = Convert.ToInt32(inGroup);
#if DEVELOPMENT
                if (group >= MAX_GROUPS)
                    throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
#endif // DEVELOPMENT
                return m.GetPaused(group);
            }
            return false;
        }
        
        /// <summary>
        /// Gets the time scale for the given group.
        /// </summary>
        static public float GetGroupTimeScale(int inGroup)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
#if DEVELOPMENT
                if (inGroup >= MAX_GROUPS)
                    throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
#endif // DEVELOPMENT
                return m.GetTimescale(inGroup);
            }
            return 1.0f;
        }

        /// <summary>
        /// Gets the time scale for the given group.
        /// </summary>
        static public float GetGroupTimeScale(Enum inGroup)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                int group = Convert.ToInt32(inGroup);
#if DEVELOPMENT
                if (group >= MAX_GROUPS)
                    throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
#endif // DEVELOPMENT
                return m.GetTimescale(group);
            }
            return 1.0f;
        }

        /// <summary>
        /// Sets the time scale for the given group.
        /// </summary>
        static public void SetGroupTimeScale(int inGroup, float inScale)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
#if DEVELOPMENT
                if (inGroup >= MAX_GROUPS)
                    throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
#endif // DEVELOPMENT
                m.SetTimescale(inGroup, inScale);
            }
        }

        /// <summary>
        /// Sets the time scale for the given group.
        /// </summary>
        static public void SetGroupTimeScale(Enum inGroup, float inScale)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                int group = Convert.ToInt32(inGroup);
#if DEVELOPMENT
                if (group >= MAX_GROUPS)
                    throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
#endif // DEVELOPMENT
                m.SetTimescale(group, inScale);
            }
        }

        /// <summary>
        /// Resets the time scale for the given group.
        /// </summary>
        static public void ResetGroupTimeScale(int inGroup)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
#if DEVELOPMENT
                if (inGroup >= MAX_GROUPS)
                    throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
#endif // DEVELOPMENT
                m.SetTimescale(inGroup, 1.0f);
            }
        }

        /// <summary>
        /// Resets the time scale for the given group.
        /// </summary>
        static public void ResetGroupTimeScale(Enum inGroup)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                int group = Convert.ToInt32(inGroup);
#if DEVELOPMENT
                if (group >= MAX_GROUPS)
                    throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
#endif // DEVELOPMENT
                m.SetTimescale(group, 1.0f);
            }
        }

        /// <summary>
        /// Returns the group mask for the given group.
        /// </summary>
        static public int GetGroupMask(int inGroup)
        {
            return 1 << inGroup;
        }

        /// <summary>
        /// Returns the group mask for the given group.
        /// </summary>
        static public int GetGroupMask(Enum inGroup)
        {
            return 1 << Convert.ToInt32(inGroup);
        }

        /// <summary>
        /// Returns the group mask for the given groups.
        /// </summary>
        static public int GetGroupMask(int inGroupA, params int[] inGroups)
        {
            int mask = 1 << inGroupA;
            for (int i = 0; i < inGroups.Length; ++i)
                mask |= 1 << inGroups[i];
            return mask;
        }

        /// <summary>
        /// Returns the group mask for the given groups.
        /// </summary>
        static public int GetGroupMask(Enum inGroupA, params Enum[] inGroups)
        {
            int mask = 1 << Convert.ToInt32(inGroupA);
            for (int i = 0; i < inGroups.Length; ++i)
                mask |= 1 << Convert.ToInt32(inGroups[i]);
            return mask;
        }
    }
}

/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Time.cs
 * Purpose: Maintains timing information for Routines.
 *          Facilitates per-object and per-group time scaling.
*/

using System;
using UnityEngine;

namespace BeauRoutine
{
    public partial struct Routine
    {
        /// <summary>
        /// Maximum number of routine groups supported.
        /// </summary>
        public const int MAX_GROUPS = 32;

        /// <summary>
        /// Global time scale.
        /// </summary>
        static public float TimeScale
        {
            get { return Time.timeScale; }
            set { Time.timeScale = value; }
        }

        #region Group time scaling

        static private float[] s_GroupTimeScales;

        static private void InitializeGroupTimeScales()
        {
            s_GroupTimeScales = new float[MAX_GROUPS];
            for (int i = 0; i < MAX_GROUPS; ++i)
                s_GroupTimeScales[i] = 1.0f;
        }

        /// <summary>
        /// Gets the time scale for the given group.
        /// </summary>
        static public float GetGroupTimeScale(int inGroup)
        {
            if (inGroup >= MAX_GROUPS)
                throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
            return s_GroupTimeScales[inGroup];
        }

        /// <summary>
        /// Sets the time scale for the given group.
        /// </summary>
        static public void SetGroupTimeScale(int inGroup, float inScale)
        {
            if (inGroup >= MAX_GROUPS)
                throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
            s_GroupTimeScales[inGroup] = inScale;
        }

        /// <summary>
        /// Resets the time scale for the given group.
        /// </summary>
        static public void ResetGroupTimeScale(int inGroup)
        {
            if (inGroup >= MAX_GROUPS)
                throw new ArgumentException(string.Format("Invalid group index {0} - only {1} are allowed", inGroup, MAX_GROUPS));
            s_GroupTimeScales[inGroup] = 1.0f;
        }

        #endregion

        #region Per Object Scaling

        // Calculates the time scale for the given object
        static private float CalculateTimeScale(GameObject inGameObject)
        {
            RoutineIdentity identity = RoutineIdentity.Find(inGameObject);
            if (identity != null)
                return identity.TimeScale * s_GroupTimeScales[identity.Group];
            return 1.0f;
        }

        #endregion

        #region Delta Time

        static private float s_RawDeltaTime;
        static private float s_ScaledDeltaTime;

        /// <summary>
        /// Current delta time.
        /// </summary>
        static public float DeltaTime
        {
            get { return s_ScaledDeltaTime; }
        }

        /// <summary>
        /// Raw delta time.
        /// </summary>
        static public float UnscaledDeltaTime
        {
            get { return s_RawDeltaTime; }
        }

        /// <summary>
        /// Calculates the delta time for the given host.
        /// </summary>
        static public float CalculateDeltaTime(GameObject inHost)
        {
            return s_RawDeltaTime * CalculateTimeScale(inHost);
        }

        /// <summary>
        /// Calculates the delta time for the given host.
        /// </summary>
        static public float CalculateDeltaTime(MonoBehaviour inHost)
        {
            return CalculateDeltaTime(inHost.gameObject);
        }

        /// <summary>
        /// Calculates the delta time for the given host.
        /// </summary>
        static public float CalculateDeltaTime(RoutineIdentity inIdentity)
        {
            return s_RawDeltaTime * inIdentity.TimeScale * s_GroupTimeScales[inIdentity.Group];
        }

        // Resets delta time to use Time.deltaTime
        static private void ResetDeltaTime()
        {
            s_RawDeltaTime = Time.deltaTime;
            s_ScaledDeltaTime = s_RawDeltaTime;
        }

        // Sets the scaling value on DeltaTime
        static private void ScaleDeltaTime(float inScale)
        {
            s_ScaledDeltaTime = s_RawDeltaTime * inScale;
        }

        #endregion
    }
}

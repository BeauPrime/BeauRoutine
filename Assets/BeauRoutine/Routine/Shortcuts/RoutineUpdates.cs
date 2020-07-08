/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    5 March 2018
 * 
 * File:    RoutineUpdates.cs
 * Purpose: Methods for setting update functions.
 */

using System;
using System.Collections;
using BeauRoutine.Internal;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for setting update loops.
    /// </summary>
    static public class RoutineUpdates
    {
        private const string ROUTINE_UPDATER_UPDATE = Fiber.RESERVED_NAME_PREFIX + "Update";
        private const string ROUTINE_UPDATER_LATEUPDATE = Fiber.RESERVED_NAME_PREFIX + "LateUpdate";
        private const string ROUTINE_UPDATER_FIXEDUPDATE = Fiber.RESERVED_NAME_PREFIX + "FixedUpdate";
        private const string ROUTINE_UPDATER_THINKUPDATE = Fiber.RESERVED_NAME_PREFIX + "ThinkUpdate";
        private const string ROUTINE_UPDATER_CUSTOMUPDATE = Fiber.RESERVED_NAME_PREFIX + "CustomUpdate";
        private const string ROUTINE_UPDATER_REALTIMEUPDATE = Fiber.RESERVED_NAME_PREFIX + "RealtimeUpdate";
        private const string ROUTINE_UPDATER_MANUAL = Fiber.RESERVED_NAME_PREFIX + "Manual";

        static private string GetPhaseUpdaterName(RoutinePhase inPhase)
        {
            switch (inPhase)
            {
                case RoutinePhase.FixedUpdate:
                    return ROUTINE_UPDATER_FIXEDUPDATE;

                case RoutinePhase.LateUpdate:
                    return ROUTINE_UPDATER_LATEUPDATE;

                case RoutinePhase.Update:
                    return ROUTINE_UPDATER_UPDATE;

                case RoutinePhase.Manual:
                    return ROUTINE_UPDATER_MANUAL;

                case RoutinePhase.CustomUpdate:
                    return ROUTINE_UPDATER_CUSTOMUPDATE;

                case RoutinePhase.ThinkUpdate:
                    return ROUTINE_UPDATER_THINKUPDATE;

                case RoutinePhase.RealtimeUpdate:
                    return ROUTINE_UPDATER_REALTIMEUPDATE;

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Returns the update routine associated with the object and update phase.
        /// </summary>
        static public Routine GetUpdateRoutine(this MonoBehaviour inHost, RoutinePhase inPhase = RoutinePhase.Update)
        {
            return Routine.Find(inHost, GetPhaseUpdaterName(inPhase));
        }

        /// <summary>
        /// Sets the update routine for the object and update phase.
        /// </summary>
        static public Routine SetUpdateRoutine(this MonoBehaviour inHost, Action inAction, RoutinePhase inPhase = RoutinePhase.Update)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                string phaseName = GetPhaseUpdaterName(inPhase);
                Routine routine = Routine.Find(inHost, phaseName)
                    .Replace(Routine.StartLoop(inHost, inAction)).SetPhase(inPhase);
                Fiber fiber = m.Fibers[routine];
                if (fiber != null)
                    fiber.SetNameUnchecked(phaseName);
                return routine;
            }
            return Routine.Null;
        }

        /// <summary>
        /// Sets the update routine for the object and update phase.
        /// </summary>
        static public Routine SetUpdateRoutine(this MonoBehaviour inHost, IEnumerator inUpdateLoop, RoutinePhase inPhase = RoutinePhase.Update)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                string phaseName = GetPhaseUpdaterName(inPhase);
                Routine routine = Routine.Find(inHost, phaseName)
                    .Replace(inHost, inUpdateLoop).SetPhase(inPhase);
                Fiber fiber = m.Fibers[routine];
                if (fiber != null)
                    fiber.SetNameUnchecked(phaseName);
                return routine;
            }
            return Routine.Null;
        }

        /// <summary>
        /// Sets the update routine for the object and update phase.
        /// </summary>
        static public Routine SetUpdateRoutineGenerator(this MonoBehaviour inHost, Func<IEnumerator> inUpdateFunc, RoutinePhase inPhase = RoutinePhase.Update)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                string phaseName = GetPhaseUpdaterName(inPhase);
                Routine routine = Routine.Find(inHost, phaseName)
                    .Replace(Routine.StartLoopRoutine(inHost, inUpdateFunc)).SetPhase(inPhase);
                Fiber fiber = m.Fibers[routine];
                if (fiber != null)
                    fiber.SetNameUnchecked(phaseName);
                return routine;
            }
            return Routine.Null;
        }

        /// <summary>
        /// Clears the update routine for the object and update phase.
        /// </summary>
        static public Routine ClearUpdateRoutine(this MonoBehaviour inHost, RoutinePhase inPhase = RoutinePhase.Update)
        {
            Manager m = Manager.Get();
            if (m != null)
            {
                string phaseName = GetPhaseUpdaterName(inPhase);
                Routine.Stop(inHost, phaseName);
            }
            return Routine.Null;
        }
    }
}
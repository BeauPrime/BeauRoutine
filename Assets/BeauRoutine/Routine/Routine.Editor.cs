/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Editor.cs
 * Purpose: Editor-only API for retrieving stats on running
 *          BeauRoutines.
 *          
 * Notes:   Not particularly happy this is editor-only and public.
 *          I would have kept it private and only shared data
 *          with the debugger, but Unity won't recognize nested
 *          classes within a struct.
*/

using UnityEngine;

namespace BeauRoutine
{
    public partial struct Routine
    {
#if UNITY_EDITOR
        /// <summary>
        /// Editor only.  Used to retrieve stats on
        /// currently running Routines.
        /// </summary>
        static public class Editor
        {
            /// <summary>
            /// Returns an array of stats on all currently
            /// running Routines.
            /// </summary>
            static public RoutineStats[] GetRoutineStats()
            {
                if (s_ActiveFibers == null || s_ActiveFibers.Count == 0)
                    return null;

                RoutineStats[] stats = new RoutineStats[s_ActiveFibers.Count];

                var node = s_ActiveFibers.First;
                int i = 0;
                while (node != null)
                {
                    Fiber fiber = node.Value;
                    stats[i++] = fiber.GetStats();
                    node = node.Next;
                }

                return stats;
            }

            /// <summary>
            /// Returns stats about the overall state
            /// of the BeauRoutine engine.
            /// </summary>
            static public GlobalStats GetGlobalStats()
            {
                GlobalStats stats = new GlobalStats();
                stats.Running = s_FiberTable.Length - s_FreeFibers.Count;
                stats.Capacity = s_FiberTable.Length;
                stats.Max = s_MaxConcurrent;
                stats.AvgMillisecs = (s_UpdateSamples == 0 ? 0 : (s_TotalUpdateTime / 10000f) / s_UpdateSamples);
                stats.MaxSnapshot = s_Snapshot;
                return stats;
            }

            /// <summary>
            /// State of a Routine.
            /// </summary>
            public struct RoutineStats
            {
                public Routine Handle;
                public MonoBehaviour Host;
                public RoutineState State;
                public float TimeScale;
                public string Name;
                public string Function;
                public int StackDepth;
                public RoutineStats[] Nested;
            }

            /// <summary>
            /// Current status of a Routine.
            /// </summary>
            public enum RoutineState : byte
            {
                Running,
                Disposing,
                WaitTime,
                WaitUnity,
                Paused
            }

            /// <summary>
            /// State of the BeauRoutine engine.
            /// </summary>
            public struct GlobalStats
            {
                public int Running;
                public int Max;
                public int Capacity;
                public float AvgMillisecs;
                public RoutineStats[] MaxSnapshot;
            }
        }
#endif
    }
}
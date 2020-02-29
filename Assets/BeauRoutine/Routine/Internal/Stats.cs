/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    3 Apr 2017
 * 
 * File:    Stats.cs
 * Purpose: Definition of relevant stats structures.
*/

using UnityEngine;

namespace BeauRoutine.Internal
{
    /// <summary>
    /// State of a Routine.
    /// </summary>
    public struct RoutineStats
    {
        public Routine Handle;
        public MonoBehaviour Host;
        public string State;
        public RoutinePhase Phase;
        public float TimeScale;
        public int Priority;
        public string Name;
        public string Function;
        public int StackDepth;
        public RoutineStats[] Nested;
    }

    /// <summary>
    /// Current status of a Routine.
    /// </summary>
    static public class RoutineState
    {
        public const string Running = "Running";
        public const string Disposing = "Disposing";
        public const string WaitTime = "Wait (Seconds)";
        public const string WaitUnity = "Wait (Unity)";
        public const string WaitFixedUpdate = "Wait (FixedUpdate)";
        public const string WaitEndOfFrame = "Wait (EndOfFrame)";
        public const string WaitLateUpdate = "Wait (LateUpdate)";
        public const string WaitUpdate = "Wait (Update)";
        public const string WaitThinkUpdate = "Wait (ThinkUpdate)";
        public const string WaitCustomUpdate = "Wait (CustomUpdate)";
        public const string WaitRealtimeUpdate = "Wait (RealtimeUpdate)";
        public const string Locked = "Locked";
        public const string Paused = "Paused";
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

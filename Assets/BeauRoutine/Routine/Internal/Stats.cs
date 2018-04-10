/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
        public RoutineState State;
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
    public enum RoutineState : byte
    {
        Running,
        Disposing,
        WaitTime,
        WaitUnity,
        WaitFixedUpdate,
        WaitEndOfFrame,
        WaitLateUpdate,
        WaitUpdate,
        WaitThinkUpdate,
        WaitCustomUpdate,
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

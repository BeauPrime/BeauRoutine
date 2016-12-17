/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Manager.cs
 * Purpose: Public API for starting, stopping, pausing, resuming,
 *          and querying Routines.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace BeauRoutine
{
    public partial struct Routine
    {
        #region Initialization

        static private bool s_Initialized = false;

        static Routine()
        {
            InitializeInternal();
        }

        static private void InitializeInternal()
        {
            if (s_Initialized)
                return;
            s_Initialized = true;

            s_Table = new FiberTable();
            s_Table.SetCapacity(16);
            ResetDeltaTime();
            InitializeGroupTimeScales();
        }

        #endregion

        #region Manager Object

        static private Manager s_Manager;
        static private bool s_Updating = false;
#if UNITY_EDITOR
        static private Stopwatch s_UpdateTimer;
#endif

        // This has to be separate to avoid threading issues
        static private void InitializeManager()
        {
            if (ReferenceEquals(s_Manager, null))
            {
                GameObject managerGO = new GameObject("Routine::Manager");
                managerGO.hideFlags = HideFlags.HideAndDontSave;
                Manager.DontDestroyOnLoad(managerGO);

                s_Manager = managerGO.AddComponent<Manager>();
#if UNITY_EDITOR
                s_UpdateTimer = new Stopwatch();
#endif
            }
        }

        private sealed class Manager : MonoBehaviour
        {
            private void LateUpdate()
            {
                // We only do this now to ensure a consistent
                // update for this frame
                s_PausedGroups = s_QueuedPausedGroups;

                // Read in the delta time
                ResetDeltaTime();

                int fibersToUpdate = s_Table.TotalActive;

                if (s_Paused || fibersToUpdate == 0)
                    return;

                s_Updating = true;

#if UNITY_EDITOR
                s_UpdateTimer.Reset();
                s_UpdateTimer.Start();
#endif

                // Traverse the active fiber list
                // but only for the nodes that existed
                // at the beginning of this frame.
                int next = 0;
                Fiber fiber = s_Table.StartActive(ref next);
                while(fibersToUpdate-- > 0 && fiber != null)
                {
                    Fiber nextFiber = s_Table.Traverse(ref next);
                    fiber.Run();
                    fiber = nextFiber;
                }

                ResetDeltaTimeScale();

#if UNITY_EDITOR
                s_UpdateTimer.Stop();
                if (s_UpdateSamples >= MAX_UPDATE_SAMPLES || s_UpdateTimer.ElapsedMilliseconds > 1000)
                {
                    s_UpdateSamples = 0;
                    s_TotalUpdateTime = 0;
                }

                s_UpdateSamples++;
                s_TotalUpdateTime += s_UpdateTimer.ElapsedTicks;
#endif

                s_Updating = false;
            }

            private void OnApplicationQuit()
            {
                s_Updating = false;
                StopAll();
            }
        }

        #endregion

        #region Fibers

        static private FiberTable s_Table;

#if UNITY_EDITOR
        static private int s_MaxConcurrent;
        static private Editor.RoutineStats[] s_Snapshot;

        static private int s_UpdateSamples;
        static private long s_TotalUpdateTime;

        private const int MAX_UPDATE_SAMPLES = 60;
#endif

        // Finds the fiber with the given handle.
        static private Fiber Find(Routine inRoutine)
        {
            if (inRoutine.m_Value == 0)
                return null;

            int index = (int)(inRoutine.m_Value & INDEX_MASK);
            Fiber fiber = s_Table[index];
            return (fiber.HasHandle(inRoutine) ? fiber : null);
        }

        // Runs a fiber with the given host and routine.
        static private Routine RunFiber(MonoBehaviour inHost, IEnumerator inStart)
        {
            if (inStart == null)
                return Routine.Null;

            InitializeManager();

            if (ReferenceEquals(inHost, null))
                inHost = s_Manager;

            Routine handle;
            bool needSnapshot;
            Fiber fiber = CreateFiber(inHost, inStart, false, out handle, out needSnapshot);
            s_Table.AddActiveFiber(fiber);

#if UNITY_EDITOR
            if (needSnapshot && s_SnapshotEnabled)
                s_Snapshot = Editor.GetRoutineStats();
#endif

            return handle;
        }

        // Creates a fiber with the given routine, but doesn't auto-run it.
        static private Fiber ChainFiber(IEnumerator inStart)
        {
            InitializeManager();

            Routine handle;
            bool needSnapshot;
            Fiber fiber = CreateFiber(s_Manager, inStart, true, out handle, out needSnapshot);

#if UNITY_EDITOR
            if (needSnapshot && s_SnapshotEnabled)
                s_Snapshot = Editor.GetRoutineStats();
#endif

            return fiber;
        }

        // Creates a fiber to run the given routine with the given host.
        // Will expand the fiber table if necessary.
        static private Fiber CreateFiber(MonoBehaviour inHost, IEnumerator inStart, bool inbChained, out Routine outHandle, out bool outNeedSnapshot)
        {
            Fiber fiber = s_Table.GetFreeFiber();
            outHandle = fiber.Initialize(inHost, inStart, inbChained);

#if UNITY_EDITOR
            if (!s_SnapshotEnabled)
                outNeedSnapshot = false;
            else
            {
                int running = s_Table.TotalRunning;
                if (running > s_MaxConcurrent)
                {
                    s_MaxConcurrent = running;
                    outNeedSnapshot = true;
                }
                else
                {
                    outNeedSnapshot = false;
                }
            }
#else
                outNeedSnapshot = false;
#endif

            return fiber;
        }

        

        #endregion

        #region Groups

        static private int s_PausedGroups = 0;
        static private int s_QueuedPausedGroups = 0;

        /// <summary>
        /// Pauses routines on the given groups.
        /// </summary>
        static public void PauseGroups(int inGroupMask)
        {
            s_QueuedPausedGroups |= inGroupMask;
        }

        /// <summary>
        /// Resumes routines on the given layers.
        /// </summary>
        static public void ResumeGroups(int inGroupMask)
        {
            s_QueuedPausedGroups &= ~inGroupMask;
        }

        /// <summary>
        /// Returns the mask for the given group.
        /// </summary>
        static public int GetGroupMask(int inGroup)
        {
            return 1 << inGroup;
        }

        /// <summary>
        /// Returns the mask for the given groups.
        /// </summary>
        static public int GetGroupMask(params int[] inGroups)
        {
            int mask = 0;
            for (int i = inGroups.Length - 1; i >= 0; --i)
                mask |= 1 << inGroups[i];
            return mask;
        }

        /// <summary>
        /// Returns the mask for the given groups.
        /// </summary>
        static public int GetGroupMask(IList<int> inGroups)
        {
            int mask = 0;
            for (int i = inGroups.Count - 1; i >= 0; --i)
                mask |= 1 << inGroups[i];
            return mask;
        }

        #endregion

        #region Operations

        static private bool s_Paused = false;

        #region Start

        /// <summary>
        /// Runs a routine.
        /// </summary>
        static public Routine Start(IEnumerator inRoutine)
        {
            return RunFiber(null, inRoutine);
        }

        /// <summary>
        /// Runs a routine.
        /// </summary>
        static public Routine Start(MonoBehaviour inHost, IEnumerator inRoutine)
        {
            return RunFiber(inHost, inRoutine);
        }

        #endregion

        #region Stop

        /// <summary>
        /// Stops all routines with the given host.
        /// </summary>
        static public void StopAll(MonoBehaviour inHost)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                Fiber nextFiber = s_Table.Traverse(ref next);
                if (fiber.HasHost(inHost))
                {
                    fiber.Stop();

                    // If we aren't updating right now,
                    // we can freely dispose of the routine.
                    if (!s_Updating)
                        fiber.Run();
                }

                fiber = nextFiber;
            }
        }

        /// <summary>
        /// Stops all routines with the given host
        /// with the given optional name.
        /// </summary>
        static public void Stop(MonoBehaviour inHost, string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                Fiber nextFiber = s_Table.Traverse(ref next);
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                {
                    fiber.Stop();

                    // If we aren't updating right now,
                    // we can freely dispose of the routine.
                    if (!s_Updating)
                        fiber.Run();
                }

                fiber = nextFiber;
            }
        }

        /// <summary>
        /// Stops all routines with a host that belongs
        /// to the given GameObject.
        /// </summary>
        static public void StopAll(GameObject inHost)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                Fiber nextFiber = s_Table.Traverse(ref next);
                if (fiber.HasHost(inHost))
                {
                    fiber.Stop();

                    // If we aren't updating right now,
                    // we can freely dispose of the routine.
                    if (!s_Updating)
                    {
                        fiber.Run();
                    }
                }

                fiber = nextFiber;
            }
        }

        /// <summary>
        /// Stops all routines with a host that belongs
        /// to the given GameObject and have the optional name
        /// </summary>
        static public void Stop(GameObject inHost, string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                Fiber nextFiber = s_Table.Traverse(ref next);
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                {
                    fiber.Stop();

                    // If we aren't updating right now,
                    // we can freely dispose of the routine.
                    if (!s_Updating)
                    {
                        fiber.Run();
                    }
                }

                fiber = nextFiber;
            }
        }

        /// <summary>
        /// Stops all routines.
        /// </summary>
        static public void StopAll()
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                Fiber nextFiber = s_Table.Traverse(ref next);
                fiber.Stop();

                // If we aren't updating right now,
                // we can freely dispose of the routines.
                if (!s_Updating)
                {
                    fiber.Run();
                }

                fiber = nextFiber;
            }
        }

        /// <summary>
        /// Stops all routines with the given optional name.
        /// </summary>
        static public void Stop(string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                Fiber nextFiber = s_Table.Traverse(ref next);
                if (fiber.HasName(inName))
                {
                    fiber.Stop();

                    // If we aren't updating right now,
                    // we can freely dispose of the routines.
                    if (!s_Updating)
                    {
                        fiber.Run();
                    }
                }

                fiber = nextFiber;
            }
        }

        #endregion

        #region Pause

        /// <summary>
        /// Pauses all routines with the given host.
        /// </summary>
        static public void PauseAll(MonoBehaviour inHost)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost))
                    fiber.Pause();
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Pauses all routines with the given host and name.
        /// </summary>
        static public void Pause(MonoBehaviour inHost, string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                    fiber.Pause();
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Pauses all routines with a host that belongs
        /// to the given GameObject.
        /// </summary>
        static public void PauseAll(GameObject inHost)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost))
                    fiber.Pause();
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Pauses all routines with a host that belongs
        /// to the given GameObject and have the given name.
        /// </summary>
        static public void Pause(GameObject inHost, string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                    fiber.Pause();
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Pauses all routines.
        /// </summary>
        static public void PauseAll()
        {
            s_Paused = true;
        }

        /// <summary>
        /// Pauses all routines with the given name.
        /// </summary>
        static public void Pause(string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasName(inName))
                    fiber.Pause();
                fiber = s_Table.Traverse(ref next);
            }
        }

        #endregion

        #region Resume

        /// <summary>
        /// Resumes all routines with the given host.
        /// </summary>
        static public void ResumeAll(MonoBehaviour inHost)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost))
                    fiber.Resume();
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Resumes all routines with the given host and given name.
        /// </summary>
        static public void Resume(MonoBehaviour inHost, string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                    fiber.Resume();
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Resumes all routines with a host that belongs
        /// to the given GameObject.
        /// </summary>
        static public void ResumeAll(GameObject inHost)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost))
                    fiber.Resume();
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Resumes all routines with a host that belongs
        /// to the given GameObject and have the given name.
        /// </summary>
        static public void Resume(GameObject inHost, string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                    fiber.Resume();
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Resumes all routines.
        /// </summary>
        static public void ResumeAll()
        {
            s_Paused = false;
        }

        /// <summary>
        /// Resumes all routines with the given name.
        /// </summary>
        static public void Resume(string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasName(inName))
                    fiber.Resume();
                fiber = s_Table.Traverse(ref next);
            }
        }

        #endregion

        #region Queries

        /// <summary>
        /// Finds all routines with the given host
        /// and adds their handles to the given collection.
        /// </summary>
        static public void FindAll(MonoBehaviour inHost, ref ICollection<Routine> ioRoutines)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost))
                {
                    if (ioRoutines == null)
                        ioRoutines = new List<Routine>();
                    ioRoutines.Add(fiber.GetHandle());
                }
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Finds all routines with the given host and name
        /// and adds their handles to the given collection.
        /// </summary>
        static public void Find(MonoBehaviour inHost, string inName, ref ICollection<Routine> ioRoutines)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                {
                    if (ioRoutines == null)
                        ioRoutines = new List<Routine>();
                    ioRoutines.Add(fiber.GetHandle());
                }
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Finds the first routine on the given host
        /// with the given name.
        /// </summary>
        static public Routine Find(MonoBehaviour inHost, string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                    return fiber.GetHandle();
                fiber = s_Table.Traverse(ref next);
            }
            return Routine.Null;
        }

        /// <summary>
        /// Finds all routines with the given host
        /// and adds their handles to the given collection.
        /// </summary>
        static public void FindAll(GameObject inHost, ref ICollection<Routine> ioRoutines)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost))
                {
                    if (ioRoutines == null)
                        ioRoutines = new List<Routine>();
                    ioRoutines.Add(fiber.GetHandle());
                }
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Finds all routines with the given host and name
        /// and adds their handles to the given collection.
        /// </summary>
        static public void Find(GameObject inHost, string inName, ref ICollection<Routine> ioRoutines)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                {
                    if (ioRoutines == null)
                        ioRoutines = new List<Routine>();
                    ioRoutines.Add(fiber.GetHandle());
                }
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Finds the first routine with the given host and name.
        /// </summary>
        static public Routine Find(GameObject inHost, string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasHost(inHost) && fiber.HasName(inName))
                    return fiber.GetHandle();
                fiber = s_Table.Traverse(ref next);
            }
            return Routine.Null;
        }

        /// <summary>
        /// Finds all routines with the given name
        /// and adds their handles to the given collection.
        /// </summary>
        static public void Find(string inName, ref ICollection<Routine> ioRoutines)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasName(inName))
                {
                    if (ioRoutines == null)
                        ioRoutines = new List<Routine>();
                    ioRoutines.Add(fiber.GetHandle());
                }
                fiber = s_Table.Traverse(ref next);
            }
        }

        /// <summary>
        /// Finds the first routine with the given name
        /// </summary>
        static public Routine Find(string inName)
        {
            int next = 0;
            Fiber fiber = s_Table.StartActive(ref next);
            while (fiber != null)
            {
                if (fiber.HasName(inName))
                    return fiber.GetHandle();
                fiber = s_Table.Traverse(ref next);
            }
            return Routine.Null;
        }

        #endregion

        #endregion

        #region Global Settings

#if UNITY_EDITOR
        static private bool s_SnapshotEnabled = true;
#endif

        /// <summary>
        /// Initializes the Routine system.
        /// </summary>
        static public void Initialize()
        {
            InitializeInternal();
            InitializeManager();
        }

        /// <summary>
        /// Sets the minimum number of Routines that
        /// can be run concurrently without incurring
        /// an additional GC cost.
        /// </summary>
        static public void SetCapacity(int inCapacity)
        {
            s_Table.SetCapacity(inCapacity);
        }

        /// <summary>
        /// Enables/disables snapshotting.
        /// A snapshot is created whenever a new
        /// max number of concurrent routines is set.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        static public void SetSnapshotEnabled(bool inbEnabled)
        {
#if UNITY_EDITOR
            s_SnapshotEnabled = inbEnabled;
            if (!s_SnapshotEnabled)
                s_Snapshot = null;
#endif
        }

        #endregion
    }
}

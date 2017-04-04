/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 Apr 2017
 * 
 * File:    Manager.cs
 * Purpose: Manages BeauRoutine lifecycle.
*/

using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace BeauRoutine.Internal
{
    public sealed class Manager
    {
        // HACK: Needed at the moment for Editor access
        static private Manager s_Instance;
        static public Manager Instance
        {
            get { return s_Instance; }
        }

        // Current state
        private bool m_Initialized = false;
        private bool m_Updating = false;
        private bool m_Destroying = false;

        // Group properties
        private int m_QueuedPauseMask;
        private float[] m_QueuedGroupTimescale;
        private bool m_GroupTimescaleDirty;

        // Profiling
        private Stopwatch m_UpdateTimer;
        private int m_MaxConcurrent;
        private bool m_NeedsSnapshot;
        private RoutineStats[] m_Snapshot;
        private int m_UpdateSamples;
        private long m_TotalUpdateTime;
        private float m_LastProfileLogTime;
        private const int MAX_UPDATE_SAMPLES = 60;
        private const long MAX_UPDATE_MS = 1000;

        /// <summary>
        /// Table containing all fibers.
        /// </summary>
        public readonly Table Fibers;

        /// <summary>
        /// Unity host object.
        /// </summary>
        public UnityHost Host;

        /// <summary>
        /// If updates should be run or not.
        /// </summary>
        public bool Paused = false;

        /// <summary>
        /// Settings for the current frame.
        /// </summary>
        public Frame Frame;

        /// <summary>
        /// Additional checking and profiling.
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        /// Determines whether snapshots should be taken
        /// of record numbers of running routines
        /// </summary>
        public bool SnapshotEnabled = false;

        /// <summary>
        /// Is the manager in the middle of updating routines right now?
        /// </summary>
        public bool IsUpdating
        {
            get { return m_Updating; }
        }

        #region Lifecycle

        public Manager()
        {
            s_Instance = this;

            Fibers = new Table(this);
            Fibers.SetCapacity(16);

            m_UpdateTimer = new Stopwatch();

            Frame.ResetTime();

            m_QueuedGroupTimescale = new float[Routine.MAX_GROUPS];
            Frame.GroupTimeScale = new float[Routine.MAX_GROUPS];
            for (int i = 0; i < Routine.MAX_GROUPS; ++i)
                m_QueuedGroupTimescale[i] = Frame.GroupTimeScale[i] = 1.0f;
        }

        /// <summary>
        /// Initializes the Unity Host object.
        /// </summary>
        public void Initialize()
        {
            if (m_Initialized)
                return;

            DebugMode = UnityEngine.Debug.isDebugBuild;

            GameObject hostGO = new GameObject("Routine::Manager");
            Host = hostGO.AddComponent<UnityHost>();
            Host.Initialize(this);
            hostGO.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(hostGO);

            Log("Initialize()");

            m_Initialized = true;
            m_LastProfileLogTime = Time.unscaledTime;
        }

        /// <summary>
        /// Updates all running routines.
        /// </summary>
        public void Update()
        {
            Frame.ResetTime();
            Frame.PauseMask = m_QueuedPauseMask;
            if (m_GroupTimescaleDirty)
            {
                m_GroupTimescaleDirty = false;
                Array.Copy(m_QueuedGroupTimescale, Frame.GroupTimeScale, Routine.MAX_GROUPS);
            }

            m_Updating = true;
            {
                if (DebugMode)
                {
                    m_UpdateTimer.Reset();
                    m_UpdateTimer.Start();

                    if (m_NeedsSnapshot)
                    {
                        m_Snapshot = GetRoutineStats();
                        m_NeedsSnapshot = false;
                    }

                    if (!Paused && Fibers.TotalActive > 0)
                    {
                        Fibers.RunActive();
                        Frame.ResetTimeScale();
                    }

                    m_UpdateTimer.Stop();

                    if (m_UpdateSamples >= MAX_UPDATE_SAMPLES || m_UpdateTimer.ElapsedMilliseconds > MAX_UPDATE_MS)
                    {
                        m_UpdateSamples = 0;
                        m_TotalUpdateTime = 0;
                    }

                    ++m_UpdateSamples;
                    m_TotalUpdateTime += m_UpdateTimer.ElapsedTicks;

                    if (Time.unscaledTime >= m_LastProfileLogTime + 10f)
                    {
                        m_LastProfileLogTime = Time.unscaledTime;
                        Log(string.Format("Running: {0}/{1}/{2}; Avg Update: {3}ms", Fibers.TotalActive, m_MaxConcurrent, Fibers.TotalCapacity, (m_TotalUpdateTime / 10000f) / m_UpdateSamples));
                    }
                }
                else
                {
                    if (!Paused && Fibers.TotalActive > 0)
                    {
                        Fibers.RunActive();
                        Frame.ResetTimeScale();
                    }
                }
            }
            m_Updating = false;

            if (m_Destroying)
            {
                m_Destroying = false;
                Routine.Shutdown();
            }
        }

        /// <summary>
        /// Stops all routines and shuts down the 
        /// </summary>
        public void Shutdown()
        {
            if (!m_Initialized)
                return;

            Fibers.ClearAll();

            if (Host != null)
            {
                Host.Shutdown();
                GameObject.Destroy(Host.gameObject);
                Host = null;
            }

            Log("Shutdown()");

            s_Instance = null;
            m_Initialized = false;
        }

        /// <summary>
        /// Queues a shutdown at the end of an update.
        /// </summary>
        public void QueueShutdown()
        {
            m_Destroying = true;
        }

        #endregion

        #region Fiber Creation

        /// <summary>
        /// Creates a Fiber and runs it.
        /// </summary>
        public Routine RunFiber(MonoBehaviour inHost, IEnumerator inStart)
        {
            if (inStart == null)
                return Routine.Null;

            if (!m_Initialized)
                Initialize();

            if (ReferenceEquals(inHost, null))
                inHost = Host;

            Routine handle;
            Fiber fiber = CreateFiber(inHost, inStart, false, out handle);
            Fibers.AddActiveFiber(fiber);
            return handle;
        }

        /// <summary>
        /// Creates a chained Fiber.
        /// </summary>
        public Fiber ChainFiber(IEnumerator inStart)
        {
            Routine handle;
            return CreateFiber(Host, inStart, true, out handle);
        }

        /// <summary>
        /// Recycles the given Fiber back into the table.
        /// </summary>
        public void RecycleFiber(Fiber inFiber, bool inbChained)
        {
            if (!inbChained)
                Fibers.RemoveActiveFiber(inFiber);
            Fibers.AddFreeFiber(inFiber);
        }

        // Creates a fiber to run the given routine with the given host.
        // Will expand the fiber table if necessary.
        private Fiber CreateFiber(MonoBehaviour inHost, IEnumerator inStart, bool inbChained, out Routine outHandle)
        {
            Fiber fiber = Fibers.GetFreeFiber();
            outHandle = fiber.Initialize(inHost, inStart, inbChained);

            if (DebugMode)
            {
                int running = Fibers.TotalRunning;
                if (running > m_MaxConcurrent)
                {
                    m_MaxConcurrent = running;
                    m_NeedsSnapshot = SnapshotEnabled;
                }
            }

            return fiber;
        }

        #endregion

        #region Stats

        /// <summary>
        /// Returns an array of stats on all currently
        /// running Routines.
        /// </summary>
        public RoutineStats[] GetRoutineStats()
        {
            if (Fibers.TotalActive == 0)
                return null;

            RoutineStats[] stats = new RoutineStats[Fibers.TotalActive];

            int next = 0;
            Fiber fiber = Fibers.StartActive(ref next);
            int i = 0;
            while (fiber != null)
            {
                stats[i++] = fiber.GetStats();
                fiber = Fibers.Traverse(ref next);
            }

            return stats;
        }

        /// <summary>
        /// Returns stats about the overall state
        /// of the BeauRoutine engine.
        /// </summary>
        public GlobalStats GetGlobalStats()
        {
            GlobalStats stats = new GlobalStats();
            stats.Running = Fibers.TotalRunning;
            stats.Capacity = Fibers.TotalCapacity;
            stats.Max = m_MaxConcurrent;
            stats.AvgMillisecs = (m_UpdateSamples == 0 ? 0 : (m_TotalUpdateTime / 10000f) / m_UpdateSamples);
            stats.MaxSnapshot = m_Snapshot;
            return stats;
        }

        #endregion

        #region Groups

        /// <summary>
        /// Queues the given groups to be paused next frame.
        /// </summary>
        public void PauseGroups(int inGroups)
        {
            m_QueuedPauseMask |= inGroups;
        }

        /// <summary>
        /// Queues the given groups to be unpaused next frame.
        /// </summary>
        public void ResumeGroups(int inGroups)
        {
            m_QueuedPauseMask &= ~inGroups;
        }

        /// <summary>
        /// Returns the given group's timescale for the next frame.
        /// </summary>
        public float GetTimescale(int inGroup)
        {
            return m_QueuedGroupTimescale[inGroup];
        }

        /// <summary>
        /// Queues the given group's timescale to change for next frame.
        /// </summary>
        public void SetTimescale(int inGroup, float inTimescale)
        {
            if (m_QueuedGroupTimescale[inGroup] != inTimescale)
            {
                m_QueuedGroupTimescale[inGroup] = inTimescale;
                m_GroupTimescaleDirty = true;
            }
        }

        #endregion

        /// <summary>
        /// Logs a message to the console in Debug Mode.
        /// </summary>
        public void Log(string inMessage)
        {
            if (DebugMode)
            {
                UnityEngine.Debug.Log("[BeauRoutine] " + inMessage);
            }
        }
    }
}

/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 Apr 2017
 * 
 * File:    Manager.cs
 * Purpose: Manages BeauRoutine lifecycle.
*/

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    #define DEVELOPMENT
#endif

using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace BeauRoutine.Internal
{
    public sealed class Manager
    {
        // Version number
        static public readonly Version VERSION = new Version("0.9.0");

        // Used to perform check for starting a Routine.Inline in development builds
        static private readonly IntPtr TYPEHANDLE_DECORATOR = typeof(RoutineDecorator).TypeHandle.Value;

        #region Singleton

        static private Manager s_Instance = new Manager();
        static private bool s_AppQuitting = false;

        /// <summary>
        /// Ensures the Manager exists and is initialized.
        /// </summary>
        static public void Create()
        {
            if (s_Instance == null)
                s_Instance = new Manager();
            s_Instance.Initialize();
        }

        /// <summary>
        /// Shuts down the Manager and nulls out the reference.
        /// </summary>
        static public void Destroy()
        {
            if (s_Instance != null)
            {
                if (!s_Instance.m_Updating)
                {
                    s_Instance.Shutdown();
                    s_Instance = null;
                    return;
                }

                s_Instance.QueueShutdown();
            }
        }

        /// <summary>
        /// Returns the Manager singleton.
        /// </summary>
        static public Manager Get()
        {
            if (s_Instance == null && !s_AppQuitting)
                throw new InvalidOperationException("BeauRoutine has been shutdown. Please call Initialize() before anything else.");
            return s_Instance;
        }

        /// <summary>
        /// Returns if the Manager singleton exists.
        /// </summary>
        static public bool Exists()
        {
            return s_Instance != null;
        }

        /// <summary>
        /// Returns if the Manager was shut down due to application shutting down.
        /// </summary>
        static public bool IsShuttingDown()
        {
            return s_AppQuitting;
        }

        #endregion

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
        private int m_TotalUpdateFrames;
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
        public RoutineUnityHost Host;

        /// <summary>
        /// If updates should be run or not.
        /// </summary>
        public bool Paused = false;

        /// <summary>
        /// From settings: If updates should be run or not.
        /// </summary>
        public bool SystemPaused = false;

        /// <summary>
        /// Global time scale.
        /// </summary>
        public float TimeScale = 1;

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
        /// Determines whether exception handling is enabled.
        /// </summary>
        public bool HandleExceptions = true;

        /// <summary>
        /// Default update phase for routines.
        /// </summary>
        public RoutinePhase DefaultPhase = RoutinePhase.LateUpdate;

        /// <summary>
        /// Is the manager in the middle of updating routines right now?
        /// </summary>
        public bool IsUpdating()
        {
            return m_Updating;
        }
        
        /// <summary>
        /// Is the manager in the middle of updating the given update list.
        /// </summary>
        public bool IsUpdating(RoutinePhase inUpdate)
        {
            return Fibers.GetIsUpdating(inUpdate);
        }

        #region Lifecycle

        public Manager()
        {
            s_Instance = this;

            Fibers = new Table(this);
            Fibers.SetCapacity(16);

            m_UpdateTimer = new Stopwatch();

            Frame.ResetTime(TimeScale);

            m_QueuedGroupTimescale = new float[Routine.MAX_GROUPS];
            Frame.GroupTimeScale = new float[Routine.MAX_GROUPS];
            for (int i = 0; i < Routine.MAX_GROUPS; ++i)
                m_QueuedGroupTimescale[i] = Frame.GroupTimeScale[i] = 1.0f;

#if DEVELOPMENT
    #if UNITY_EDITOR
            DebugMode = true;
    #else
            DebugMode = UnityEngine.Debug.isDebugBuild;
    #endif // UNITY_EDITOR
#else
            DebugMode = false;
#endif // DEVELOPMENT

            HandleExceptions = DebugMode;
        }

        /// <summary>
        /// Initializes the Unity Host object.
        /// </summary>
        public void Initialize()
        {
            if (m_Initialized)
                return;

            GameObject hostGO = new GameObject("Routine::Manager");
            Host = hostGO.AddComponent<RoutineUnityHost>();
            Host.Initialize(this);
            hostGO.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(hostGO);

            Log("Initialize() -- Version " + VERSION.ToString());

            m_Initialized = true;
            m_LastProfileLogTime = Time.unscaledTime;
        }

        /// <summary>
        /// Updates all running routines.
        /// </summary>
        public void Update(RoutinePhase inPhase)
        {
            Frame.IncrementSerial();
            Frame.ResetTime(TimeScale);
            Frame.PauseMask = m_QueuedPauseMask;
            if (m_GroupTimescaleDirty)
            {
                m_GroupTimescaleDirty = false;
                Array.Copy(m_QueuedGroupTimescale, Frame.GroupTimeScale, Routine.MAX_GROUPS);
            }

            bool bPrevUpdating = m_Updating;
            m_Updating = true;
            {
#if DEVELOPMENT
                if (DebugMode)
                {
                    m_UpdateTimer.Reset();
                    m_UpdateTimer.Start();

                    if (m_NeedsSnapshot)
                    {
                        m_Snapshot = GetRoutineStats();
                        m_NeedsSnapshot = false;
                    }

                    if (!Paused && !SystemPaused && Fibers.GetUpdateCount(inPhase) > 0)
                    {
                        Fibers.RunUpdate(inPhase);
                        Frame.ResetTimeScale();
                    }

                    m_UpdateTimer.Stop();

                    m_TotalUpdateTime += m_UpdateTimer.ElapsedTicks;

                    if (inPhase == RoutinePhase.LateUpdate)
                    {
                        ++m_TotalUpdateFrames;
                        if (Time.unscaledTime >= m_LastProfileLogTime + 10f)
                        {
                            m_LastProfileLogTime = Time.unscaledTime;
                            Log(string.Format("Running: {0}/{1}/{2}; Avg Update: {3}ms", Fibers.TotalActive, m_MaxConcurrent, Fibers.TotalCapacity, (m_TotalUpdateTime / 10000f) / m_TotalUpdateFrames));
                        }

                        if (m_TotalUpdateFrames >= MAX_UPDATE_SAMPLES || m_UpdateTimer.ElapsedMilliseconds > MAX_UPDATE_MS)
                        {
                            m_TotalUpdateFrames = 0;
                            m_TotalUpdateTime = 0;
                        }
                    }
                }
                else
#endif // DEVELOPMENT
                {
                    if (!Paused && !SystemPaused && Fibers.GetUpdateCount(inPhase) > 0)
                    {
                        Fibers.RunUpdate(inPhase);
                        Frame.ResetTimeScale();
                    }
                }
            }
            m_Updating = bPrevUpdating;

            if (!m_Updating)
            {
                if (m_Destroying)
                {
                    m_Destroying = false;
                    Destroy();
                }
            }
        }

        /// <summary>
        /// Manually updates routines set to Manual.
        /// </summary>
        public bool ManualUpdate(float inDeltaTime)
        {
            if (Fibers.GetIsUpdating(RoutinePhase.Manual))
            {
                UnityEngine.Debug.LogWarning("[BeauRoutine] Cannot nest ManualUpdate calls!");
                return false;
            }

            Frame oldFrame = Frame;
            bool bPrevUpdating = m_Updating;

            if (bPrevUpdating)
            {
                // Copy this, so we have a consistent group time scale
                // for any previous updates
                oldFrame.GroupTimeScale = (float[])oldFrame.GroupTimeScale.Clone();
            }

            Frame.IncrementSerial();
            Frame.ResetTime(inDeltaTime, TimeScale);
            Frame.PauseMask = m_QueuedPauseMask;
            if (m_GroupTimescaleDirty)
            {
                if (!bPrevUpdating)
                    m_GroupTimescaleDirty = false;
                
                // This is only temporary, so we won't reset the dirty flag here
                Array.Copy(m_QueuedGroupTimescale, Frame.GroupTimeScale, Routine.MAX_GROUPS);
            }
            
            m_Updating = true;
            {
#if DEVELOPMENT
                if (DebugMode)
                {
                    m_UpdateTimer.Reset();
                    m_UpdateTimer.Start();

                    if (m_NeedsSnapshot)
                    {
                        m_Snapshot = GetRoutineStats();
                        m_NeedsSnapshot = false;
                    }

                    if (Fibers.GetUpdateCount(RoutinePhase.Manual) > 0)
                    {
                        Fibers.RunUpdate(RoutinePhase.Manual);
                        Frame.ResetTimeScale();
                    }

                    m_UpdateTimer.Stop();

                    m_TotalUpdateTime += m_UpdateTimer.ElapsedTicks;
                }
                else
#endif // DEVELOPMENT
                {
                    if (Fibers.GetUpdateCount(RoutinePhase.Manual) > 0)
                    {
                        Fibers.RunUpdate(RoutinePhase.Manual);
                        Frame.ResetTimeScale();
                    }
                }
            }
            m_Updating = bPrevUpdating;
            Frame = oldFrame;

            if (!m_Updating)
            {
                if (m_Destroying)
                {
                    m_Destroying = false;
                    Destroy();
                }
            }

            return true;
        }

        /// <summary>
        /// Manually updates the given routine.
        /// </summary>
        public bool ManualUpdate(Fiber inFiber, float inDeltaTime)
        {
            if (!inFiber.PrepareManualUpdate())
                return false;

            Frame oldFrame = Frame;
            bool bPrevUpdating = m_Updating;

            if (bPrevUpdating)
            {
                // Copy this, so we have a consistent group time scale
                // for any previous updates
                oldFrame.GroupTimeScale = (float[])oldFrame.GroupTimeScale.Clone();
            }

            Frame.IncrementSerial();
            Frame.ResetTime(inDeltaTime, TimeScale);
            Frame.PauseMask = m_QueuedPauseMask;
            if (m_GroupTimescaleDirty)
            {
                if (!bPrevUpdating)
                    m_GroupTimescaleDirty = false;
                Array.Copy(m_QueuedGroupTimescale, Frame.GroupTimeScale, Routine.MAX_GROUPS);
            }

            m_Updating = true;
            {
#if DEVELOPMENT
                if (DebugMode)
                {
                    m_UpdateTimer.Reset();
                    m_UpdateTimer.Start();

                    if (m_NeedsSnapshot)
                    {
                        m_Snapshot = GetRoutineStats();
                        m_NeedsSnapshot = false;
                    }

                    Fibers.RunManualUpdate(inFiber);
                    Frame.ResetTimeScale();

                    m_UpdateTimer.Stop();

                    m_TotalUpdateTime += m_UpdateTimer.ElapsedTicks;
                }
                else
#endif // DEVELOPMENT
                {
                    Fibers.RunManualUpdate(inFiber);
                    Frame.ResetTimeScale();
                }
            }
            m_Updating = bPrevUpdating;
            Frame = oldFrame;

            if (!m_Updating)
            {
                if (m_Destroying)
                {
                    m_Destroying = false;
                    Destroy();
                }
            }

            return true;
        }

        /// <summary>
        /// Stops all routines and shuts down the manager.
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

            m_Initialized = false;
            s_Instance = null;
        }

        /// <summary>
        /// Queues a shutdown at the end of an update.
        /// </summary>
        public void QueueShutdown()
        {
            m_Destroying = true;
        }

        /// <summary>
        /// Called when the application is quitting.
        /// </summary>
        public void OnApplicationQuit()
        {
            s_AppQuitting = true;
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

#if DEVELOPMENT
            if (inStart.GetType().TypeHandle.Value == TYPEHANDLE_DECORATOR)
            {
                RoutineDecorator decorator = (RoutineDecorator)inStart;
                if ((decorator.Flags & RoutineDecoratorFlag.Inline) != 0)
                {
                    UnityEngine.Debug.LogWarning("[BeauRoutine] Starting a BeauRoutine with an Inlined coroutine. This is not supported, and the coroutine will not execute immediately.");
                }
            }
#endif // DEVELOPMENT

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

        #region Yield Updates

        public void UpdateYield(YieldPhase inYieldUpdate)
        {
            Frame.IncrementSerial();
            Frame.ResetTime(TimeScale);
            Frame.PauseMask = m_QueuedPauseMask;
            if (m_GroupTimescaleDirty)
            {
                m_GroupTimescaleDirty = false;
                Array.Copy(m_QueuedGroupTimescale, Frame.GroupTimeScale, Routine.MAX_GROUPS);
            }

            m_Updating = true;
            {
#if DEVELOPMENT
                if (DebugMode)
                {
                    m_UpdateTimer.Reset();
                    m_UpdateTimer.Start();

                    if (m_NeedsSnapshot)
                    {
                        m_Snapshot = GetRoutineStats();
                        m_NeedsSnapshot = false;
                    }

                    if (!Paused && !SystemPaused && Fibers.GetYieldCount(inYieldUpdate) > 0)
                    {
                        Fibers.RunYieldUpdate(inYieldUpdate);
                        Frame.ResetTimeScale();
                    }

                    m_UpdateTimer.Stop();

                    m_TotalUpdateTime += m_UpdateTimer.ElapsedTicks;
                }
                else
#endif // DEVELOPMENT
                {
                    if (!Paused && !SystemPaused && Fibers.GetYieldCount(inYieldUpdate) > 0)
                    {
                        Fibers.RunYieldUpdate(inYieldUpdate);
                        Frame.ResetTimeScale();
                    }
                }
            }
            m_Updating = false;

            if (m_Destroying)
            {
                m_Destroying = false;
                Destroy();
            }
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
                fiber = Fibers.TraverseActive(ref next);
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
            stats.AvgMillisecs = (m_TotalUpdateFrames == 0 ? 0 : (m_TotalUpdateTime / 10000f) / m_TotalUpdateFrames);
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
        /// Returns the pause state of the given group.
        /// </summary>
        public bool GetPaused(int inGroup)
        {
            return (m_QueuedPauseMask & (1 << inGroup)) != 0;
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
        [Conditional("DEVELOPMENT")]
        public void Log(string inMessage)
        {
#if DEVELOPMENT
            if (DebugMode)
            {
                UnityEngine.Debug.Log("[BeauRoutine] " + inMessage);
            }
#endif // DEVELOPMENT
        }
    }
}

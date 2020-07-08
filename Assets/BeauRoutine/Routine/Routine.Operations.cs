/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 Nov 2017
 * 
 * File:    Routine.Operations.cs
 * Purpose: Public API for starting, stopping, pausing, resuming,
 *          and querying Routines.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using BeauRoutine.Internal;
using UnityEngine;

namespace BeauRoutine
{
    public partial struct Routine
    {
        #region Start

        /// <summary>
        /// Runs a routine.
        /// </summary>
        static public Routine Start(IEnumerator inCoroutine)
        {
            Manager m = Manager.Get();
            if (m != null)
                return m.RunFiber(null, inCoroutine);
            return Routine.Null;
        }

        /// <summary>
        /// Runs a routine.
        /// </summary>
        static public Routine Start(MonoBehaviour inHost, IEnumerator inCoroutine)
        {
            Manager m = Manager.Get();
            if (m != null)
                return m.RunFiber(inHost, inCoroutine);
            return Routine.Null;
        }

        #endregion

        #region Stop

        /// <summary>
        /// Stops all currently running routines.
        /// </summary>
        static public void StopAll()
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new NullQuery(), new StopOperation());
        }

        /// <summary>
        /// Stops currently running routines with the given name.
        /// </summary>
        static public void Stop(string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new NameQuery() { Name = inName }, new StopOperation());
        }

        /// <summary>
        /// Stops all currently running routines that match up with the given group mask.
        /// </summary>
        static public void Stop(int inGroupMask)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GroupQuery() { GroupMask = inGroupMask }, new StopOperation());
        }

        /// <summary>
        /// Stops all currently running routines on the given host.
        /// </summary>
        static public void StopAll(MonoBehaviour inHost)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new MonoBehaviourQuery() { Host = inHost }, new StopOperation());
        }

        /// <summary>
        /// Stops currently running routines on the given host with the given name.
        /// </summary>
        static public void Stop(MonoBehaviour inHost, string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new StopOperation());
        }

        /// <summary>
        /// Stops currently running routines on the given host in the given groups.
        /// </summary>
        static public void Stop(MonoBehaviour inHost, int inGroupMask)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new MonoBehaviourGroupQuery() { Host = inHost, GroupMask = inGroupMask }, new StopOperation());
        }

        /// <summary>
        /// Stops all currently running routines on the given host.
        /// </summary>
        static public void StopAll(GameObject inHost)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GameObjectQuery() { Host = inHost }, new StopOperation());
        }

        /// <summary>
        /// Stops currently running routines on the given host with the given name.
        /// </summary>
        static public void Stop(GameObject inHost, string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GameObjectNameQuery() { Host = inHost, Name = inName }, new StopOperation());
        }

        /// <summary>
        /// Stops currently running routines on the given host in the given groups.
        /// </summary>
        static public void Stop(GameObject inHost, int inGroupMask)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GameObjectGroupQuery() { Host = inHost, GroupMask = inGroupMask }, new StopOperation());
        }

        #endregion

        #region Pause

        /// <summary>
        /// Pauses all currently running routines.
        /// </summary>
        static public void PauseAll()
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Paused = true;
        }

        /// <summary>
        /// Pauses all currently running routines on the given host.
        /// </summary>
        static public void PauseAll(MonoBehaviour inHost)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new MonoBehaviourQuery() { Host = inHost }, new PauseOperation());
        }

        /// <summary>
        /// Pauses currently running routines on the given host with the given name.
        /// </summary>
        static public void Pause(MonoBehaviour inHost, string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new PauseOperation());
        }

        /// <summary>
        /// Pauses all currently running routines on the given host.
        /// </summary>
        static public void PauseAll(GameObject inHost)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GameObjectQuery() { Host = inHost }, new PauseOperation());
        }

        /// <summary>
        /// Pauses currently running routines on the given host with the given name.
        /// </summary>
        static public void Pause(GameObject inHost, string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GameObjectNameQuery() { Host = inHost, Name = inName }, new PauseOperation());
        }

        /// <summary>
        /// Pauses currently running routines with the given name.
        /// </summary>
        static public void Pause(string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new NameQuery() { Name = inName }, new PauseOperation());
        }

        #endregion

        #region Resume

        /// <summary>
        /// Resumes all currently running routines.
        /// </summary>
        static public void ResumeAll()
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Paused = false;
        }

        /// <summary>
        /// Resumes all currently running routines on the given host.
        /// </summary>
        static public void ResumeAll(MonoBehaviour inHost)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new MonoBehaviourQuery() { Host = inHost }, new ResumeOperation());
        }

        /// <summary>
        /// Resumes currently running routines on the given host with the given name.
        /// </summary>
        static public void Resume(MonoBehaviour inHost, string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new ResumeOperation());
        }

        /// <summary>
        /// Resumes all currently running routines on the given host.
        /// </summary>
        static public void ResumeAll(GameObject inHost)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GameObjectQuery() { Host = inHost }, new ResumeOperation());
        }

        /// <summary>
        /// Resumes currently running routines on the given host with the given name.
        /// </summary>
        static public void Resume(GameObject inHost, string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GameObjectNameQuery() { Host = inHost, Name = inName }, new ResumeOperation());
        }

        /// <summary>
        /// Resumes currently running routines with the given name.
        /// </summary>
        static public void Resume(string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new NameQuery() { Name = inName }, new ResumeOperation());
        }

        #endregion

        #region Query

        /// <summary>
        /// Adds all currently running routines on the given host to the given collection.
        /// </summary>
        static public void FindAll(MonoBehaviour inHost, ref ICollection<Routine> ioRoutines)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new MonoBehaviourQuery() { Host = inHost }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Adds currently running routines on the given host with the given name to the given collection.
        /// </summary>
        static public void Find(MonoBehaviour inHost, string inName, ref ICollection<Routine> ioRoutines)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Returns the first currently running routine on the given host with the given name.
        /// </summary>
        static public Routine Find(MonoBehaviour inHost, string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                return m.Fibers.RunQueryFirst(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new NullOperation());
            return Routine.Null;
        }

        /// <summary>
        /// Adds all currently running routines on the given host to the given collection.
        /// </summary>
        static public void FindAll(GameObject inHost, ref ICollection<Routine> ioRoutines)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GameObjectQuery() { Host = inHost }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Adds currently running routines on the given host with the given name to the given collection.
        /// </summary>
        static public void Find(GameObject inHost, string inName, ref ICollection<Routine> ioRoutines)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new GameObjectNameQuery() { Host = inHost, Name = inName }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Returns the first currently running routine on the given host with the given name.
        /// </summary>
        static public Routine Find(GameObject inHost, string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                return m.Fibers.RunQueryFirst(new GameObjectNameQuery() { Host = inHost, Name = inName }, new NullOperation());
            return Routine.Null;
        }

        /// <summary>
        /// Adds currently running routines with the given name to the given collection.
        /// </summary>
        static public void Find(string inName, ref ICollection<Routine> ioRoutines)
        {
            Manager m = Manager.Get();
            if (m != null)
                m.Fibers.RunQueryAll(new NameQuery() { Name = inName }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Returns the first currently running routine with the given name.
        /// </summary>
        static public Routine Find(string inName)
        {
            Manager m = Manager.Get();
            if (m != null)
                return m.Fibers.RunQueryFirst(new NameQuery() { Name = inName }, new NullOperation());
            return Routine.Null;
        }

        #endregion

        #region Manual Updates

        /// <summary>
        /// Updates all routines with the Manual update mode.
        /// Note that you cannot nest a ManualUpdate call within a ManualUpdate call.
        /// </summary>
        static public bool ManualUpdate()
        {
            return ManualUpdate(Time.deltaTime);
        }

        /// <summary>
        /// Updates all routines with the Manual update mode with the given delta time.
        /// Note that you cannot nest a ManualUpdate call within a ManualUpdate call.
        /// </summary>
        static public bool ManualUpdate(float inDeltaTime)
        {
            Manager m = Manager.Get();
            if (m != null)
                return m.ManualUpdate(inDeltaTime);
            return false;
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the BeauRoutine system.
        /// This will happen automatically on startup.
        /// </summary>
        static public void Initialize()
        {
            Manager.Create();
        }

        /// <summary>
        /// Shuts down the BeauRoutine system.
        /// Any further calls to BeauRoutine functions will throw an exception
        /// until Initialize() is explicitly called again.
        /// </summary>
        static public void Shutdown()
        {
            Manager.Destroy();
        }

        #endregion

        #region Settings

        /// <summary>
        /// Contains global settings.
        /// </summary>
        static public class Settings
        {
            /// <summary>
            /// BeauRoutine version number.
            /// </summary>
            static public Version Version
            {
                get { return Manager.VERSION; }
            }

            /// <summary>
            /// Sets the maximum number of concurrent routines
            /// before more memory must be allocated to run routines.
            /// </summary>
            static public void SetCapacity(int inCapacity)
            {
                Manager m = Manager.Get();
                if (m != null)
                    m.Fibers.SetCapacity(inCapacity);
            }

            /// <summary>
            /// Enables/disables debug mode.
            /// Debug mode actives additional checks and profiling.
            /// </summary>
            static public bool DebugMode
            {
                get
                {
                    #if DEVELOPMENT
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.DebugMode;
                    #endif // DEVELOPMENT
                    return false;
                }
                set
                {
                    #if DEVELOPMENT
                    Manager m = Manager.Get();
                    if (m != null)
                        m.DebugMode = value;
                    #endif // DEVELOPMENT
                }
            }

            /// <summary>
            /// Enables/disables profiling.
            /// Profiling will capture data about execution time
            /// and periodically log stats to the console.
            /// </summary>
            static public bool ProfilingEnabled
            {
                get
                {
                    #if DEVELOPMENT
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.ProfilingEnabled;
                    #endif // DEVELOPMENT
                    return false;
                }
                set
                {
                    #if DEVELOPMENT
                    Manager m = Manager.Get();
                    if (m != null)
                        m.ProfilingEnabled = value;
                    #endif // DEVELOPMENT
                }
            }

            /// <summary>
            /// Enables/disables exception handling.
            /// By default this is enabled.
            /// </summary>
            static public bool HandleExceptions
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.HandleExceptions;
                    return true;
                }
                set
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        m.HandleExceptions = value;
                }
            }

            /// <summary>
            /// Gets/sets the default phase for instantiated routines.
            /// </summary>
            static public RoutinePhase DefaultPhase
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.DefaultPhase;
                    return RoutinePhase.LateUpdate;
                }
                set
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        m.DefaultPhase = value;
                }
            }

            /// <summary>
            /// Enables/disables snapshot profiling.
            /// Snapshots capture the routines running at the point
            /// when a new high watermark is established for concurrent routines.
            /// This will only function when both Debug Mode and Profiling are enabled.
            /// </summary>
            static public bool SnapshotEnabled
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.SnapshotEnabled;
                    return false;
                }
                set
                {
                    #if DEVELOPMENT
                    Manager m = Manager.Get();
                    if (m != null)
                        m.SnapshotEnabled = value;
                    #endif // DEVELOPMENT
                }
            }

            /// <summary>
            /// Pauses/resumes Routine updates.
            /// </summary>
            static public bool Paused
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.SystemPaused;
                    return false;
                }
                set
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        m.SystemPaused = value;
                }
            }

            /// <summary>
            /// Gets/sets the interval between ThinkUpdate phases.
            /// </summary>
            static public float ThinkUpdateInterval
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.ThinkUpdateInterval;
                    return Manager.DEFAULT_THINKUPDATE_INTERVAL;
                }
                set
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        m.SetThinkUpdateInterval(value);
                }
            }

            /// <summary>
            /// Gets/sets the interval between CustomUpdate phases.
            /// </summary>
            static public float CustomUpdateInterval
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.CustomUpdateInterval;
                    return Manager.DEFAULT_CUSTOMUPDATE_INTERVAL;
                }
                set
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        m.SetCustomUpdateInterval(value);
                }
            }

            /// <summary>
            /// Gets/sets the expected milliseconds budget per frame.
            /// </summary>
            static public double FrameDurationBudgetMS
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.FrameDurationBudgetTicks / TimeSpan.TicksPerMillisecond;
                    return Manager.CalculateDefaultFrameBudgetTicks() / TimeSpan.TicksPerMillisecond;
                }
                set
                {
                    Manager m = Manager.Get();
                    if (m != null)
                    {
                        m.SetFrameBudget(value);
                    }
                }
            }

            /// <summary>
            /// Gets/sets the minimum async milliseconds budget per frame.
            /// </summary>
            static public double AsyncBudgetMinMS
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.AsyncBudgetTicksMin / TimeSpan.TicksPerMillisecond;
                    return Manager.CalculateDefaultFrameBudgetTicks() * Manager.DEFAULT_ASYNC_PERCENTAGE_MIN / TimeSpan.TicksPerMillisecond;
                }
                set
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        m.AsyncBudgetTicksMin = (long) (TimeSpan.TicksPerMillisecond * value);
                }
            }

            /// <summary>
            /// Gets/sets the maximum async milliseconds budget per frame.
            /// </summary>
            static public double AsyncBudgetMaxMS
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.AsyncBudgetTicksMax / TimeSpan.TicksPerMillisecond;
                    return Manager.CalculateDefaultFrameBudgetTicks() * Manager.DEFAULT_ASYNC_PERCENTAGE_MAX / TimeSpan.TicksPerMillisecond;
                }
                set
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        m.AsyncBudgetTicksMax = (long) (TimeSpan.TicksPerMillisecond * value);
                }
            }

            /// <summary>
            /// Gets/sets whether or not to force single-threaded mode for async operations.
            /// </summary>
            static public bool ForceSingleThreaded
            {
                get
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        return m.ForceSingleThreaded;
                    return false;
                }
                set
                {
                    Manager m = Manager.Get();
                    if (m != null)
                        m.ForceSingleThreaded = value;
                }
            }
        }

        #endregion
    }
}
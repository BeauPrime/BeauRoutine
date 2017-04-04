/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 Nov 2017
 * 
 * File:    Routine.Operations.cs
 * Purpose: Public API for starting, stopping, pausing, resuming,
 *          and querying Routines.
*/

using BeauRoutine.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine
{
    public partial struct Routine
    {
        // Start with a manager.
        static private Manager s_Manager = new Manager();

        static private Manager GetManager()
        {
            if (s_Manager == null)
                throw new InvalidOperationException("BeauRoutine has been shutdown. Please call Initialize() before anything else.");
            return s_Manager;
        }

        #region Start

        /// <summary>
        /// Runs a routine.
        /// </summary>
        static public Routine Start(IEnumerator inCoroutine)
        {
            return GetManager().RunFiber(null, inCoroutine);
        }

        /// <summary>
        /// Runs a routine.
        /// </summary>
        static public Routine Start(MonoBehaviour inHost, IEnumerator inCoroutine)
        {
            return GetManager().RunFiber(inHost, inCoroutine);
        }

        #endregion

        #region Stop

        /// <summary>
        /// Stops all currently running routines.
        /// </summary>
        static public void StopAll()
        {
            GetManager().Fibers.RunQueryAll(new NullQuery(), new StopOperation());
        }

        /// <summary>
        /// Stops all currently running routines on the given host.
        /// </summary>
        static public void StopAll(MonoBehaviour inHost)
        {
            GetManager().Fibers.RunQueryAll(new MonoBehaviourQuery() { Host = inHost }, new StopOperation());
        }

        /// <summary>
        /// Stops currently running routines on the given host with the given name.
        /// </summary>
        static public void Stop(MonoBehaviour inHost, string inName)
        {
            GetManager().Fibers.RunQueryAll(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new StopOperation());
        }

        /// <summary>
        /// Stops all currently running routines on the given host.
        /// </summary>
        static public void StopAll(GameObject inHost)
        {
            GetManager().Fibers.RunQueryAll(new GameObjectQuery() { Host = inHost }, new StopOperation());
        }

        /// <summary>
        /// Stops currently running routines on the given host with the given name.
        /// </summary>
        static public void Stop(GameObject inHost, string inName)
        {
            GetManager().Fibers.RunQueryAll(new GameObjectNameQuery() { Host = inHost, Name = inName }, new StopOperation());
        }

        /// <summary>
        /// Stops currently running routines with the given name.
        /// </summary>
        static public void Stop(string inName)
        {
            GetManager().Fibers.RunQueryAll(new NameQuery() { Name = inName }, new StopOperation());
        }

        #endregion

        #region Pause

        /// <summary>
        /// Pauses all currently running routines.
        /// </summary>
        static public void PauseAll()
        {
            GetManager().Paused = true;
        }

        /// <summary>
        /// Pauses all currently running routines on the given host.
        /// </summary>
        static public void PauseAll(MonoBehaviour inHost)
        {
            GetManager().Fibers.RunQueryAll(new MonoBehaviourQuery() { Host = inHost }, new PauseOperation());
        }

        /// <summary>
        /// Pauses currently running routines on the given host with the given name.
        /// </summary>
        static public void Pause(MonoBehaviour inHost, string inName)
        {
            GetManager().Fibers.RunQueryAll(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new PauseOperation());
        }

        /// <summary>
        /// Pauses all currently running routines on the given host.
        /// </summary>
        static public void PauseAll(GameObject inHost)
        {
            GetManager().Fibers.RunQueryAll(new GameObjectQuery() { Host = inHost }, new PauseOperation());
        }

        /// <summary>
        /// Pauses currently running routines on the given host with the given name.
        /// </summary>
        static public void Pause(GameObject inHost, string inName)
        {
            GetManager().Fibers.RunQueryAll(new GameObjectNameQuery() { Host = inHost, Name = inName }, new PauseOperation());
        }

        /// <summary>
        /// Pauses currently running routines with the given name.
        /// </summary>
        static public void Pause(string inName)
        {
            GetManager().Fibers.RunQueryAll(new NameQuery() { Name = inName }, new PauseOperation());
        }

        #endregion

        #region Resume

        /// <summary>
        /// Resumes all currently running routines.
        /// </summary>
        static public void ResumeAll()
        {
            GetManager().Paused = false;
        }

        /// <summary>
        /// Resumes all currently running routines on the given host.
        /// </summary>
        static public void ResumeAll(MonoBehaviour inHost)
        {
            GetManager().Fibers.RunQueryAll(new MonoBehaviourQuery() { Host = inHost }, new ResumeOperation());
        }

        /// <summary>
        /// Resumes currently running routines on the given host with the given name.
        /// </summary>
        static public void Resume(MonoBehaviour inHost, string inName)
        {
            GetManager().Fibers.RunQueryAll(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new ResumeOperation());
        }

        /// <summary>
        /// Resumes all currently running routines on the given host.
        /// </summary>
        static public void ResumeAll(GameObject inHost)
        {
            GetManager().Fibers.RunQueryAll(new GameObjectQuery() { Host = inHost }, new ResumeOperation());
        }

        /// <summary>
        /// Resumes currently running routines on the given host with the given name.
        /// </summary>
        static public void Resume(GameObject inHost, string inName)
        {
            GetManager().Fibers.RunQueryAll(new GameObjectNameQuery() { Host = inHost, Name = inName }, new ResumeOperation());
        }

        /// <summary>
        /// Resumes currently running routines with the given name.
        /// </summary>
        static public void Resume(string inName)
        {
            GetManager().Fibers.RunQueryAll(new NameQuery() { Name = inName }, new ResumeOperation());
        }

        #endregion

        #region Query

        /// <summary>
        /// Adds all currently running routines on the given host to the given collection.
        /// </summary>
        static public void FindAll(MonoBehaviour inHost, ref ICollection<Routine> ioRoutines)
        {
            GetManager().Fibers.RunQueryAll(new MonoBehaviourQuery() { Host = inHost }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Adds currently running routines on the given host with the given name to the given collection.
        /// </summary>
        static public void Find(MonoBehaviour inHost, string inName, ref ICollection<Routine> ioRoutines)
        {
            GetManager().Fibers.RunQueryAll(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Returns the first currently running routine on the given host with the given name.
        /// </summary>
        static public Routine Find(MonoBehaviour inHost, string inName)
        {
            return GetManager().Fibers.RunQueryFirst(new MonoBehaviourNameQuery() { Host = inHost, Name = inName }, new NullOperation());
        }

        /// <summary>
        /// Adds all currently running routines on the given host to the given collection.
        /// </summary>
        static public void FindAll(GameObject inHost, ref ICollection<Routine> ioRoutines)
        {
            GetManager().Fibers.RunQueryAll(new GameObjectQuery() { Host = inHost }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Adds currently running routines on the given host with the given name to the given collection.
        /// </summary>
        static public void Find(GameObject inHost, string inName, ref ICollection<Routine> ioRoutines)
        {
            GetManager().Fibers.RunQueryAll(new GameObjectNameQuery() { Host = inHost, Name = inName }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Returns the first currently running routine on the given host with the given name.
        /// </summary>
        static public Routine Find(GameObject inHost, string inName)
        {
            return GetManager().Fibers.RunQueryFirst(new GameObjectNameQuery() { Host = inHost, Name = inName }, new NullOperation());
        }

        /// <summary>
        /// Adds currently running routines with the given name to the given collection.
        /// </summary>
        static public void Find(string inName, ref ICollection<Routine> ioRoutines)
        {
            GetManager().Fibers.RunQueryAll(new NameQuery() { Name = inName }, new NullOperation(), ref ioRoutines);
        }

        /// <summary>
        /// Returns the first currently running routine with the given name.
        /// </summary>
        static public Routine Find(string inName)
        {
            return GetManager().Fibers.RunQueryFirst(new NameQuery() { Name = inName }, new NullOperation());
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the BeauRoutine system.
        /// This will happen automatically on startup.
        /// </summary>
        static public void Initialize()
        {
            if (s_Manager == null)
                s_Manager = new Manager();
            s_Manager.Initialize();
        }

        /// <summary>
        /// Shuts down the BeauRoutine system.
        /// Any further calls to BeauRoutine functions will throw an exception
        /// until Initialize() is explicitly called again.
        /// </summary>
        static public void Shutdown()
        {
            if (s_Manager != null)
            {
                if (!s_Manager.IsUpdating)
                {
                    s_Manager.Shutdown();
                    s_Manager = null;
                    return;
                }

                s_Manager.QueueShutdown();
            }
        }

        #endregion

        #region Settings

        /// <summary>
        /// Contains global settings.
        /// </summary>
        static public class Settings
        {
            /// <summary>
            /// Sets the maximum number of concurrent routines
            /// before more memory must be allocated to run routines.
            /// </summary>
            static public void SetCapacity(int inCapacity)
            {
                GetManager().Fibers.SetCapacity(inCapacity);
            }

            /// <summary>
            /// Enables/disables debug mode.
            /// Debug mode actives additional checks and profiling.
            /// </summary>
            static public bool DebugMode
            {
                get { return GetManager().DebugMode; }
                set { GetManager().DebugMode = value; }
            }

            /// <summary>
            /// Enables/disables snapshotting.
            /// Snapshots capture the routines running at the point
            /// when a new high watermark is established for concurrent routines.
            /// </summary>
            static public bool SnapshotEnabled
            {
                get { return GetManager().SnapshotEnabled; }
                set { GetManager().SnapshotEnabled = value; }
            }
        }

        #endregion
    }
}

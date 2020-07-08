/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    RoutineIdentity.cs
 * Purpose: Component allowing for per-object and per-group
 *          time scaling.
*/

using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Identifies a time scale and group for Routines run
    /// on this GameObject. Provides a pause state for the GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("BeauRoutine/Routine Identity", 1)]
    public sealed class RoutineIdentity : MonoBehaviour
    {
        /// <summary>
        /// Pauses all Routines on the GameObject.
        /// </summary>
        public bool Paused = false;

        /// <summary>
        /// Time scale to apply to Routines on this object.
        /// </summary>
        public float TimeScale = 1.0f;

        /// <summary>
        /// Group to use for time scaling.
        /// </summary>
        [RoutineGroupRef]
        public int Group = 0;

        #region Unity Events

        private void Awake()
        {
            RegisterIdentity(this);
        }

        private void OnDestroy()
        {
            UnregisterIdentity(this);
        }

        #endregion

        /// <summary>
        /// Finds or creates the RoutineIdentity for the given GameObject.
        /// </summary>
        static public RoutineIdentity Require(GameObject inGameObject)
        {
            RoutineIdentity obj = Find(inGameObject);
            if (obj == null)
                obj = inGameObject.AddComponent<RoutineIdentity>();
            return obj;
        }

        /// <summary>
        /// Finds or creates the RoutineIdentity for the given behaviour.
        /// </summary>
        static public RoutineIdentity Require(MonoBehaviour inBehaviour)
        {
            RoutineIdentity obj = Find(inBehaviour.gameObject);
            if (obj == null)
                obj = inBehaviour.gameObject.AddComponent<RoutineIdentity>();
            return obj;
        }

        /// <summary>
        /// Returns the RoutineIdentity associated with the given GameObject.
        /// </summary>
        static public RoutineIdentity Find(GameObject inGameObject)
        {
            RoutineIdentity id;
            s_IdentityRegistry.TryGetValue(inGameObject.GetInstanceID(), out id);
            return id;
        }

        /// <summary>
        /// Returns the RoutineIdentity associated with the given behavior.
        /// </summary>
        static public RoutineIdentity Find(MonoBehaviour inBehavior)
        {
            RoutineIdentity id;
            s_IdentityRegistry.TryGetValue(inBehavior.gameObject.GetInstanceID(), out id);
            return id;
        }

        #region Identity Registry

        static private Dictionary<int, RoutineIdentity> s_IdentityRegistry = new Dictionary<int, RoutineIdentity>();

        static private void RegisterIdentity(RoutineIdentity inIdentity)
        {
            int key = inIdentity.gameObject.GetInstanceID();
            if (s_IdentityRegistry.ContainsKey(key))
                return;
            s_IdentityRegistry.Add(key, inIdentity);
        }

        static private void UnregisterIdentity(RoutineIdentity inIdentity)
        {
            int key = inIdentity.gameObject.GetInstanceID();
            s_IdentityRegistry.Remove(key);
        }

        #endregion
    }
}

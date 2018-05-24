/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    10 Apr 2018
 * 
 * File:    RoutineBootstrap.cs
 * Purpose: Component that initializes Routine settings.
*/

using UnityEngine;
using BeauRoutine.Internal;
using System;

namespace BeauRoutine
{
    /// <summary>
    /// Initializes BeauRoutine configuration settings.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("BeauRoutine/Routine Bootstrap", 0)]
    public sealed class RoutineBootstrap : MonoBehaviour
    {
        #region Inspector

        [Header("General Settings")]

        [SerializeField, Tooltip("The initial capacity for concurrent routines before more memory is allocated.")]
        private int m_Capacity = Manager.DEFAULT_CAPACITY;

        [SerializeField, Tooltip("The initial capacity for pooled tweens.")]
        private int m_TweenPool = Tween.DEFAULT_POOL_SIZE;

        [Header("Debug Settings")]

        [SerializeField, Tooltip("Enables type checks.")]
        private bool m_DebugMode = true;

        [SerializeField, Tooltip("Enables profiling. Only functions if DebugMode is enabled.")]
        private bool m_Profiling = true;

        [SerializeField, Tooltip("Enables snapshot profiling. Only functions if Profiling is enabled.")]
        private bool m_Snapshots = false;

        [SerializeField, Tooltip("Enables exception handling by default.")]
        private bool m_HandleExceptions = true;

        [Header("Phase Settings")]

        [SerializeField, Tooltip("The default update phase for new routines.")]
        private RoutinePhase m_DefaultPhase = RoutinePhase.LateUpdate;

        [SerializeField, Tooltip("Interval between ThinkUpdate phases, in seconds.")]
        private float m_ThinkUpdateInterval = Manager.DEFAULT_THINKUPDATE_INTERVAL;

        [SerializeField, Tooltip("Interval between CustomUpdate phases, in seconds.")]
        private float m_CustomUpdateInterval = Manager.DEFAULT_CUSTOMUPDATE_INTERVAL;

        #endregion // Inspector

#if UNITY_EDITOR
        [NonSerialized]
        private bool m_Awoken = false;
#endif // UNITY_EDITOR

        private void Awake()
        {
#if UNITY_EDITOR
            m_Awoken = true;
#endif // UNITY_EDITOR
            Apply();
        }

        private void Apply()
        {
            Routine.Settings.DebugMode = m_DebugMode;
            Routine.Settings.ProfilingEnabled = m_Profiling;
            Routine.Settings.SnapshotEnabled = m_Snapshots;
            Routine.Settings.HandleExceptions = m_HandleExceptions;
            Routine.Settings.SetCapacity(m_Capacity);

            if (m_TweenPool > 0)
                Tween.SetPooled(m_TweenPool);

            Routine.Initialize();

            Routine.Settings.DefaultPhase = m_DefaultPhase;
            Routine.Settings.ThinkUpdateInterval = m_ThinkUpdateInterval;
            Routine.Settings.CustomUpdateInterval = m_CustomUpdateInterval;
        }

        #if UNITY_EDITOR

        private void OnValidate()
        {
            if (UnityEditor.EditorApplication.isPlaying && m_Awoken)
                Apply();
        }

        #endif // UNITY_EDITOR
    }
}

/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 Apr 2018
 * 
 * File:    RoutineBootstrap.cs
 * Purpose: Component that initializes Routine settings.
 */

using System;
using BeauRoutine.Internal;
using UnityEngine;

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

        [Header("Async Settings")]

        [SerializeField, Tooltip("Frame budget, in milliseconds.\nSet to 0 or less to use default budget for framerate.")]
        private double m_FrameBudget = -1;

        [SerializeField, Tooltip("Minimum async budget, in milliseconds.\nSet to 0 or less to use default budget for async.")]
        private double m_AsyncBudgetMin = -1;

        [SerializeField, Tooltip("Maximum async budget, in milliseconds.\nSet to 0 or less to use default budget for async.")]
        private double m_AsyncBudgetMax = -1;

        [SerializeField, Tooltip("If set, single-threaded mode will be enabled for async operations.")]
        private bool m_ForceSingleThreaded = false;

        // private float m_

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

            if (m_FrameBudget > 0)
            {
                Routine.Settings.FrameDurationBudgetMS = m_FrameBudget;
                Routine.Settings.AsyncBudgetMinMS = m_FrameBudget * Manager.DEFAULT_ASYNC_PERCENTAGE_MIN;
                Routine.Settings.AsyncBudgetMaxMS = m_FrameBudget * Manager.DEFAULT_ASYNC_PERCENTAGE_MAX;
            }

            if (m_AsyncBudgetMin > 0)
            {
                Routine.Settings.AsyncBudgetMinMS = m_AsyncBudgetMin;
            }

            if (m_AsyncBudgetMax > 0)
            {
                Routine.Settings.AsyncBudgetMaxMS = m_AsyncBudgetMax;
            }

            Routine.Settings.ForceSingleThreaded = m_ForceSingleThreaded;
        }

        #if UNITY_EDITOR

        private void OnValidate()
        {
            if (UnityEditor.EditorApplication.isPlaying && m_Awoken)
                Apply();

            if (m_FrameBudget > 0 && m_AsyncBudgetMin > m_FrameBudget)
            {
                m_AsyncBudgetMin = m_FrameBudget;
            }

            if (m_FrameBudget > 0 && m_AsyncBudgetMax > m_FrameBudget)
            {
                m_AsyncBudgetMax = m_FrameBudget;
            }
        }

        #endif // UNITY_EDITOR
    }
}
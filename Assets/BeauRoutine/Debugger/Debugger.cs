/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Debugger.cs
 * Purpose: Debugger window for viewing stats and modifying
 *          Routines in editor.
*/

#if UNITY_EDITOR

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
    #define SUPPORTS_DELAYEDFIELDS
#endif

using System;
using UnityEditor;
using UnityEngine;
using BeauRoutine.Internal;

namespace BeauRoutine.Editor
{
    internal class Debugger : EditorWindow
    {
        private const float FIELD_NAME_WIDTH = 80;
        private const float BAR_HEIGHT = 16;

        private enum Page
        {
            Stats,
            Details,
            Options
        }

        public Debugger()
            : base()
        { }

        private Vector2 m_Scroll;
        private Vector2 m_SnapshotScroll;
        private Page m_CurrentPage = Page.Stats;
        private bool m_ShowSnapshot = false;

        [MenuItem("Window/BeauRoutine Debugger", priority=1000)]
        static private void Open()
        {
            var window = EditorWindow.GetWindow<Debugger>();
            window.titleContent = new GUIContent("BeauRoutine");
            window.minSize = new Vector2(400, 200);
            window.hideFlags = HideFlags.HideAndDontSave;
            window.Show();
        }

        private void Update()
        {
            Repaint();
        }

        private void OnGUI()
        {
            InitializeStyles();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("BEAUROUTINE/ ", EditorStyles.boldLabel, GUILayout.Width(120));
                if (EditorApplication.isPlaying || EditorApplication.isPaused)
                    EditorGUILayout.LabelField("Running", GUILayout.Width(100));
                else
                    EditorGUILayout.LabelField("Not Running", GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                PageTab("STATS", Page.Stats);
                PageTab("OPTIONS", Page.Options);
                PageTab("DETAILS", Page.Details);
            }
            EditorGUILayout.EndHorizontal();
            HorizontalDivider();

            GlobalStats globalStats = Manager.Exists() ? Manager.Get().GetGlobalStats() : default(GlobalStats);

            switch(m_CurrentPage)
            {
                case Page.Details:
                    RenderDetails();
                    break;
                case Page.Options:
                    RenderOptions();
                    break;
                case Page.Stats:
                    RenderStats(globalStats);
                    break;
            }
        }

        private void PageTab(string inName, Page inPage)
        {
            bool bSelected = m_CurrentPage == inPage;
            if (bSelected)
                inName = "/" + inName + "/";
            if (GUILayout.Button(inName, EditorStyles.toolbarButton, GUILayout.Width(FIELD_NAME_WIDTH)))
            {
                m_CurrentPage = inPage;
            }
        }

        #region Styles

        [NonSerialized]
        private bool m_StylesInitialzed = false;

        private GUIStyle m_Style_Running;
        private GUIStyle m_Style_Watermark;
        private GUIStyle m_Style_Capacity;

        private void InitializeStyles()
        {
            if (m_StylesInitialzed)
                return;
            m_StylesInitialzed = true;

            m_Style_Running = new GUIStyle(EditorStyles.miniButton);
            m_Style_Running.normal.background = CreateColorTexture(Color.white);
            m_Style_Running.margin = new RectOffset();
            m_Style_Running.padding = new RectOffset();
            m_Style_Running.alignment = TextAnchor.MiddleLeft;

            m_Style_Watermark = new GUIStyle(m_Style_Running);
            m_Style_Watermark.normal.background = CreateColorTexture(Color.gray);
            m_Style_Watermark.alignment = TextAnchor.MiddleRight;

            m_Style_Capacity = new GUIStyle(m_Style_Running);
            m_Style_Capacity.normal.background = CreateColorTexture(Color.black);
            m_Style_Capacity.alignment = TextAnchor.MiddleRight;
        }

        private Texture2D CreateColorTexture(Color inColor)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.hideFlags = HideFlags.DontSave;
            tex.SetPixel(0, 0, inColor);
            tex.Apply();
            return tex;
        }

        #endregion

        #region Stats

        private void RenderStats(GlobalStats inStats)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                RenderUsageStats(inStats);
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            RenderUsageBar(inStats);
            GUILayout.Space(4);
            HorizontalDivider();

            if (inStats.MaxSnapshot == null)
                return;

            m_ShowSnapshot = EditorGUILayout.Foldout(m_ShowSnapshot, "SNAPSHOT/" + inStats.Max.ToString() + "/");
            if (m_ShowSnapshot)
            {
                m_SnapshotScroll = EditorGUILayout.BeginScrollView(m_SnapshotScroll, false, true);
                {
                    BeginIndent();
                    RenderStatGroup(inStats.MaxSnapshot, false, false);
                    EndIndent();
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void RenderUsageStats(GlobalStats inStats)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("RUNNING/", GUILayout.Width(FIELD_NAME_WIDTH));
                EditorGUILayout.LabelField(inStats.Running.ToString());
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("MAX/", GUILayout.Width(FIELD_NAME_WIDTH));
                EditorGUILayout.LabelField(inStats.Max.ToString());
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("CAPACITY/", GUILayout.Width(FIELD_NAME_WIDTH));
                EditorGUILayout.LabelField(inStats.Capacity.ToString());
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("AVG_TIME/", GUILayout.Width(FIELD_NAME_WIDTH));
                EditorGUILayout.LabelField(inStats.AvgMillisecs.ToString("00.000") + "ms");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void RenderUsageBar(GlobalStats inStats)
        {
            float barWidth = this.position.width - 40;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);

            float remainingPercent = 1.0f;

            float runningPercent = (float)inStats.Running / inStats.Capacity;
            if (runningPercent > 0)
            {
                GUILayout.Box(inStats.Running.ToString(), m_Style_Running,
                    GUILayout.Width(barWidth * runningPercent), GUILayout.Height(BAR_HEIGHT));
                remainingPercent -= runningPercent;
            }

            float watermarkPercent = (float)inStats.Max / inStats.Capacity;
            if (watermarkPercent > runningPercent)
            {
                GUILayout.Box((inStats.Max).ToString(), m_Style_Watermark,
                    GUILayout.Width(barWidth * (watermarkPercent - runningPercent)), GUILayout.Height(BAR_HEIGHT));
                remainingPercent -= watermarkPercent - runningPercent;
            }
            
            if (remainingPercent > 0)
                GUILayout.Box((inStats.Capacity).ToString(), m_Style_Capacity,
                    GUILayout.Width(barWidth * (remainingPercent)), GUILayout.Height(BAR_HEIGHT));
            EditorGUILayout.EndHorizontal();
        }

        private void RenderUsageHistory()
        {
            if (Event.current.type != EventType.Repaint)
                return;
        }

        #endregion

        #region Options

        private void RenderOptions()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    #if SUPPORTS_DELAYEDFIELDS
                    Routine.TimeScale = Mathf.Clamp(EditorGUILayout.DelayedFloatField("Time Scale", Routine.TimeScale), 0, 100);
                    #else
                    Routine.TimeScale = Mathf.Clamp(EditorGUILayout.FloatField("Time Scale", Routine.TimeScale), 0, 100);
                    #endif
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Reset", GUILayout.Width(80)))
                        Routine.TimeScale = 1.0f;
                    GUI.enabled = (Routine.TimeScale * 2) < 100;
                    if (GUILayout.Button("Double", GUILayout.Width(80)))
                        Routine.TimeScale *= 2.0f;
                    GUI.enabled = true;
                    if (GUILayout.Button("Half", GUILayout.Width(80)))
                        Routine.TimeScale *= 0.5f;
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if (Manager.Exists())
                {
                    if (Routine.Settings.SnapshotEnabled)
                    {
                        if (GUILayout.Button("Disable Snapshot", GUILayout.Width(150)))
                            Routine.Settings.SnapshotEnabled = false;
                    }
                    else
                    {
                        if (GUILayout.Button("Enable Snapshot", GUILayout.Width(150)))
                            Routine.Settings.SnapshotEnabled = true;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Details

        private void RenderDetails()
        {
            var globalStats = Manager.Exists() ? Manager.Get().GetGlobalStats() : default(GlobalStats);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("RUNNING/", GUILayout.Width(FIELD_NAME_WIDTH));
                EditorGUILayout.LabelField(globalStats.Running.ToString() + " / " + globalStats.Capacity.ToString());
            }
            EditorGUILayout.EndHorizontal();

            var stats = Manager.Exists() ? Manager.Get().GetRoutineStats() : null;
            if (stats == null)
                return;

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll, false, true);
            {
                BeginIndent();
                RenderStatGroup(stats, true, false);
                EndIndent();
            }
            EditorGUILayout.EndScrollView();
        }

        private void RenderStatGroup(RoutineStats[] inStats, bool inbCanAdjust, bool inbNested)
        {
            for (int i = 0; i < inStats.Length; ++i)
            {
                if (i > 0)
                    HorizontalDivider();
                RenderStats(inStats[i], inbCanAdjust, inbNested);
            }
        }

        private void RenderStats(RoutineStats inStats, bool inbCanAdjust, bool inbNested)
        {
            if (!inbNested)
            {
                // Render id
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("ID/", GUILayout.Width(FIELD_NAME_WIDTH));
                    uint handleID = (uint)inStats.Handle;
                    uint index = handleID & Table.INDEX_MASK;
                    uint counter = (handleID & Table.COUNTER_MASK) >> Table.COUNTER_SHIFT;
                    EditorGUILayout.LabelField(index.ToString() + " (" + counter.ToString("X2") + ")", GUILayout.ExpandWidth(true));
                }
                EditorGUILayout.EndHorizontal();

                // Render name
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("NAME/", GUILayout.Width(FIELD_NAME_WIDTH));
                    if (!inbCanAdjust)
                        GUI.enabled = false;
                    string newName = EditorGUILayout.TextField(inStats.Name, GUILayout.ExpandWidth(true));
                    if (newName != inStats.Name)
                        inStats.Handle.SetName(newName);
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (!inbNested && !ReferenceEquals(inStats.Host, null))
            {
                // Render host
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("HOST/", GUILayout.Width(FIELD_NAME_WIDTH));
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(inStats.Host, typeof(MonoBehaviour), true, GUILayout.ExpandWidth(true));
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }

            // Current state
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("STATUS/", GUILayout.Width(FIELD_NAME_WIDTH));
                EditorGUILayout.LabelField(inStats.State, GUILayout.ExpandWidth(true));
            }
            EditorGUILayout.EndHorizontal();

            // Time Scale
            if (!inbNested)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("TIMESCALE/", GUILayout.Width(FIELD_NAME_WIDTH));
                    if (!inbCanAdjust)
                        GUI.enabled = false;
                    #if SUPPORTS_DELAYEDFIELDS
                    float timeScale = EditorGUILayout.DelayedFloatField(inStats.TimeScale, GUILayout.ExpandWidth(true));
                    #else
                    float timeScale = EditorGUILayout.FloatField(inStats.TimeScale, GUILayout.ExpandWidth(true));
                    #endif
                    if (timeScale < 0)
                        timeScale = 0;
                    if (timeScale != inStats.TimeScale)
                        inStats.Handle.SetTimeScale(timeScale);
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }

            // Priority
            if (!inbNested)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("PRIORITY/", GUILayout.Width(FIELD_NAME_WIDTH));
                    if (!inbCanAdjust)
                        GUI.enabled = false;
                    #if SUPPORTS_DELAYEDFIELDS
                    int priority = EditorGUILayout.DelayedIntField(inStats.Priority, GUILayout.ExpandWidth(true));
                    #else
                    int priority = EditorGUILayout.IntField(inStats.Priority, GUILayout.ExpandWidth(true));
                    #endif
                    if (priority != inStats.Priority)
                        inStats.Handle.SetPriority(priority);
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("PHASE/", GUILayout.Width(FIELD_NAME_WIDTH));
                    if (!inbCanAdjust)
                        GUI.enabled = false;
                    RoutinePhase update = (RoutinePhase)EditorGUILayout.EnumPopup(inStats.Phase, GUILayout.ExpandWidth(true));
                    if (update != inStats.Phase)
                        inStats.Handle.SetPhase(update);
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }

            // Current Function Name
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("FUNCTION/" + inStats.StackDepth + "/", GUILayout.Width(FIELD_NAME_WIDTH));
                EditorGUILayout.LabelField(inStats.Function == null ? "[Null]" : inStats.Function, GUILayout.ExpandWidth(true));
            }
            EditorGUILayout.EndHorizontal();

            if (inStats.State != RoutineState.Disposing && inbCanAdjust)
            {
                // Functions
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Pause"))
                        inStats.Handle.Pause();
                    if (GUILayout.Button("Resume"))
                        inStats.Handle.Resume();
                    if (GUILayout.Button("Stop"))
                        inStats.Handle.Stop();
                }
                EditorGUILayout.EndHorizontal();
            }

            if (inStats.Nested != null && inStats.Nested.Length > 0)
            {
                EditorGUILayout.LabelField("NESTED/");
                BeginIndent();
                RenderStatGroup(inStats.Nested, inbCanAdjust, true);
                EndIndent();
            }
        }

        #endregion

        #region Utils

        static private void BeginIndent(float inWidth = 20)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(inWidth);
            EditorGUILayout.BeginVertical();
        }

        static private void EndIndent()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        static private void HorizontalDivider(float inHeight = 2)
        {
            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(inHeight));
        }

        static private void VerticalDivider(float inWidth = 2)
        {
            GUILayout.Box(string.Empty, GUILayout.ExpandHeight(true), GUILayout.Width(inWidth));
        }

        #endregion
    }
}

#endif // UNITY_EDITOR
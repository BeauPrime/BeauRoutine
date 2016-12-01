﻿/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Debugger.cs
 * Purpose: Debugger window for viewing stats and modifying
 *          Routines in editor.
*/

using System;
using UnityEditor;
using UnityEngine;

namespace BeauRoutine.Editor
{
    public class Debugger : EditorWindow
    {
        private const float FIELD_NAME_WIDTH = 80;
        private const float BAR_HEIGHT = 16;

        private enum Page
        {
            Stats,
            Details,
            Groups
        }

        public Debugger()
            : base()
        { }

        private Vector2 m_Scroll;
        private Vector2 m_SnapshotScroll;
        private Page m_CurrentPage = Page.Stats;
        private bool m_ShowSnapshot = false;

        [MenuItem("BeauRoutine/Debugger")]
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
                //PageTab("GROUPS", Page.Groups);
                PageTab("DETAILS", Page.Details);
            }
            EditorGUILayout.EndHorizontal();
            HorizontalDivider();

            Routine.Editor.GlobalStats globalStats = Routine.Editor.GetGlobalStats();

            switch(m_CurrentPage)
            {
                case Page.Details:
                    RenderDetails();
                    break;
                case Page.Groups:
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

        private void RenderStats(Routine.Editor.GlobalStats inStats)
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
                    RenderStatGroup(inStats.MaxSnapshot, false);
                    EndIndent();
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void RenderUsageStats(Routine.Editor.GlobalStats inStats)
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

        private void RenderUsageBar(Routine.Editor.GlobalStats inStats)
        {
            float barWidth = this.position.width - 40;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            float runningPercent = (float)inStats.Running / inStats.Capacity;
            if (runningPercent > 0)
                GUILayout.Box(inStats.Running.ToString(), m_Style_Running,
                    GUILayout.Width(barWidth * runningPercent), GUILayout.Height(BAR_HEIGHT));
            float watermarkPercent = (float)inStats.Max / inStats.Capacity;
            if (watermarkPercent > runningPercent)
                GUILayout.Box((inStats.Max).ToString(), m_Style_Watermark,
                    GUILayout.Width(barWidth * (watermarkPercent - runningPercent)), GUILayout.Height(BAR_HEIGHT));
            float remainingPercent = 1.0f;
            if (watermarkPercent < remainingPercent)
                GUILayout.Box((inStats.Capacity).ToString(), m_Style_Capacity,
                    GUILayout.Width(barWidth * (remainingPercent - watermarkPercent)), GUILayout.Height(BAR_HEIGHT));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void RenderUsageHistory()
        {
            if (Event.current.type != EventType.Repaint)
                return;
        }

        #endregion

        #region Details

        private void RenderDetails()
        {
            var globalStats = Routine.Editor.GetGlobalStats();
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("RUNNING/", GUILayout.Width(FIELD_NAME_WIDTH));
                EditorGUILayout.LabelField(globalStats.Running.ToString() + " / " + globalStats.Capacity.ToString());
            }
            EditorGUILayout.EndHorizontal();

            var stats = Routine.Editor.GetRoutineStats();
            if (stats == null)
                return;

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll, false, true);
            {
                BeginIndent();
                RenderStatGroup(stats, true);
                EndIndent();
            }
            EditorGUILayout.EndScrollView();
        }

        private void RenderStatGroup(Routine.Editor.RoutineStats[] inStats, bool inbCanAdjust)
        {
            for (int i = 0; i < inStats.Length; ++i)
            {
                if (i > 0)
                    HorizontalDivider();
                RenderStats(inStats[i], inbCanAdjust);
            }
        }

        private void RenderStats(Routine.Editor.RoutineStats inStats, bool inbCanAdjust)
        {
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

            if (!ReferenceEquals(inStats.Host, null))
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
                EditorGUILayout.LabelField(inStats.State.ToString(), GUILayout.ExpandWidth(true));
            }
            EditorGUILayout.EndHorizontal();

            // Time Scale
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("TIMESCALE/", GUILayout.Width(FIELD_NAME_WIDTH));
                if (!inbCanAdjust)
                    GUI.enabled = false;
                float timeScale = EditorGUILayout.FloatField(inStats.TimeScale, GUILayout.ExpandWidth(true));
                if (timeScale < 0)
                    timeScale = 0;
                if (timeScale != inStats.TimeScale)
                    inStats.Handle.SetTimeScale(timeScale);
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            // Current Function Name
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("FUNCTION/" + inStats.StackDepth + "/", GUILayout.Width(FIELD_NAME_WIDTH));
                EditorGUILayout.LabelField(inStats.Function == null ? "[Null]" : inStats.Function, GUILayout.ExpandWidth(true));
            }
            EditorGUILayout.EndHorizontal();

            if (inStats.State != Routine.Editor.RoutineState.Disposing && inbCanAdjust)
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
                RenderStatGroup(inStats.Nested, inbCanAdjust);
                EndIndent();
            }
        }

        #endregion

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
    }
}
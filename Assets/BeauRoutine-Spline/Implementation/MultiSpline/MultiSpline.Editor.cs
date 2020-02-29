/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 May 2018
 * 
 * File:    MultiSpline.Editor.cs
 * Purpose: Custom editor for a MultiSpline.
*/

#if UNITY_EDITOR
#define DEVELOPMENT

using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace BeauRoutine.Splines
{
    public sealed partial class MultiSpline : MonoBehaviour, ISpline
    {
        [CustomEditor(typeof(MultiSpline))]
        private class MultiSplineEditor : UnityEditor.Editor
        {
            #region Styles

            [NonSerialized]
            static private bool s_StylesInitialized = false;

            static private GUIStyle s_ActivatedButton;

            static private void InitializeStyles()
            {
                if (s_StylesInitialized)
                    return;

                s_ActivatedButton = new GUIStyle(EditorStyles.miniButton);
                s_ActivatedButton.normal.textColor = s_ActivatedButton.hover.textColor = s_ActivatedButton.active.textColor = s_ActivatedButton.focused.textColor = Color.yellow;
                s_StylesInitialized = true;
            }

            #endregion

            [SerializeField]
            private MultiSpline s_CurrentlyEditing = null;

            [NonSerialized]
            private bool m_UndoMarked = false;

            [NonSerialized]
            private int m_SelectedNodeIdx = -1;

            #region Unity Events

            private void OnEnable()
            {
                Undo.undoRedoPerformed -= MarkSplineAsDirty;
                Undo.undoRedoPerformed += MarkSplineAsDirty;
            }

            private void OnDisable()
            {
                if (s_CurrentlyEditing == target)
                    s_CurrentlyEditing = null;

                Undo.undoRedoPerformed += MarkSplineAsDirty;
            }

            #endregion // Unity Events

            #region InspectorGUI

            public override void OnInspectorGUI()
            {
                MultiSpline ms = (MultiSpline)target;
                if (!ms)
                    return;

                InitializeStyles();
                ResetUndo();

                // Editing controls
                if (ms.m_Type != GenType.Uninitialized)
                {
                    HeaderText("Edit Controls");

                    if (s_CurrentlyEditing == ms)
                    {
                        if (GUILayout.Button("Stop Editing", s_ActivatedButton))
                        {
                            s_CurrentlyEditing = null;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Edit", EditorStyles.miniButton))
                        {
                            s_CurrentlyEditing = ms;
                        }
                    }
                }

                using (new EnableGUI(s_CurrentlyEditing == ms || ms.m_Type == GenType.Uninitialized))
                {
                    HeaderText("Spline Types");

                    if (ms.m_Type == GenType.Uninitialized)
                    {
                        EditorGUILayout.HelpBox("This spline has not been initialized. Select one of the following types before starting.", MessageType.Warning);
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Simple Spline", ms.m_Type == GenType.Simple ? s_ActivatedButton : EditorStyles.miniButton))
                        {
                            MarkDirty("Converted to Simple");
                            ConvertTo(ms, GenType.Simple);
                            s_CurrentlyEditing = ms;
                        }
                        if (GUILayout.Button("Linear Spline", ms.m_Type == GenType.Linear ? s_ActivatedButton : EditorStyles.miniButton))
                        {
                            MarkDirty("Converted to Linear");
                            ConvertTo(ms, GenType.Linear);
                            s_CurrentlyEditing = ms;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("C-Spline", ms.m_Type == GenType.CSpline ? s_ActivatedButton : EditorStyles.miniButton))
                        {
                            MarkDirty("Converted to CSpline");
                            ConvertTo(ms, GenType.CSpline);
                            s_CurrentlyEditing = ms;
                        }
                        if (GUILayout.Button("Catmull-Rom / Cardinal", ms.m_Type == GenType.Cardinal ? s_ActivatedButton : EditorStyles.miniButton))
                        {
                            MarkDirty("Converted to Cardinal");
                            ConvertTo(ms, GenType.Cardinal);
                            s_CurrentlyEditing = ms;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (ms.m_Type != GenType.Uninitialized)
                    {
                        GUILayout.Space(8);

                        switch (ms.m_Type)
                        {
                            case GenType.Simple:
                                InspectorGUI_Simple(ms);
                                break;

                            case GenType.Linear:
                                InspectorGUI_Linear(ms);
                                break;

                            case GenType.CSpline:
                                InspectorGUI_CSpline(ms);
                                break;

                            case GenType.Cardinal:
                                InspectorGUI_Cardinal(ms);
                                break;
                        }

                        GUILayout.Space(8);

                        HeaderText("Utilities", 80);

                        if (GUILayout.Button("Re-center Vertices"))
                        {
                            MarkDirty("Recenter Vertices");
                            RecenterVertices(ms, Vector3.zero);
                        }
                    }
                }
            }

            #region Per-Spline Inspectors

            private void InspectorGUI_Simple(MultiSpline inSpline)
            {
                HeaderText("Simple Spline");

                Vector3Field("Start Position", ref inSpline.m_Vertices[0].Point);
                Vector3Field("End Position", ref inSpline.m_Vertices[1].Point);
                Vector3Field("Control", ref inSpline.m_ControlPointA);
            }

            private void InspectorGUI_Linear(MultiSpline inSpline)
            {
                HeaderText("Linear Spline");

                ToggleField("Looped", ref inSpline.m_Looped);
            }

            private void InspectorGUI_CSpline(MultiSpline inSpline)
            {
                HeaderText("C-Spline", 80);

                ToggleField("Looped", ref inSpline.m_Looped);
            }

            private void InspectorGUI_Cardinal(MultiSpline inSpline)
            {
                HeaderText("Catmull-Rom / Cardinal Spline", 200);

                ToggleField("Looped", ref inSpline.m_Looped);

                SliderField("Tension", ref inSpline.m_CRTension, -3, 1);
            }

            #endregion // Per-Spline Inspectors

            #region Field Types

            private void ToggleField(string inName, ref bool ioValue)
            {
                EditorGUI.BeginChangeCheck();
                bool bNewValue = EditorGUILayout.Toggle(inName, ioValue);
                if (EditorGUI.EndChangeCheck() && bNewValue != ioValue)
                {
                    MarkDirty("Changed " + inName);
                    ioValue = bNewValue;
                }
            }

            private void SliderField(string inName, ref float ioValue, int inMin, int inMax)
            {
                EditorGUI.BeginChangeCheck();
                float newValue = EditorGUILayout.Slider(inName, ioValue, inMin, inMax);
                if (EditorGUI.EndChangeCheck() && newValue != ioValue)
                {
                    MarkDirty("Changed " + inName);
                    ioValue = newValue;
                }
            }

            private void Vector3Field(string inName, ref Vector3 ioValue)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newValue = EditorGUILayout.Vector3Field(inName, ioValue);
                if (EditorGUI.EndChangeCheck())
                {
                    MarkDirty("Changed " + inName);
                    ioValue = newValue;
                }
            }

            #endregion // Field Types

            #endregion // InspectorGUI

            #region Scene GUI

            public void OnSceneGUI()
            {
                MultiSpline ms = (MultiSpline)target;
                if (!ms || ms.m_Type == GenType.Uninitialized || s_CurrentlyEditing != ms)
                    return;

                InitializeStyles();
                ResetUndo();

                switch (ms.m_Type)
                {
                    case GenType.Simple:
                        SceneGUI_Simple(ms);
                        break;

                    case GenType.Linear:
                        SceneGUI_Linear(ms);
                        break;

                    case GenType.CSpline:
                        SceneGUI_CSpline(ms);
                        break;

                    case GenType.Cardinal:
                        SceneGUI_Cardinal(ms);
                        break;
                }
            }

            private void SceneGUI_Simple(MultiSpline inSpline)
            {
                Transform tr = inSpline.transform;

                if (Tools.current == Tool.Move)
                {
                    // Starting position
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 newStartPos = Handles.PositionHandle(
                            tr.TransformPoint(inSpline.m_Vertices[0].Point),
                            Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            MarkDirty("Changed starting position");
                            inSpline.m_Vertices[0].Point = tr.InverseTransformPoint(newStartPos);
                        }
                    }

                    // Ending position
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 newEndPos = Handles.PositionHandle(
                            tr.TransformPoint(inSpline.m_Vertices[1].Point),
                            Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            MarkDirty("Changed ending position");
                            inSpline.m_Vertices[1].Point = tr.InverseTransformPoint(newEndPos);
                        }
                    }

                    // Control position
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 newControlPos = Handles.PositionHandle(
                            tr.TransformPoint(inSpline.m_ControlPointA),
                            Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            MarkDirty("Changed control position");
                            inSpline.m_ControlPointA = tr.InverseTransformPoint(newControlPos);
                        }
                    }
                }
            }

            private void SceneGUI_Linear(MultiSpline inSpline)
            {
                Transform tr = inSpline.transform;

                if (Tools.current == Tool.Move)
                {
                    for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 pos = Handles.PositionHandle(
                            tr.TransformPoint(inSpline.m_Vertices[i].Point),
                            Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            MarkDirty("Changed node position");
                            inSpline.m_Vertices[i].Point = tr.InverseTransformPoint(pos);
                        }
                    }
                }
            }

            private void SceneGUI_CSpline(MultiSpline inSpline)
            {
                Transform tr = inSpline.transform;

                if (Tools.current == Tool.Move)
                {
                    for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 pos = Handles.PositionHandle(
                            tr.TransformPoint(inSpline.m_Vertices[i].Point),
                            Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            MarkDirty("Changed node position");
                            inSpline.m_Vertices[i].Point = tr.InverseTransformPoint(pos);
                        }
                    }
                }
            }

            private void SceneGUI_Cardinal(MultiSpline inSpline)
            {
                Transform tr = inSpline.transform;

                if (Tools.current == Tool.Move)
                {
                    for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 pos = Handles.PositionHandle(
                            tr.TransformPoint(inSpline.m_Vertices[i].Point),
                            Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            MarkDirty("Changed node position");
                            inSpline.m_Vertices[i].Point = tr.InverseTransformPoint(pos);
                        }
                    }

                    if (!inSpline.m_Looped)
                    {
                        // Control point A
                        {
                            EditorGUI.BeginChangeCheck();
                            Vector3 pos = Handles.PositionHandle(
                                tr.TransformPoint(inSpline.m_ControlPointA),
                                Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                MarkDirty("Changed control point");
                                inSpline.m_ControlPointA = tr.InverseTransformPoint(pos);
                            }
                        }

                        // Control point B
                        {
                            EditorGUI.BeginChangeCheck();
                            Vector3 pos = Handles.PositionHandle(
                                tr.TransformPoint(inSpline.m_ControlPointB),
                                Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                MarkDirty("Changed control point");
                                inSpline.m_ControlPointB = tr.InverseTransformPoint(pos);
                            }
                        }
                    }
                }
            }

            #endregion // Scene GUI

            #region Utilities

            private struct EnableGUI : IDisposable
            {
                private bool m_WasEnabled;

                public EnableGUI(bool inbEnable)
                {
                    m_WasEnabled = GUI.enabled;
                    GUI.enabled = inbEnable;
                }

                public void Dispose()
                {
                    GUI.enabled = m_WasEnabled;
                }
            }

            static private void HeaderText(string inText, float inWidth = 100f)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(inText, EditorStyles.boldLabel, GUILayout.Width(inWidth));
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }

            #endregion // Utilities

            #region Undo

            private void ResetUndo()
            {
                m_UndoMarked = false;
            }

            private void MarkDirty(string inTag)
            {
                if (!m_UndoMarked)
                {
                    Undo.RecordObject(target, "[MultiSpline]: " + inTag);
                    m_UndoMarked = true;
                    EditorUtility.SetDirty(target);
                    MarkSplineAsDirty();
                }
            }

            private void MarkSplineAsDirty()
            {
                MultiSpline ms = (MultiSpline)target;
                if (ms != null)
                    ms.m_Dirty = true;
            }

            #endregion // Undo
        }
    }
}

#endif // UNITY_EDITOR
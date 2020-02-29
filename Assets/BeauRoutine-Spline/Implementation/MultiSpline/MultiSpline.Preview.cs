/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    29 May 2018
 * 
 * File:    MultiSpline.Preview.cs
 * Purpose: MultiSpline preview rendering.
*/

#if UNITY_EDITOR
#define DEVELOPMENT

using System;
using UnityEngine;
using UnityEditor;

namespace BeauRoutine.Splines
{
    public sealed partial class MultiSpline : MonoBehaviour, ISpline
    {
        static private Vector3[] s_CachedSamples = new Vector3[256];
        static private class GizmoConfig
        {
            static public readonly Vector3 NodeSizeStart = new Vector3(0.4f, 0.4f, 0.4f);
            static public readonly Vector3 NodeSizeNormal = new Vector3(0.2f, 0.2f, 0.2f);
            static public readonly Vector3 NodeSizeControl = new Vector3(0.15f, 0.15f, 0.15f);
            static public readonly Vector3 NodeSizeOutTangent = NodeSizeControl;
            static public readonly Vector3 NodeSizeInTangent = NodeSizeControl;

            static public readonly Color NodeColorStart = Color.cyan;
            static public readonly Color NodeColorNormal = Color.green;
            static public readonly Color NodeColorControl = Color.yellow;
            static public readonly Color NodeColorOutTangent = Color.yellow;
            static public readonly Color NodeColorInTangent = Color.blue;

            static public readonly Color LineColorNormalStart = Color.cyan;
            static public readonly Color LineColorNormalEnd = Color.red;
            static public readonly Color LineColorControl = Color.yellow;
            static public readonly Color LineColorOutTangent = Color.yellow;
            static public readonly Color LineColorInTangent = Color.blue;
        }

        private void OnDrawGizmos()
        {
            if (m_Type == GenType.Uninitialized)
                return;

            RefreshSpline();

            switch (m_Type)
            {
                case GenType.Simple:
                    RenderGizmo_SimplePreview(this, transform);
                    break;

                case GenType.Linear:
                    RenderGizmo_LinearPreview(this, transform);
                    break;

                case GenType.CSpline:
                    RenderGizmo_CSplinePreview(this, transform);
                    break;

                case GenType.Cardinal:
                    RenderGizmo_CardinalPreview(this, transform);
                    break;
            }
        }

        static private void RenderGizmo_SimplePreview(MultiSpline inSpline, Transform inTransform)
        {
            RenderGizmo_PathPreview(inSpline, 0, 1, 24);

            RenderGizmo_PathNode(inTransform, inSpline.m_Vertices[0].Point, true);
            RenderGizmo_PathNode(inTransform, inSpline.m_Vertices[1].Point, false);
            RenderGizmo_ControlPoint(inTransform, inSpline.m_ControlPointA);
            RenderGizmo_ControlLine(inTransform, inSpline.m_Vertices[0].Point, inSpline.m_ControlPointA);
            RenderGizmo_ControlLine(inTransform, inSpline.m_Vertices[1].Point, inSpline.m_ControlPointA);
        }

        static private void RenderGizmo_LinearPreview(MultiSpline inSpline, Transform inTransform)
        {
            int vertCount = inSpline.m_Vertices.Length;
            if (!inSpline.m_Looped)
                --vertCount;

            RenderGizmo_PathPreview(inSpline, 0, 1, 1 + vertCount * 8);

            for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
            {
                RenderGizmo_PathNode(inTransform, inSpline.m_Vertices[i].Point, i == 0);
            }
        }

        static private void RenderGizmo_CSplinePreview(MultiSpline inSpline, Transform inTransform)
        {
            int vertCount = inSpline.m_Vertices.Length;
            if (!inSpline.m_Looped)
                --vertCount;

            for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
            {
                if (i < vertCount)
                    RenderGizmo_PathPreview(inSpline, (float)i / vertCount, (float)(i + 1) / vertCount, 24);

                RenderGizmo_PathNode(inTransform, inSpline.m_Vertices[i].Point, i == 0);
                
                if (inSpline.m_Looped || i > 0)
                {
                    RenderGizmo_TangentLine(inTransform, inSpline.m_Vertices[i].Point, -inSpline.m_Vertices[i].InTangent, false);
                    RenderGizmo_TangentPoint(inTransform, inSpline.m_Vertices[i].Point, -inSpline.m_Vertices[i].InTangent, false);
                }

                if (i < vertCount)
                {
                    RenderGizmo_TangentLine(inTransform, inSpline.m_Vertices[i].Point, inSpline.m_Vertices[i].OutTangent, true);
                    RenderGizmo_TangentPoint(inTransform, inSpline.m_Vertices[i].Point, inSpline.m_Vertices[i].OutTangent, true);
                }
            }
        }

        static private void RenderGizmo_CardinalPreview(MultiSpline inSpline, Transform inTransform)
        {
            for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
            {
                RenderGizmo_PathPreview(inSpline, (float)i / inSpline.m_Vertices.Length, (float)(i + 1) / inSpline.m_Vertices.Length, 24);
                RenderGizmo_PathNode(inTransform, inSpline.m_Vertices[i].Point, i == 0);
            }

            if (!inSpline.m_Looped)
            {
                RenderGizmo_ControlLine(inTransform, inSpline.m_Vertices[0].Point, inSpline.m_ControlPointA);
                RenderGizmo_ControlPoint(inTransform, inSpline.m_ControlPointA);

                RenderGizmo_ControlLine(inTransform, inSpline.m_Vertices[inSpline.m_Vertices.Length - 1].Point, inSpline.m_ControlPointB);
                RenderGizmo_ControlPoint(inTransform, inSpline.m_ControlPointB);
            }
        }

        static private void RenderGizmo_PathNode(Transform inTransform, Vector3 inPoint, bool inbIsStart)
        {
            if (inbIsStart)
            {
                Gizmos.color = GizmoConfig.NodeColorStart;
                Gizmos.DrawCube(
                    inTransform.TransformPoint(inPoint),
                    GizmoConfig.NodeSizeStart);
            }
            else
            {
                Gizmos.color = GizmoConfig.NodeColorNormal;
                Gizmos.DrawCube(
                    inTransform.TransformPoint(inPoint),
                    GizmoConfig.NodeSizeNormal);
            }
        }

        static private void RenderGizmo_ControlPoint(Transform inTransform, Vector3 inPoint)
        {
            Gizmos.color = GizmoConfig.NodeColorControl;
            Gizmos.DrawCube(
                inTransform.TransformPoint(inPoint),
                GizmoConfig.NodeSizeControl);
        }

        static private void RenderGizmo_TangentPoint(Transform inTransform, Vector3 inStart, Vector3 inTangent, bool inbIsOut)
        {
            Gizmos.color = inbIsOut ? GizmoConfig.NodeColorOutTangent : GizmoConfig.NodeColorInTangent;
            Gizmos.DrawCube(
                inTransform.TransformPoint(inStart + inTangent),
                inbIsOut ? GizmoConfig.NodeSizeOutTangent : GizmoConfig.NodeSizeInTangent);
        }

        static private void RenderGizmo_Line(Transform inTransform, Vector3 inStart, Vector3 inEnd)
        {
            Gizmos.color = GizmoConfig.LineColorNormalStart;
            Gizmos.DrawLine(
                inTransform.TransformPoint(inStart),
                inTransform.TransformPoint(inEnd)
            );
        }

        static private void RenderGizmo_ControlLine(Transform inTransform, Vector3 inStart, Vector3 inEnd)
        {
            Handles.color = GizmoConfig.LineColorControl;
            Handles.DrawDottedLine(
                inTransform.TransformPoint(inStart),
                inTransform.TransformPoint(inEnd),
                4
            );
        }

        static private void RenderGizmo_TangentLine(Transform inTransform, Vector3 inStart, Vector3 inTangent, bool inbIsOut)
        {
            Handles.color = inbIsOut ? GizmoConfig.LineColorOutTangent : GizmoConfig.LineColorInTangent;
            Handles.DrawDottedLine(
                inTransform.TransformPoint(inStart),
                inTransform.TransformPoint(inStart + inTangent),
                4
            );
        }

        static private void RenderGizmo_PathPreview(MultiSpline inSpline, float inStart = 0, float inEnd = 1, int inSegments = 64)
        {
            Transform tr = inSpline.transform;
            int numSamples = Spline.Sample(inSpline.m_TransformWrapper, s_CachedSamples, inStart, inEnd, 0, inSegments);
            for (int i = 0; i < numSamples - 1; ++i)
            {
                Vector3 start = s_CachedSamples[i];
                Vector3 next = s_CachedSamples[i + 1];

                Gizmos.color = Color.Lerp(GizmoConfig.LineColorNormalStart, GizmoConfig.LineColorNormalEnd, inStart + (inEnd - inStart) * ((float)i / (numSamples - 1)));
                Gizmos.DrawLine(start, next);
            }
        }
    }
}

#endif // UNITY_EDITOR
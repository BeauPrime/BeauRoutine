/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 May 2018
 * 
 * File:    MultiSpline.Convert.cs
 * Purpose: Methods for converting between multi-spline types
*/

#if UNITY_EDITOR
#define DEVELOPMENT

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    public sealed partial class MultiSpline : MonoBehaviour, ISpline
    {
        #region Conversion

        static private void ConvertTo(MultiSpline inSpline, GenType inType)
        {
            switch(inType)
            {
                case GenType.Simple:
                    ConvertToSimple(inSpline);
                    break;

                case GenType.Linear:
                    ConvertToLinear(inSpline);
                    break;

                case GenType.CSpline:
                    ConvertToCSpline(inSpline);
                    break;

                case GenType.Cardinal:
                    ConvertToCardinal(inSpline);
                    break;

                default:
                    Debug.LogError("[MultiSpline] Type " + inType.ToString() + " not yet supported in editor.");
                    return;
            }

            inSpline.m_Type = inType;
            inSpline.m_Dirty = true;
        }

        static private void ConvertToSimple(MultiSpline inSpline)
        {
            Vector3 start, end, control;

            switch(inSpline.m_Type)
            {
                case GenType.Uninitialized:
                default:
                    {
                        // initialize with default values
                        inSpline.m_Looped = false;
                        start = Vector3.left;
                        end = Vector3.right;
                        control = Vector3.up;
                        break;
                    }

                case GenType.Simple:
                    {
                        // Already a simple spline
                        return;
                    }

                case GenType.Linear:
                    {
                        // take start and end
                        int endIdx = inSpline.m_Vertices.Length - 1;
                        start = inSpline.m_Vertices[0].Point;
                        end = inSpline.m_Vertices[endIdx].Point;

                        // average the mid vertices to get control point
                        if (endIdx > 1)
                        {
                            Vector3 vSum = Vector3.zero;
                            for (int i = 1; i < endIdx; ++i)
                                vSum += inSpline.m_Vertices[i].Point;
                            vSum.x /= (endIdx - 1);
                            vSum.y /= (endIdx - 1);
                            vSum.z /= (endIdx - 1);
                            control = vSum;
                        }
                        else
                        {
                            control = (start + end) * 0.5f;
                        }

                        break;
                    }

                case GenType.CSpline:
                case GenType.Cardinal:
                    {
                        // take start and end
                        int endIdx = inSpline.m_Vertices.Length - 1;
                        start = inSpline.m_Vertices[0].Point;
                        end = inSpline.m_Vertices[endIdx].Point;

                        // Average the out/in control points
                        control = ((start - inSpline.m_Vertices[0].OutTangent)
                            + (end + inSpline.m_Vertices[endIdx].InTangent)) * 0.5f;

                        break;
                    }
            }

            Array.Resize(ref inSpline.m_Vertices, 2);
            inSpline.m_Vertices[0] = new CSplineVertex(start);
            inSpline.m_Vertices[1] = new CSplineVertex(end);
            inSpline.m_ControlPointA = control;
            inSpline.m_Looped = false;
        }

        static private void ConvertToLinear(MultiSpline inSpline)
        {
            switch(inSpline.m_Type)
            {
                case GenType.Uninitialized:
                default:
                    {
                        // Default values
                        inSpline.m_Looped = false;
                        Array.Resize(ref inSpline.m_Vertices, 2);
                        inSpline.m_Vertices[0] = new CSplineVertex(Vector3.left);
                        inSpline.m_Vertices[1] = new CSplineVertex(Vector3.right);
                        break;
                    }

                case GenType.Simple:
                    {
                        // Swap control and end points for linear order
                        Vector3 control = inSpline.m_ControlPointA;
                        Array.Resize(ref inSpline.m_Vertices, 3);
                        inSpline.m_Vertices[2] = inSpline.m_Vertices[1];
                        inSpline.m_Vertices[1] = new CSplineVertex(control);

                        // Default to not closed
                        inSpline.m_Looped = false;
                        break;
                    }

                case GenType.Linear:
                    {
                        // Already linear, no need to do anything
                        return;
                    }

                case GenType.CSpline:
                case GenType.Cardinal:
                    {
                        // Copy CSpline points over
                        Array.Resize(ref inSpline.m_Vertices, inSpline.m_Vertices.Length);
                        for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
                            inSpline.m_Vertices[i] = inSpline.m_Vertices[i];
                        break;
                    }
            }
        }

        static private void ConvertToCSpline(MultiSpline inSpline)
        {
            //TODO(Autumn): Implement
        }

        static private void ConvertToCardinal(MultiSpline inSpline)
        {
            switch(inSpline.m_Type)
            {
                case GenType.Uninitialized:
                default:
                    {
                        // Default values
                        inSpline.m_CRTension = 0;
                        inSpline.m_Looped = false;

                        Array.Resize(ref inSpline.m_Vertices, 4);
                        inSpline.m_Vertices[0].Point = new Vector3(-1, -1, 0);
                        inSpline.m_Vertices[1].Point = new Vector3(-1, 0, 0);
                        inSpline.m_Vertices[2].Point = new Vector3(1, 0, 0);
                        inSpline.m_Vertices[3].Point = new Vector3(1, -1, 0);
                        break;
                    }

                case GenType.Simple:
                    {
                        inSpline.m_Looped = false;
                        Vector3 start = inSpline.m_Vertices[0].Point;
                        Vector3 end = inSpline.m_Vertices[1].Point;
                        Vector3 control = inSpline.m_ControlPointA;

                        Vector3 startTangent = 0.5f * (start - control);
                        Vector3 endTangent = 0.5f * (end - control);

                        Array.Resize(ref inSpline.m_Vertices, 2);
                        inSpline.m_ControlPointA = start + startTangent;
                        inSpline.m_Vertices[0].Point = start;
                        inSpline.m_Vertices[1].Point = end;
                        inSpline.m_ControlPointB = end + endTangent;
                        break;
                    }

                case GenType.Linear:
                case GenType.CSpline:
                    {
                        // Points are identical, just need to set control points properly
                        inSpline.m_ControlPointA = inSpline.m_Vertices[0].Point;
                        inSpline.m_ControlPointB = inSpline.m_Vertices[inSpline.m_Vertices.Length - 1].Point;
                        break;
                    }

                case GenType.Cardinal:
                    {
                        // Converts directly
                        return;
                    }
            }
        }

        #endregion // Conversion

        #region Utilities

        static private void RecenterVertices(MultiSpline inSpline, Vector3 inZero)
        {
            // Center the vertices
            if (inSpline.m_Vertices != null)
            {
                Vector3 currentCenter = Vector3.zero;
                for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
                    currentCenter += inSpline.m_Vertices[i].Point;
                currentCenter /= inSpline.m_Vertices.Length;

                Vector3 offset = inZero - currentCenter;
                for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
                    inSpline.m_Vertices[i].Point += offset;

                inSpline.m_ControlPointA += offset;
                inSpline.m_ControlPointB += offset;
            }
        }

        static private void InsertVertexAfter(MultiSpline inSpline, int inIndex)
        {
            if (inSpline.m_Vertices == null || inSpline.m_Type == GenType.Uninitialized || inSpline.m_Type == GenType.Simple)
                return;

            int currentIndex = inIndex;
            float lerp = 0.5f;
            bool bAddToEnd = false;

            int segCount = inSpline.m_Vertices.Length;
            if (!inSpline.m_Looped)
                --segCount;

            if (currentIndex == inSpline.m_Vertices.Length - 1)
            {
                bAddToEnd = true;
                if (!inSpline.m_Looped)
                {
                    --currentIndex;
                    lerp = 1.5f;
                }
            }

            float realLerp = (float)(currentIndex + lerp) / segCount;
            Vector3 next = inSpline.GetPoint(realLerp);
            Vector3 dir = inSpline.GetDirection(realLerp);

            CSplineVertex vert;
            vert.Point = next;
            vert.InTangent = vert.OutTangent = inSpline.m_Type == GenType.CSpline ? dir : Vector3.zero;

            if (bAddToEnd)
                UnityEditor.ArrayUtility.Add(ref inSpline.m_Vertices, vert);
            else
                UnityEditor.ArrayUtility.Insert(ref inSpline.m_Vertices, currentIndex + 1, vert);
        }

        static private void InsertVertexBefore(MultiSpline inSpline, int inIndex)
        {
            if (inSpline.m_Vertices == null || inSpline.m_Type == GenType.Uninitialized || inSpline.m_Type == GenType.Simple)
                return;

            int currentIndex = inIndex;
            float lerp = -0.5f;

            int segCount = inSpline.m_Vertices.Length;
            if (!inSpline.m_Looped)
                --segCount;

            if (currentIndex == 0)
            {
                if (!inSpline.m_Looped)
                {
                    ++currentIndex;
                    lerp = -1.5f;
                }
            }

            float realLerp = (float)(currentIndex + lerp) / segCount;
            Vector3 next = inSpline.GetPoint(realLerp);
            Vector3 dir = inSpline.GetDirection(realLerp);

            CSplineVertex vert;
            vert.Point = next;
            vert.InTangent = vert.OutTangent = inSpline.m_Type == GenType.CSpline ? dir : Vector3.zero;

            UnityEditor.ArrayUtility.Insert(ref inSpline.m_Vertices, currentIndex, vert);
            inSpline.m_Dirty = true;
        }

        static private void DeleteVertex(MultiSpline inSpline, int inIndex)
        {
            if (inSpline.m_Vertices == null || inSpline.m_Vertices.Length < 3 || inSpline.m_Type == GenType.Uninitialized || inSpline.m_Type == GenType.Simple)
                return;

            UnityEditor.ArrayUtility.RemoveAt(ref inSpline.m_Vertices, inIndex);
            inSpline.m_Dirty = true;
        }

        #endregion // Utilities
    }
}

#endif // UNITY_EDITOR
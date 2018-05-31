/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
                case GenType.KBSpline:
                    ConvertToCardinalOrKB(inSpline);
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
                        start = inSpline.m_Vertices[0];
                        end = inSpline.m_Vertices[endIdx];

                        // average the mid vertices to get control point
                        if (endIdx > 1)
                        {
                            Vector3 vSum = Vector3.zero;
                            for (int i = 1; i < endIdx; ++i)
                                vSum += inSpline.m_Vertices[i];
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
                    {
                        // take start and end
                        int endIdx = inSpline.m_CSplineVertices.Length - 1;
                        start = inSpline.m_CSplineVertices[0].Point;
                        end = inSpline.m_CSplineVertices[endIdx].Point;

                        // Average the out/in control points
                        control = ((inSpline.m_CSplineVertices[0].Point + inSpline.m_CSplineVertices[0].OutTangent)
                            + (inSpline.m_CSplineVertices[endIdx].Point - inSpline.m_CSplineVertices[endIdx].InTangent)) * 0.5f;

                        break;
                    }

                case GenType.Cardinal:
                case GenType.KBSpline:
                    {
                        int startIdx = 0;
                        int endIdx = inSpline.m_Vertices.Length - 1;

                        if (!inSpline.m_Looped)
                        {
                            ++startIdx;
                            --endIdx;
                        }

                        start = inSpline.m_Vertices[startIdx];
                        end = inSpline.m_Vertices[endIdx];

                        if (endIdx > startIdx + 1)
                        {
                            Vector3 vSum = Vector3.zero;
                            int count = endIdx - startIdx;
                            for (int i = startIdx + 1; i < endIdx; ++i)
                                vSum += inSpline.m_Vertices[i];
                            vSum.x /= count;
                            vSum.y /= count;
                            vSum.z /= count;
                            control = vSum;
                        }
                        else
                        {
                            control = (start + end) * 0.5f;
                        }

                        break;
                    }
            }

            Array.Resize(ref inSpline.m_Vertices, 3);
            inSpline.m_Vertices[0] = start;
            inSpline.m_Vertices[1] = end;
            inSpline.m_Vertices[2] = control;
            inSpline.m_Looped = false;

            inSpline.m_CSplineVertices = null;
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
                        inSpline.m_Vertices[0] = Vector3.left;
                        inSpline.m_Vertices[1] = Vector3.right;
                        break;
                    }

                case GenType.Simple:
                    {
                        // Swap control and end points for linear order
                        Vector3 control = inSpline.m_Vertices[2];
                        inSpline.m_Vertices[2] = inSpline.m_Vertices[1];
                        inSpline.m_Vertices[1] = control;

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
                    {
                        // Copy CSpline points over
                        Array.Resize(ref inSpline.m_Vertices, inSpline.m_CSplineVertices.Length);
                        for (int i = 0; i < inSpline.m_CSplineVertices.Length; ++i)
                            inSpline.m_Vertices[i] = inSpline.m_CSplineVertices[i].Point;
                        break;
                    }

                case GenType.Cardinal:
                case GenType.KBSpline:
                    {
                        if (!inSpline.m_Looped)
                        {
                            // If not closed, we need to eliminate the control points at either end.
                            int newSize = inSpline.m_Vertices.Length - 2;
                            Array.Copy(inSpline.m_Vertices, 1, inSpline.m_Vertices, 0, newSize);
                            Array.Resize(ref inSpline.m_Vertices, newSize);
                        }
                        break;
                    }
            }

            inSpline.m_CSplineVertices = null;
        }

        static private void ConvertToCSpline(MultiSpline inSpline)
        {
            //TODO(Alex): Implement

            inSpline.m_Vertices = null;
        }

        static private void ConvertToCardinalOrKB(MultiSpline inSpline)
        {
            switch(inSpline.m_Type)
            {
                case GenType.Uninitialized:
                default:
                    {
                        // Default values
                        inSpline.m_CRTension = 0;
                        inSpline.m_KBBias = 0;
                        inSpline.m_KBContinuity = 0;
                        inSpline.m_KBTension = 0;
                        inSpline.m_Looped = false;

                        Array.Resize(ref inSpline.m_Vertices, 4);
                        inSpline.m_Vertices[0] = new Vector3(-1, -1, 0);
                        inSpline.m_Vertices[1] = new Vector3(-1, 0, 0);
                        inSpline.m_Vertices[2] = new Vector3(1, 0, 0);
                        inSpline.m_Vertices[3] = new Vector3(1, -1, 0);
                        break;
                    }

                case GenType.Simple:
                    {
                        inSpline.m_Looped = false;
                        Vector3 start = inSpline.m_Vertices[0];
                        Vector3 end = inSpline.m_Vertices[1];
                        Vector3 control = inSpline.m_Vertices[2];

                        Vector3 startTangent = 2 * (start - control);
                        Vector3 endTangent = 2 * (end - control);

                        Array.Resize(ref inSpline.m_Vertices, 4);
                        inSpline.m_Vertices[0] = start + startTangent;
                        inSpline.m_Vertices[1] = start;
                        inSpline.m_Vertices[2] = end;
                        inSpline.m_Vertices[3] = end + endTangent;
                        break;
                    }

                case GenType.Linear:
                    {
                        if (!inSpline.m_Looped)
                        {
                            // If not closed, we need to generate a few new points
                            int newSize = inSpline.m_Vertices.Length + 2;
                            Array.Resize(ref inSpline.m_Vertices, newSize);
                            Array.Copy(inSpline.m_Vertices, 0, inSpline.m_Vertices, 1, newSize - 2);

                            Vector3 firstTangent = inSpline.m_Vertices[1] - inSpline.m_Vertices[2];
                            inSpline.m_Vertices[0] = inSpline.m_Vertices[1] + (firstTangent * 0.5f);

                            Vector3 lastTangent = inSpline.m_Vertices[newSize - 2] - inSpline.m_Vertices[newSize - 3];
                            inSpline.m_Vertices[newSize - 1] = inSpline.m_Vertices[newSize - 2] + (lastTangent * 0.5f);
                        }
                        break;
                    }

                case GenType.CSpline:
                    {
                        int newSize = inSpline.m_CSplineVertices.Length;
                        int startIdx = 0;
                        if (!inSpline.m_Looped)
                        {
                            newSize += 2;
                            startIdx = 1;
                        }
                        Array.Resize(ref inSpline.m_Vertices, newSize);
                        for (int i = 0; i < inSpline.m_CSplineVertices.Length; ++i)
                            inSpline.m_Vertices[startIdx + i] = inSpline.m_CSplineVertices[i].Point;

                        if (!inSpline.m_Looped)
                        {
                            // If not closed, we need to generate control points
                            inSpline.m_Vertices[0] = inSpline.m_CSplineVertices[0].Point - inSpline.m_CSplineVertices[0].OutTangent;

                            int lastCSplineIdx = inSpline.m_CSplineVertices.Length - 1;
                            inSpline.m_Vertices[newSize - 1] = inSpline.m_CSplineVertices[lastCSplineIdx].Point + inSpline.m_CSplineVertices[lastCSplineIdx].InTangent;
                        }
                        break;
                    }

                case GenType.Cardinal:
                case GenType.KBSpline:
                    {
                        // Converts directly
                        return;
                    }
            }

            inSpline.m_CSplineVertices = null;
        }

        #endregion // Conversion

        #region Utilities

        static private void RecenterVertices(MultiSpline inSpline, Vector3 inZero)
        {
            // Center the CSpline vertices
            if (inSpline.m_CSplineVertices != null)
            {
                Vector3 currentCenter = Vector3.zero;
                for (int i = 0; i < inSpline.m_CSplineVertices.Length; ++i)
                    currentCenter += inSpline.m_CSplineVertices[i].Point;
                currentCenter /= inSpline.m_CSplineVertices.Length;

                Vector3 offset = inZero - currentCenter;
                for (int i = 0; i < inSpline.m_CSplineVertices.Length; ++i)
                    inSpline.m_CSplineVertices[i].Point += offset;
            }

            // Center the normal vertices
            if (inSpline.m_Vertices != null)
            {
                Vector3 currentCenter = Vector3.zero;
                for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
                    currentCenter += inSpline.m_Vertices[i];
                currentCenter /= inSpline.m_Vertices.Length;

                Vector3 offset = inZero - currentCenter;
                for (int i = 0; i < inSpline.m_Vertices.Length; ++i)
                    inSpline.m_Vertices[i] += offset;
            }
        }

        #endregion // Utilities
    }
}

#endif // UNITY_EDITOR
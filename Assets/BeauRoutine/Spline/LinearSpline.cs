/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    11 May 2018
 * 
 * File:    LinearSpline.cs
 * Purpose: Polygonal "spline". Linear path between points.
*/

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Polygonal "spline". Contains a sequence of points.
    /// </summary>
    public sealed class LinearSpline : ISpline
    {
        private const int StartingSize = 8;

        private struct VertexData
        {
            public float DirectStart;
            public float DirectLength;
        }

        private float m_Distance;
        private bool m_Looped;
        private bool m_Dirty;

        private int m_VertexCount;

        private Vector3[] m_Vertices;
        private VertexData[] m_VertexData;

        public LinearSpline(int inStartingVertexCount = StartingSize)
        {
            m_Vertices = new Vector3[inStartingVertexCount];
            m_VertexData = new VertexData[inStartingVertexCount];
            m_VertexCount = 0;
        }

        #region Construction

        public void SetLooped(bool inbLooped)
        {
            if (m_Looped != inbLooped)
            {
                m_Looped = inbLooped;
                m_Dirty = true;
            }
        }

        public void SetVertices(params Vector3[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            inVertices.CopyTo(m_Vertices, 0);
            m_Dirty = true;
        }

        public void SetVertices(params Vector2[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inVertices[i];
            m_Dirty = true;
        }

        public void SetVertices(List<Vector3> inVertices)
        {
            SetVertexCount(inVertices.Count);
            inVertices.CopyTo(m_Vertices);
            m_Dirty = true;
        }

        public void SetVertices(List<Vector2> inVertices)
        {
            SetVertexCount(inVertices.Count);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inVertices[i];
            m_Dirty = true;
        }

        private void SetVertexCount(int inVertexCount)
        {
            if (m_VertexCount != inVertexCount)
            {
                m_VertexCount = inVertexCount;
                m_Dirty = true;

                int newCapacity = Mathf.NextPowerOfTwo(inVertexCount);
                if (newCapacity > m_Vertices.Length)
                {
                    Array.Resize(ref m_Vertices, newCapacity);
                    Array.Resize(ref m_VertexData, newCapacity);
                }
            }
        }

        #endregion // Construction

        #region ISpline

        public SplineType GetSplineType() { return SplineType.LinearSpline; }

        public float GetDistance()
        {
            if (m_Dirty)
                Process();
            return m_Distance;
        }

        public float GetDirectDistance()
        {
            if (m_Dirty)
                Process();
            return m_Distance;
        }

        public int GetVertexCount() { return m_VertexCount; }
        public Vector3 GetVertex(int inIndex) { return m_Vertices[inIndex]; }

        public bool IsLooped() { return m_Looped; }

        public float TransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            if (m_Dirty)
                Process();

            if (m_Looped)
                inPercent = (inPercent + 1) % 1;

            if (inPercent == 0 || inPercent == 1)
                return inPercent;

            switch (inLerpMethod)
            {
                case SplineLerp.Vertex:
                default:
                    {
                        return inPercent;
                    }

                case SplineLerp.Direct:
                case SplineLerp.Precise:
                    {
                        int vertCount = m_VertexCount;

                        if (!m_Looped)
                            --vertCount;

                        for (int i = vertCount - 1; i >= 1; --i)
                        {
                            if (m_VertexData[i].DirectStart <= inPercent)
                            {
                                float lerp = (inPercent - m_VertexData[i].DirectStart) / m_VertexData[i].DirectLength;
                                return (i + lerp) / vertCount;
                            }
                        }

                        // If these fail, use the starting node
                        {
                            float lerp = (inPercent) / m_VertexData[0].DirectLength;
                            return lerp / vertCount;
                        }
                    }
            }
        }

        public float InvTransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            if (m_Dirty)
                Process();

            if (m_Looped)
                inPercent = (inPercent + 1) % 1;

            switch(inLerpMethod)
            {
                case SplineLerp.Vertex:
                default:
                    {
                        return inPercent;
                    }

                case SplineLerp.Direct:
                case SplineLerp.Precise:
                    {
                        SplineSegment seg;
                        GetSegment(inPercent, out seg);
                        return m_VertexData[seg.VertexA].DirectStart + m_VertexData[seg.VertexA].DirectLength * seg.Interpolation;
                    }
            }
        }

        public void Process()
        {
            if (!m_Dirty)
                return;

#if DEVELOPMENT
            if (m_VertexCount < 2)
                throw new Exception("Fewer than 2 vertices provided to LinearSpline");
#endif // DEVELOPMENT

            int vertCount = m_VertexCount;
            if (!m_Looped)
            {
                --vertCount;
                m_VertexData[vertCount].DirectStart = 1;
                m_VertexData[vertCount].DirectLength = 0;
            }

            float distance = 0;
            float segmentDist = 0;

            for (int i = 0; i < vertCount; ++i)
            {
                Vector3 myPos = m_Vertices[i];
                Vector3 nextPos = m_Vertices[(i + 1) % m_VertexCount];

                segmentDist = Vector3.Distance(myPos, nextPos);

                m_VertexData[i].DirectStart = distance;
                m_VertexData[i].DirectLength = segmentDist;

                distance += segmentDist;
            }

            float invDistance = 1f / distance;

            for (int i = 0; i < vertCount; ++i)
            {
                m_VertexData[i].DirectStart *= invDistance;
                m_VertexData[i].DirectLength *= invDistance;
            }

            m_Distance = distance;

            m_Dirty = false;
        }

        public Vector3 GetPoint(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            if (m_Dirty)
                Process();

            SplineSegment segment;
            GetSegment(inPercent, out segment);

            return Vector3.LerpUnclamped(m_Vertices[segment.VertexA], m_Vertices[segment.VertexB], inSegmentCurve.Evaluate(segment.Interpolation));
        }

        public Vector3 GetDirection(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            if (m_Dirty)
                Process();

            SplineSegment segment;
            GetSegment(inPercent, out segment);

            Vector3 dir = m_Vertices[segment.VertexB] - m_Vertices[segment.VertexA];
            dir.Normalize();
            return dir;
        }

        public void GetSegment(float inPercent, out SplineSegment outSegment)
        {
            int vertCount = m_VertexCount;
            if (m_Looped)
                inPercent = (inPercent + 1) % 1;
            else
                --vertCount;

            float vertAF = inPercent * vertCount;
            int vertA = (int)vertAF;
            if (!m_Looped)
            {
                if (vertA < 0)
                    vertA = 0;
                else if (vertA >= vertCount)
                    vertA = vertCount - 1;
            }

            outSegment.VertexA = vertA;
            outSegment.VertexB = (vertA + 1) % m_VertexCount;
            outSegment.Interpolation = vertAF - vertA;
        }

        #endregion // ISpline
    }
}
/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 May 2018
 * 
 * File:    CSpline.cs
 * Purpose: Cubic hermite spline.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Cubic Hermite curve. Contains points with tangents.
    /// </summary>
    public sealed class CSpline : ISpline
    {
        private const int STARTING_SIZE = 8;

        private struct VertexData
        {
            public float DirectStart;
            public float DirectLength;

            public float PreciseStart;
            public float PreciseLength;
        }

        private float m_DirectDistance;
        private float m_PreciseDistance;

        private bool m_IsClosed;
        private bool m_Dirty;

        private int m_VertexCount;

        private CSplineVertex[] m_Vertices;
        private VertexData[] m_VertexData;

        public CSpline(int inStartingVertexCount = STARTING_SIZE)
        {
            m_Vertices = new CSplineVertex[inStartingVertexCount];
            m_VertexData = new VertexData[inStartingVertexCount];
            m_VertexCount = 0;
        }

        #region Construction

        public void SetClosed(bool inbClosed)
        {
            if (m_IsClosed != inbClosed)
            {
                m_IsClosed = inbClosed;
                m_Dirty = true;
            }
        }

        public void SetVertices(params CSplineVertex[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            inVertices.CopyTo(m_Vertices, 0);
        }

        public void SetVertices(List<CSplineVertex> inVertices)
        {
            SetVertexCount(inVertices.Count);
            inVertices.CopyTo(m_Vertices);
        }

        private void SetVertexCount(int inVertexCount)
        {
            if (m_VertexCount != inVertexCount)
            {
                m_VertexCount = inVertexCount;
                m_Dirty = true;

                int newCapacity = Mathf.ClosestPowerOfTwo(inVertexCount);
                if (newCapacity > m_Vertices.Length)
                {
                    Array.Resize(ref m_Vertices, newCapacity);
                    Array.Resize(ref m_VertexData, newCapacity);
                }
            }
        }

        #endregion // Construction

        #region ISpline

        public SplineType GetSplineType() { return SplineType.CSpline; }

        public float GetDistance() { return m_PreciseDistance; }
        public float GetDirectDistance() { return m_DirectDistance; }

        public int GetVertexCount() { return m_VertexCount; }
        public Vector3 GetVertex(int inIndex) { return m_Vertices[inIndex].Point; }

        public bool IsClosed() { return m_IsClosed; }

        public float CorrectPercent(float inPercent, SplineLerp inLerpMethod)
        {
            switch (inLerpMethod)
            {
                case SplineLerp.Vertex:
                default:
                    {
                        return inPercent;
                    }

                case SplineLerp.Direct:
                    {
                        int vertCount = m_VertexCount;

                        if (m_IsClosed)
                            inPercent = (inPercent + 1) % 1;
                        else
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

                case SplineLerp.Precise:
                    {
                        int vertCount = m_VertexCount;

                        if (m_IsClosed)
                            inPercent = (inPercent + 1) % 1;
                        else
                            --vertCount;

                        for (int i = vertCount - 1; i >= 1; --i)
                        {
                            if (m_VertexData[i].PreciseStart <= inPercent)
                            {
                                float lerp = (inPercent - m_VertexData[i].PreciseStart) / m_VertexData[i].PreciseLength;
                                return (i + lerp) / vertCount;
                            }
                        }

                        // If these fail, use the starting node
                        {
                            float lerp = (inPercent) / m_VertexData[0].PreciseLength;
                            return lerp / vertCount;
                        }
                    }
            }
        }

        public void Process()
        {
            if (!m_Dirty)
                return;

#if DEVELOPMENT
            if (m_VertexCount < 2)
                throw new Exception("Fewer than 2 vertices provided to CSpline");
#endif // DEVELOPMENT

            int vertCount = m_VertexCount;
            if (!m_IsClosed)
            {
                --vertCount;
                m_VertexData[vertCount].DirectStart = 1;
                m_VertexData[vertCount].DirectLength = 0;
            }

            float directDistance = 0;
            float directSegmentDist = 0;

            float preciseDistance = 0;
            float preciseSegmentDist = 0;

            for (int i = 0; i < vertCount; ++i)
            {
                Vector3 myPos = m_Vertices[i].Point;
                Vector3 nextPos = m_Vertices[(i + 1) % m_VertexCount].Point;

                directSegmentDist = Vector3.Distance(myPos, nextPos);

                m_VertexData[i].DirectStart = directDistance;
                m_VertexData[i].DirectLength = directSegmentDist;

                directSegmentDist *= directSegmentDist / preciseSegmentDist;
                directDistance += directSegmentDist;

                preciseDistance += preciseSegmentDist;
            }

            float invDirectDistance = 1f / directDistance;
            float invPreciseDistance = 1f / preciseDistance;

            for (int i = 0; i < vertCount; ++i)
            {
                m_VertexData[i].DirectStart *= invDirectDistance;
                m_VertexData[i].DirectLength *= invDirectDistance;

                m_VertexData[i].PreciseStart *= invPreciseDistance;
                m_VertexData[i].PreciseLength *= invPreciseDistance;
            }

            m_DirectDistance = directDistance;
            m_PreciseDistance = preciseDistance;

            m_Dirty = false;
        }

        public Vector3 Lerp(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            if (m_Dirty)
                Process();

            int vertCount = m_VertexCount;
            if (m_IsClosed)
                inPercent = (inPercent + 1) % 1;
            else
                --vertCount;

            float vertAF = inPercent * vertCount;
            int vertA = (int)vertAF;
            if (!m_IsClosed)
            {
                if (vertA < 0)
                    vertA = 0;
                else if (vertA >= vertCount)
                    vertA = vertCount - 1;
            }

            float lerp = vertAF - vertA;
            int vertB = (vertA + 1) % m_VertexCount;

            return SplineMath.CubicHermite(m_Vertices[vertA].OutTangent, m_Vertices[vertA].Point, m_Vertices[vertB].Point, m_Vertices[vertB].InTangent, inSegmentCurve.Evaluate(lerp));
        }

        #endregion // ISpline
    }

    [Serializable]
    public sealed class SerializedCSpline
    {
        public CSplineVertex[] Vertices;
        public bool IsClosed;

        public CSpline Generate()
        {
            CSpline spline = null;
            Generate(ref spline);
            return spline;
        }

        public void Generate(ref CSpline ioSpline)
        {
            if (ioSpline == null)
                ioSpline = new CSpline(Vertices.Length);
            ioSpline.SetClosed(IsClosed);
            ioSpline.SetVertices(Vertices);
        }
    }
}
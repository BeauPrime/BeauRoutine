/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    11 May 2018
 * 
 * File:    VertexSpline.cs
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
    public sealed class VertexSpline : ISpline
    {
        private const int STARTING_SIZE = 8;

        private struct VertexData
        {
            public float DirectStart;
            public float DirectLength;
        }

        private float m_Distance;
        private bool m_IsClosed;
        private bool m_Dirty;

        private int m_VertexCount;

        private Vector3[] m_Vertices;
        private VertexData[] m_VertexData;

        public VertexSpline(int inStartingVertexCount = STARTING_SIZE)
        {
            m_Vertices = new Vector3[inStartingVertexCount];
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

        public void SetVertices(params Vector3[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            inVertices.CopyTo(m_Vertices, 0);
        }

        public void SetVertices(params Vector2[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inVertices[i];
        }

        public void SetVertices(List<Vector3> inVertices)
        {
            SetVertexCount(inVertices.Count);
            inVertices.CopyTo(m_Vertices);
        }

        public void SetVertices(List<Vector2> inVertices)
        {
            SetVertexCount(inVertices.Count);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inVertices[i];
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

        public SplineType GetSplineType() { return SplineType.VertexSpline; }

        public float GetDistance() { return m_Distance; }
        public float GetDirectDistance() { return m_Distance; }

        public int GetVertexCount() { return m_VertexCount; }
        public Vector3 GetVertex(int inIndex) { return m_Vertices[inIndex]; }

        public bool IsClosed() { return m_IsClosed; }

        public float CorrectPercent(float inPercent, SplineLerpSpace inLerpMethod)
        {
            switch (inLerpMethod)
            {
                case SplineLerpSpace.Vertex:
                default:
                    {
                        return inPercent;
                    }

                case SplineLerpSpace.Direct:
                case SplineLerpSpace.Precise:
                    {
                        if (m_IsClosed)
                            inPercent = (inPercent + 1) % 1;

                        for (int i = 1; i < m_VertexCount; ++i)
                        {
                            if (m_VertexData[i].DirectStart <= inPercent)
                            {
                                float lerp = (inPercent - m_VertexData[i].DirectStart) / m_VertexData[i].DirectLength;
                                return (i + lerp) / m_VertexCount;
                            }
                        }

                        // If these fail, use the starting node
                        {
                            float lerp = (inPercent) / m_VertexData[0].DirectLength;
                            return lerp / m_VertexCount;
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
                throw new Exception("Fewer than 2 vertices provided to VertexSpline");
#endif // DEVELOPMENT

            int vertCount = m_VertexCount;
            if (!m_IsClosed)
            {
                --vertCount;
                m_VertexData[vertCount].DirectStart = 1;
                m_VertexData[vertCount].DirectLength = 0;
            }

            float distance = 0;

            for (int i = 0; i < vertCount; ++i)
            {
                Vector3 myPos = m_Vertices[i];
                Vector3 nextPos = m_Vertices[(i + 1) % m_VertexCount];
            }

            m_Dirty = false;
        }

        public Vector3 Lerp(float inPercent)
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

            return Vector3.LerpUnclamped(m_Vertices[vertA], m_Vertices[vertB], lerp);
        }

        #endregion // ISpline
    }

    [Serializable]
    public sealed class SerializedVertexSpline
    {
        public Vector3[] Positions;
        public bool IsClosed;

        public void Generate(ref VertexSpline ioSpline)
        {
            if (ioSpline == null)
                ioSpline = new VertexSpline(Positions.Length);
            ioSpline.SetClosed(IsClosed);
            ioSpline.SetVertices(Positions);
        }
    }
}
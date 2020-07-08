/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    11 May 2018
 * 
 * File:    LinearSpline.cs
 * Purpose: Polygonal spline. Linear path between points.
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
    /// Polygonal spline. Contains a sequence of points.
    /// </summary>
    public sealed class LinearSpline : ISpline
    {
        private const int StartingSize = 8;

        private struct VertexData
        {
            public object UserData;

            public float Marker;
            public float Length;
        }

        private Action<ISpline> m_OnUpdated;

        private float m_Distance;
        private bool m_Looped;
        private bool m_Dirty;

        private int m_VertexCount;
        private int m_SegmentCount;

        private Vector3[] m_Vertices;
        private VertexData[] m_VertexData;

        public LinearSpline(int inStartingVertexCount = StartingSize)
        {
            m_Vertices = new Vector3[inStartingVertexCount];
            m_VertexData = new VertexData[inStartingVertexCount];
            m_VertexCount = 0;
        }

        #region Construction

        /// <summary>
        /// Sets if the spline is looped.
        /// </summary>
        public void SetLooped(bool inbLooped)
        {
            if (m_Looped != inbLooped)
            {
                m_Looped = inbLooped;
                m_SegmentCount = inbLooped ? m_VertexCount : m_VertexCount - 1;
                m_Dirty = true;
            }
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// </summary>
        public void SetVertices(CSplineVertex[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inVertices[i].Point;
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// </summary>
        public void SetVertices(List<CSplineVertex> inVertices)
        {
            SetVertexCount(inVertices.Count);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inVertices[i].Point;
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// </summary>
        public void SetVertices(Vector3[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            inVertices.CopyTo(m_Vertices, 0);
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// </summary>
        public void SetVertices(List<Vector3> inVertices)
        {
            SetVertexCount(inVertices.Count);
            inVertices.CopyTo(m_Vertices);
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// </summary>
        public void SetVertices(Vector2[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inVertices[i];
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// </summary>
        public void SetVertices(List<Vector2> inVertices)
        {
            SetVertexCount(inVertices.Count);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inVertices[i];
            m_Dirty = true;
        }

        /// <summary>
        /// Copies positions from the given Transforms into the spline.
        /// </summary>
        public void SetVertices(Transform[] inTransforms, Space inSpace = Space.World)
        {
            SetVertexCount(inTransforms.Length);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inTransforms[i].GetPosition(Axis.XYZ, inSpace);
            m_Dirty = true;
        }

        /// <summary>
        /// Copies positions from the given Transforms into the spline.
        /// </summary>
        public void SetVertices(List<Transform> inTransforms, Space inSpace = Space.World)
        {
            SetVertexCount(inTransforms.Count);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i] = inTransforms[i].GetPosition(Axis.XYZ, inSpace);
            m_Dirty = true;
        }

        // Sets vertex count and ensures storage space for the vertex data.
        private void SetVertexCount(int inVertexCount)
        {
            if (m_VertexCount != inVertexCount)
            {
                m_VertexCount = inVertexCount;
                m_SegmentCount = m_Looped ? inVertexCount : inVertexCount - 1;
                m_Dirty = true;

                int newCapacity = Mathf.NextPowerOfTwo(inVertexCount);
                if (newCapacity > m_Vertices.Length)
                {
                    Array.Resize(ref m_Vertices, newCapacity);
                    Array.Resize(ref m_VertexData, newCapacity);
                }

                // Clear all user data
                for (int i = inVertexCount; i < newCapacity; ++i)
                    m_VertexData[i].UserData = null;
            }
        }

        #endregion // Construction

        #region ISpline

        #region Basic Info

        public SplineType GetSplineType() { return SplineType.LinearSpline; }

        public bool IsLooped() { return m_Looped; }

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

        #endregion // Basic Info

        #region Vertex Access

        public int GetVertexCount()
        {
            return m_VertexCount;
        }

        public Vector3 GetVertex(int inIndex)
        {
#if DEVELOPMENT
            if (inIndex < 0 || inIndex >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            return m_Vertices[inIndex];
        }

        public void SetVertex(int inIndex, Vector3 inVertex)
        {
#if DEVELOPMENT
            if (inIndex < 0 || inIndex >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            m_Vertices[inIndex] = inVertex;
            m_Dirty = true;
        }

        public object GetVertexUserData(int inIndex)
        {
#if DEVELOPMENT
            if (inIndex < 0 || inIndex >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            return m_VertexData[inIndex].UserData;
        }

        public void SetVertexUserData(int inIndex, object inUserData)
        {
#if DEVELOPMENT
            if (inIndex < 0 || inIndex >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            m_VertexData[inIndex].UserData = inUserData;
        }

        public int GetControlCount()
        {
            return 0;
        }

        public Vector3 GetControlPoint(int inIndex)
        {
            throw new InvalidOperationException("LinearSplines do not have control points.");
        }
        
        public void SetControlPoint(int inIndex, Vector3 inVertex)
        {
            throw new InvalidOperationException("LinearSplines do not have control points.");
        }

        #endregion // Vertex Access

        #region Percent Info

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
                        int segCount = m_SegmentCount;

                        for (int i = segCount - 1; i > 0; --i)
                        {
                            if (m_VertexData[i].Marker <= inPercent)
                            {
                                float lerp = (inPercent - m_VertexData[i].Marker) / m_VertexData[i].Length;
                                return (i + lerp) / segCount;
                            }
                        }

                        // If these fail, use the starting node
                        {
                            float lerp = (inPercent) / m_VertexData[0].Length;
                            return lerp / segCount;
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
                        return m_VertexData[seg.VertexA].Marker + m_VertexData[seg.VertexA].Length * seg.Interpolation;
                    }
            }
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
            int segCount = m_SegmentCount;
            if (m_Looped)
                inPercent = (inPercent + 1) % 1;

            float vertAF = inPercent * segCount;
            int vertA = (int)vertAF;
            if (!m_Looped)
            {
                if (vertA < 0)
                    vertA = 0;
                else if (vertA >= segCount)
                    vertA = segCount - 1;
            }

            outSegment.VertexA = vertA;
            outSegment.VertexB = (vertA + 1) % m_VertexCount;
            outSegment.Interpolation = vertAF - vertA;
        }

        #endregion // Percent Info

        #region Operations

        public bool Process()
        {
            if (!m_Dirty)
                return false;

#if DEVELOPMENT
            if (m_VertexCount < 2)
                throw new Exception("Fewer than 2 vertices provided to LinearSpline");
#endif // DEVELOPMENT

            int segCount = m_SegmentCount;
            if (!m_Looped)
            {
                m_VertexData[segCount].Marker = 1;
                m_VertexData[segCount].Length = 0;
            }

            float distance = 0;
            float segmentDist = 0;

            for (int i = 0; i < segCount; ++i)
            {
                Vector3 myPos = m_Vertices[i];
                Vector3 nextPos = m_Vertices[(i + 1) % m_VertexCount];

                segmentDist = Vector3.Distance(myPos, nextPos);

                m_VertexData[i].Marker = distance;
                m_VertexData[i].Length = segmentDist;

                distance += segmentDist;
            }

            float invDistance = 1f / distance;

            for (int i = 0; i < segCount; ++i)
            {
                m_VertexData[i].Marker *= invDistance;
                m_VertexData[i].Length *= invDistance;
            }

            m_Distance = distance;

            m_Dirty = false;
            if (m_OnUpdated != null)
                m_OnUpdated(this);
            return true;
        }

        public Action<ISpline> OnUpdated
        {
            get { return m_OnUpdated; }
            set { m_OnUpdated = value; }
        }

        #endregion // Operations

        #endregion // ISpline
    }
}
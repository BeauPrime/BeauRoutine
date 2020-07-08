/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 May 2018
 * 
 * File:    CSpline.cs
 * Purpose: Cubic hermite spline.
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
    /// Cubic Hermite curve. Contains points with tangents.
    /// </summary>
    public sealed class CSpline : ISpline
    {
        private const int StartingSize = 8;

        private struct VertexData
        {
            public object UserData;

            public float Marker;
            public float Length;
        }

        private struct PreciseSegment
        {
            public float Marker;
            public float Length;
        }

        private Action<ISpline> m_OnUpdated;

        private float m_DirectDistance;
        private float m_PreciseDistance;

        private bool m_Looped;
        private bool m_Dirty;
        private int m_SubdivisionsPerSegment;

        private int m_VertexCount;
        private int m_SegmentCount;
        private int m_PreciseSegmentCount;

        private SplineType m_SplineType = SplineType.CSpline;

        private CSplineVertex[] m_Vertices;
        private VertexData[] m_VertexData;
        private PreciseSegment[] m_PreciseSegmentData;

        private Vector3 m_ControlPointA;
        private Vector3 m_ControlPointB;
        private float m_CardinalTension;

        public CSpline(int inStartingVertexCount = StartingSize)
        {
            m_Vertices = new CSplineVertex[inStartingVertexCount];
            m_VertexData = new VertexData[inStartingVertexCount];
            m_VertexCount = 0;

            m_SubdivisionsPerSegment = SplineMath.DistancePrecision;
        }

        #region Construction

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
            inVertices.CopyTo(m_Vertices, 0);
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// </summary>
        public void SetVertices(List<CSplineVertex> inVertices)
        {
            SetVertexCount(inVertices.Count);
            inVertices.CopyTo(m_Vertices);
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// Will not affect existing tangents.
        /// </summary>
        public void SetVertices(Vector3[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i].Point = inVertices[i];
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// Will not affect existing tangents.
        /// </summary>
        public void SetVertices(List<Vector3> inVertices)
        {
            SetVertexCount(inVertices.Count);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i].Point = inVertices[i];
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// Will not affect existing tangents.
        /// </summary>
        public void SetVertices(Vector2[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i].Point = inVertices[i];
            m_Dirty = true;
        }

        /// <summary>
        /// Copies the given vertices into the spline.
        /// Will not affect existing tangents.
        /// </summary>
        public void SetVertices(List<Vector2> inVertices)
        {
            SetVertexCount(inVertices.Count);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i].Point = inVertices[i];
            m_Dirty = true;
        }

        /// <summary>
        /// Copies positions from the given Transforms into the spline.
        /// Will not affect existing tangents.
        /// </summary>
        public void SetVertices(Transform[] inTransforms, Space inSpace = Space.World)
        {
            SetVertexCount(inTransforms.Length);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i].Point = inTransforms[i].GetPosition(Axis.XYZ, inSpace);
            m_Dirty = true;
        }

        /// <summary>
        /// Copies positions from the given Transforms into the spline.
        /// Will not affect existing tangents.
        /// </summary>
        public void SetVertices(List<Transform> inTransforms, Space inSpace = Space.World)
        {
            SetVertexCount(inTransforms.Count);
            for (int i = 0; i < m_VertexCount; ++i)
                m_Vertices[i].Point = inTransforms[i].GetPosition(Axis.XYZ, inSpace);
            m_Dirty = true;
        }

        /// <summary>
        /// Sets this spline as a CSpline.
        /// Tangents must be manually provided.
        /// </summary>
        public void SetAsCSpline()
        {
            if (m_SplineType != SplineType.CSpline)
            {
                m_SplineType = SplineType.CSpline;
                m_Dirty = true;
            }
        }

        /// <summary>
        /// Sets this spline as a Catmull-Rom spline.
        /// Tangents will be automatically generated.
        /// </summary>
        public void SetAsCatmullRom()
        {
            SetAsCardinal(0);
        }

        /// <summary>
        /// Sets this spline as a Cardinal spline.
        /// Tangents will be automatically generated.
        /// </summary>
        public void SetAsCardinal(float inTension)
        {
            if (m_SplineType != SplineType.Cardinal || m_CardinalTension != inTension)
            {
                m_SplineType = SplineType.Cardinal;
                m_CardinalTension = inTension;
                m_Dirty = true;
            }
        }

        /// <summary>
        /// Sets the control points for a non-looped Catmull-Rom/Cardinal spline.
        /// </summary>
        public void SetControlPoints(Vector3 inControlStart, Vector3 inControlEnd)
        {
#if DEVELOPMENT
            if (m_SplineType != SplineType.Cardinal)
                throw new NotSupportedException("Control points not supported for CSplines");
            if (m_Looped)
                throw new NotSupportedException("Looped cardinal splines do not have control points");
#endif // DEVELOPMENT
            m_ControlPointA = inControlStart;
            m_ControlPointB = inControlEnd;
            m_Dirty = true;
        }

        /// <summary>
        /// Sets the control points for a non-looped Catmull-Rom/Cardinal spline
        /// as offsets from the existing start and end points.
        /// </summary>
        public void SetControlPointsByOffset(Vector3 inControlStartOffset, Vector3 inControlEndOffset)
        {
#if DEVELOPMENT
            if (m_SplineType != SplineType.Cardinal)
                throw new NotSupportedException("Control points not supported for CSplines");
            if (m_Looped)
                throw new NotSupportedException("Looped cardinal splines do not have control points");
            if (m_VertexCount < 2)
                throw new IndexOutOfRangeException("Not enough vertices to set control points by offset");
#endif // DEVELOPMENT
            m_ControlPointA = inControlStartOffset + m_Vertices[0].Point;
            m_ControlPointB = inControlEndOffset + m_Vertices[m_VertexCount - 1].Point;
            m_Dirty = true;
        }

        /// <summary>
        /// Resets the control points for a non-looped Catmull-Rom/Cardinal spline
        /// to the existing start and end points.
        /// </summary>
        public void ResetControlPoints()
        {
#if DEVELOPMENT
            if (m_SplineType != SplineType.Cardinal)
                throw new NotSupportedException("Control points not supported for CSplines");
            if (m_Looped)
                throw new NotSupportedException("Looped cardinal splines do not have control points");
            if (m_VertexCount < 2)
                throw new IndexOutOfRangeException("Not enough vertices to set control points by offset");
#endif // DEVELOPMENT
            m_ControlPointA = m_Vertices[0].Point;
            m_ControlPointB = m_Vertices[m_VertexCount - 1].Point;
            m_Dirty = true;
        }

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
            }
        }

        #endregion // Construction

        #region ISpline

        #region Basic Info

        public SplineType GetSplineType() { return m_SplineType; }

        public bool IsLooped() { return m_Looped; }

        public float GetDistance()
        {
            if (m_Dirty)
                Process();
            return m_PreciseDistance;
        }

        public float GetDirectDistance()
        {
            if (m_Dirty)
                Process();
            return m_DirectDistance;
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

            return m_Vertices[inIndex].Point;
        }

        public void SetVertex(int inIndex, Vector3 inVertex)
        {
#if DEVELOPMENT
            if (inIndex < 0 || inIndex >= m_VertexCount)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            m_Vertices[inIndex].Point = inVertex;
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
            switch (m_SplineType)
            {
                case SplineType.CSpline:
                default:
                    return 0;

                case SplineType.Cardinal:
                    return m_Looped ? 0 : 2;
            }
        }

        public Vector3 GetControlPoint(int inIndex)
        {
            switch (m_SplineType)
            {
                case SplineType.CSpline:
                default:
                    throw new InvalidOperationException("CSpline does not have any control points");

                case SplineType.Cardinal:
                    if (m_Looped)
                        throw new NotSupportedException("Looped Cardinal spline with does not have any control points");
#if DEVELOPMENT
                    if (inIndex < 0 || inIndex > 1)
                        throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT
                    return inIndex == 0 ? m_ControlPointA : m_ControlPointB;
            }
        }

        public void SetControlPoint(int inIndex, Vector3 inVertex)
        {
            switch (m_SplineType)
            {
                case SplineType.CSpline:
                default:
                    throw new InvalidOperationException("CSpline does not have any control points");

                case SplineType.Cardinal:
                    if (m_Looped)
                        throw new NotSupportedException("Looped Cardinal spline with does not have any control points");
#if DEVELOPMENT
                    if (inIndex < 0 || inIndex > 1)
                        throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT
                    if (inIndex == 0)
                        m_ControlPointA = inVertex;
                    else
                        m_ControlPointB = inVertex;

                    m_Dirty = true;

                    break;
            }
        }

        #endregion // Vertex Info

        #region Evaluation

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
                    {
                        int segCount = m_SegmentCount;

                        for (int i = segCount - 1; i >= 1; --i)
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

                case SplineLerp.Precise:
                    {
                        int segCount = m_PreciseSegmentCount;
                        
                        for (int i = segCount - 1; i > 0; --i)
                        {
                            if (m_PreciseSegmentData[i].Marker <= inPercent)
                            {
                                float lerp = (inPercent - m_PreciseSegmentData[i].Marker) / m_PreciseSegmentData[i].Length;
                                return (i + lerp) / segCount;
                            }
                        }

                        // If this fails, use the starting node
                        {
                            float lerp = inPercent / m_PreciseSegmentData[0].Length;
                            return (lerp) / segCount;
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
                    {
                        SplineSegment seg;
                        GetSegment(inPercent, out seg);
                        return m_VertexData[seg.VertexA].Marker + m_VertexData[seg.VertexA].Length * seg.Interpolation;
                    }

                case SplineLerp.Precise:
                    {
                        int segCount = m_PreciseSegmentCount;
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

                        float interpolation = vertAF - vertA;
                        return m_PreciseSegmentData[vertA].Marker + interpolation * m_PreciseSegmentData[vertA].Length;
                    }
            }
        }

        public Vector3 GetPoint(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            if (m_Dirty)
                Process();

            SplineSegment segment;
            GetSegment(inPercent, out segment);

            return SplineMath.Hermite(m_Vertices[segment.VertexA].OutTangent, m_Vertices[segment.VertexA].Point,
                m_Vertices[segment.VertexB].Point, m_Vertices[segment.VertexB].InTangent,
                inSegmentCurve.Evaluate(segment.Interpolation)
                );
        }

        public Vector3 GetDirection(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            if (m_Dirty)
                Process();

            SplineSegment segment;
            GetSegment(inPercent, out segment);

            float p1 = inSegmentCurve.Evaluate(segment.Interpolation);
            float p2 = p1 + SplineMath.LookAhead;

            Vector3 v1 = SplineMath.Hermite(m_Vertices[segment.VertexA].OutTangent, m_Vertices[segment.VertexA].Point,
                m_Vertices[segment.VertexB].Point, m_Vertices[segment.VertexB].InTangent,
                p1);
            Vector3 v2 = SplineMath.Hermite(m_Vertices[segment.VertexA].OutTangent, m_Vertices[segment.VertexA].Point,
                m_Vertices[segment.VertexB].Point, m_Vertices[segment.VertexB].InTangent,
                p2);

            Vector3 dir = new Vector3(
                v2.x - v1.x,
                v2.y - v1.y,
                v2.z - v1.z
            );
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

        #endregion // Evaluation

        #region Operations

        public bool Process()
        {
            if (!m_Dirty)
                return false;

#if DEVELOPMENT
            if (m_VertexCount < 2)
                throw new Exception("Fewer than 2 vertices provided to CSpline");
#endif // DEVELOPMENT

            if (m_SplineType == SplineType.Cardinal)
                GenerateCardinalTangents();
            CalculateLengths();

            m_Dirty = false;
            if (m_OnUpdated != null)
                m_OnUpdated(this);
            return true;
        }

        private void GenerateCardinalTangents()
        {
#if DEVELOPMENT
            if (m_VertexCount < 2)
                throw new Exception("Fewer than 2 vertices available for Cardinal generation");
#endif // DEVELOPMENT

            float tangentMultiplier = (1 - m_CardinalTension) * 0.5f;
            Vector3 next, prev, tangent;

            if (m_Looped)
            {
                for (int i = 0; i < m_VertexCount; ++i)
                {
                    prev = m_Vertices[(i + m_VertexCount - 1) % m_VertexCount].Point;
                    next = m_Vertices[(i + 1) % m_VertexCount].Point;

                    tangent = (next - prev) * tangentMultiplier;
                    m_Vertices[i].InTangent = m_Vertices[i].OutTangent = tangent;
                }
            }
            else
            {
                // Index 0
                {
                    prev = m_ControlPointA;
                    next = m_Vertices[1].Point;

                    tangent = (next - prev) * tangentMultiplier;
                    m_Vertices[0].InTangent = m_Vertices[0].OutTangent = tangent;
                }

                for (int i = 1; i < m_VertexCount - 1; ++i)
                {
                    prev = m_Vertices[i - 1].Point;
                    next = m_Vertices[(i + 1) % m_VertexCount].Point;

                    tangent = (next - prev) * tangentMultiplier;
                    m_Vertices[i].InTangent = m_Vertices[i].OutTangent = tangent;
                }

                // Last index
                {
                    prev = m_Vertices[m_VertexCount - 2].Point;
                    next = m_ControlPointB;

                    tangent = (next - prev) * tangentMultiplier;
                    m_Vertices[m_VertexCount - 1].InTangent = m_Vertices[m_VertexCount - 1].OutTangent = tangent;
                }
            }
        }

        private void CalculateLengths()
        {
            int segCount = m_SegmentCount;
            if (!m_Looped)
            {
                m_VertexData[segCount].Marker = 1;
                m_VertexData[segCount].Length = 0;
            }

            if (m_SubdivisionsPerSegment < 1)
                m_SubdivisionsPerSegment = 1;

            m_PreciseSegmentCount = segCount * m_SubdivisionsPerSegment;
            Array.Resize(ref m_PreciseSegmentData, Mathf.NextPowerOfTwo(m_PreciseSegmentCount));

            float directDistance = 0;
            float preciseDistance = 0;

            for (int i = 0; i < segCount; ++i)
            {
                int nextVertIndex = (i + 1) % m_VertexCount;
                Vector3 myPos = m_Vertices[i].Point;
                Vector3 nextPos = m_Vertices[nextVertIndex].Point;

                float directSegmentDist = Vector3.Distance(myPos, nextPos);

                float preciseSegmentDist = 0;
                Vector3 prevPrecisePoint = myPos;
                Vector3 nextPrecisePoint;
                for (int j = 0; j < m_SubdivisionsPerSegment; ++j)
                {
                    nextPrecisePoint = SplineMath.Hermite(m_Vertices[i].OutTangent, myPos, nextPos, m_Vertices[nextVertIndex].InTangent, (float)(j + 1) / m_SubdivisionsPerSegment);
                    preciseSegmentDist = Vector3.Distance(prevPrecisePoint, nextPrecisePoint);

                    int idx = i * m_SubdivisionsPerSegment + j;
                    m_PreciseSegmentData[idx].Marker = preciseDistance;
                    m_PreciseSegmentData[idx].Length = preciseSegmentDist;

                    preciseDistance += preciseSegmentDist;
                    prevPrecisePoint = nextPrecisePoint;
                }

                m_VertexData[i].Marker = directDistance;
                m_VertexData[i].Length = directSegmentDist;

                directDistance += directSegmentDist;
            }

            float invDirectDistance = 1f / directDistance;
            float invPreciseDistance = 1f / preciseDistance;

            for (int i = 0; i < segCount; ++i)
            {
                m_VertexData[i].Marker *= invDirectDistance;
                m_VertexData[i].Length *= invDirectDistance;
            }

            for (int i = 0; i < m_PreciseSegmentCount; ++i)
            {
                m_PreciseSegmentData[i].Marker *= invPreciseDistance;
                m_PreciseSegmentData[i].Length *= invPreciseDistance;
            }

            m_DirectDistance = directDistance;
            m_PreciseDistance = preciseDistance;
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
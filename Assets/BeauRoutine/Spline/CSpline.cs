/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
            public float DirectStart;
            public float DirectLength;

            public float PreciseStart;
            public float PreciseLength;
        }

        private float m_DirectDistance;
        private float m_PreciseDistance;

        private bool m_Looped;
        private bool m_Dirty;

        private int m_VertexCount;

        private CSplineVertex[] m_Vertices;
        private VertexData[] m_VertexData;

        public CSpline(int inStartingVertexCount = StartingSize)
        {
            m_Vertices = new CSplineVertex[inStartingVertexCount];
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

        public void SetVertices(params CSplineVertex[] inVertices)
        {
            SetVertexCount(inVertices.Length);
            inVertices.CopyTo(m_Vertices, 0);
            m_Dirty = true;
        }

        public void SetVertices(List<CSplineVertex> inVertices)
        {
            SetVertexCount(inVertices.Count);
            inVertices.CopyTo(m_Vertices);
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

        public void SetCatmullRom(params Vector3[] inPoints)
        {
            SetCardinal(0, inPoints);
        }

        public void SetCardinal(float inTension, params Vector3[] inPoints)
        {
            int idxOffset = 0;
            int totalPoints = inPoints.Length;
            int vertCount = totalPoints;
            if (!m_Looped)
            {
                idxOffset = 1;
                vertCount -= 2;
            }

#if DEVELOPMENT
            if (vertCount < 2)
                throw new Exception("Fewer than 2 vertices available for Cardinal generation");
#endif // DEVELOPMENT

            SetVertexCount(vertCount);

            float tensionModifier = 1 - inTension;

            for (int i = 0; i < vertCount; ++i)
            {
                int idx = (i + idxOffset) % totalPoints;
                int prevIdx = (idx + totalPoints - 1) % totalPoints;
                int nextIdx = (idx + 1) % totalPoints;

                m_Vertices[i].Point = inPoints[idx];
                Vector3 tangent = (inPoints[nextIdx] - inPoints[prevIdx]) * tensionModifier * 0.5f;
                m_Vertices[i].InTangent = m_Vertices[i].OutTangent = tangent;
            }

            m_Dirty = true;
        }

        public void SetCatmullRom(params Vector2[] inPoints)
        {
            SetCardinal(0, inPoints);
        }

        public void SetCardinal(float inTension, params Vector2[] inPoints)
        {
            int idxOffset = 0;
            int totalPoints = inPoints.Length;
            int vertCount = totalPoints;
            if (!m_Looped)
            {
                idxOffset = 1;
                vertCount -= 2;
            }

#if DEVELOPMENT
            if (vertCount < 2)
                throw new Exception("Fewer than 2 vertices available for Cardinal generation");
#endif // DEVELOPMENT

            SetVertexCount(vertCount);

            float tensionModifier = 1 - inTension;

            for (int i = 0; i < vertCount; ++i)
            {
                int idx = (i + idxOffset) % totalPoints;
                int prevIdx = (idx + totalPoints - 1) % totalPoints;
                int nextIdx = (idx + 1) % totalPoints;

                m_Vertices[i].Point = inPoints[idx];
                Vector2 tangent = (inPoints[nextIdx] - inPoints[prevIdx]) * tensionModifier * 0.5f;
                m_Vertices[i].InTangent = m_Vertices[i].OutTangent = tangent;
            }

            m_Dirty = true;
        }

        public void SetKBSpline(float inTension, float inBias, float inContinuity, params Vector3[] inPoints)
        {
            int idxOffset = 0;
            int totalPoints = inPoints.Length;
            int vertCount = totalPoints;
            if (!m_Looped)
            {
                idxOffset = 1;
                vertCount -= 2;
            }

#if DEVELOPMENT
            if (vertCount < 2)
                throw new Exception("Fewer than 2 vertices available for KB-Spline generation");
#endif // DEVELOPMENT

            SetVertexCount(vertCount);

            float a1 = (1 - inTension) * (1 + inBias) * (1 + inContinuity) * 0.5f;
            float a2 = (1 - inTension) * (1 - inBias) * (1 - inContinuity) * 0.5f;
            float b1 = (1 - inTension) * (1 + inBias) * (1 - inContinuity) * 0.5f;
            float b2 = (1 - inTension) * (1 - inBias) * (1 + inContinuity) * 0.5f;

            for (int i = 0; i < vertCount; ++i)
            {
                int idx = (i + idxOffset) % totalPoints;
                int prevIdx = (idx + totalPoints - 1) % totalPoints;
                int nextIdx = (idx + 1) % totalPoints;
                int nextIdx2 = (idx + 2) % totalPoints;

                int i2 = (i + 1) % vertCount;

                m_Vertices[i].Point = inPoints[idx];

                Vector3 firstTangent = inPoints[idx] - inPoints[prevIdx];
                Vector3 secondTangent = inPoints[nextIdx] - inPoints[idx];
                Vector3 thirdTangent = inPoints[nextIdx2] - inPoints[nextIdx];

                m_Vertices[i].OutTangent = a1 * firstTangent + a2 * secondTangent;
                m_Vertices[i2].InTangent = b1 * secondTangent + b2 * thirdTangent;
            }

            m_Dirty = true;
        }

        public void SetKBSpline(float inTension, float inBias, float inContinuity, params Vector2[] inPoints)
        {
            int idxOffset = 0;
            int totalPoints = inPoints.Length;
            int vertCount = totalPoints;
            if (!m_Looped)
            {
                idxOffset = 1;
                vertCount -= 2;
            }

#if DEVELOPMENT
            if (vertCount < 2)
                throw new Exception("Fewer than 2 vertices available for KB-Spline generation");
#endif // DEVELOPMENT

            SetVertexCount(vertCount);

            float a1 = (1 - inTension) * (1 + inBias) * (1 + inContinuity) * 0.5f;
            float a2 = (1 - inTension) * (1 - inBias) * (1 - inContinuity) * 0.5f;
            float b1 = (1 - inTension) * (1 + inBias) * (1 - inContinuity) * 0.5f;
            float b2 = (1 - inTension) * (1 - inBias) * (1 + inContinuity) * 0.5f;

            for (int i = 0; i < vertCount; ++i)
            {
                int idx = (i + idxOffset) % totalPoints;
                int prevIdx = (idx + totalPoints - 1) % totalPoints;
                int nextIdx = (idx + 1) % totalPoints;
                int nextIdx2 = (idx + 2) % totalPoints;

                int i2 = (i + 1) % vertCount;

                m_Vertices[i].Point = inPoints[idx];

                Vector2 firstTangent = inPoints[idx] - inPoints[prevIdx];
                Vector2 secondTangent = inPoints[nextIdx] - inPoints[idx];
                Vector2 thirdTangent = inPoints[nextIdx2] - inPoints[nextIdx];

                m_Vertices[i].OutTangent = a1 * firstTangent + a2 * secondTangent;
                m_Vertices[i2].InTangent = b1 * secondTangent + b2 * thirdTangent;
            }

            m_Dirty = true;
        }

        #endregion // Construction

        #region ISpline

        public SplineType GetSplineType() { return SplineType.CSpline; }

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

        public int GetVertexCount() { return m_VertexCount; }
        public Vector3 GetVertex(int inIndex) { return m_Vertices[inIndex].Point; }

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

                case SplineLerp.Precise:
                    {
                        int vertCount = m_VertexCount;

                        if (!m_Looped)
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
                        return m_VertexData[seg.VertexA].DirectStart + m_VertexData[seg.VertexA].DirectLength * seg.Interpolation;
                    }

                case SplineLerp.Precise:
                    {
                        SplineSegment seg;
                        GetSegment(inPercent, out seg);
                        return m_VertexData[seg.VertexA].PreciseStart + m_VertexData[seg.VertexA].PreciseLength * seg.Interpolation;
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
            if (!m_Looped)
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
                int nextVertIndex = (i + 1) % m_VertexCount;
                Vector3 myPos = m_Vertices[i].Point;
                Vector3 nextPos = m_Vertices[nextVertIndex].Point;

                directSegmentDist = Vector3.Distance(myPos, nextPos);

                if (SplineMath.DistancePrecision <= 0)
                {
                    preciseSegmentDist = directSegmentDist;
                }
                else
                {
                    preciseSegmentDist = 0;
                    Vector3 prevPrecisePoint = myPos;
                    Vector3 nextPrecisePoint;
                    for (int j = 1; j < SplineMath.DistancePrecision; ++j)
                    {
                        nextPrecisePoint = SplineMath.Hermite(m_Vertices[i].OutTangent, myPos, nextPos, m_Vertices[nextVertIndex].InTangent, (float)j / SplineMath.DistancePrecision);
                        preciseSegmentDist += Vector3.Distance(prevPrecisePoint, nextPrecisePoint);
                        prevPrecisePoint = nextPrecisePoint;
                    }
                    preciseSegmentDist += Vector3.Distance(prevPrecisePoint, nextPos);
                }

                m_VertexData[i].DirectStart = directDistance;
                m_VertexData[i].DirectLength = directSegmentDist;

                m_VertexData[i].PreciseStart = preciseDistance;
                m_VertexData[i].PreciseLength = preciseSegmentDist;

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
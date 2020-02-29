/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    8 May 2018
 * 
 * File:    SimpleSpline.cs
 * Purpose: Quadratic bezier curve.
*/

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Quadratic Bezier curve. Contains start, end, and a single control point.
    /// </summary>
    [Serializable]
    public class SimpleSpline : ISpline
    {
        private struct PreciseSegment
        {
            public float Marker;
            public float Length;
        }

        #region Inspector

        [SerializeField]
        private Vector3 m_Start = default(Vector3);

        [SerializeField]
        private Vector3 m_End = default(Vector3);

        [SerializeField]
        private Vector3 m_Control = default(Vector3);

        #endregion // Inspector

        private Action<ISpline> m_OnUpdated;

        private object m_StartUserData;
        private object m_EndUserData;

        private float m_Distance;
        private float m_PreciseDistance;

        private int m_PreciseSegmentCount;

        private PreciseSegment[] m_PreciseSegmentData;

        private bool m_Dirty;

        public SimpleSpline(Vector3 inStart, Vector3 inEnd, Vector3 inControl)
        {
            m_Start = inStart;
            m_End = inEnd;
            m_Control = inControl;

            m_Distance = 0;
            m_PreciseDistance = 0;
            m_Dirty = true;

            m_StartUserData = m_EndUserData = null;
            m_PreciseSegmentCount = SplineMath.DistancePrecision;
        }

        public SimpleSpline(Vector2 inStart, Vector2 inEnd, Vector2 inControl)
        {
            m_Start = inStart;
            m_End = inEnd;
            m_Control = inControl;

            m_Distance = 0;
            m_PreciseDistance = 0;
            m_Dirty = true;

            m_StartUserData = m_EndUserData = null;
            m_PreciseSegmentCount = SplineMath.DistancePrecision;
        }

        #region Construction

        /// <summary>
        /// Starting point of the spline.
        /// </summary>
        public Vector3 Start
        {
            get { return m_Start; }
            set
            {
                if (m_Start != value)
                {
                    m_Start = value;
                    m_Dirty = true;
                }
            }
        }

        /// <summary>
        /// Ending point of the spline.
        /// </summary>
        public Vector3 End
        {
            get { return m_End; }
            set
            {
                if (m_End != value)
                {
                    m_End = value;
                    m_Dirty = true;
                }
            }
        }

        /// <summary>
        /// Spline control point.
        /// </summary>
        public Vector3 Control
        {
            get { return m_Control; }
            set
            {
                if (m_Control != value)
                {
                    m_Control = value;
                    m_Dirty = true;
                }
            }
        }

        #endregion // Construction

        #region ISpline

        #region Basic Info

        public SplineType GetSplineType()
        {
            return SplineType.SimpleSpline;
        }

        public bool IsLooped()
        {
            return false;
        }

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
            return m_Distance;
        }

        #endregion // Basic Info

        #region Vertex Info

        public int GetVertexCount()
        {
            return 2;
        }

        public Vector3 GetVertex(int inIndex)
        {
#if DEVELOPMENT
            if (inIndex < 0 || inIndex > 1)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            return (inIndex == 0 ? m_Start : m_End);
        }

        public void SetVertex(int inIndex, Vector3 inVertex)
        {
#if DEVELOPMENT
            if (inIndex < 0 || inIndex > 1)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            if (inIndex == 0)
                m_Start = inVertex;
            else
                m_End = inVertex;

            m_Dirty = true;
        }

        public object GetVertexUserData(int inIndex)
        {
#if DEVELOPMENT
            if (inIndex < 0 || inIndex > 1)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            return (inIndex == 0 ? m_StartUserData : m_EndUserData);
        }

        public void SetVertexUserData(int inIndex, object inUserData)
        {
#if DEVELOPMENT
            if (inIndex < 0 || inIndex > 1)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            if (inIndex == 0)
                m_StartUserData = inUserData;
            else
                m_EndUserData = inUserData;
        }

        public int GetControlCount() { return 1; }

        public Vector3 GetControlPoint(int inIndex)
        {
#if DEVELOPMENT
            if (inIndex != 0)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            return m_Control;
        }

        public void SetControlPoint(int inIndex, Vector3 inVertex)
        {
#if DEVELOPMENT
            if (inIndex != 0)
                throw new ArgumentOutOfRangeException("inIndex");
#endif // DEVELOPMENT

            m_Control = inVertex;
            m_Dirty = true;
        }

        #endregion // Vertex Info

        #region Evaluation

        public float TransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            if (m_Dirty)
                Process();

            if (inPercent == 0 || inPercent == 1)
                return inPercent;

            switch(inLerpMethod)
            {
                case SplineLerp.Vertex:
                case SplineLerp.Direct:
                default:
                    {
                        return inPercent;
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

            if (inPercent == 0 || inPercent == 1)
                return inPercent;

            switch(inLerpMethod)
            {
                case SplineLerp.Vertex:
                case SplineLerp.Direct:
                default:
                    {
                        return inPercent;
                    }

                case SplineLerp.Precise:
                    {
                        float vertAF = inPercent * m_PreciseSegmentCount;
                        int vertA = (int)vertAF;
                        if (vertA < 0)
                            vertA = 0;
                        else if (vertA >= m_PreciseSegmentData.Length)
                            vertA = m_PreciseSegmentData.Length - 1;

                        float interpolation = vertAF - vertA;
                        return m_PreciseSegmentData[vertA].Marker + interpolation * m_PreciseSegmentData[vertA].Length;
                    }
            }
        }

        public Vector3 GetPoint(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            if (m_Dirty)
                Process();

            return SplineMath.Quadratic(m_Start, m_Control, m_End, inSegmentCurve.Evaluate(inPercent));
        }

        public Vector3 GetDirection(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            if (m_Dirty)
                Process();

            float p = inSegmentCurve.Evaluate(inPercent);

            Vector3 dir = SplineMath.QuadraticDerivative(m_Start, m_Control, m_End, p);
            dir.Normalize();

            return dir;
        }

        public void GetSegment(float inPercent, out SplineSegment outSegment)
        {
            outSegment.Interpolation = inPercent;
            outSegment.VertexA = 0;
            outSegment.VertexB = 1;
        }

        #endregion // Evaluation

        #region Operations

        public bool Process()
        {
            if (!m_Dirty)
                return false;

            m_Distance = Vector3.Distance(m_Start, m_End);

            float preciseDist = 0;
            float segmentDist = 0;

            Vector3 prev = m_Start;
            Vector3 next;

            if (m_PreciseSegmentCount < 1)
                m_PreciseSegmentCount = 1;

            Array.Resize(ref m_PreciseSegmentData, Mathf.NextPowerOfTwo(m_PreciseSegmentCount));

            for (int i = 0; i < m_PreciseSegmentCount; ++i)
            {
                next = SplineMath.Quadratic(m_Start, m_Control, m_End, (float)(i + 1) / m_PreciseSegmentCount);
                segmentDist = Vector3.Distance(prev, next);

                m_PreciseSegmentData[i].Marker = preciseDist;
                m_PreciseSegmentData[i].Length = segmentDist;

                preciseDist += segmentDist;
                prev = next;
            }

            m_PreciseDistance = preciseDist;

            float invPreciseDist = 1f / preciseDist;
            for (int i = 0; i < m_PreciseSegmentCount; ++i)
            {
                m_PreciseSegmentData[i].Marker *= invPreciseDist;
                m_PreciseSegmentData[i].Length *= invPreciseDist;
            }

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
/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 May 2018
 * 
 * File:    SimpleSpline.cs
 * Purpose: Quadratic bezier curve.
*/

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Quadratic Bezier curve. Contains start, end, and a single control point.
    /// </summary>
    [Serializable]
    public struct SimpleSpline : ISpline
    {
        #region Inspector

        [SerializeField]
        private Vector3 m_Start;

        [SerializeField]
        private Vector3 m_End;

        [SerializeField]
        private Vector3 m_Control;

        #endregion // Inspector

        private float m_Distance;
        private float m_PreciseDistance;

        private bool m_Dirty;

        public SimpleSpline(Vector3 inStart, Vector3 inEnd, Vector3 inControl)
        {
            m_Start = inStart;
            m_End = inEnd;
            m_Control = inControl;

            m_Distance = 0;
            m_PreciseDistance = 0;
            m_Dirty = true;
        }

        public SimpleSpline(Vector2 inStart, Vector2 inEnd, Vector2 inControl)
        {
            m_Start = inStart;
            m_End = inEnd;
            m_Control = inControl;

            m_Distance = 0;
            m_PreciseDistance = 0;
            m_Dirty = true;
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

        public SplineType GetSplineType() { return SplineType.SimpleSpline; }

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

        public int GetVertexCount() { return 2; }
        public Vector3 GetVertex(int inIndex)
        {
            return ((inIndex % 2) == 0 ? m_Start : m_End);
        }

        public bool IsLooped() { return false; }

        public float TransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            if (m_Dirty)
                Process();

            return inPercent;
        }

        public float InvTransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            if (m_Dirty)
                Process();

            return inPercent;
        }

        public void Process()
        {
            if (!m_Dirty)
                return;

            m_Distance = Vector3.Distance(m_Start, m_End);

            float preciseDist = 0;
            Vector3 prev = m_Start;
            Vector3 next;
            for (int i = 1; i < SplineMath.DistancePrecision; ++i)
            {
                next = SplineMath.Quadratic(m_Start, m_Control, m_End, (float)i / SplineMath.DistancePrecision);
                preciseDist += Vector3.Distance(prev, next);
                prev = next;
            }
            next = m_End;
            preciseDist += Vector3.Distance(prev, next);

            m_PreciseDistance = preciseDist;

            m_Dirty = false;
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

        #endregion // ISpline
    }
}
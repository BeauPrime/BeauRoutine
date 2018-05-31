/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    24 May 2018
 * 
 * File:    MultiSpline.cs
 * Purpose: Generic serialized spline.
*/

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using UnityEngine;

namespace BeauRoutine.Splines
{
    /// <summary>
    /// Serialized spline.
    /// </summary>
    [Serializable]
    public sealed partial class MultiSpline : MonoBehaviour, ISpline
    {
        private enum GenType { Uninitialized, Simple, Linear, CSpline, Cardinal, KBSpline }

        #region Inspector

        [SerializeField]
        private GenType m_Type = default(GenType);

        [SerializeField]
        private Vector3[] m_Vertices = null;

        [SerializeField]
        private CSplineVertex[] m_CSplineVertices = null;

        [SerializeField, Range(0, 1)]
        private float m_CRTension = 0;

        [SerializeField, Range(-1, 1)]
        private float m_KBTension = 0;

        [SerializeField, Range(-1, 1)]
        private float m_KBBias = 0;

        [SerializeField, Range(-1, 1)]
        private float m_KBContinuity = 0;

        [SerializeField]
        private bool m_Looped = false;

        #endregion // Inspector

        [NonSerialized]
        private ISpline m_GeneratedSpline;

        [NonSerialized]
        private bool m_Dirty = true;

        private void UpdateSpline()
        {
            if (m_Dirty || m_GeneratedSpline == null)
            {
                Generate(ref m_GeneratedSpline);
                m_Dirty = false;
            }
        }

        private void Generate(ref ISpline ioSpline)
        {
            switch(m_Type)
            {
                case GenType.Simple:
                    {
                        SimpleSpline s;
                        if (ioSpline == null || ioSpline.GetSplineType() != SplineType.SimpleSpline)
                        {
                            ioSpline = s = new SimpleSpline(m_Vertices[0], m_Vertices[1], m_Vertices[2]);
                        }
                        else
                        {
                            s = (SimpleSpline)ioSpline;

                            s.Start = m_Vertices[0];
                            s.End = m_Vertices[1];
                            s.Control = m_Vertices[2];

                            ioSpline = s;
                        }
                        break;
                    }

                case GenType.Linear:
                    {
                        LinearSpline s;
                        if (ioSpline == null || ioSpline.GetSplineType() != SplineType.LinearSpline)
                            ioSpline = s = new LinearSpline(m_Vertices.Length);
                        else
                            s = (LinearSpline)ioSpline;

                        s.SetLooped(m_Looped);
                        s.SetVertices(m_Vertices);
                        break;
                    }

                case GenType.CSpline:
                    {
                        CSpline s;
                        if (ioSpline == null || ioSpline.GetSplineType() != SplineType.CSpline)
                            ioSpline = s = new CSpline(m_CSplineVertices.Length);
                        else
                            s = (CSpline)ioSpline;

                        s.SetLooped(m_Looped);
                        s.SetVertices(m_CSplineVertices);
                        break;
                    }

                case GenType.Cardinal:
                    {
                        CSpline s;
                        if (ioSpline == null || ioSpline.GetSplineType() != SplineType.CSpline)
                            ioSpline = s = new CSpline(m_Vertices.Length);
                        else
                            s = (CSpline)ioSpline;

                        s.SetLooped(m_Looped);
                        s.SetCardinal(m_CRTension, m_Vertices);
                        break;
                    }

                case GenType.KBSpline:
                    {
                        CSpline s;
                        if (ioSpline == null || ioSpline.GetSplineType() != SplineType.CSpline)
                            ioSpline = s = new CSpline(m_Vertices.Length);
                        else
                            s = (CSpline)ioSpline;

                        s.SetLooped(m_Looped);
                        if (m_KBTension == 0 && m_KBBias == 0 && m_KBContinuity == 0)
                        {
                            // Tension, Bias, and Continuity at 0 result in Catmull-Rom
                            s.SetCatmullRom(m_Vertices);
                        }
                        else
                        {
                            s.SetKBSpline(m_KBTension, m_KBBias, m_KBContinuity, m_Vertices);
                        }
                        break;
                    }

                default:
                    throw new Exception("MultiSpline has not been properly set up in the inspector!");
            }
        }

        #region ISpline

        public SplineType GetSplineType()
        {
            UpdateSpline();
            return m_GeneratedSpline.GetSplineType();
        }

        public float GetDistance()
        {
            UpdateSpline();
            return m_GeneratedSpline.GetDistance();
        }

        public float GetDirectDistance()
        {
            UpdateSpline();
            return m_GeneratedSpline.GetDirectDistance();
        }

        public int GetVertexCount()
        {
            UpdateSpline();
            return m_GeneratedSpline.GetVertexCount();
        }

        public Vector3 GetVertex(int inIndex)
        {
            UpdateSpline();
            return m_GeneratedSpline.GetVertex(inIndex);
        }

        public bool IsLooped()
        {
            UpdateSpline();
            return m_GeneratedSpline.IsLooped();
        }

        public float TransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            UpdateSpline();
            return m_GeneratedSpline.TransformPercent(inPercent, inLerpMethod);
        }

        public float InvTransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            UpdateSpline();
            return m_GeneratedSpline.InvTransformPercent(inPercent, inLerpMethod);
        }

        public void Process()
        {
            UpdateSpline();
            m_GeneratedSpline.Process();
        }

        public Vector3 GetPoint(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            UpdateSpline();
            return m_GeneratedSpline.GetPoint(inPercent, inSegmentCurve);
        }

        public Vector3 GetDirection(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            UpdateSpline();
            return m_GeneratedSpline.GetDirection(inPercent, inSegmentCurve);
        }

        public void GetSegment(float inPercent, out SplineSegment outSegment)
        {
            UpdateSpline();
            m_GeneratedSpline.GetSegment(inPercent, out outSegment);
        }

        #endregion // ISpline
    }
}
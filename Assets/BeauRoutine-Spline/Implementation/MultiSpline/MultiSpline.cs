/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
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
    [ExecuteInEditMode]
    public sealed partial class MultiSpline : MonoBehaviour, ISpline
    {
        private enum GenType { Uninitialized, Simple, Linear, CSpline, Cardinal }

        #region Inspector

        [SerializeField]
        private GenType m_Type = default(GenType);

        [SerializeField]
        private CSplineVertex[] m_Vertices = null;

        [SerializeField]
        private Vector3 m_ControlPointA = default(Vector3);

        [SerializeField]
        private Vector3 m_ControlPointB = default(Vector3);

        [SerializeField, Range(-2, 2)]
        private float m_CRTension = 0;

        [SerializeField]
        private bool m_Looped = false;

        #endregion // Inspector

        [NonSerialized]
        private ISpline m_GeneratedSpline;

        [NonSerialized]
        private ISpline m_TransformWrapper;

        [NonSerialized]
        private bool m_Dirty = true;

        [NonSerialized]
        private Action<ISpline> m_OnUpdated;

        private ISpline RefreshSpline()
        {
            if (m_Dirty || m_GeneratedSpline == null)
            {
                if (Generate(ref m_GeneratedSpline))
                    m_TransformWrapper = new TransformedSpline<ISpline>(transform, m_GeneratedSpline);
                m_Dirty = false;
            }
            return m_TransformWrapper;
        }

        private bool Generate(ref ISpline ioSpline)
        {
            bool bCreatedNew = false;

            switch(m_Type)
            {
                case GenType.Simple:
                    {
                        SimpleSpline s;
                        if (ioSpline == null || ioSpline.GetSplineType() != SplineType.SimpleSpline)
                        {
                            ioSpline = s = new SimpleSpline(m_Vertices[0].Point, m_Vertices[1].Point, m_ControlPointA);
                            bCreatedNew = true;
                        }
                        else
                        {
                            s = (SimpleSpline)ioSpline;

                            s.Start = m_Vertices[0].Point;
                            s.End = m_Vertices[1].Point;
                            s.Control = m_ControlPointA;

                            ioSpline = s;
                        }
                        break;
                    }

                case GenType.Linear:
                    {
                        LinearSpline s;
                        if (ioSpline == null || ioSpline.GetSplineType() != SplineType.LinearSpline)
                        {
                            ioSpline = s = new LinearSpline(m_Vertices.Length);
                            bCreatedNew = true;
                        }
                        else
                        {
                            s = (LinearSpline)ioSpline;
                        }

                        s.SetLooped(m_Looped);
                        s.SetVertices(m_Vertices);
                        break;
                    }

                case GenType.CSpline:
                    {
                        CSpline s;
                        if (ioSpline == null || (ioSpline.GetSplineType() != SplineType.Cardinal && ioSpline.GetSplineType() != SplineType.CSpline))
                        {
                            ioSpline = s = new CSpline(m_Vertices.Length);
                            bCreatedNew = true;
                        }
                        else
                        {
                            s = (CSpline)ioSpline;
                        }

                        s.SetAsCSpline();

                        s.SetLooped(m_Looped);
                        s.SetVertices(m_Vertices);
                        break;
                    }

                case GenType.Cardinal:
                    {
                        CSpline s;
                        if (ioSpline == null || (ioSpline.GetSplineType() != SplineType.Cardinal && ioSpline.GetSplineType() != SplineType.CSpline))
                        {
                            ioSpline = s = new CSpline(m_Vertices.Length);
                            bCreatedNew = true;
                        }
                        else
                        {
                            s = (CSpline)ioSpline;
                        }

                        s.SetAsCardinal(m_CRTension);

                        s.SetLooped(m_Looped);
                        s.SetVertices(m_Vertices);
                        if (!m_Looped)
                        {
                            s.SetControlPoints(m_ControlPointA, m_ControlPointB);
                        }
                        break;
                    }

                default:
                    throw new Exception("MultiSpline has not been properly set up in the inspector!");
            }

            return bCreatedNew;
        }

        #region ISpline

        #region Basic Info

        public SplineType GetSplineType()
        {
            return RefreshSpline().GetSplineType();
        }

        public bool IsLooped()
        {
            return RefreshSpline().IsLooped();
        }

        public float GetDistance()
        {
            return RefreshSpline().GetDistance();
        }

        public float GetDirectDistance()
        {
            return RefreshSpline().GetDirectDistance();
        }

        #endregion // Basic Info

        #region Vertex Info

        public int GetVertexCount()
        {
            return RefreshSpline().GetVertexCount();
        }

        public Vector3 GetVertex(int inIndex)
        {
            return RefreshSpline().GetVertex(inIndex);
        }

        public void SetVertex(int inIndex, Vector3 inVertex)
        {
            RefreshSpline().SetVertex(inIndex, inVertex);
        }

        public object GetVertexUserData(int inIndex)
        {
            return RefreshSpline().GetVertexUserData(inIndex);
        }

        public void SetVertexUserData(int inIndex, object inUserData)
        {
            RefreshSpline().SetVertexUserData(inIndex, inUserData);
        }

        public int GetControlCount()
        {
            RefreshSpline();
            return RefreshSpline().GetControlCount();
        }

        public Vector3 GetControlPoint(int inIndex)
        {
            return RefreshSpline().GetControlPoint(inIndex);
        }

        public void SetControlPoint(int inIndex, Vector3 inVertex)
        {
            RefreshSpline().SetControlPoint(inIndex, inVertex);
        }

        #endregion // Vertex Info

        #region Evaluation

        public float TransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            return RefreshSpline().TransformPercent(inPercent, inLerpMethod);
        }

        public float InvTransformPercent(float inPercent, SplineLerp inLerpMethod)
        {
            return RefreshSpline().InvTransformPercent(inPercent, inLerpMethod);
        }

        public Vector3 GetPoint(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            return RefreshSpline().GetPoint(inPercent, inSegmentCurve);
        }

        public Vector3 GetDirection(float inPercent, Curve inSegmentCurve = Curve.Linear)
        {
            return RefreshSpline().GetDirection(inPercent, inSegmentCurve);
        }

        public void GetSegment(float inPercent, out SplineSegment outSegment)
        {
            RefreshSpline().GetSegment(inPercent, out outSegment);
        }

        #endregion // Evaluation

        #region Operations

        public bool Process()
        {
            if (RefreshSpline().Process())
            {
                if (m_OnUpdated != null)
                    m_OnUpdated(this);
                return true;
            }

            return false;
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
/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    4 June 2018
 * 
 * File:    SplineRenderer.cs
 * Purpose: Spline line renderer.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeauRoutine.Splines
{
    [ExecuteInEditMode]
    public class SplinePositioner : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private Transform[] m_Objects = null;

        [SerializeField]
        private Space m_PositionSpace = Space.World;

        [SerializeField]
        private Axis m_PositionAxis = Axis.XYZ;

        [SerializeField]
        private SplineLerp m_Precision = SplineLerp.Vertex;

        [SerializeField]
        private SplineOrientationSettings m_Settings = default(SplineOrientationSettings);

        [SerializeField]
        private MultiSpline m_EditorSpline = null;

        #endregion // Inspector

        private void Update()
        {
            if (m_Objects != null && m_Objects.Length > 0 && m_EditorSpline)
                Refresh();
        }

        private void Refresh()
        {
            for (int i = 0; i < m_Objects.Length; ++i)
            {
                float amt = (float) (i + 0.5) / (m_Objects.Length);
                Spline.Align(m_EditorSpline, m_Objects[i], amt, m_PositionAxis, m_PositionSpace, m_Precision, Curve.Linear, m_Settings);
            }
        }
    }
}
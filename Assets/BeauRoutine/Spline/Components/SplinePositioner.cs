/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
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
        private SplineLerp m_LerpSpace = SplineLerp.Precise;

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
            Spline.Sample(m_EditorSpline, s_CachedPoints, 0, 1, 0, m_Objects.Length, m_LerpSpace);
            for (int i = 0; i < m_Objects.Length; ++i)
            {
                m_Objects[i].SetPosition(s_CachedPoints[i], m_PositionAxis, m_PositionSpace);
            }
        }

        static private Vector3[] s_CachedPoints = new Vector3[512];
    }
}
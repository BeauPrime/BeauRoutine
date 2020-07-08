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
using UnityEngine;

namespace BeauRoutine.Splines
{
    [ExecuteInEditMode]
    public class SplineRenderer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        private LineRenderer m_Renderer = null;

        [SerializeField]
        private MultiSpline m_EditorSpline = null;

        [SerializeField, Range(2, 512)]
        private int m_VertexCount = 64;

        #endregion // Inspector

        private void Update()
        {
            if (m_Renderer && m_EditorSpline)
                Refresh();
        }

        private void Refresh()
        {
            Spline.Sample(m_EditorSpline, s_CachedPoints, 0, 1, 0, m_VertexCount, SplineLerp.Precise);
            m_Renderer.positionCount = m_VertexCount;
            m_Renderer.SetPositions(s_CachedPoints);
        }

        static private Vector3[] s_CachedPoints = new Vector3[512];
    }
}
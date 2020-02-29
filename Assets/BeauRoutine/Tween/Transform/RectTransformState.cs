/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    RectTransformState.cs
 * Purpose: Maintains a record of a RectTransform's properties
            for restoration or interpolation.
*/

using BeauRoutine.Internal;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Holds a record of the state of a RectTransform's properties.
    /// </summary>
    public struct RectTransformState
    {
        private Vector3 m_AnchoredPos;
        private Vector4 m_Anchors;
        private Vector2 m_SizeDelta;
        private Vector2 m_Pivot;

        private Vector3 m_Scale;
        private Vector3 m_Rotation;

        /// <summary>
        /// Recorded position.
        /// </summary>
        public Vector2 AnchoredPos
        {
            get { return (Vector2)m_AnchoredPos; }
        }

        /// <summary>
        /// Recorded position.
        /// </summary>
        public Vector3 AnchoredPos3D
        {
            get { return m_AnchoredPos; }
        }

        /// <summary>
        /// Recorded anchors.
        /// </summary>
        public Vector4 Anchors
        {
            get { return m_Anchors; }
        }

        /// <summary>
        /// Recorded size delta.
        /// </summary>
        public Vector2 SizeDelta
        {
            get { return m_SizeDelta; }
        }

        /// <summary>
        /// Recorded pivot.
        /// </summary>
        public Vector2 Pivot
        {
            get { return m_Pivot; }
        }

        /// <summary>
        /// Recorded scale.
        /// </summary>
        public Vector3 Scale
        {
            get { return m_Scale; }
        }

        /// <summary>
        /// Last known rotation.
        /// </summary>
        public Quaternion Rotation
        {
            get { return Quaternion.Euler(m_Rotation); }
        }

        /// <summary>
        /// Last known rotation, in euler angles.
        /// </summary>
        public Vector3 EulerAngles
        {
            get { return m_Rotation; }
        }

        /// <summary>
        /// Returns an empty state.
        /// </summary>
        static public RectTransformState Create()
        {
            return new RectTransformState();
        }

        /// <summary>
        /// Returns a new world-space state for the given transform.
        /// </summary>
        static public RectTransformState Create(RectTransform inTransform)
        {
            return new RectTransformState(inTransform);
        }

        /// <summary>
        /// Creates a new RectTransformState for the given transform.
        /// </summary>
        public RectTransformState(RectTransform inTransform)
        {
            m_Scale = inTransform.localScale;
            m_Rotation = Vector3.zero;

            m_AnchoredPos = inTransform.anchoredPosition3D;

            Vector2 minAnchors = inTransform.anchorMin;
            Vector2 maxAnchors = inTransform.anchorMax;

            m_Anchors = new Vector4(minAnchors.x, minAnchors.y, maxAnchors.x, maxAnchors.y);
            m_SizeDelta = inTransform.sizeDelta;
            m_Pivot = inTransform.pivot;

            m_Rotation = GetRotation(inTransform);
        }

        /// <summary>
        /// Refreshes the values from the given RectTransform.
        /// </summary>
        public void Refresh(RectTransform inTransform, RectTransformProperties inProperties = RectTransformProperties.All)
        {
            if ((inProperties & RectTransformProperties.Scale) != 0)
                m_Scale = inTransform.localScale;

            if ((inProperties & RectTransformProperties.AnchoredPosition) != 0)
                m_AnchoredPos = inTransform.anchoredPosition3D;

            if ((inProperties & RectTransformProperties.Rotation) != 0)
                m_Rotation = GetRotation(inTransform);
        }

        /// <summary>
        /// Applies a RectTransformState to the given RectTransform.
        /// </summary>
        public void Apply(RectTransform inTransform, RectTransformProperties inProperties = RectTransformProperties.All)
        {
            if ((inProperties & RectTransformProperties.AnchoredPosition) != 0)
            {
                inTransform.SetAnchorPos(m_AnchoredPos, inProperties.ToAnchoredPositionAxis());
            }

            if ((inProperties & RectTransformProperties.SizeDelta) != 0)
            {
                inTransform.SetSizeDelta(m_SizeDelta, inProperties.ToSizeDeltaAxis());
            }

            if ((inProperties & RectTransformProperties.Scale) != 0)
            {
                inTransform.SetScale(m_Scale, inProperties.ToScaleAxis());
            }

            if ((inProperties & RectTransformProperties.Rotation) != 0)
            {
                inTransform.SetRotation(m_Rotation, inProperties.ToRotationAxis(), Space.Self);
            }

            if ((inProperties & RectTransformProperties.Pivot) != 0)
            {
                Vector2 pivot = inTransform.pivot;
                if ((inProperties & RectTransformProperties.PivotX) != 0)
                    pivot.x = m_Pivot.x;
                if ((inProperties & RectTransformProperties.PivotY) != 0)
                    pivot.y = m_Pivot.y;
                inTransform.pivot = pivot;
            }

            if ((inProperties & RectTransformProperties.Anchors) != 0)
            {
                Vector2 minAnchors = inTransform.anchorMin;
                Vector2 maxAnchors = inTransform.anchorMax;

                if ((inProperties & RectTransformProperties.AnchorMinX) != 0)
                    minAnchors.x = m_Anchors.x;
                if ((inProperties & RectTransformProperties.AnchorMinY) != 0)
                    minAnchors.y = m_Anchors.y;

                if ((inProperties & RectTransformProperties.AnchorMaxX) != 0)
                    maxAnchors.x = m_Anchors.z;
                if ((inProperties & RectTransformProperties.AnchorMaxY) != 0)
                    maxAnchors.y = m_Anchors.w;

                inTransform.anchorMin = minAnchors;
                inTransform.anchorMax = maxAnchors;
            }
        }

        private Vector3 GetRotation(RectTransform inTransform)
        {
            EulerStorage record = EulerStorage.GetRecord(inTransform);
            if (record != null)
                return record.Get(Space.Self);
            return inTransform.localEulerAngles;
        }

        /// <summary>
        /// Interpolates betwen two RectTransformStates.
        /// </summary>
        static public RectTransformState Lerp(RectTransformState inStateA, RectTransformState inStateB, float inPercent)
        {
            RectTransformState newState = new RectTransformState();
            newState.m_AnchoredPos = Vector3.LerpUnclamped(inStateA.m_AnchoredPos, inStateB.m_AnchoredPos, inPercent);
            newState.m_Anchors = Vector4.LerpUnclamped(inStateA.m_Anchors, inStateB.m_Anchors, inPercent);
            newState.m_SizeDelta = Vector2.LerpUnclamped(inStateA.m_SizeDelta, inStateB.m_SizeDelta, inPercent);
            newState.m_Pivot = Vector2.LerpUnclamped(inStateA.m_Pivot, inStateB.m_Pivot, inPercent);

            newState.m_Scale = Vector3.LerpUnclamped(inStateA.m_Scale, inStateB.m_Scale, inPercent);
            newState.m_Rotation = Quaternion.SlerpUnclamped(inStateA.Rotation, inStateB.Rotation, inPercent).eulerAngles;
            return newState;
        }

        /// <summary>
        /// Interpolates betwen two RectTransformStates.
        /// </summary>
        static public void Lerp(ref RectTransformState outNewState, RectTransformState inStateA, RectTransformState inStateB, float inPercent)
        {
            outNewState.m_AnchoredPos = Vector3.LerpUnclamped(inStateA.m_AnchoredPos, inStateB.m_AnchoredPos, inPercent);
            outNewState.m_Anchors = Vector4.LerpUnclamped(inStateA.m_Anchors, inStateB.m_Anchors, inPercent);
            outNewState.m_SizeDelta = Vector2.LerpUnclamped(inStateA.m_SizeDelta, inStateB.m_SizeDelta, inPercent);
            outNewState.m_Pivot = Vector2.LerpUnclamped(inStateA.m_Pivot, inStateB.m_Pivot, inPercent);

            outNewState.m_Scale = Vector3.LerpUnclamped(inStateA.m_Scale, inStateB.m_Scale, inPercent);
            outNewState.m_Rotation = Quaternion.SlerpUnclamped(inStateA.Rotation, inStateB.Rotation, inPercent).eulerAngles;
        }
    }
}

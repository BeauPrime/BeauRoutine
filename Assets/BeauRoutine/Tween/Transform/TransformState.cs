/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TransformState.cs
 * Purpose: Maintains a record of a Transform's position, scale,
 *          and rotation for restoration or interpolation.
*/

using BeauRoutine.Internal;
using System;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Holds a record of the state of a transform's position, scale, and rotation.
    /// </summary>
    public struct TransformState
    {
        private Vector3 m_Position;
        private Vector3 m_Scale;
        private Vector3 m_Rotation;
        private Space m_Space;

        /// <summary>
        /// Recorded position.
        /// </summary>
        public Vector3 Position
        {
            get { return m_Position; }
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
        /// Space the TransformState is in.
        /// </summary>
        public Space Space
        {
            get { return m_Space; }
        }

        /// <summary>
        /// Returns an empty world-space state.
        /// </summary>
        static public TransformState WorldState()
        {
            return new TransformState(Space.World);
        }

        /// <summary>
        /// Returns a new world-space state for the given transform.
        /// </summary>
        static public TransformState WorldState(Transform inTransform)
        {
            return new TransformState(inTransform, Space.World);
        }

        /// <summary>
        /// Returns an empty local-space state.
        /// </summary>
        static public TransformState LocalState()
        {
            return new TransformState(Space.Self);
        }

        /// <summary>
        /// Returns a new local-space state for the given transform.
        /// </summary>
        static public TransformState LocalState(Transform inTransform)
        {
            return new TransformState(inTransform, Space.Self);
        }

        // Creates a new TransformState in the given space.
        private TransformState(Space inSpace)
        {
            m_Space = inSpace;
            m_Position = m_Rotation = Vector3.zero;
            m_Scale = Vector3.one;
        }

        /// <summary>
        /// Creates a new TransformState for the given transform.
        /// </summary>
        public TransformState(Transform inTransform, Space inSpace = Space.World)
        {
            m_Space = inSpace;
            m_Scale = inTransform.localScale;
            m_Rotation = Vector3.zero;

            if (inSpace == Space.Self)
                m_Position = inTransform.localPosition;
            else
                m_Position = inTransform.position;

            m_Rotation = GetRotation(inTransform);
        }

        /// <summary>
        /// Refreshes the values from the given transform.
        /// </summary>
        public void Refresh(Transform inTransform, TransformProperties inProperties = TransformProperties.All)
        {
            if ((inProperties & TransformProperties.Scale) != 0)
                m_Scale = inTransform.localScale;

            if (m_Space == Space.Self)
            {
                if ((inProperties & TransformProperties.Position) != 0)
                    m_Position = inTransform.localPosition;
            }
            else
            {
                if ((inProperties & TransformProperties.Position) != 0)
                    m_Position = inTransform.position;
            }

            if ((inProperties & TransformProperties.Rotation) != 0)
                m_Rotation = GetRotation(inTransform);
        }

        /// <summary>
        /// Applies a TransformState to the given transform.
        /// </summary>
        public void Apply(Transform inTransform, TransformProperties inProperties = TransformProperties.All)
        {
            if ((inProperties & TransformProperties.Position) != 0)
            {
                inTransform.SetPosition(m_Position, inProperties.ToPositionAxis(), m_Space);
            }

            if ((inProperties & TransformProperties.Scale) != 0)
            {
                inTransform.SetScale(m_Scale, inProperties.ToScaleAxis());
            }

            if ((inProperties & TransformProperties.Rotation) != 0)
            {
                inTransform.SetRotation(m_Rotation, inProperties.ToRotationAxis(), m_Space);
            }
        }

        private Vector3 GetRotation(Transform inTransform)
        {
            EulerStorage record = EulerStorage.GetRecord(inTransform);
            if (record != null)
                return record.Get(m_Space);
            return m_Space == UnityEngine.Space.Self ? inTransform.localEulerAngles : inTransform.eulerAngles;
        }

        /// <summary>
        /// Interpolates betwen two TransformStates.
        /// </summary>
        static public TransformState Lerp(TransformState inStateA, TransformState inStateB, float inPercent)
        {
            if (inStateA.m_Space != inStateB.m_Space)
                throw new InvalidOperationException("Cannot interpolate between world and self space TransformStates!");

            TransformState newState = new TransformState();
            newState.m_Position = Vector3.LerpUnclamped(inStateA.m_Position, inStateB.m_Position, inPercent);
            newState.m_Scale = Vector3.LerpUnclamped(inStateA.m_Scale, inStateB.m_Scale, inPercent);
            newState.m_Rotation = Quaternion.SlerpUnclamped(inStateA.Rotation, inStateB.Rotation, inPercent).eulerAngles;
            newState.m_Space = inStateA.m_Space;
            return newState;
        }

        /// <summary>
        /// Interpolates betwen two TransformStates.
        /// </summary>
        static public void Lerp(ref TransformState outNewState, TransformState inStateA, TransformState inStateB, float inPercent)
        {
            if (inStateA.m_Space != inStateB.m_Space)
                throw new InvalidOperationException("Cannot interpolate between world and self space TransformStates!");

            outNewState.m_Position = Vector3.LerpUnclamped(inStateA.m_Position, inStateB.m_Position, inPercent);
            outNewState.m_Scale = Vector3.LerpUnclamped(inStateA.m_Scale, inStateB.m_Scale, inPercent);
            outNewState.m_Rotation = Quaternion.SlerpUnclamped(inStateA.Rotation, inStateB.Rotation, inPercent).eulerAngles;
            outNewState.m_Space = inStateA.m_Space;
        }
    }
}

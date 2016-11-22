/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TransformUtil.cs
 * Purpose: Extension methods for dealing with Transforms
 *          on different axis values.
*/

using System.Collections;
using UnityEngine;
using BeauRoutine.Internal;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for dealing with transforms.
    /// </summary>
    public static class TransformUtil
    {
        #region Axis

        /// <summary>
        /// Converts the TransformProperties to an equivalent
        /// Axis value for positions.
        /// </summary>
        static public Axis ToPositionAxis(this TransformProperties inProperties)
        {
            uint axisValue = (uint)inProperties & 0x07;
            return (Axis)axisValue;
        }

        /// <summary>
        /// Converts the TransformProperties to an equivalent
        /// Axis value for scales.
        /// </summary>
        static public Axis ToScaleAxis(this TransformProperties inProperties)
        {
            uint axisValue = ((uint)inProperties >> 3) & 0x07;
            return (Axis)axisValue;
        }

        /// <summary>
        /// Converts the TransformProperties to an equivalent
        /// Axis value for rotations.
        /// </summary>
        static public Axis ToRotationAxis(this TransformProperties inProperties)
        {
            uint axisValue = ((uint)inProperties >> 6) & 0x07;
            return (Axis)axisValue;
        }

        #endregion

        #region Properties

        #region Position

        /// <summary>
        /// Returns the transform's position for the given axis.
        /// </summary>
        static public Vector3 GetPosition(this Transform inTransform, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return Vector3.zero.CopyFrom(
                inSpace == Space.Self ? inTransform.localPosition : inTransform.position,
                inAxis);
        }

        /// <summary>
        /// Sets the transform's position on the given axis.
        /// </summary>
        static public void SetPosition(this Transform inTransform, Vector3 inPosition, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            if (inAxis != 0)
            {
                Vector3 pos;
                if (inAxis == Axis.XYZ)
                    pos = inPosition;
                else
                    pos = (inSpace == Space.Self ? inTransform.localPosition : inTransform.position).CopyFrom(inPosition, inAxis);

                if (inSpace == Space.Self)
                    inTransform.localPosition = pos;
                else
                    inTransform.position = pos;
            }
        }

        /// <summary>
        /// Returns the transform's position for the given axis.
        /// </summary>
        static public float GetPositionAxis(this Transform inTransform, Axis inAxis, Space inSpace = Space.World)
        {
            return (inSpace == Space.Self ? inTransform.localPosition : inTransform.position).GetAxis(inAxis);
        }

        /// <summary>
        /// Sets the transform's position on the given axis.
        /// </summary>
        static public void SetPosition(this Transform inTransform, float inPosition, Axis inAxis, Space inSpace = Space.World)
        {
            SetPosition(inTransform, new Vector3(inPosition, inPosition, inPosition), inAxis, inSpace);
        }

        #endregion

        #region Scale

        /// <summary>
        /// Returns the transform's scale for the given axis.
        /// </summary>
        static public Vector3 GetScale(this Transform inTransform, Axis inAxis = Axis.XYZ)
        {
            return Vector3.one.CopyFrom(inTransform.localScale, inAxis);
        }

        /// <summary>
        /// Sets the transform's scale on the given axis.
        /// </summary>
        static public void SetScale(this Transform inTransform, Vector3 inScale, Axis inAxis = Axis.XYZ)
        {
            if (inAxis != 0)
            {
                Vector3 scale;
                if (inAxis == Axis.XYZ)
                    scale = inScale;
                else
                    scale = inTransform.localScale.CopyFrom(inScale, inAxis);

                inTransform.localScale = scale;
            }
        }

        /// <summary>
        /// Returns the transform's scale for the given axis.
        /// </summary>
        static public float GetScaleAxis(this Transform inTransform, Axis inAxis)
        {
            return inTransform.localScale.GetAxis(inAxis);
        }

        /// <summary>
        /// Sets the transform's scale on the given axis.
        /// </summary>
        static public void SetScale(this Transform inTransform, float inScale, Axis inAxis = Axis.XYZ)
        {
            SetScale(inTransform, new Vector3(inScale, inScale, inScale), inAxis);
        }

        #endregion

        #region Rotation

        static private Vector3 GetRotationInternal(Transform inTransform, Space inSpace)
        {
            EulerStorage record = EulerStorage.GetRecord(inTransform);
            if (record != null)
                return record.Get(inSpace);
            return inSpace == UnityEngine.Space.Self ? inTransform.localEulerAngles : inTransform.eulerAngles;
        }

        /// <summary>
        /// Returns the transform's rotation for the given axis.
        /// </summary>
        static public Vector3 GetRotation(this Transform inTransform, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return Vector3.zero.CopyFrom(
                GetRotationInternal(inTransform, inSpace),
                inAxis);
        }

        /// <summary>
        /// Sets the transform's rotation on the given axis.
        /// </summary>
        static public void SetRotation(this Transform inTransform, Vector3 inRotation, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            if (inAxis != 0)
            {
                Vector3 rotation;
                if (inAxis == Axis.XYZ)
                {
                    rotation = inRotation;
                }
                else
                {
                    EulerStorage record = EulerStorage.GetRecord(inTransform);
                    if (record != null)
                    {
                        rotation = record.Get(inSpace).CopyFrom(inRotation, inAxis);
                        record.Set(inSpace, rotation);
                    }
                    else
                    {
                        rotation = (inSpace == Space.Self ? inTransform.localEulerAngles : inTransform.eulerAngles).CopyFrom(inRotation, inAxis);
                    }
                }

                if (inSpace == Space.Self)
                    inTransform.localEulerAngles = rotation;
                else
                    inTransform.localEulerAngles = rotation;
            }
        }

        /// <summary>
        /// Returns the transform's rotation for the given axis.
        /// </summary>
        static public float GetRotationAxis(this Transform inTransform, Axis inAxis, Space inSpace = Space.World)
        {
            return GetRotationInternal(inTransform, inSpace).GetAxis(inAxis);
        }

        /// <summary>
        /// Sets the transform's rotation on the given axis.
        /// </summary>
        static public void SetRotation(this Transform inTransform, float inRotation, Axis inAxis, Space inSpace = Space.World)
        {
            SetRotation(inTransform, new Vector3(inRotation, inRotation, inRotation), inAxis, inSpace);
        }

        #endregion

        #region AnchoredPosition

        /// <summary>
        /// Returns the RectTransform's anchored position for the given axis.
        /// </summary>
        static public Vector2 GetAnchorPos(this RectTransform inTransform, Axis inAxis = Axis.XY)
        {
            return Vector2.zero.CopyFrom(inTransform.anchoredPosition, inAxis);
        }

        /// <summary>
        /// Sets the RectTransform's anchored position on the given axis.
        /// </summary>
        static public void SetAnchorPos(this RectTransform inTransform, Vector2 inPosition, Axis inAxis = Axis.XYZ)
        {
            if (inAxis != 0)
            {
                Vector2 pos;
                if (inAxis == Axis.XYZ || inAxis == Axis.XY)
                    pos = inPosition;
                else
                    pos = inTransform.anchoredPosition.CopyFrom(inPosition, inAxis);

                inTransform.anchoredPosition = pos;
            }
        }

        /// <summary>
        /// Returns the RectTransform's anchored position for the given axis.
        /// </summary>
        static public float GetAnchorPosAxis(this RectTransform inTransform, Axis inAxis)
        {
            return inTransform.anchoredPosition.GetAxis(inAxis);
        }

        /// <summary>
        /// Sets the RectTransform's anchored position on the given axis.
        /// </summary>
        static public void SetAnchorPos(this RectTransform inTransform, float inPosition, Axis inAxis)
        {
            SetAnchorPos(inTransform, new Vector2(inPosition, inPosition), inAxis);
        }

        #endregion

        #endregion

        #region Snapping

        /// <summary>
        /// Snaps this transform's properties to those on the given transform.
        /// </summary>
        static public void SnapTo(this Transform inTransform, Transform inTarget, TransformProperties inProperties = TransformProperties.All, Space inSpace = Space.World)
        {
            TransformState state = (inSpace == Space.World ? TransformState.WorldState : TransformState.LocalState);
            state.Refresh(inTarget, inProperties);
            state.Apply(inTransform, inProperties);
        }

        #endregion

        #region Vector Operations

        /// <summary>
        /// Returns the value of the vector for the given axis.
        /// </summary>
        static public float GetAxis(this Vector3 inVector, Axis inAxis)
        {
            if ((inAxis & Axis.X) != 0)
                return inVector.x;
            if ((inAxis & Axis.Y) != 0)
                return inVector.y;
            if ((inAxis & Axis.Z) != 0)
                return inVector.z;
            return float.NaN;
        }

        /// <summary>
        /// Returns the value of the vector for the given axis.
        /// </summary>
        static public float GetAxis(this Vector2 inVector, Axis inAxis)
        {
            if ((inAxis & Axis.X) != 0)
                return inVector.x;
            if ((inAxis & Axis.Y) != 0)
                return inVector.y;
            return float.NaN;
        }

        /// <summary>
        /// Returns the initial vector with the given axis values
        /// copied from the given source.
        /// </summary>
        static public Vector3 CopyFrom(this Vector3 inVector, Vector3 inSource, Axis inAxis = Axis.XYZ)
        {
            if (inAxis == Axis.XYZ)
                return inSource;
            if (inAxis == 0)
                return inVector;

            if ((inAxis & Axis.X) != 0)
                inVector.x = inSource.x;
            if ((inAxis & Axis.Y) != 0)
                inVector.y = inSource.y;
            if ((inAxis & Axis.Z) != 0)
                inVector.z = inSource.z;

            return inVector;
        }

        /// <summary>
        /// Returns the initial vector with the given axis values
        /// copied from the given source.
        /// </summary>
        static public Vector2 CopyFrom(this Vector2 inVector, Vector2 inSource, Axis inAxis = Axis.XY)
        {
            if (inAxis == Axis.XY || inAxis == Axis.XYZ)
                return inSource;
            if (inAxis == 0)
                return inVector;

            if ((inAxis & Axis.X) != 0)
                inVector.x = inSource.x;
            if ((inAxis & Axis.Y) != 0)
                inVector.y = inSource.y;

            return inVector;
        }

        #endregion
    }
}

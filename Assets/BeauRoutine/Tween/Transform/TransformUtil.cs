/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TransformUtil.cs
 * Purpose: Extension methods for dealing with Transforms
 *          on different axis values.
*/

using BeauRoutine.Internal;
using UnityEngine;

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

        /// <summary>
        /// Converts the RectTransformProperties to an equivalent
        /// Axis value for anchored positions.
        /// </summary>
        static public Axis ToAnchoredPositionAxis(this RectTransformProperties inProperties)
        {
            uint axisValue = (uint)inProperties & 0x07;
            return (Axis)axisValue;
        }

        /// <summary>
        /// Converts the RectTransformProperties to an equivalent
        /// Axis value for size delta.
        /// </summary>
        static public Axis ToSizeDeltaAxis(this RectTransformProperties inProperties)
        {
            uint axisValue = ((uint)inProperties >> 9) & 0x03;
            return (Axis)axisValue;
        }

        /// <summary>
        /// Converts the RectTransformProperties to an equivalent
        /// Axis value for scales.
        /// </summary>
        static public Axis ToScaleAxis(this RectTransformProperties inProperties)
        {
            uint axisValue = ((uint)inProperties >> 11) & 0x07;
            return (Axis)axisValue;
        }

        /// <summary>
        /// Converts the RectTransformProperties to an equivalent
        /// Axis value for rotations.
        /// </summary>
        static public Axis ToRotationAxis(this RectTransformProperties inProperties)
        {
            uint axisValue = ((uint)inProperties >> 14) & 0x07;
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
            Vector3 vector = Vector3.zero;
            VectorUtil.CopyFrom(ref vector, inSpace == Space.Self ? inTransform.localPosition : inTransform.position, inAxis);
            return vector;
        }

        /// <summary>
        /// Sets the transform's position on the given axis.
        /// </summary>
        static public void SetPosition(this Transform inTransform, Vector3 inPosition, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            if (inAxis != 0)
            {
                Vector3 pos;
                if ((inAxis & Axis.XYZ) == Axis.XYZ)
                    pos = inPosition;
                else
                {
                    pos = (inSpace == Space.Self ? inTransform.localPosition : inTransform.position);
                    VectorUtil.CopyFrom(ref pos, inPosition, inAxis);
                }

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
            Vector3 scale = new Vector3(1, 1, 1);
            VectorUtil.CopyFrom(ref scale, inTransform.localScale, inAxis);
            return scale;
        }

        /// <summary>
        /// Sets the transform's scale on the given axis.
        /// </summary>
        static public void SetScale(this Transform inTransform, Vector3 inScale, Axis inAxis = Axis.XYZ)
        {
            if (inAxis != 0)
            {
                Vector3 scale;
                if ((inAxis & Axis.XYZ) == Axis.XYZ)
                    scale = inScale;
                else
                {
                    scale = inTransform.localScale;
                    VectorUtil.CopyFrom(ref scale, inScale, inAxis);
                }

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
            Vector3 rot = new Vector3(0, 0, 0);
            VectorUtil.CopyFrom(ref rot,
                GetRotationInternal(inTransform, inSpace),
                inAxis);
            return rot;
        }

        /// <summary>
        /// Sets the transform's rotation on the given axis.
        /// </summary>
        static public void SetRotation(this Transform inTransform, Vector3 inRotation, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            if (inAxis != 0)
            {
                Vector3 rotation;
                if ((inAxis & Axis.XYZ) == Axis.XYZ)
                {
                    rotation = inRotation;
                }
                else
                {
                    EulerStorage record = EulerStorage.GetRecord(inTransform);
                    if (record != null)
                    {
                        rotation = record.Get(inSpace);
                        VectorUtil.CopyFrom(ref rotation, inRotation, inAxis);
                        record.Set(inSpace, rotation);
                    }
                    else
                    {
                        rotation = (inSpace == Space.Self ? inTransform.localEulerAngles : inTransform.eulerAngles);
                        VectorUtil.CopyFrom(ref rotation, inRotation, inAxis);
                    }
                }

                if (inSpace == Space.Self)
                    inTransform.localEulerAngles = rotation;
                else
                    inTransform.eulerAngles = rotation;
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
            Vector2 pos = new Vector2(0, 0);
            VectorUtil.CopyFrom(ref pos, inTransform.anchoredPosition, inAxis);
            return pos;
        }

        /// <summary>
        /// Sets the RectTransform's anchored position on the given axis.
        /// </summary>
        static public void SetAnchorPos(this RectTransform inTransform, Vector2 inPosition, Axis inAxis = Axis.XYZ)
        {
            if (inAxis != 0)
            {
                Vector2 pos;
                if ((inAxis & Axis.XY) == Axis.XY)
                    pos = inPosition;
                else
                {
                    pos = inTransform.anchoredPosition;
                    VectorUtil.CopyFrom(ref pos, inPosition, inAxis);
                }

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

        #region SizeDelta

        /// <summary>
        /// Returns the RectTransform's sizeDelta for the given axis.
        /// </summary>
        static public Vector2 GetSizeDelta(this RectTransform inTransform, Axis inAxis = Axis.XY)
        {
            Vector2 size = new Vector2(0, 0);
            VectorUtil.CopyFrom(ref size, inTransform.sizeDelta, inAxis);
            return size;
        }

        /// <summary>
        /// Sets the RectTransform's size delta on the given axis.
        /// </summary>
        static public void SetSizeDelta(this RectTransform inTransform, Vector2 inSize, Axis inAxis = Axis.XYZ)
        {
            if (inAxis != 0)
            {
                Vector2 pos;
                if ((inAxis & Axis.XY) == Axis.XY)
                    pos = inSize;
                else
                {
                    pos = inTransform.sizeDelta;
                    VectorUtil.CopyFrom(ref pos, inSize, inAxis);
                }

                inTransform.sizeDelta = pos;
            }
        }

        /// <summary>
        /// Returns the RectTransform's sizeDelta for the given axis.
        /// </summary>
        static public float GetSizeDeltaAxis(this RectTransform inTransform, Axis inAxis)
        {
            return inTransform.sizeDelta.GetAxis(inAxis);
        }

        /// <summary>
        /// Sets the RectTransform's sizeDelta on the given axis.
        /// </summary>
        static public void SetSizeDelta(this RectTransform inTransform, float inSize, Axis inAxis)
        {
            SetSizeDelta(inTransform, new Vector2(inSize, inSize), inAxis);
        }

        #endregion

        #endregion

        #region Snapping

        /// <summary>
        /// Snaps this transform's properties to those on the given transform.
        /// </summary>
        static public void SnapTo(this Transform inTransform, Transform inTarget, TransformProperties inProperties = TransformProperties.All, Space inSpace = Space.World)
        {
            TransformState state = (inSpace == Space.World ? TransformState.WorldState() : TransformState.LocalState());
            state.Refresh(inTarget, inProperties);
            state.Apply(inTransform, inProperties);
        }

        #endregion
    }
}

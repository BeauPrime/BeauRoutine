/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenShortcuts.Transform.cs
 * Purpose: Extension methods for creating Tweens affecting
 *          properties on a Transform.
*/

using BeauRoutine.Internal;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for generating tweens.
    /// </summary>
    static public partial class TweenShortcuts
    {
        #region Position

        private sealed class TweenData_Transform_PositionFixed : ITweenData
        {
            private Transform m_Transform;
            private Vector3 m_Target;
            private Space m_Space;
            private Axis m_Axis;

            private Vector3 m_Start;
            private Vector3 m_Delta;

            public TweenData_Transform_PositionFixed(Transform inTransform, Vector3 inTarget, Space inSpace, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Space = inSpace;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = (m_Space == Space.World ? m_Transform.position : m_Transform.localPosition);
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 final = new Vector3(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent,
                    m_Start.z + m_Delta.z * inPercent);

                m_Transform.SetPosition(final, m_Axis, m_Space);
            }

            public override string ToString()
            {
                return "Transform: Position (Fixed)";
            }
        }

        private sealed class TweenData_Transform_PositionDynamic : ITweenData
        {
            private Transform m_Transform;
            private Transform m_Target;
            private Space m_Space;
            private Axis m_Axis;

            private Vector3 m_Start;

            public TweenData_Transform_PositionDynamic(Transform inTransform, Transform inTarget, Space inSpace, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Space = inSpace;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = (m_Space == Space.World ? m_Transform.position : m_Transform.localPosition);
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 delta = (m_Space == Space.World ? m_Target.position : m_Target.localPosition);
                VectorUtil.Subtract(ref delta, m_Start);
                Vector3 final = new Vector3(
                    m_Start.x + delta.x * inPercent,
                    m_Start.y + delta.y * inPercent,
                    m_Start.z + delta.z * inPercent);

                m_Transform.SetPosition(final, m_Axis, m_Space);
            }

            public override string ToString()
            {
                return "Transform: Position (Dynamic)";
            }
        }

        /// <summary>
        /// Moves the Transform to another position over time.
        /// </summary>
        static public Tween MoveTo(this Transform inTransform, Vector3 inTarget, float inTime, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_PositionFixed(inTransform, inTarget, inSpace, inAxis), inTime);
        }

        /// <summary>
        /// Moves the Transform to another position over time.
        /// </summary>
        static public Tween MoveTo(this Transform inTransform, Vector3 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_PositionFixed(inTransform, inTarget, inSpace, inAxis), inSettings);
        }

        /// <summary>
        /// Moves the Transform to another position over time.
        /// </summary>
        static public Tween MoveTo(this Transform inTransform, float inPosition, float inTime, Axis inAxis, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_PositionFixed(inTransform, new Vector3(inPosition, inPosition, inPosition), inSpace, inAxis), inTime);
        }

        /// <summary>
        /// Moves the Transform to another position over time.
        /// </summary>
        static public Tween MoveTo(this Transform inTransform, float inPosition, TweenSettings inSettings, Axis inAxis, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_PositionFixed(inTransform, new Vector3(inPosition, inPosition, inPosition), inSpace, inAxis), inSettings);
        }

        /// <summary>
        /// Moves the Transform to the position of another Transform over time.
        /// </summary>
        static public Tween MoveTo(this Transform inTransform, Transform inTarget, float inTime, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_PositionDynamic(inTransform, inTarget, inSpace, inAxis), inTime);
        }

        /// <summary>
        /// Moves the Transform to the position of another Transform over time.
        /// </summary>
        static public Tween MoveTo(this Transform inTransform, Transform inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_PositionDynamic(inTransform, inTarget, inSpace, inAxis), inSettings);
        }

        /// <summary>
        /// Moves the Transform to another position with the given average speed.
        /// Note: Duration is calculated at call time, not when the tween starts.
        /// </summary>
        static public Tween MoveToWithSpeed(this Transform inTransform, Vector3 inTarget, float inSpeed, Axis inAxis = Axis.XYZ, Space inSpace = Space.World)
        {
            Vector3 diff = new Vector3(0, 0, 0);
            VectorUtil.CopyFrom(ref diff, inTarget - inTransform.GetPosition(Axis.XYZ, inSpace), inAxis);
            float distance = diff.magnitude;
            return Tween.Create(new TweenData_Transform_PositionFixed(inTransform, inTarget, inSpace, inAxis), distance / inSpeed);
        }

        #endregion

        #region Scale

        private sealed class TweenData_Transform_ScaleFixed : ITweenData
        {
            private Transform m_Transform;
            private Vector3 m_Target;
            private Axis m_Axis;

            private Vector3 m_Start;
            private Vector3 m_Delta;

            public TweenData_Transform_ScaleFixed(Transform inTransform, Vector3 inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.localScale;
                m_Delta = m_Target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 final = new Vector3(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent,
                    m_Start.z + m_Delta.z * inPercent);

                m_Transform.SetScale(final, m_Axis);
            }

            public override string ToString()
            {
                return "Transform: Scale (Fixed)";
            }
        }

        private sealed class TweenData_Transform_ScaleDynamic : ITweenData
        {
            private Transform m_Transform;
            private Transform m_Target;
            private Axis m_Axis;

            private Vector3 m_Start;

            public TweenData_Transform_ScaleDynamic(Transform inTransform, Transform inTarget, Axis inAxis)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.localScale;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 delta = m_Target.localScale;
                VectorUtil.Subtract(ref delta, m_Start);
                Vector3 final = new Vector3(
                    m_Start.x + delta.x * inPercent,
                    m_Start.y + delta.y * inPercent,
                    m_Start.z + delta.z * inPercent);

                m_Transform.SetScale(final, m_Axis);
            }

            public override string ToString()
            {
                return "Transform: Scale (Dynamic)";
            }
        }

        /// <summary>
        /// Scales the Transform to another scale over time.
        /// </summary>
        static public Tween ScaleTo(this Transform inTransform, Vector3 inTarget, float inTime, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_ScaleFixed(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Scales the Transform to another scale over time.
        /// </summary>
        static public Tween ScaleTo(this Transform inTransform, Vector3 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_ScaleFixed(inTransform, inTarget, inAxis), inSettings);
        }

        /// <summary>
        /// Scales the Transform to another scale over time.
        /// </summary>
        static public Tween ScaleTo(this Transform inTransform, float inTarget, float inTime, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_ScaleFixed(inTransform, new Vector3(inTarget, inTarget, inTarget), inAxis), inTime);
        }

        /// <summary>
        /// Scales the Transform to another scale over time.
        /// </summary>
        static public Tween ScaleTo(this Transform inTransform, float inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_ScaleFixed(inTransform, new Vector3(inTarget, inTarget, inTarget), inAxis), inSettings);
        }

        /// <summary>
        /// Scales the Transform to the scale of another Transform over time.
        /// </summary>
        static public Tween ScaleTo(this Transform inTransform, Transform inTarget, float inTime, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_ScaleDynamic(inTransform, inTarget, inAxis), inTime);
        }

        /// <summary>
        /// Scales the Transform to the scale of another Transform over time.
        /// </summary>
        static public Tween ScaleTo(this Transform inTransform, Transform inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_ScaleDynamic(inTransform, inTarget, inAxis), inSettings);
        }

        #endregion

        #region Rotation

        private sealed class TweenData_Transform_RotationFixed : ITweenData
        {
            private Transform m_Transform;
            private Quaternion m_Target;
            private Space m_Space;

            private Quaternion m_Start;

            public TweenData_Transform_RotationFixed(Transform inTransform, Quaternion inTarget, Space inSpace)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Space = inSpace;
            }

            public void OnTweenStart()
            {
                m_Start = (m_Space == Space.World ? m_Transform.rotation : m_Transform.localRotation);
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Quaternion final = UnityEngine.Quaternion.SlerpUnclamped(m_Start, m_Target, inPercent);

                switch (m_Space)
                {
                    case Space.Self:
                        m_Transform.localRotation = final;
                        break;
                    case Space.World:
                        m_Transform.rotation = final;
                        break;
                }
            }

            public override string ToString()
            {
                return "Transform: Rotation (Fixed)";
            }
        }

        private sealed class TweenData_Transform_RotationDynamic : ITweenData
        {
            private Transform m_Transform;
            private Transform m_Target;
            private Space m_Space;

            private Quaternion m_Start;

            public TweenData_Transform_RotationDynamic(Transform inTransform, Transform inTarget, Space inSpace)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Space = inSpace;
            }

            public void OnTweenStart()
            {
                m_Start = (m_Space == Space.World ? m_Transform.rotation : m_Transform.localRotation);
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Quaternion target = (m_Space == Space.World ? m_Target.rotation : m_Target.localRotation);
                Quaternion final = UnityEngine.Quaternion.SlerpUnclamped(m_Start, target, inPercent);

                switch (m_Space)
                {
                    case Space.Self:
                        m_Transform.localRotation = final;
                        break;
                    case Space.World:
                        m_Transform.rotation = final;
                        break;
                }
            }

            public override string ToString()
            {
                return "Transform: Rotation (Dynamic)";
            }
        }

        /// <summary>
        /// Rotates the Transform to another orientation over time.
        /// </summary>
        static public Tween RotateQuaternionTo(this Transform inTransform, Quaternion inTarget, float inTime, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_RotationFixed(inTransform, inTarget, inSpace), inTime);
        }

        /// <summary>
        /// Rotates the Transform to another orientation over time.
        /// </summary>
        static public Tween RotateQuaternionTo(this Transform inTransform, Quaternion inTarget, TweenSettings inSettings, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_RotationFixed(inTransform, inTarget, inSpace), inSettings);
        }

        /// <summary>
        /// Rotates the Transform to the orientation of another Transform over time.
        /// </summary>
        static public Tween RotateTo(this Transform inTransform, Transform inTarget, float inTime, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_RotationDynamic(inTransform, inTarget, inSpace), inTime);
        }

        /// <summary>
        /// Rotates the Transform to the orientation of another Transform over time.
        /// </summary>
        static public Tween RotateTo(this Transform inTransform, Transform inTarget, TweenSettings inSettings, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_RotationDynamic(inTransform, inTarget, inSpace), inSettings);
        }

        #endregion

        #region EulerRotation

        private sealed class TweenData_Transform_EulerRotationFixed : ITweenData
        {
            private Transform m_Transform;
            private Vector3 m_Target;
            private Space m_Space;
            private Axis m_Axis;
            private AngleMode m_Mode;

            private Vector3 m_Start;
            private Vector3 m_Delta;
            private EulerStorage m_Record;
            private int m_RecordID;

            public TweenData_Transform_EulerRotationFixed(Transform inTransform, Vector3 inTarget, Space inSpace, Axis inAxis, AngleMode inMode)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Space = inSpace;
                m_Axis = inAxis;
                m_Mode = inMode;
            }

            public void OnTweenStart()
            {
                m_Record = EulerStorage.AddTransform(m_Transform);
                m_RecordID = m_Transform.GetInstanceID();

                m_Start = m_Record.Get(m_Space);
                m_Delta = m_Target - m_Start;

                if (m_Mode != AngleMode.Absolute)
                    WrapEuler(ref m_Delta);
            }

            public void OnTweenEnd()
            {
                EulerStorage.RemoveTransform(m_RecordID);
            }

            public void ApplyTween(float inPercent)
            {
                Vector3 tweened = m_Start;
                VectorUtil.Add(ref tweened, m_Delta, inPercent);
                Vector3 final = m_Record.Get(m_Space);
                VectorUtil.CopyFrom(ref final, tweened, m_Axis);
                WrapEuler(ref final);

                m_Transform.SetRotation(final, Axis.XYZ, m_Space);
                m_Record.Set(m_Space, final);
            }

            public override string ToString()
            {
                return "Transform: Euler Rotation (Fixed)";
            }

            static private void WrapEuler(ref Vector3 ioEuler)
            {
                WrapAngle(ref ioEuler.x);
                WrapAngle(ref ioEuler.y);
                WrapAngle(ref ioEuler.z);
            }

            static private void WrapAngle(ref float ioAngle)
            {
                while (ioAngle >= 180)
                    ioAngle -= 360;
                while (ioAngle < -180)
                    ioAngle += 360;
            }
        }

        /// <summary>
        /// Rotates the Transform to another euler orientation over time.
        /// </summary>
        static public Tween RotateTo(this Transform inTransform, Vector3 inTarget, float inTime, Axis inAxis = Axis.XYZ, Space inSpace = Space.World, AngleMode inMode = AngleMode.Shortest)
        {
            return Tween.Create(new TweenData_Transform_EulerRotationFixed(inTransform, inTarget, inSpace, inAxis, inMode), inTime);
        }

        /// <summary>
        /// Rotates the Transform to another euler orientation over time.
        /// </summary>
        static public Tween RotateTo(this Transform inTransform, Vector3 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ, Space inSpace = Space.World, AngleMode inMode = AngleMode.Shortest)
        {
            return Tween.Create(new TweenData_Transform_EulerRotationFixed(inTransform, inTarget, inSpace, inAxis, inMode), inSettings);
        }

        /// <summary>
        /// Rotates the Transform to another euler orientation over time.
        /// </summary>
        static public Tween RotateTo(this Transform inTransform, float inTarget, float inTime, Axis inAxis, Space inSpace = Space.World, AngleMode inMode = AngleMode.Shortest)
        {
            return Tween.Create(new TweenData_Transform_EulerRotationFixed(inTransform, new Vector3(inTarget, inTarget, inTarget), inSpace, inAxis, inMode), inTime);
        }

        /// <summary>
        /// Rotates the Transform to another euler orientation over time.
        /// </summary>
        static public Tween RotateTo(this Transform inTransform, float inTarget, TweenSettings inSettings, Axis inAxis, Space inSpace = Space.World, AngleMode inMode = AngleMode.Shortest)
        {
            return Tween.Create(new TweenData_Transform_EulerRotationFixed(inTransform, new Vector3(inTarget, inTarget, inTarget), inSpace, inAxis, inMode), inSettings);
        }

        #endregion

        #region LookAt

        private sealed class TweenData_Transform_LookAtFixed : ITweenData
        {
            private Transform m_Transform;
            private Vector3 m_Target;
            private Axis m_Axis;
            private Vector3 m_Up;

            private Quaternion m_Start;

            public TweenData_Transform_LookAtFixed(Transform inTransform, Vector3 inTarget, Axis inAxis, Vector3 inUp)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
                m_Up = inUp;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.rotation;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 vector = (m_Target - m_Transform.position);
                VectorUtil.CopyFrom(ref vector, Vector3.zero, m_Axis);
                Quaternion target = UnityEngine.Quaternion.LookRotation(vector, m_Up);
                m_Transform.rotation = UnityEngine.Quaternion.SlerpUnclamped(m_Start, target, inPercent);
            }

            public override string ToString()
            {
                return "Transform: LookAt (Fixed)";
            }
        }

        private sealed class TweenData_Transform_LookAtDynamic : ITweenData
        {
            private Transform m_Transform;
            private Transform m_Target;
            private Axis m_Axis;
            private Vector3 m_Up;

            private Quaternion m_Start;

            public TweenData_Transform_LookAtDynamic(Transform inTransform, Transform inTarget, Axis inAxis, Vector3 inUp)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Axis = inAxis;
                m_Up = inUp;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.rotation;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 vector = (m_Target.position - m_Transform.position);
                VectorUtil.CopyFrom(ref vector, Vector3.zero, m_Axis);
                Quaternion target = UnityEngine.Quaternion.LookRotation(vector, m_Up);
                m_Transform.rotation = UnityEngine.Quaternion.SlerpUnclamped(m_Start, target, inPercent);
            }

            public override string ToString()
            {
                return "Transform: LookAt (Dynamic)";
            }
        }

        /// <summary>
        /// Rotates the Transform to look at the given point over time.
        /// </summary>
        static public Tween RotateLookAt(this Transform inTransform, Vector3 inTarget, float inTime, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_LookAtFixed(inTransform, inTarget, inAxis, Vector3.up), inTime);
        }

        /// <summary>
        /// Rotates the Transform to look at the given point over time.
        /// </summary>
        static public Tween RotateLookAt(this Transform inTransform, Vector3 inTarget, float inTime, Axis inAxis, Vector3 inUp)
        {
            return Tween.Create(new TweenData_Transform_LookAtFixed(inTransform, inTarget, inAxis, inUp), inTime);
        }

        /// <summary>
        /// Rotates the Transform to look at the given point over time.
        /// </summary>
        static public Tween RotateLookAt(this Transform inTransform, Vector3 inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_LookAtFixed(inTransform, inTarget, inAxis, Vector3.up), inSettings);
        }

        /// <summary>
        /// Rotates the Transform to look at the given point over time.
        /// </summary>
        static public Tween RotateLookAt(this Transform inTransform, Vector3 inTarget, TweenSettings inSettings, Axis inAxis, Vector3 inUp)
        {
            return Tween.Create(new TweenData_Transform_LookAtFixed(inTransform, inTarget, inAxis, inUp), inSettings);
        }

        /// <summary>
        /// Rotates the Transform to look at another Transform over time.
        /// </summary>
        static public Tween RotateLookAt(this Transform inTransform, Transform inTarget, float inTime, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_LookAtDynamic(inTransform, inTarget, inAxis, Vector3.up), inTime);
        }

        /// <summary>
        /// Rotates the Transform to look at another Transform over time.
        /// </summary>
        static public Tween RotateLookAt(this Transform inTransform, Transform inTarget, float inTime, Axis inAxis, Vector3 inUp)
        {
            return Tween.Create(new TweenData_Transform_LookAtDynamic(inTransform, inTarget, inAxis, inUp), inTime);
        }

        /// <summary>
        /// Rotates the Transform to look at another Transform over time.
        /// </summary>
        static public Tween RotateLookAt(this Transform inTransform, Transform inTarget, TweenSettings inSettings, Axis inAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_LookAtDynamic(inTransform, inTarget, inAxis, Vector3.up), inSettings);
        }

        /// <summary>
        /// Rotates the Transform to look at another Transform over time.
        /// </summary>
        static public Tween RotateLookAt(this Transform inTransform, Transform inTarget, TweenSettings inSettings, Axis inAxis, Vector3 inUp)
        {
            return Tween.Create(new TweenData_Transform_LookAtDynamic(inTransform, inTarget, inAxis, inUp), inSettings);
        }

        #endregion

        #region Transform

        private sealed class TweenData_Transform_TransformState : ITweenData
        {
            private Transform m_Transform;
            private TransformState m_Target;
            private TransformProperties m_Properties;

            private TransformState m_Start;
            private TransformState m_Current;
            private int m_RecordID;

            public TweenData_Transform_TransformState(Transform inTransform, TransformState inTarget, TransformProperties inProperties)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Properties = inProperties;
            }

            public void OnTweenStart()
            {
                m_Start = new TransformState(m_Transform, m_Target.Space);

                if ((m_Properties & TransformProperties.Rotation) != 0)
                {
                    EulerStorage.AddTransform(m_Transform);
                    m_RecordID = m_Transform.GetInstanceID();
                }
            }

            public void OnTweenEnd()
            {
                if ((m_Properties & TransformProperties.Rotation) != 0)
                    EulerStorage.RemoveTransform(m_RecordID);
            }

            public void ApplyTween(float inPercent)
            {
                TransformState.Lerp(ref m_Current, m_Start, m_Target, inPercent);
                m_Current.Apply(m_Transform, m_Properties);
            }

            public override string ToString()
            {
                return "Transform: TransformState (Fixed)";
            }
        }

        private sealed class TweenData_Transform_Transform : ITweenData
        {
            private Transform m_Transform;
            private Transform m_Target;
            private TransformProperties m_Properties;

            private TransformState m_Start;
            private TransformState m_End;
            private TransformState m_Current;

            private int m_RecordID;

            public TweenData_Transform_Transform(Transform inTransform, Transform inTarget, Space inSpace, TransformProperties inProperties)
            {
                m_Transform = inTransform;
                m_Target = inTarget;
                m_Properties = inProperties;
                m_End = new TransformState(inTarget, inSpace);
            }

            public void OnTweenStart()
            {
                m_Start = new TransformState(m_Transform, m_End.Space);
                if ((m_Properties & TransformProperties.Rotation) != 0)
                {
                    EulerStorage.AddTransform(m_Transform);
                    m_RecordID = m_Transform.GetInstanceID();
                }
            }

            public void OnTweenEnd()
            {
                if ((m_Properties & TransformProperties.Rotation) != 0)
                    EulerStorage.RemoveTransform(m_RecordID);
            }

            public void ApplyTween(float inPercent)
            {
                m_End.Refresh(m_Target, m_Properties);
                TransformState.Lerp(ref m_Current, m_Start, m_End, inPercent);
                m_Current.Apply(m_Transform, m_Properties);
            }

            public override string ToString()
            {
                return "Transform: TransformState (Dynamic)";
            }
        }

        /// <summary>
        /// Transforms the Transform to another Transform over time.
        /// </summary>
        static public Tween TransformTo(this Transform inTransform, TransformState inTarget, float inTime, TransformProperties inProperties = TransformProperties.All)
        {
            return Tween.Create(new TweenData_Transform_TransformState(inTransform, inTarget, inProperties), inTime);
        }

        /// <summary>
        /// Transforms the Transform to another Transform over time.
        /// </summary>
        static public Tween TransformTo(this Transform inTransform, TransformState inTarget, TweenSettings inSettings, TransformProperties inProperties = TransformProperties.All)
        {
            return Tween.Create(new TweenData_Transform_TransformState(inTransform, inTarget, inProperties), inSettings);
        }

        /// <summary>
        /// Transforms the Transform to another Transform over time.
        /// </summary>
        static public Tween TransformTo(this Transform inTransform, Transform inTarget, float inTime, TransformProperties inProperties = TransformProperties.All, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_Transform(inTransform, inTarget, inSpace, inProperties), inTime);
        }

        /// <summary>
        /// Transforms the Transform to another Transform over time.
        /// </summary>
        static public Tween TransformTo(this Transform inTransform, Transform inTarget, TweenSettings inSettings, TransformProperties inProperties = TransformProperties.All, Space inSpace = Space.World)
        {
            return Tween.Create(new TweenData_Transform_Transform(inTransform, inTarget, inSpace, inProperties), inSettings);
        }

        #endregion
    
        #region Squash/Stretch

        private sealed class TweenData_Transform_SquashStretch : ITweenData
        {
            private Transform m_Transform;
            private Vector3 m_TargetSquash;
            private Axis m_SquashAxis;
            private Axis m_DependentAxis;

            private Vector3 m_Start;
            private Vector3 m_Delta;

            public TweenData_Transform_SquashStretch(Transform inTransform, Vector3 inTarget, Axis inSquashAxis, Axis inDependentAxis)
            {
                m_Transform = inTransform;
                m_TargetSquash = inTarget;
                m_SquashAxis = inSquashAxis;
                m_DependentAxis = inDependentAxis & ~m_SquashAxis;
            }

            public void OnTweenStart()
            {
                m_Start = m_Transform.localScale;

                Vector3 axisMultipliers = Vector3.one;
                float totalSquashMultiplier = 1f;
                int dependentCount = 0;
                float totalVolume = 1;

                if ((m_SquashAxis & Axis.X) != 0)
                {
                    totalVolume *= m_Start.x;
                    axisMultipliers.x = m_TargetSquash.x / m_Start.x;
                    totalSquashMultiplier *= axisMultipliers.x;
                }
                else if ((m_DependentAxis & Axis.X) != 0)
                {
                    totalVolume *= m_Start.x;
                    ++dependentCount;
                }

                if ((m_SquashAxis & Axis.Y) != 0)
                {
                    totalVolume *= m_Start.y;
                    axisMultipliers.y = m_TargetSquash.y / m_Start.y;
                    totalSquashMultiplier *= axisMultipliers.y;
                }
                else if ((m_DependentAxis & Axis.Y) != 0)
                {
                    totalVolume *= m_Start.y;
                    ++dependentCount;
                }

                if ((m_SquashAxis & Axis.Z) != 0)
                {
                    totalVolume *= m_Start.z;
                    axisMultipliers.z = m_TargetSquash.z / m_Start.z;
                    totalSquashMultiplier *= axisMultipliers.z;
                }
                else if ((m_DependentAxis & Axis.Z) != 0)
                {
                    totalVolume *= m_Start.z;
                    ++dependentCount;
                }

                if (dependentCount > 0)
                {
                    float multiplier = 1f / totalSquashMultiplier;
                    if (dependentCount > 1)
                        multiplier = Mathf.Pow(multiplier, 1f / dependentCount);
                    if ((m_DependentAxis & Axis.X) != 0)
                        axisMultipliers.x = multiplier;
                    if ((m_DependentAxis & Axis.Y) != 0)
                        axisMultipliers.y = multiplier;
                    if ((m_DependentAxis & Axis.Z) != 0)
                        axisMultipliers.z = multiplier;
                }

                Vector3 target = m_Start;
                if (totalVolume == 0)
                {
                    Debug.LogWarning("[Tween: Transform SquashStretch] One or more axis scales are 0. Cannot maintain volume.");
                    if ((m_SquashAxis & Axis.X) != 0)
                        target.x = m_TargetSquash.x;
                    else if ((m_SquashAxis & Axis.Y) != 0)
                        target.y = m_TargetSquash.y;
                    else if ((m_SquashAxis & Axis.Z) != 0)
                        target.z = m_TargetSquash.z;
                }
                else
                {
                    target.x *= axisMultipliers.x;
                    target.y *= axisMultipliers.y;
                    target.z *= axisMultipliers.z;
                }

                m_Delta = target - m_Start;
            }

            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                Vector3 final = new Vector3(
                    m_Start.x + m_Delta.x * inPercent,
                    m_Start.y + m_Delta.y * inPercent,
                    m_Start.z + m_Delta.z * inPercent);

                m_Transform.SetScale(final, m_SquashAxis | m_DependentAxis);
            }

            public override string ToString()
            {
                return "Transform: SquashStretch";
            }
        }

        /// <summary>
        /// Scales the Transform to another scale over time while maintaining volume.
        /// </summary>
        static public Tween SquashStretchTo(this Transform inTransform, Vector3 inTarget, float inTime, Axis inPrimaryAxis, Axis inDependentAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_SquashStretch(inTransform, inTarget, inPrimaryAxis, inDependentAxis), inTime);
        }

        /// <summary>
        /// Scales the Transform to another scale over time while maintaining volume.
        /// </summary>
        static public Tween SquashStretchTo(this Transform inTransform, Vector3 inTarget, TweenSettings inSettings, Axis inPrimaryAxis, Axis inDependentAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_SquashStretch(inTransform, inTarget, inPrimaryAxis, inDependentAxis), inSettings);
        }

        /// <summary>
        /// Scales the Transform to another scale over time while maintaining volume.
        /// </summary>
        static public Tween SquashStretchTo(this Transform inTransform, float inTarget, float inTime, Axis inPrimaryAxis, Axis inDependentAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_SquashStretch(inTransform, new Vector3(inTarget, inTarget, inTarget), inPrimaryAxis, inDependentAxis), inTime);
        }

        /// <summary>
        /// Scales the Transform to another scale over time while maintaining volume.
        /// </summary>
        static public Tween SquashStretchTo(this Transform inTransform, float inTarget, TweenSettings inSettings, Axis inPrimaryAxis, Axis inDependentAxis = Axis.XYZ)
        {
            return Tween.Create(new TweenData_Transform_SquashStretch(inTransform, new Vector3(inTarget, inTarget, inTarget), inPrimaryAxis, inDependentAxis), inSettings);
        }

        #endregion // Squash/Stretch
    }
}

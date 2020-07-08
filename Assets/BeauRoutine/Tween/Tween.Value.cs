/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Tween.Value.cs
 * Purpose: Generic value-based tweens.  Useful if tweening
 *          a field or value that can't be tweened using
 *          an extension method.
*/

using System;
using UnityEngine;

namespace BeauRoutine
{
    public sealed partial class Tween
    {
        /// <summary>
        /// Function used to interpolate between two values.
        /// </summary>
        public delegate T Interpolator<T>(T inStart, T inEnd, float inPercent);

        private abstract class TweenData_Value<T> : ITweenData
        {
            protected T m_Start;
            protected T m_End;
            protected Action<T> m_Setter;

            public TweenData_Value(T inStart, T inEnd, Action<T> inSetter)
            {
                m_Start = inStart;
                m_End = inEnd;
                m_Setter = inSetter;
            }

            public void OnTweenStart() {}
            public void OnTweenEnd() { }

            public abstract void ApplyTween(float inPercent);

            public override string ToString()
            {
                return "Value<" + typeof(T).Name + ">";
            }
        }

        #region Null

        private class TweenData_Null : ITweenData
        {
            public void OnTweenStart() { }
            public void ApplyTween(float inPercent) { }
            public void OnTweenEnd() { }
        }

        static private readonly TweenData_Null NULL_DATA = new TweenData_Null();

        #endregion

        #region Generic

        private class TweenData_Value_Generic<T> : TweenData_Value<T>
        {
            protected Interpolator<T> m_LerpFunction;

            public TweenData_Value_Generic(T inStart, T inEnd, Action<T> inSetter, Interpolator<T> inLerp)
                : base(inStart, inEnd, inSetter)
            {
                m_LerpFunction = inLerp;
            }

            public override void ApplyTween(float inPercent)
            {
                m_Setter(m_LerpFunction(m_Start, m_End, inPercent));
            }
        }

        /// <summary>
        /// Tweens from one value to another over time.
        /// </summary>
        static public Tween Value<T>(T inStart, T inEnd, Action<T> inSetter, Interpolator<T> inLerp, float inTime)
        {
            return Tween.Create(new TweenData_Value_Generic<T>(inStart, inEnd, inSetter, inLerp), inTime);
        }

        /// <summary>
        /// Tweens from one value to another over time.
        /// </summary>
        static public Tween Value<T>(T inStart, T inEnd, Action<T> inSetter, Interpolator<T> inLerp, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Generic<T>(inStart, inEnd, inSetter, inLerp), inSettings);
        }

        #endregion

        #region Floats

        private class TweenData_Value_Float : TweenData_Value<float>
        {
            public TweenData_Value_Float(float inStart, float inEnd, Action<float> inSetter)
                : base(inStart, inEnd, inSetter)
            { }

            public override void ApplyTween(float inPercent)
            {
                m_Setter(m_Start + (m_End - m_Start) * inPercent);
            }
        }

        private class TweenData_Value_FloatCurve : ITweenData
        {
            private AnimationCurve m_Curve;
            private float m_CurveDuration;
            protected Action<float> m_Setter;

            public TweenData_Value_FloatCurve(AnimationCurve inCurve, Action<float> inSetter)
            {
                m_Curve = inCurve;
                m_Setter = inSetter;
                m_CurveDuration = inCurve.length > 0 ? inCurve.keys[inCurve.length - 1].time : 0;
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Setter(m_Curve.Evaluate(inPercent * m_CurveDuration));
            }

            public override string ToString()
            {
                return "Float (AnimationCurve)";
            }
        }

        /// <summary>
        /// Tweens from one float to another over time.
        /// </summary>
        static public Tween Float(float inStart, float inEnd, Action<float> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Float(inStart, inEnd, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from one float to another over time.
        /// </summary>
        static public Tween Float(float inStart, float inEnd, Action<float> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Float(inStart, inEnd, inSetter), inSettings);
        }

        /// <summary>
        /// Tweens over the given curve over time.
        /// </summary>
        static public Tween FloatCurve(AnimationCurve inCurve, Action<float> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_FloatCurve(inCurve, inSetter), inTime);
        }

        /// <summary>
        /// Tweens over the given curve over time.
        /// </summary>
        static public Tween FloatCurve(AnimationCurve inCurve, Action<float> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_FloatCurve(inCurve, inSetter), inSettings);
        }

        /// <summary>
        /// Tweens from zero to one over time.
        /// </summary>
        static public Tween ZeroToOne(Action<float> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Float(0, 1, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from zero to one over time.
        /// </summary>
        static public Tween ZeroToOne(Action<float> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Float(0, 1, inSetter), inSettings);
        }

        /// <summary>
        /// Tweens from one to zero over time.
        /// </summary>
        static public Tween OneToZero(Action<float> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Float(1, 0, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from one to zero over time.
        /// </summary>
        static public Tween OneToZero(Action<float> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Float(1, 0, inSetter), inSettings);
        }

        #endregion

        #region Integers

        private class TweenData_Value_Int : TweenData_Value<int>
        {
            public TweenData_Value_Int(int inStart, int inEnd, Action<int> inSetter)
                : base(inStart, inEnd, inSetter)
            { }

            public override void ApplyTween(float inPercent)
            {
                m_Setter((int)(m_Start + (m_End - m_Start) * inPercent));
            }
        }

        /// <summary>
        /// Tweens from one int to another over time.
        /// </summary>
        static public Tween Int(int inStart, int inEnd, Action<int> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Int(inStart, inEnd, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from one int to another over time.
        /// </summary>
        static public Tween Int(int inStart, int inEnd, Action<int> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Int(inStart, inEnd, inSetter), inSettings);
        }

        #endregion

        #region Vector2

        private class TweenData_Value_Vector2 : TweenData_Value<Vector2>
        {
            public TweenData_Value_Vector2(Vector2 inStart, Vector2 inEnd, Action<Vector2> inSetter)
                : base(inStart, inEnd, inSetter)
            { }

            public override void ApplyTween(float inPercent)
            {
                m_Setter(Vector2.LerpUnclamped(m_Start, m_End, inPercent));
            }
        }

        /// <summary>
        /// Tweens from one Vector2 to another over time.
        /// </summary>
        static public Tween Vector(Vector2 inStart, Vector2 inEnd, Action<Vector2> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Vector2(inStart, inEnd, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from one Vector2 to another over time.
        /// </summary>
        static public Tween Vector(Vector2 inStart, Vector2 inEnd, Action<Vector2> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Vector2(inStart, inEnd, inSetter), inSettings);
        }

        #endregion

        #region Vector3

        private class TweenData_Value_Vector3 : TweenData_Value<Vector3>
        {
            public TweenData_Value_Vector3(Vector3 inStart, Vector3 inEnd, Action<Vector3> inSetter)
                : base(inStart, inEnd, inSetter)
            { }

            public override void ApplyTween(float inPercent)
            {
                m_Setter(Vector3.LerpUnclamped(m_Start, m_End, inPercent));
            }
        }

        /// <summary>
        /// Tweens from one Vector3 to another over time.
        /// </summary>
        static public Tween Vector(Vector3 inStart, Vector3 inEnd, Action<Vector3> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Vector3(inStart, inEnd, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from one Vector3 to another over time.
        /// </summary>
        static public Tween Vector(Vector3 inStart, Vector3 inEnd, Action<Vector3> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Vector3(inStart, inEnd, inSetter), inSettings);
        }

        #endregion

        #region Vector4

        private class TweenData_Value_Vector4 : TweenData_Value<Vector4>
        {
            public TweenData_Value_Vector4(Vector4 inStart, Vector4 inEnd, Action<Vector4> inSetter)
                : base(inStart, inEnd, inSetter)
            { }

            public override void ApplyTween(float inPercent)
            {
                m_Setter(Vector4.LerpUnclamped(m_Start, m_End, inPercent));
            }
        }

        /// <summary>
        /// Tweens from one Vector4 to another over time.
        /// </summary>
        static public Tween Vector(Vector4 inStart, Vector4 inEnd, Action<Vector4> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Vector4(inStart, inEnd, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from one Vector4 to another over time.
        /// </summary>
        static public Tween Vector(Vector4 inStart, Vector4 inEnd, Action<Vector4> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Vector4(inStart, inEnd, inSetter), inSettings);
        }

        #endregion

        #region Rect

        private class TweenData_Value_Rect : TweenData_Value<Rect>
        {
            public TweenData_Value_Rect(Rect inStart, Rect inEnd, Action<Rect> inSetter)
                : base(inStart, inEnd, inSetter)
            { }

            public override void ApplyTween(float inPercent)
            {
                Rect final = new UnityEngine.Rect(
                    m_Start.x + (m_End.x - m_Start.x) * inPercent,
                    m_Start.y + (m_End.y - m_Start.y) * inPercent,
                    m_Start.width + (m_End.width - m_Start.width) * inPercent,
                    m_Start.height + (m_End.height - m_Start.height) * inPercent);
                m_Setter(final);
            }
        }

        /// <summary>
        /// Tweens from one Rect to another over time.
        /// </summary>
        static public Tween Rect(Rect inStart, Rect inEnd, Action<Rect> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Rect(inStart, inEnd, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from one Rect to another over time.
        /// </summary>
        static public Tween Rect(Rect inStart, Rect inEnd, Action<Rect> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Rect(inStart, inEnd, inSetter), inSettings);
        }

        #endregion

        #region RectOffset

        private class TweenData_Value_RectOffset : TweenData_Value<RectOffset>
        {
            public TweenData_Value_RectOffset(RectOffset inStart, RectOffset inEnd, Action<RectOffset> inSetter)
                : base(inStart, inEnd, inSetter)
            { }

            public override void ApplyTween(float inPercent)
            {
                RectOffset final = new UnityEngine.RectOffset(
                    (int)(m_Start.left + (m_End.left - m_Start.left) * inPercent),
                    (int)(m_Start.right + (m_End.right - m_Start.right) * inPercent),
                    (int)(m_Start.top + (m_End.top - m_Start.top) * inPercent),
                    (int)(m_Start.bottom + (m_End.bottom - m_Start.bottom) * inPercent));
                m_Setter(final);
            }
        }

        /// <summary>
        /// Tweens from one RectOffset to another over time.
        /// </summary>
        static public Tween RectOffset(RectOffset inStart, RectOffset inEnd, Action<RectOffset> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_RectOffset(inStart, inEnd, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from one RectOffset to another over time.
        /// </summary>
        static public Tween RectOffset(RectOffset inStart, RectOffset inEnd, Action<RectOffset> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_RectOffset(inStart, inEnd, inSetter), inSettings);
        }

        #endregion

        #region Quaternion

        private class TweenData_Value_Quaternion : TweenData_Value<Quaternion>
        {
            public TweenData_Value_Quaternion(Quaternion inStart, Quaternion inEnd, Action<Quaternion> inSetter)
                : base(inStart, inEnd, inSetter)
            { }

            public override void ApplyTween(float inPercent)
            {
                m_Setter(UnityEngine.Quaternion.SlerpUnclamped(m_Start, m_End, inPercent));
            }
        }

        /// <summary>
        /// Tweens from one Quaternion to another over time.
        /// </summary>
        static public Tween Quaternion(Quaternion inStart, Quaternion inEnd, Action<Quaternion> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Quaternion(inStart, inEnd, inSetter), inTime);
        }

        /// <summary>
        /// Tweens from one Quaternion to another over time.
        /// </summary>
        static public Tween Quaternion(Quaternion inStart, Quaternion inEnd, Action<Quaternion> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Quaternion(inStart, inEnd, inSetter), inSettings);
        }

        #endregion

        #region Color

        private class TweenData_Value_Color : TweenData_Value<Color>
        {
            private ColorUpdate m_Update;

            public TweenData_Value_Color(Color inStart, Color inEnd, Action<Color> inSetter, ColorUpdate inUpdate)
                : base(inStart, inEnd, inSetter)
            {
                m_Update = inUpdate;
            }

            public override void ApplyTween(float inPercent)
            {
                Color final = UnityEngine.Color.LerpUnclamped(m_Start, m_End, inPercent);
                if (m_Update == ColorUpdate.PreserveAlpha)
                    final.a = m_Start.a;
                m_Setter(final);
            }
        }

        private class TweenData_Value_Gradient : ITweenData
        {
            private Gradient m_Gradient;
            protected Action<Color> m_Setter;

            public TweenData_Value_Gradient(Gradient inGradient, Action<Color> inSetter)
            {
                m_Gradient = inGradient;
                m_Setter = inSetter;
            }

            public void OnTweenStart() { }
            public void OnTweenEnd() { }

            public void ApplyTween(float inPercent)
            {
                m_Setter(m_Gradient.Evaluate(inPercent));
            }

            public override string ToString()
            {
                return "Color (Gradient)";
            }
        }

        /// <summary>
        /// Tweens from one Color to another over time.
        /// </summary>
        static public Tween Color(Color inStart, Color inEnd, Action<Color> inSetter, float inTime, ColorUpdate inUpdateMode = ColorUpdate.FullColor)
        {
            return Tween.Create(new TweenData_Value_Color(inStart, inEnd, inSetter, inUpdateMode), inTime);
        }

        /// <summary>
        /// Tweens from one Color to another over time.
        /// </summary>
        static public Tween Color(Color inStart, Color inEnd, Action<Color> inSetter, TweenSettings inSettings, ColorUpdate inUpdateMode = ColorUpdate.FullColor)
        {
            return Tween.Create(new TweenData_Value_Color(inStart, inEnd, inSetter, inUpdateMode), inSettings);
        }

        /// <summary>
        /// Tweens over the given Gradient over time.
        /// </summary>
        static public Tween Gradient(Gradient inGradient, Action<Color> inSetter, float inTime)
        {
            return Tween.Create(new TweenData_Value_Gradient(inGradient, inSetter), inTime);
        }

        /// <summary>
        /// Tweens over the given Gradient over time.
        /// </summary>
        static public Tween Gradient(Gradient inGradient, Action<Color> inSetter, TweenSettings inSettings)
        {
            return Tween.Create(new TweenData_Value_Gradient(inGradient, inSetter), inSettings);
        }

        #endregion
    }
}

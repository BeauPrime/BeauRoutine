/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    TweenUtil.cs
 * Purpose: Easing and interpolation functions for dealing with
 *          Tweens, curves, and interpolation over delta time.
 */

// Easing functions ported from Robert Penner's easing functions.
// http://robertpenner.com/easing
// Open source under the BSD License.
//
// Copyright © 2001 Robert Penner
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// - Neither the name of the author nor the names of contributors may be used to endorse
// or promote products derived from this software without specific prior written permission.
// - THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;

namespace BeauRoutine
{
    /// <summary>
    /// Contains helper functions for tweening.
    /// </summary>
    public static class TweenUtil
    {
        #region Curves

        // Magic numbers!
        private const float BACK_MULTIPLIER = 2.70158f;
        private const float BACK_SUBTRACT = 1.70158f;

        private const float ELASTIC_DIVIDER = 0.3f;
        private const float ELASTIC_SUBTRACT = ELASTIC_DIVIDER / 4.0f;

        /// <summary>
        /// Evalutes a premade easing function.
        /// </summary>
        /// <param name="inCurve">The curve to evaluate.</param>
        /// <param name="t">Percent from 0 - 1 to evaluate.</param>
        static public float Evaluate(this Curve inCurve, float t)
        {
            switch (inCurve)
            {
                case Curve.Smooth:
                    return t * t * (3 - 2 * t);

                case Curve.QuadIn:
                    return t * t;
                case Curve.QuadOut:
                    t = 1 - t;
                    return 1 - (t * t);
                case Curve.QuadInOut:
                    return EvaluateCombined(Curve.QuadIn, Curve.QuadOut, t);

                case Curve.CubeIn:
                    return t * t * t;
                case Curve.CubeOut:
                    t = 1 - t;
                    return 1 - (t * t * t);
                case Curve.CubeInOut:
                    return EvaluateCombined(Curve.CubeIn, Curve.CubeOut, t);

                case Curve.QuartIn:
                    return t * t * t * t;
                case Curve.QuartOut:
                    t = 1 - t;
                    return 1 - (t * t * t * t);
                case Curve.QuartInOut:
                    return EvaluateCombined(Curve.QuartIn, Curve.QuartOut, t);

                case Curve.QuintIn:
                    return t * t * t * t * t;
                case Curve.QuintOut:
                    t = 1 - t;
                    return 1 - (t * t * t * t * t);
                case Curve.QuintInOut:
                    return EvaluateCombined(Curve.QuintIn, Curve.QuintOut, t);

                case Curve.BackIn:
                    return t * t * (BACK_MULTIPLIER * t - BACK_SUBTRACT);
                case Curve.BackOut:
                    t = 1 - t;
                    return 1 - (t * t * (BACK_MULTIPLIER * t - BACK_SUBTRACT));
                case Curve.BackInOut:
                    return EvaluateCombined(Curve.BackIn, Curve.BackOut, t);

                case Curve.SineIn:
                    return -(float) Math.Cos(Math.PI * t * 0.5) + 1;
                case Curve.SineOut:
                    return (float) Math.Sin(Math.PI * t * 0.5);
                case Curve.SineInOut:
                    return -(float) Math.Cos(Math.PI * t) * 0.5f + 0.5f;

                case Curve.CircleIn:
                    return 1 - (float) Math.Sqrt(1 - (t * t));
                case Curve.CircleOut:
                    return (float) Math.Sqrt((2 - t) * t);
                case Curve.CircleInOut:
                    return EvaluateCombined(Curve.CircleIn, Curve.CircleOut, t);

                case Curve.BounceIn:
                    {
                        double tt = (1 - t);
                        if (tt < 1 / 2.75f)
                        {
                            return (float) (1 - (7.5625 * tt * tt));
                        }
                        else if (tt < 2 / 2.75)
                        {
                            tt -= (1.5 / 2.75);
                            return (float) (1 - (7.5625 * (tt * tt) + 0.75));
                        }
                        else if (tt < 2.5 / 2.75)
                        {
                            tt -= (2.25 / 2.75);
                            return (float) (1 - (7.5625 * (tt * tt) + 0.9375));
                        }
                        else
                        {
                            tt -= (2.625 / 2.75);
                            return (float) (1 - (7.5625 * (tt * tt) + 0.984375));
                        }
                    }
                case Curve.BounceOut:
                    {
                        double tt = t;
                        if (tt < 1 / 2.75)
                        {
                            return (float) (7.5625 * tt * tt);
                        }
                        else if (tt < 2 / 2.75)
                        {
                            tt -= (1.5 / 2.75);
                            return (float) (7.5625f * (tt * tt) + 0.75f);
                        }
                        else if (tt < 2.5 / 2.75f)
                        {
                            tt -= (2.25 / 2.75);
                            return (float) (7.5625 * (tt * tt) + 0.9375);
                        }
                        else
                        {
                            tt -= (2.625 / 2.75);
                            return (float) (7.5625 * (tt * tt) + 0.984375);
                        }
                    }
                case Curve.BounceInOut:
                    return EvaluateCombined(Curve.BounceIn, Curve.BounceOut, t);

                case Curve.ElasticIn:
                    if (t <= 0 || t >= 1)
                        return t;

                    float inverse = t - 1;
                    return (float) (-1 * Math.Pow(2, 10 * inverse) * Math.Sin((inverse - ELASTIC_SUBTRACT) * (2 * Math.PI) / ELASTIC_DIVIDER));
                case Curve.ElasticOut:
                    if (t <= 0 || t >= 1)
                        return t;

                    return (float) (1 * Math.Pow(2, -10 * t) * Math.Sin((t - ELASTIC_SUBTRACT) * (2 * Math.PI) / ELASTIC_DIVIDER)) + 1;
                case Curve.ElasticInOut:
                    return EvaluateCombined(Curve.ElasticIn, Curve.ElasticOut, t);

                case Curve.Linear:
                default:
                    return t;
            }
        }

        // Evaluates two curves, switching between them halfway through
        static public float EvaluateCombined(Curve inA, Curve inB, float t)
        {
            return (t <= 0.5f ?
                0.5f * Evaluate(inA, t * 2) :
                0.5f + 0.5f * Evaluate(inB, t * 2 - 1)
            );
        }

        /// <summary>
        /// Evaluates a mirrored version of the curve.
        /// </summary>
        static public float EvaluateMirrored(this Curve inCurve, float t)
        {
            return 1 - Evaluate(inCurve, 1 - t);
        }

        #endregion // Curves

        #region Time-Independent Lerp

        static private float s_DefaultLerpPeriod = 1.0f;

        /// <summary>
        /// Sets the default lerp period.
        /// Used for the Lerp and LerpDecay functions.
        /// </summary>
        static public void SetDefaultLerpPeriod(float inPeriod)
        {
            s_DefaultLerpPeriod = inPeriod;
        }

        /// <summary>
        /// Sets the default lerp period to 1 / framerate.
        /// Used for the Lerp and LerpDecay functions.
        /// </summary>
        static public void SetDefaultLerpPeriodByFramerate(float inFrameRate)
        {
            s_DefaultLerpPeriod = 1f / inFrameRate;
        }

        /// <summary>
        /// Returns a lerp percent for interpolating over
        /// given percent in a given period of time, scaled
        /// for delta time.
        /// </summary>
        static public float Lerp(float inPercent)
        {
            return (float) (1.0f - Math.Exp(-inPercent * BeauRoutine.Routine.DeltaTime / s_DefaultLerpPeriod));
        }

        /// <summary>
        /// Returns a lerp percent for interpolating over
        /// given percent in a given period of time, scaled
        /// for delta time.
        /// </summary>
        static public float Lerp(float inPercent, float inPeriod)
        {
            return (float) (1.0f - Math.Exp(-inPercent * BeauRoutine.Routine.DeltaTime / inPeriod));
        }

        /// <summary>
        /// Returns a lerp percent for interpolating over
        /// given percent in a given period of time, scaled
        /// for the given delta time.
        /// </summary>
        static public float Lerp(float inPercent, float inPeriod, float inDeltaTime)
        {
            return (float) (1.0f - Math.Exp(-inPercent * inDeltaTime / inPeriod));
        }

        /// <summary>
        /// Returns a lerp percent for decaying towards 0 by a
        /// given percent in a given period of time, scaled
        /// for delta time.
        /// </summary>
        static public float LerpDecay(float inPercent)
        {
            return (float) Math.Exp(-inPercent * BeauRoutine.Routine.DeltaTime / s_DefaultLerpPeriod);
        }

        /// <summary>
        /// Returns a lerp percent for decaying towards 0 by a
        /// given percent in a given period of time, scaled
        /// for delta time.
        /// </summary>
        static public float LerpDecay(float inPercent, float inPeriod)
        {
            return (float) Math.Exp(-inPercent * BeauRoutine.Routine.DeltaTime / inPeriod);
        }

        /// <summary>
        /// Returns a lerp percent for decaying towards 0 by a
        /// given percent in a given period of time, scaled
        /// for the given delta time.
        /// </summary>
        static public float LerpDecay(float inPercent, float inPeriod, float inDeltaTime)
        {
            return (float) (Math.Exp(-inPercent * inDeltaTime / inPeriod));
        }

        #endregion // Time-Independent Lerp

        #region Shortcuts

        /// <summary>
        /// Starts a Tween as a Routine.
        /// </summary>
        static public Routine Play(this Tween inTween)
        {
            return Routine.Start(inTween);
        }

        /// <summary>
        /// Starts a Tween as a Routine, hosted by the given host.
        /// </summary>
        static public Routine Play(this Tween inTween, UnityEngine.MonoBehaviour inHost)
        {
            return Routine.Start(inHost, inTween);
        }

        #endregion // Shortcuts
    }
}
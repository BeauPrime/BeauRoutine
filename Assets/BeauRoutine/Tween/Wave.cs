/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Wave.cs
 * Purpose: Set of wave functions, along with a tuple
 *          of wave function to frequency.
*/

using System;

namespace BeauRoutine
{
    /// <summary>
    /// Oscillates percentages over time.
    /// </summary>
    public struct Wave
    {
        /// <summary>
        /// Function type.
        /// </summary>
        public readonly Function Type;

        /// <summary>
        /// Wave frequency.
        /// </summary>
        public readonly float Frequency;

        /// <summary>
        /// Default Wave. Does not affect the percent.
        /// </summary>
        static public readonly Wave None = default(Wave);

        /// <summary>
        /// Creates a new Wave.
        /// </summary>
        /// <param name="inMode">How the value will be oscillated.</param>
        /// <param name="inFrequency">The number of waves.</param>
        public Wave(Function inMode, float inFrequency)
        {
            Type = inMode;
            Frequency = inFrequency;
        }

        /// <summary>
        /// Returns a percent adjusted on a wave.
        /// </summary>
        public float Evaluate(float inPercent)
        {
            switch(Type)
            {
                case Function.Sin:
                    return Sin(inPercent, Frequency);
                case Function.SinFade:
                    return SinFade(inPercent, Frequency);
                case Function.Cos:
                    return Cos(inPercent, Frequency);
                case Function.CosFade:
                    return CosFade(inPercent, Frequency);
                case Function.Square:
                    return Square(inPercent, Frequency);
                case Function.SquareFade:
                    return SquareFade(inPercent, Frequency);
                case Function.Triangle:
                    return Triangle(inPercent, Frequency);
                case Function.TriangleFade:
                    return TriangleFade(inPercent, Frequency);
                case Function.TriangleOdd:
                    return TriangleOdd(inPercent, Frequency);
                case Function.TriangleOddFade:
                    return TriangleOddFade(inPercent, Frequency);

                case Function.None:
                default:
                    return inPercent;
            }
        }

        /// <summary>
        /// Wave functions.
        /// </summary>
        public enum Function : byte
        {
            /// <summary>
            /// Does not oscillate.
            /// </summary>
            None = 0,

            /// <summary>
            /// Oscillates in a sine wave.
            /// </summary>
            Sin,

            /// <summary>
            /// Oscillates and fades in a sine wave.
            /// </summary>
            SinFade,

            /// <summary>
            /// Oscillates in a cosine wave.
            /// </summary>
            Cos,
            
            /// <summary>
            /// Oscillates and fades in a cosine wave.
            /// </summary>
            CosFade,

            /// <summary>
            /// Oscillates in a triangle wave.
            /// </summary>
            Triangle,

            /// <summary>
            /// Oscillates in a triangle wave, starting at 1.
            /// </summary>
            TriangleOdd,

            /// <summary>
            /// Oscillates and fades in a triangle wave.
            /// </summary>
            TriangleFade,

            /// <summary>
            /// Oscillates and fades in a triangle wave, starting at 1.
            /// </summary>
            TriangleOddFade,

            /// <summary>
            /// Oscillates in a square wave.
            /// </summary>
            Square,

            /// <summary>
            /// Oscillates and fades in a square wave.
            /// </summary>
            SquareFade,
        }

        #region Math

        private const double TWO_PI = Math.PI * 2;

        /// <summary>
        /// Sine wave.
        /// </summary>
        static public float Sin(float inPercent, float inFrequency)
        {
            return (float)Math.Sin(inPercent * TWO_PI * inFrequency);
        }

        /// <summary>
        /// Fading sine wave.
        /// </summary>
        static public float SinFade(float inPercent, float inFrequency)
        {
            return (float)Math.Sin(inPercent * TWO_PI * inFrequency) * (1 - inPercent);
        }

        /// <summary>
        /// Cosine wave.
        /// </summary>
        static public float Cos(float inPercent, float inFrequency)
        {
            return (float)Math.Cos(inPercent * TWO_PI * inFrequency);
        }

        /// <summary>
        /// Fading cosine wave.
        /// </summary>
        static public float CosFade(float inPercent, float inFrequency)
        {
            return (float)Math.Cos(inPercent * TWO_PI * inFrequency) * (1 - inPercent);
        }

        /// <summary>
        /// Triangle wave.
        /// </summary>
        static public float Triangle(float inPercent, float inFrequency)
        {
            float x = ((inPercent * inFrequency) + 0.25f) % 1;
            return 4 * Math.Abs(x - 0.5f) - 1;
        }

        /// <summary>
        /// Fading triangle wave.
        /// </summary>
        static public float TriangleFade(float inPercent, float inFrequency)
        {
            float x = ((inPercent * inFrequency) + 0.25f) % 1;
            return (4 * Math.Abs(x - 0.5f) - 1) * (1 - inPercent);
        }

        /// <summary>
        /// Fading odd triangle wave.
        /// </summary>
        static public float TriangleOdd(float inPercent, float inFrequency)
        {
            float x = (inPercent * inFrequency) % 1;
            return 4 * Math.Abs(x - 0.5f) - 1;
        }

        /// <summary>
        /// Fading odd triangle wave.
        /// </summary>
        static public float TriangleOddFade(float inPercent, float inFrequency)
        {
            float x = (inPercent * inFrequency) % 1;
            return (4 * Math.Abs(x - 0.5f) - 1) * (1 - inPercent);
        }

        /// <summary>
        /// Square wave.
        /// </summary>
        static public float Square(float inPercent, float inFrequency)
        {
            float x = (inPercent * inFrequency) % 1;
            return Math.Sign(x - 0.5f);
        }

        /// <summary>
        /// Fading square wave.
        /// </summary>
        static public float SquareFade(float inPercent, float inFrequency)
        {
            float x = (inPercent * inFrequency) % 1;
            return Math.Sign(x - 0.5f) * (1 - inPercent);
        }

        #endregion
    }
}

/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Nov 2018
 * 
 * File:    AngularKinematicAnimation.cs
 * Purpose: Randomized angular kinematic animation.
 */

using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Angular kinematic animation.
    /// </summary>
    [Serializable]
    public class AngularKinematicAnimation
    {
        /// <summary>
        /// Callback delegate.
        /// </summary>
        public delegate void Callback(Transform inTransform, Vector3 inRotation, Vector3 inVelocity);

        [SerializeField, Tooltip("Velocity, in degrees per second")]
        public Vector3Range Velocity;

        [SerializeField, Tooltip("Acceleration, in degrees per second squared")]
        public Vector3Range Acceleration;

        [SerializeField, Tooltip("Damping, in percentage per second")]
        public float Damping = 0;

        /// <summary>
        /// Runs basic angular kinematic simulation on the given Transform for the given duration.
        /// This will modify local/world euler angles, depending on the given space.
        /// Angle CANNOT be modified mid-simulation.
        /// </summary>
        /// <param name="inTransform">Transform to simulate.</param>
        /// <param name="inDuration">Duration, in seconds</param>
        /// <param name="inSpace">Space to simulate in.</param>
        /// <param name="inCallback">Optional callback for per-frame rotation and velocity information.</param>
        /// <param name="inRandom">Optional Random for generating the starting state.</param>
        public IEnumerator Simulate(Transform inTransform, float inDuration, Space inSpace = Space.Self, Callback inCallback = null, System.Random inRandom = null)
        {
            Runtime runtime = new Runtime();
            runtime.Duration = inDuration;
            runtime.Space = inSpace;
            runtime.Callback = inCallback;

            if (inRandom != null)
            {
                runtime.Velocity = Velocity.Generate(inRandom);
                runtime.Acceleration = Acceleration.Generate(inRandom);
            }
            else
            {
                runtime.Velocity = Velocity.Generate();
                runtime.Acceleration = Acceleration.Generate();
            }

            runtime.Damping = Damping;

            return Simulate(inTransform, runtime);
        }

        #region Runtime

        private class Runtime
        {
            public float Duration;
            public Space Space;
            public Callback Callback;

            public Vector3 Velocity;
            public Vector3 Acceleration;
            public float Damping;
        }

        static private IEnumerator Simulate(Transform inTransform, Runtime inRuntime)
        {
            Vector3 currentEuler = inTransform.GetRotation(Axis.XYZ, inRuntime.Space);
            Vector3 deltaEuler;
            float dt, dt2, damp;

            while (inRuntime.Duration > 0)
            {
                dt = Routine.DeltaTime;
                dt2 = dt * dt;
                damp = inRuntime.Damping == 0 ? 1 : TweenUtil.LerpDecay(inRuntime.Damping, 1, dt);

                deltaEuler.x = deltaEuler.y = deltaEuler.z = 0;
                VectorUtil.Add(ref deltaEuler, inRuntime.Velocity, dt);
                VectorUtil.Add(ref deltaEuler, inRuntime.Acceleration, 0.5f * dt2);

                VectorUtil.Add(ref currentEuler, deltaEuler);

                inTransform.SetRotation(currentEuler, Axis.XYZ, inRuntime.Space);

                VectorUtil.Add(ref inRuntime.Velocity, inRuntime.Acceleration, dt);
                VectorUtil.Multiply(ref inRuntime.Velocity, damp);

                inRuntime.Duration -= dt;

                if (inRuntime.Callback != null)
                    inRuntime.Callback(inTransform, currentEuler, inRuntime.Velocity);

                yield return null;
            }
        }

        #endregion // Runtime
    }
}
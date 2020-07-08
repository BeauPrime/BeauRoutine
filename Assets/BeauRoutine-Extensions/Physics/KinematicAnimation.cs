/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    7 Nov 2018
 * 
 * File:    KinematicAnimation.cs
 * Purpose: Randomized kinematic animation.
 */

using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine.Extensions
{
    /// <summary>
    /// Simple kinematic animation.
    /// </summary>
    [Serializable]
    public class KinematicAnimation
    {
        /// <summary>
        /// Callback delegate.
        /// </summary>
        public delegate void Callback(Transform inTransform, Vector3 inPosition, Vector3 inVelocity);

        [SerializeField, Tooltip("Velocity, in units per second")]
        public Vector3Range Velocity;

        [SerializeField, Tooltip("Acceleration, in units per second squared")]
        public Vector3Range Acceleration;

        [SerializeField, Tooltip("Gravity, in units per second squared")]
        public Vector3Range Gravity;

        [SerializeField, Tooltip("Damping, in percentage per second")]
        public float Damping;

        /// <summary>
        /// Runs basic kinematic simulation on the given Transform for the given duration.
        /// This will modify local/world position, depending on the given space.
        /// Position can be modified mid-simulation.
        /// </summary>
        /// <param name="inTransform">Transform to simulate.</param>
        /// <param name="inDuration">Duration, in seconds</param>
        /// <param name="inSpace">Space to simulate in.</param>
        /// <param name="inCallback">Optional callback for per-frame position and velocity information.</param>
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
                runtime.Gravity = Gravity.Generate(inRandom);
            }
            else
            {
                runtime.Velocity = Velocity.Generate();
                runtime.Acceleration = Acceleration.Generate();
                runtime.Gravity = Gravity.Generate();
            }

            runtime.Damping = Damping;

            return Simulate(inTransform, runtime);
        }

        /// <summary>
        /// Runs basic kinematic simulation on the given RectTransform for the given duration.
        /// This will modify anchoredPosition3D.
        /// Position can be modified mid-simulation.
        /// </summary>
        /// <param name="inTransform">RectTransform to simulate.</param>
        /// <param name="inDuration">Duration, in seconds</param>
        /// <param name="inCallback">Optional callback for per-frame position and velocity information.</param>
        /// <param name="inRandom">Optional Random for generating the starting state.</param>
        public IEnumerator SimulateAnchorPos(RectTransform inTransform, float inDuration, Callback inCallback = null, System.Random inRandom = null)
        {
            Runtime runtime = new Runtime();
            runtime.Duration = inDuration;
            runtime.Space = Space.Self;
            runtime.Callback = inCallback;

            if (inRandom != null)
            {
                runtime.Velocity = Velocity.Generate(inRandom);
                runtime.Acceleration = Acceleration.Generate(inRandom);
                runtime.Gravity = Gravity.Generate(inRandom);
            }
            else
            {
                runtime.Velocity = Velocity.Generate();
                runtime.Acceleration = Acceleration.Generate();
                runtime.Gravity = Gravity.Generate();
            }

            runtime.Damping = Damping;

            return SimulateAnchorPos(inTransform, runtime);
        }

        #region Runtime

        private class Runtime
        {
            public float Duration;
            public Space Space;
            public Callback Callback;

            public Vector3 Velocity;
            public Vector3 Acceleration;
            public Vector3 Gravity;

            public float Damping;
        }

        static private IEnumerator Simulate(Transform inTransform, Runtime inRuntime)
        {
            Vector3 currentPos;
            Vector3 deltaPos;
            float dt, dt2, damp;

            while (inRuntime.Duration > 0)
            {
                dt = Routine.DeltaTime;
                dt2 = dt * dt;
                damp = inRuntime.Damping == 0 ? 1 : TweenUtil.LerpDecay(inRuntime.Damping, 1, dt);

                deltaPos.x = deltaPos.y = deltaPos.z = 0;
                VectorUtil.Add(ref deltaPos, inRuntime.Velocity, dt);
                VectorUtil.Add(ref deltaPos, inRuntime.Acceleration, 0.5f * dt2);
                VectorUtil.Add(ref deltaPos, inRuntime.Gravity, 0.5f * dt2);

                currentPos = inTransform.GetPosition(Axis.XYZ, inRuntime.Space);
                VectorUtil.Add(ref currentPos, deltaPos);

                inTransform.SetPosition(currentPos, Axis.XYZ, inRuntime.Space);

                VectorUtil.Add(ref inRuntime.Velocity, inRuntime.Acceleration, dt);
                VectorUtil.Add(ref inRuntime.Velocity, inRuntime.Gravity, dt);
                VectorUtil.Multiply(ref inRuntime.Velocity, damp);

                inRuntime.Duration -= dt;

                if (inRuntime.Callback != null)
                    inRuntime.Callback(inTransform, currentPos, inRuntime.Velocity);
                yield return null;
            }
        }

        static private IEnumerator SimulateAnchorPos(RectTransform inTransform, Runtime inRuntime)
        {
            Vector3 currentPos;
            Vector3 deltaPos;
            float dt, dt2, damp;

            while (inRuntime.Duration > 0)
            {
                dt = Routine.DeltaTime;
                dt2 = dt * dt;
                damp = inRuntime.Damping == 0 ? 1 : TweenUtil.LerpDecay(inRuntime.Damping, 1, dt);

                deltaPos.x = deltaPos.y = deltaPos.z = 0;
                VectorUtil.Add(ref deltaPos, inRuntime.Velocity, dt);
                VectorUtil.Add(ref deltaPos, inRuntime.Acceleration, 0.5f * dt2);
                VectorUtil.Add(ref deltaPos, inRuntime.Gravity, 0.5f * dt2);

                currentPos = inTransform.anchoredPosition3D;
                VectorUtil.Add(ref currentPos, deltaPos);

                inTransform.anchoredPosition3D = currentPos;

                VectorUtil.Add(ref inRuntime.Velocity, inRuntime.Acceleration, dt);
                VectorUtil.Add(ref inRuntime.Velocity, inRuntime.Gravity, dt);
                VectorUtil.Multiply(ref inRuntime.Velocity, damp);

                inRuntime.Duration -= dt;
                
                if (inRuntime.Callback != null)
                    inRuntime.Callback(inTransform, currentPos, inRuntime.Velocity);
                yield return null;
            }
        }

        #endregion // Runtime
    }
}
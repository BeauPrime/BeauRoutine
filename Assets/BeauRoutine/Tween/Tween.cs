/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Tween.cs
 * Purpose: Class that executes a tween, along with a builder-style
 *          interface for modifying the timing, easing, and looping.
 *          
 * Notes:   Tweens must be run inside a Routine to be evaluated.
 */

using System;
using System.Collections;
using BeauRoutine.Internal;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Runs a percentage-based tween from one value to another.
    /// </summary>
    public sealed partial class Tween : IEnumerator, IDisposable
    {
        public const int DEFAULT_POOL_SIZE = 32;

        private const int LOOPING_FOREVER = -1;

        private enum State : byte
        {
            Begin,
            Run,
            End
        }

        private enum LoopMode : byte
        {
            Single,
            Loop,
            Yoyo,
            YoyoLoop
        }

        private enum CancelMode : byte
        {
            Nothing,
            Revert,
            RevertNoWave,
            ForceEnd,
            ForceEndNoWave
        }

        private enum StartMode : byte
        {
            Restart,
            Randomize
        }

        // Tween settings
        private LoopMode m_Mode;
        private Curve m_Curve;
        private CancelMode m_Cancel;
        private AnimationCurve m_AnimCurve;
        private Wave m_WaveFunc;
        private int m_NumLoops;
        private StartMode m_StartMode;
        private float m_StartTime;

        // Flags
        private bool m_MirrorCurve;
        private bool m_FromMode;
        private bool m_Reversed;
        private bool m_Instant;

        // Events
        private Action m_OnStart;
        private Action<float> m_OnUpdate;
        private Action m_OnComplete;

        // State settings
        private float m_CurrentPercent;
        private float m_PercentIncrement;
        private State m_State = State.Begin;

        // Current data
        private ITweenData m_TweenData;

        internal Tween() { }

        private Tween SetData(ITweenData inData)
        {
            m_TweenData = inData;
            return this;
        }

        #region Looping

        /// <summary>
        /// Sets the tween to only play once.
        /// Percent will run from [0 -> 1].
        /// Default behavior.
        /// </summary>
        public Tween Once()
        {
            m_Mode = LoopMode.Single;
            m_NumLoops = 0;
            return this;
        }

        /// <summary>
        /// Sets the tween to loop forever.
        /// Percent will run from [0 -> 1] and loop.
        /// </summary>
        public Tween Loop()
        {
            m_Mode = LoopMode.Loop;
            m_NumLoops = LOOPING_FOREVER;
            return this;
        }

        /// <summary>
        /// Sets the tween to loop the given number of times.
        /// Percent will run from [0 -> 1] and loop.
        /// </summary>
        public Tween Loop(int inNumLoops)
        {
            m_Mode = LoopMode.Loop;
            m_NumLoops = inNumLoops < 1 ? 1 : inNumLoops;
            return this;
        }

        /// <summary>
        /// Sets the tween to yoyo once.
        /// Percent will run from [0 -> 1 -> 0].
        /// </summary>
        public Tween Yoyo(bool inbMirrored = false)
        {
            m_Mode = LoopMode.Yoyo;
            m_NumLoops = 0;
            m_MirrorCurve = inbMirrored;
            return this;
        }

        /// <summary>
        /// Sets the tween to yoyo and loop forever.
        /// Percent will run from [0 -> 1 -> 0] and loop.
        /// </summary>
        public Tween YoyoLoop(bool inbMirrored = false)
        {
            m_Mode = LoopMode.YoyoLoop;
            m_NumLoops = LOOPING_FOREVER;
            m_MirrorCurve = inbMirrored;
            return this;
        }

        /// <summary>
        /// Sets the tween to yoyo and loop the given number of times.
        /// Percent will run from [0 -> 1 -> 0] and loop.
        /// </summary>
        public Tween YoyoLoop(int inNumLoops, bool inbMirrored = false)
        {
            m_Mode = LoopMode.YoyoLoop;
            m_NumLoops = inNumLoops < 1 ? 1 : inNumLoops;
            m_MirrorCurve = inbMirrored;
            return this;
        }

        #endregion

        #region Time/Percent

        /// <summary>
        /// Sets the tween to use the given easing function.
        /// </summary>
        public Tween Ease(Curve inCurve)
        {
            m_Curve = inCurve;
            m_AnimCurve = null;
            return this;
        }

        /// <summary>
        /// Sets the tween to use the given custom easing curve.
        /// </summary>
        public Tween Ease(AnimationCurve inCurve)
        {
            m_Curve = Curve.Linear;
            m_AnimCurve = inCurve;
            return this;
        }

        /// <summary>
        /// Sets the tween to use the given wave function.
        /// </summary>
        public Tween Wave(Wave inWave)
        {
            m_WaveFunc = inWave;
            return this;
        }

        /// <summary>
        /// Sets the tween to use the given wave function.
        /// </summary>
        public Tween Wave(Wave.Function inMode, float inFrequency)
        {
            m_WaveFunc = new Wave(inMode, inFrequency);
            return this;
        }

        /// <summary>
        /// Sets the tween to take the given number of seconds.
        /// This is the time it takes to tween from [0 -> 1].
        /// </summary>
        public Tween Duration(float inTime)
        {
            m_PercentIncrement = inTime <= 0 ? 0 : 1.0f / inTime;
            m_Instant = (m_PercentIncrement <= 0);
            return this;
        }

        /// <summary>
        /// Sets the tween to animate from the target value
        /// to the current value.
        /// </summary>
        public Tween From()
        {
            m_FromMode = true;
            return this;
        }

        /// <summary>
        /// Sets the tween to animate to the target value.
        /// Default behavior.
        /// </summary>
        public Tween To()
        {
            m_FromMode = false;
            return this;
        }

        /// <summary>
        /// Randomizes the starting percent of this Tween.
        /// </summary>
        public Tween Randomize()
        {
            m_StartMode = StartMode.Randomize;
            return this;
        }

        /// <summary>
        /// Starts at the given time, in seconds, in the tween.
        /// </summary>
        public Tween StartsAt(float inTime)
        {
            m_StartMode = StartMode.Restart;
            m_StartTime = Mathf.Max(inTime, 0);
            return this;
        }

        /// <summary>
        /// Delays the tween by the given number of seconds.
        /// </summary>
        public Tween DelayBy(float inDelay)
        {
            m_StartMode = StartMode.Restart;
            m_StartTime = -Mathf.Max(inDelay, 0);
            return this;
        }

        /// <summary>
        /// Total duration of the tween, in seconds.
        /// </summary>
        public float TotalDuration()
        {
            float duration = m_PercentIncrement == 0 ? 0 : 1 / m_PercentIncrement;

            switch (m_Mode)
            {
                case LoopMode.Loop:
                    return m_NumLoops == LOOPING_FOREVER ? float.PositiveInfinity : duration * m_NumLoops;
                case LoopMode.Yoyo:
                    return duration * 2;
                case LoopMode.YoyoLoop:
                    return m_NumLoops == LOOPING_FOREVER ? float.PositiveInfinity : duration * m_NumLoops * 2;
                case LoopMode.Single:
                default:
                    return duration;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Registers a function to be called on start.
        /// </summary>
        public Tween OnStart(Action inStartFunction)
        {
            m_OnStart += inStartFunction;
            return this;
        }

        /// <summary>
        /// Registers a function to be called every update.
        /// </summary>
        public Tween OnUpdate(Action<float> inUpdateFunction)
        {
            m_OnUpdate += inUpdateFunction;
            return this;
        }

        /// <summary>
        /// Registers a function to be called on completion.
        /// </summary>
        public Tween OnComplete(Action inCompleteFunction)
        {
            m_OnComplete += inCompleteFunction;
            return this;
        }

        #endregion

        #region Cancel Behavior

        /// <summary>
        /// Will revert back to the starting value if the tween
        /// is cancelled mid-animation.
        /// </summary>
        public Tween RevertOnCancel(bool inbApplyWave = true)
        {
            m_Cancel = inbApplyWave ? CancelMode.Revert : CancelMode.RevertNoWave;
            return this;
        }

        /// <summary>
        /// Will force to the ending value if the tween
        /// is cancelled mid-animation.
        /// </summary>
        public Tween ForceOnCancel(bool inbApplyWave = true)
        {
            m_Cancel = inbApplyWave ? CancelMode.ForceEnd : CancelMode.ForceEndNoWave;
            return this;
        }

        /// <summary>
        /// Will keep the value where it ended up
        /// if the tween is cancelled mid-animation.
        /// Default behavior.
        /// </summary>
        public Tween KeepOnCancel()
        {
            m_Cancel = CancelMode.Nothing;
            return this;
        }

        #endregion

        #region Updates

        // Called when the tween starts.
        private void Start()
        {
            if (m_StartMode == StartMode.Randomize)
            {
                m_CurrentPercent = UnityEngine.Random.value;
                switch (m_Mode)
                {
                    case LoopMode.Yoyo:
                    case LoopMode.YoyoLoop:
                        m_Reversed = UnityEngine.Random.value < 0.5f;
                        break;

                    default:
                        m_Reversed = false;
                        break;
                }

                if (m_NumLoops > 0)
                    m_NumLoops = UnityEngine.Random.Range(1, m_NumLoops + 1);
            }
            else
            {
                m_CurrentPercent = 0;
                m_Reversed = false;
            }

            m_State = State.Run;
            m_TweenData.OnTweenStart();

            if (m_OnStart != null)
                m_OnStart();
        }

        // Called when the tween ends.
        private void End()
        {
            m_TweenData.OnTweenEnd();
            m_State = State.End;

            if (m_OnComplete != null)
                m_OnComplete();
        }

        private bool OnHitEnd(int inNumCycles, ref float ioBasePercent)
        {
            bool bAlive = true;
            switch (m_Mode)
            {
                case LoopMode.Loop:
                    bAlive = OnLoop(inNumCycles);
                    ioBasePercent = bAlive ? m_CurrentPercent : 1;
                    break;

                case LoopMode.YoyoLoop:
                    bAlive = OnYoyoLoop(inNumCycles);
                    ioBasePercent = bAlive ? m_CurrentPercent : 1;
                    break;

                case LoopMode.Yoyo:
                    bAlive = OnYoyo(inNumCycles);
                    ioBasePercent = bAlive ? m_CurrentPercent : 1;
                    break;

                case LoopMode.Single:
                default:
                    ioBasePercent = 1;
                    bAlive = false;
                    break;
            }
            return bAlive;
        }

        private bool OnLoop(int inNumLoops)
        {
            if (m_NumLoops >= 0)
            {
                m_NumLoops -= inNumLoops;
                return m_NumLoops > 0;
            }
            return true;
        }

        private bool OnYoyo(int inNumLoops)
        {
            int totalLoops = 2;
            totalLoops -= inNumLoops;
            if (m_Reversed)
                --totalLoops;

            m_Reversed = true;
            return totalLoops > 0;
        }

        private bool OnYoyoLoop(int inNumLoops)
        {
            bool nextReversed = (inNumLoops % 2) == 1 ? !m_Reversed : m_Reversed;

            // If the next time we reverse we'll be moving forward,
            // we're hitting the lower bound again
            if (!nextReversed)
            {
                if (m_NumLoops >= 0)
                {
                    m_NumLoops -= inNumLoops;
                    if (m_NumLoops >= 0)
                    {
                        m_Reversed = false;
                        return true;
                    }
                    m_Reversed = true;
                    return false;
                }
                m_Reversed = false;
                return true;
            }

            m_Reversed = true;
            return true;
        }

        private bool OnInstant()
        {
            float basePercent = m_CurrentPercent = 1;
            bool bAlive = OnHitEnd(1, ref basePercent);
            float curvedPercent = Evaluate(basePercent);
            m_TweenData.ApplyTween(curvedPercent);
            if (m_OnUpdate != null)
                m_OnUpdate(curvedPercent);
            if (!bAlive)
                End();
            return bAlive;
        }

        private void OnInstantComplete()
        {
            float curvedPercent = Evaluate(1);
            m_TweenData.ApplyTween(curvedPercent);
            if (m_OnUpdate != null)
                m_OnUpdate(curvedPercent);
            End();
        }

        private float Evaluate(float inPercent)
        {
            float curvedPercent;
            bool bReverseFinal = false;
            if (m_Reversed)
            {
                if (m_MirrorCurve)
                    inPercent = 1 - inPercent;
                else
                    bReverseFinal = true;
            }

            if (m_Curve == Curve.Linear)
                curvedPercent = m_AnimCurve != null ? m_AnimCurve.Evaluate(inPercent) : inPercent;
            else
                curvedPercent = m_Curve.Evaluate(inPercent);

            if (bReverseFinal)
                curvedPercent = 1 - curvedPercent;

            if (m_WaveFunc.Type != BeauRoutine.Wave.Function.None)
                curvedPercent = m_WaveFunc.Evaluate(curvedPercent);

            if (m_FromMode)
                curvedPercent = 1 - curvedPercent;

            return curvedPercent;
        }

        private float EvaluateNoWave(float inPercent)
        {
            float curvedPercent;
            bool bReverseFinal = false;
            if (m_Reversed)
            {
                if (m_MirrorCurve)
                    inPercent = 1 - inPercent;
                else
                    bReverseFinal = true;
            }

            curvedPercent = m_AnimCurve != null ? m_AnimCurve.Evaluate(inPercent) : m_Curve.Evaluate(inPercent);
            curvedPercent = bReverseFinal ? 1 - curvedPercent : curvedPercent;

            if (m_FromMode)
                curvedPercent = 1 - curvedPercent;

            return curvedPercent;
        }

        #endregion

        #region IEnumerator

        object IEnumerator.Current
        {
            get { return null; }
        }

        bool IEnumerator.MoveNext()
        {
            float deltaTime = Routine.DeltaTime;

            if (m_StartTime < 0)
            {
                float increment = Mathf.Min(deltaTime, -m_StartTime);
                m_StartTime += increment;
                deltaTime -= increment;
                if (m_StartTime < 0)
                    return true;
            }

            if (m_State == State.Begin)
            {
                Start();

                if (m_Instant)
                    return OnInstant();

                if (m_StartTime <= 0)
                    m_TweenData.ApplyTween(Evaluate(0));
            }

            switch (m_State)
            {
                case State.Run:
                    if (m_StartTime > 0)
                    {
                        deltaTime = m_StartTime;
                        m_StartTime = 0;
                    }

                    if (deltaTime <= 0)
                        return true;

                    if (m_Instant)
                        return OnInstant();

                    // Inlined Update
                    m_CurrentPercent += deltaTime * m_PercentIncrement;
                    int numCycles = (int) m_CurrentPercent;
                    float basePercent = m_CurrentPercent;
                    m_CurrentPercent = m_CurrentPercent % 1;

                    bool bAlive = true;

                    if (numCycles > 0)
                        bAlive = OnHitEnd(numCycles, ref basePercent);

                    float curvedPercent = Evaluate(basePercent);
                    m_TweenData.ApplyTween(curvedPercent);
                    if (m_OnUpdate != null)
                        m_OnUpdate(m_FromMode ? 1 - curvedPercent : curvedPercent);

                    if (!bAlive)
                        End();

                    return bAlive;

                case State.End:
                default:
                    return false;
            }
        }

        void IEnumerator.Reset()
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Disposes of the tween and resets all variables.
        /// </summary>
        public void Dispose()
        {
            if (m_TweenData != null)
            {
                if (m_State == State.Run && m_Cancel != CancelMode.Nothing)
                {
                    m_Reversed = false;

                    float curvedPercent = 0;
                    switch (m_Cancel)
                    {
                        case CancelMode.Revert:
                            curvedPercent = Evaluate(0);
                            break;
                        case CancelMode.RevertNoWave:
                            curvedPercent = EvaluateNoWave(0);
                            break;
                        case CancelMode.ForceEnd:
                            curvedPercent = Evaluate(m_Mode == LoopMode.Yoyo || m_Mode == LoopMode.YoyoLoop ? 0 : 1);
                            break;
                        case CancelMode.ForceEndNoWave:
                            curvedPercent = EvaluateNoWave(m_Mode == LoopMode.Yoyo || m_Mode == LoopMode.YoyoLoop ? 0 : 1);
                            break;
                    }
                    m_TweenData.ApplyTween(curvedPercent);
                    if (m_OnUpdate != null)
                        m_OnUpdate(m_FromMode ? 1 - curvedPercent : curvedPercent);
                }

                m_TweenData.OnTweenEnd();
                m_TweenData = null;
                TweenPool.Free(this);
            }

            m_Cancel = CancelMode.Nothing;
            m_Mode = LoopMode.Single;
            m_Curve = Curve.Linear;
            m_AnimCurve = null;
            m_WaveFunc = default(Wave);
            m_NumLoops = 0;
            m_OnStart = null;
            m_OnUpdate = null;
            m_OnComplete = null;

            m_Reversed = false;
            m_MirrorCurve = false;
            m_FromMode = false;
            m_Instant = false;
            m_StartMode = StartMode.Restart;
            m_StartTime = 0;

            m_CurrentPercent = 0;
            m_PercentIncrement = 0;
            m_State = State.Begin;
        }

        #endregion

        public override string ToString()
        {
            if (m_TweenData == null)
                return string.Format("Tween: [Null]");

            if (m_Mode == LoopMode.Yoyo)
                return string.Format("Tween: [{0}, {1:0.00}%, Yoyo]", m_TweenData.ToString(), m_CurrentPercent * 100);
            if (m_Mode == LoopMode.Loop)
            {
                if (m_NumLoops > 0)
                    return string.Format("Tween: [{0}, {1:0.00}%, Loop x {2}]", m_TweenData.ToString(), m_CurrentPercent * 100, m_NumLoops);
                else
                    return string.Format("Tween: [{0}, {1:0.00}%, Loop forever]", m_TweenData.ToString(), m_CurrentPercent * 100);
            }
            if (m_Mode == LoopMode.YoyoLoop)
            {
                if (m_NumLoops > 0)
                    return string.Format("Tween: [{0}, {1:0.00}%, YoyoLoop x {2}]", m_TweenData.ToString(), m_CurrentPercent * 100, m_NumLoops);
                else
                    return string.Format("Tween: [{0}, {1:0.00}%, YoyoLoop forever]", m_TweenData.ToString(), m_CurrentPercent * 100);
            }
            return string.Format("Tween: [{0}, {1:0.00}%, Oneshot]", m_TweenData.ToString(), m_CurrentPercent * 100);
        }

        #region Creation

        /// <summary>
        /// Creates a tween with the given tween parameters.
        /// </summary>
        static public Tween Create(ITweenData inTweenData, float inDuration)
        {
            return TweenPool.Alloc().SetData(inTweenData).Duration(inDuration);
        }

        /// <summary>
        /// Creates a tween with the given tween parameters.
        /// </summary>
        static public Tween Create(ITweenData inTweenData, TweenSettings inSettings)
        {
            return TweenPool.Alloc().SetData(inTweenData).Duration(inSettings.Time).Ease(inSettings.Curve);
        }

        /// <summary>
        /// Creates an empty tween with no duration and no effect.
        /// </summary>
        static public Tween CreateEmpty()
        {
            return TweenPool.Alloc().SetData(NULL_DATA).Duration(0);
        }

        /// <summary>
        /// Starts pooling tweens.
        /// </summary>
        static public void SetPooled(int inCapacity = DEFAULT_POOL_SIZE)
        {
            TweenPool.StartPooling(inCapacity);
        }

        #endregion // Creation
    }
}
/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Tween.cs
 * Purpose: Class that executes a tween, along with a builder-style
 *          interface for modifying the timing, easing, and looping.
 *          
 * Notes:   Tweens must be run inside a Routine to be evaluated.
*/

using System;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Runs a percentage-based tween from one value to another.
    /// </summary>
    public sealed partial class Tween : IRoutineEnumerator
    {
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

        // Tween settings
        private LoopMode m_Mode;
        private Curve m_Curve;
        private CancelMode m_Cancel;
        private AnimationCurve m_AnimCurve;
        private Wave m_WaveFunc;
        private float m_Duration;
        private int m_NumLoops;
        private bool m_MirrorCurve;
        private bool m_FromMode;

        // Events
        private event Action<float> m_OnUpdate;
        private event Action m_OnComplete;

        // State settings
        private float m_CurrentPercent;
        private float m_PercentIncrement;
        private bool m_Reversed = false;
        private bool m_Instant = false;
        private State m_State = State.Begin;

        // Current data
        private ITweenData m_TweenData;

        private Tween() { }

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
            m_NumLoops = 0;
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
            m_NumLoops = 0;
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
        /// <param name="inCurve"></param>
        /// <returns></returns>
        public Tween Ease(AnimationCurve inCurve)
        {
            m_Curve = Curve.Smooth;
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
            m_Duration = inTime < 0 ? 0 : inTime;
            m_PercentIncrement = inTime <= 0 ? 0 : 1.0f / m_Duration;
            m_Instant = m_PercentIncrement <= 0;
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

        #endregion

        #region Events

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
            m_CurrentPercent = 0;
            m_State = State.Run;
            m_TweenData.OnTweenStart();
        }

        // Called when the tween ends.
        private void End()
        {
            m_TweenData.OnTweenEnd();
            m_State = State.End;

            if (m_OnComplete != null)
                m_OnComplete();
        }

        // Updates and applies the current percentage.
        private bool Update(float inIncrement)
        {
            m_CurrentPercent += inIncrement;
            int numCycles = (int)m_CurrentPercent;
            float basePercent = m_CurrentPercent;
            m_CurrentPercent = m_CurrentPercent % 1;

            bool bAlive = true;

            if (numCycles > 0)
                bAlive = OnHitEnd(numCycles, ref basePercent);

            float curvedPercent = Evaluate(basePercent);
            m_TweenData.ApplyTween(curvedPercent);
            if (m_OnUpdate != null)
                m_OnUpdate(m_FromMode ? 1 - curvedPercent : curvedPercent);

            return bAlive;
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
            if (m_NumLoops > 0)
            {
                m_NumLoops -= inNumLoops;
                return m_NumLoops > 0;
            }
            return true;
        }

        private bool OnYoyo(int inNumLoops)
        {
            bool nextReversed = (inNumLoops % 2) == 1 ? !m_Reversed : m_Reversed;
            if (!nextReversed)
            {
                m_Reversed = true;
                return false;
            }

            m_Reversed = nextReversed;
            return true;
        }

        private bool OnYoyoLoop(int inNumLoops)
        {
            bool nextReversed = (inNumLoops % 2) == 1 ? !m_Reversed : m_Reversed;

            // If the next time we reverse we'll be moving forward,
            // we're hitting the lower bound again
            if (!nextReversed)
            {
                if (m_NumLoops > 0)
                {
                    m_NumLoops -= inNumLoops;
                    if (m_NumLoops > 0)
                    {
                        m_Reversed = nextReversed;
                        return true;
                    }
                    m_Reversed = true;
                    return false;
                }
                m_Reversed = nextReversed;
                return true;
            }
            else
            {
                m_Reversed = nextReversed;
                return true;
            }
        }

        private bool OnInstant()
        {
            float basePercent = m_CurrentPercent = 1;
            bool bAlive = OnHitEnd(1, ref basePercent);
            float curvedPercent = Evaluate(basePercent);
            m_TweenData.ApplyTween(curvedPercent);
            if (m_OnUpdate != null)
                m_OnUpdate(basePercent);
            if (!bAlive)
                End();
            return bAlive;
        }

        private float Evaluate(float inPercent)
        {
            float curvedPercent;
            if (m_Reversed ^ m_FromMode)
            {
                if (m_MirrorCurve)
                {
                    inPercent = 1 - inPercent;
                    curvedPercent = m_AnimCurve != null ? m_AnimCurve.Evaluate(inPercent) : m_Curve.Evaluate(inPercent);
                    curvedPercent = m_WaveFunc.Evaluate(curvedPercent);
                }
                else
                {
                    curvedPercent = m_AnimCurve != null ? m_AnimCurve.Evaluate(inPercent) : m_Curve.Evaluate(inPercent);
                    curvedPercent = m_WaveFunc.Evaluate(1 - curvedPercent);
                }
            }
            else
            {
                curvedPercent = m_AnimCurve != null ? m_AnimCurve.Evaluate(inPercent) : m_Curve.Evaluate(inPercent);
                curvedPercent = m_WaveFunc.Evaluate(curvedPercent);
            }
            return curvedPercent;
        }

        private float EvaluateNoWave(float inPercent)
        {
            float curvedPercent;
            if (m_Reversed ^ m_FromMode)
            {
                if (m_MirrorCurve)
                {
                    inPercent = 1 - inPercent;
                    curvedPercent = m_AnimCurve != null ? m_AnimCurve.Evaluate(inPercent) : m_Curve.Evaluate(inPercent);
                }
                else
                {
                    curvedPercent = m_AnimCurve != null ? m_AnimCurve.Evaluate(inPercent) : m_Curve.Evaluate(inPercent);
                    curvedPercent = 1 - curvedPercent;
                }
            }
            else
            {
                curvedPercent = m_AnimCurve != null ? m_AnimCurve.Evaluate(inPercent) : m_Curve.Evaluate(inPercent);
            }
            return curvedPercent;
        }

        #endregion

        #region IEnumerator

        public object Current
        {
            get { return null; }
        }

        public bool MoveNext()
        {
            switch (m_State)
            {
                case State.Begin:
                    Start();

                    if (m_Instant)
                        return OnInstant();

                    m_TweenData.ApplyTween(m_WaveFunc.Evaluate(0));
                    return true;

                case State.Run:
                    float deltaTime = Routine.DeltaTime;
                    if (deltaTime <= 0)
                        return true;

                    if (m_PercentIncrement <= 0)
                        return OnInstant();

                    bool bContinue = Update(deltaTime * m_PercentIncrement);
                    if (!bContinue)
                        End();
                    return bContinue;

                case State.End:
                default:
                    return false;
            }
        }

        public void Reset()
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
            if (m_TweenData != null && m_State == State.Run)
            {
                switch (m_Cancel)
                {
                    case CancelMode.Revert:
                        m_TweenData.ApplyTween(Evaluate(0));
                        break;
                    case CancelMode.RevertNoWave:
                        m_TweenData.ApplyTween(EvaluateNoWave(0));
                        break;
                    case CancelMode.ForceEnd:
                        m_TweenData.ApplyTween(Evaluate(1));
                        break;
                    case CancelMode.ForceEndNoWave:
                        m_TweenData.ApplyTween(EvaluateNoWave(1));
                        break;
                }
                m_TweenData.OnTweenEnd();
            }

            m_TweenData = null;

            m_Cancel = CancelMode.Nothing;
            m_Mode = LoopMode.Single;
            m_Curve = Curve.Linear;
            m_WaveFunc = default(Wave);
            m_Duration = 0;
            m_NumLoops = 0;
            m_OnUpdate = null;
            m_OnComplete = null;

            m_CurrentPercent = 0;
            m_PercentIncrement = 0;
            m_State = State.Begin;
            m_Reversed = false;
            m_Instant = false;
        }

        #endregion

        #region IRoutineEnumerator

        /// <summary>
        /// Called when the tween is pushed onto the stack.
        /// </summary>
        public bool OnRoutineStart()
        {
            if (m_State == State.Begin)
            {
                Start();

                // If we don't have any time
                // just skip to the end right away
                if (m_Instant)
                    return OnInstant();

                m_TweenData.ApplyTween(m_WaveFunc.Evaluate(0));
            }
            return true;
        }

        #endregion

        public override string ToString()
        {
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

        #region Pooling

        /// <summary>
        /// Creates a tween with the given tween parameters.
        /// </summary>
        static public Tween Create(ITweenData inTweenData, float inDuration)
        {
            return new Tween().SetData(inTweenData).Duration(inDuration);
        }

        /// <summary>
        /// Creates a tween with the given tween parameters.
        /// </summary>
        static public Tween Create(ITweenData inTweenData, TweenSettings inSettings)
        {
            return new Tween().SetData(inTweenData).Duration(inSettings.Time).Ease(inSettings.Curve);
        }

        /// <summary>
        /// Creates an empty tween with no duration and no effect.
        /// </summary>
        static public Tween CreateEmpty()
        {
            return new Tween().SetData(NULL_DATA).Duration(0);
        }

        #endregion
    }
}
/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 Apr 2017
 * 
 * File:    UnityHost.cs
 * Purpose: Host behavior. Contains hooks for executing BeauRoutines.
*/

using System.Collections;
using UnityEngine;

namespace BeauRoutine.Internal
{
    [AddComponentMenu("")]
    public sealed class RoutineUnityHost : MonoBehaviour
    {
        static private WaitForFixedUpdate s_CachedWaitForFixedUpdate = Routine.WaitForFixedUpdate();
        static private WaitForEndOfFrame s_CachedWaitForEndOfFrame = Routine.WaitForEndOfFrame();

        private Manager m_Manager;

        private Coroutine m_WaitForFixedUpdateRoutine;
        private Coroutine m_WaitForEndOfFrameRoutine;

        public void Initialize(Manager inManager)
        {
            m_Manager = inManager;
        }

        public void Shutdown()
        {
            StopYieldInstructions();
            m_Manager = null;
        }

        #region Unity Events

        private void OnApplicationQuit()
        {
            if (m_Manager != null)
            {
                StopYieldInstructions();
                m_Manager.OnApplicationQuit();
                m_Manager = null;
                Routine.Shutdown();
            }
        }

        private void OnDestroy()
        {
            if (m_Manager != null)
            {
                StopYieldInstructions();
                m_Manager = null;
                Routine.Shutdown();
            }
        }

        private void LateUpdate()
        {
            if (m_Manager != null)
                m_Manager.Update(RoutinePhase.LateUpdate);
            if (m_Manager.Fibers.GetYieldCount(YieldPhase.WaitForLateUpdate) > 0)
                m_Manager.UpdateYield(YieldPhase.WaitForLateUpdate);
        }

        private void Update()
        {
            if (m_Manager != null)
                m_Manager.Update(RoutinePhase.Update);
            if (m_Manager.Fibers.GetYieldCount(YieldPhase.WaitForUpdate) > 0)
                m_Manager.UpdateYield(YieldPhase.WaitForUpdate);
        }

        private void FixedUpdate()
        {
            if (m_Manager != null)
                m_Manager.Update(RoutinePhase.FixedUpdate);
        }

        #endregion

        #region Yield Instructions

        public void WaitForFixedUpdate()
        {
            if (m_WaitForFixedUpdateRoutine == null)
                m_WaitForFixedUpdateRoutine = StartCoroutine(ApplyWaitForFixedUpdate());
        }

        public void WaitForEndOfFrame()
        {
            if (m_WaitForEndOfFrameRoutine == null)
                m_WaitForEndOfFrameRoutine = StartCoroutine(ApplyWaitForEndOfFrame());
        }

        private IEnumerator ApplyWaitForFixedUpdate()
        {
            while(m_Manager.Fibers.GetYieldCount(YieldPhase.WaitForFixedUpdate) > 0)
            {
                yield return s_CachedWaitForFixedUpdate;
                m_Manager.UpdateYield(YieldPhase.WaitForFixedUpdate);
            }
            m_WaitForFixedUpdateRoutine = null;
        }

        private IEnumerator ApplyWaitForEndOfFrame()
        {
            while(m_Manager.Fibers.GetYieldCount(YieldPhase.WaitForEndOfFrame) > 0)
            {
                yield return s_CachedWaitForEndOfFrame;
                m_Manager.UpdateYield(YieldPhase.WaitForEndOfFrame);
            }
            m_WaitForEndOfFrameRoutine = null;
        }

        private void StopYieldInstructions()
        {
            if (m_WaitForFixedUpdateRoutine != null)
            {
                StopCoroutine(m_WaitForFixedUpdateRoutine);
                m_WaitForFixedUpdateRoutine = null;
            }

            if (m_WaitForEndOfFrameRoutine != null)
            {
                StopCoroutine(m_WaitForEndOfFrameRoutine);
                m_WaitForEndOfFrameRoutine = null;
            }
        }

        #endregion
    }
}

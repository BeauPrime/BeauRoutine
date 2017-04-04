/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    4 Apr 2017
 * 
 * File:    UnityHost.cs
 * Purpose: Host behavior. Contains hooks for executing BeauRoutines.
*/

using UnityEngine;

namespace BeauRoutine.Internal
{
    [AddComponentMenu("")]
    public sealed class UnityHost : MonoBehaviour
    {
        private Manager m_Manager;

        public void Initialize(Manager inManager)
        {
            m_Manager = inManager;
        }

        public void Shutdown()
        {
            m_Manager = null;
        }

        private void OnApplicationQuit()
        {
            if (m_Manager != null)
            {
                m_Manager = null;
                Routine.Shutdown();
            }
        }

        private void OnDestroy()
        {
            if (m_Manager != null)
            {
                m_Manager = null;
                Routine.Shutdown();
            }
        }

        private void LateUpdate()
        {
            if (m_Manager != null)
                m_Manager.Update();
        }
    }
}

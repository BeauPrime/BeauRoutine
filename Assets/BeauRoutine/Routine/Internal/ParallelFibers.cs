﻿/*
 * Copyright (C) 2016-2018. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    3 Apr 2017
 * 
 * File:    ParallelFibers.cs
 * Purpose: Iterator enabling pseudo-parallel execution of coroutines.
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Runs fibers in parallel.
    /// </summary>
    public sealed class ParallelFibers : IEnumerator, IDisposable
    {
        private Manager m_Manager;
        private List<IEnumerator> m_Enumerators;
        private List<Fiber> m_Fibers;
        private bool m_Race;
        private bool m_Iterating = false;
        private Fiber m_ParentFiber = null;

        public ParallelFibers(Manager inManager, List<IEnumerator> inEnumerators, bool inbRace)
        {
            m_Manager = inManager;
            m_Enumerators = inEnumerators;
            m_Race = inbRace;
            m_Fibers = new List<Fiber>(m_Enumerators.Count);
        }

        public void RemoveFiber(Fiber inFiber)
        {
            // This will only get called during Fiber.Dispose
            // If we're executing we don't have to remove from the list
            // since that will be taken care of in ParallelFibers.MoveNext
            if (!m_Iterating && m_Fibers != null)
            {
                m_Fibers.Remove(inFiber);
            }
        }

        public void SetParentFiber(Fiber inFiber)
        {
            m_ParentFiber = inFiber;
        }

        public void Dispose()
        {
            if (m_Enumerators != null)
            {
                for (int i = 0; i < m_Enumerators.Count; ++i)
                {
                    if (m_Enumerators[i] != null)
                        ((IDisposable)m_Enumerators[i]).Dispose();
                }
                m_Enumerators.Clear();
                m_Enumerators = null;
            }

            for (int i = 0; i < m_Fibers.Count; ++i)
            {
                Fiber fiber = m_Fibers[i];
                fiber.ClearParallelOwner();
                fiber.Dispose();
            }
            m_Fibers.Clear();
            m_Fibers = null;

            m_ParentFiber = null;
        }

        public object Current
        {
            get { return null; }
        }

        public bool MoveNext()
        {
            if (m_Enumerators != null)
            {
                for (int i = 0; i < m_Enumerators.Count; ++i)
                {
                    if (m_Enumerators[i] != null)
                    {
                        Fiber fiber = m_Manager.ChainFiber(m_Enumerators[i]);
                        fiber.SetParallelOwner(this);
                        fiber.SetParentFiber(m_ParentFiber);
                        m_Fibers.Add(fiber);
                    }
                }

                m_Enumerators.Clear();
                m_Enumerators = null;
                if (m_Fibers.Count == 0)
                    return false;
            }

            if (m_Fibers.Count > 0)
            {
                bool prevIterating = m_Iterating;
                m_Iterating = true;
                {
                    for (int i = 0; i < m_Fibers.Count; ++i)
                    {
                        Fiber myFiber = m_Fibers[i];
                        m_Manager.Frame.RefreshTimeScale();
                        if (!myFiber.Update())
                        {
                            m_Fibers.RemoveAt(i--);
                            if (m_Race)
                            {
                                m_Iterating = false;
                                return false;
                            }
                        }
                    }
                }
                m_Iterating = prevIterating;
            }

            return m_Fibers.Count > 0;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return m_Race ? "Routine::Race()" : "Routine::Combine()";
        }

        public RoutineStats[] GetStats()
        {
            RoutineStats[] stats = new RoutineStats[m_Fibers.Count];
            for (int i = 0; i < stats.Length; ++i)
                stats[i] = m_Fibers[i].GetStats();
            return stats;
        }
    }
}

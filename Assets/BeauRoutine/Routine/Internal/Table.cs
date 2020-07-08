/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Table.cs
 * Purpose: Stores all existing Fibers in a table and
 *          provides methods for iterating over active Fibers.
 */

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System;
using System.Collections.Generic;

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Collection of Fibers.
    /// </summary>
    internal sealed partial class Table
    {
        public const uint INDEX_MASK = 0x00FFFFFF;
        public const uint COUNTER_MASK = 0xFF000000;

        public const byte COUNTER_SHIFT = 24;
        public const byte COUNTER_MAX = 0xFF;

        /// <summary>
        /// Generates a unique ID for the given index+counter pair.
        /// </summary>
        static public uint GenerateID(uint inIndex, uint inCounter)
        {
            return (inIndex & INDEX_MASK) | ((inCounter << COUNTER_SHIFT) & COUNTER_MASK);
        }

        #region Types

        private struct Entry
        {
            public Fiber Fiber;

            // double-linked list for active/free
            public int MainPrev;
            public int MainNext;

            // double-linked list for update types
            public int UpdatePrev;
            public int UpdateNext;

            // double-linked list for yield types
            public int YieldPrev;
            public int YieldNext;

            public override string ToString()
            {
                return (Fiber.IsRunning() ? "ACTIVE" : "INACTIVE") + " Prev: " + MainPrev.ToString() + "; Next: " + MainNext.ToString();
            }
        }

        private struct MainList
        {
            public int Head;
            public int Count;

            public void Create()
            {
                Head = -1;
                Count = 0;
            }
        }

        private struct UpdateList
        {
            public int Head;
            public int Count;
            public bool Updating;
            public bool Dirty;

            public void Create()
            {
                Head = -1;
                Count = 0;
            }
        }

        private struct YieldList
        {
            public int Head;
            public int Count;
            public bool Updating;
            public bool Dirty;

            public void Create()
            {
                Head = -1;
                Count = 0;
            }
        }

        private struct UpdateStackFrame
        {
            public RoutinePhase Phase;
            public YieldPhase Yield;

            public int Next;
            public int Counter;

            public void Start(RoutinePhase inPhase, ref UpdateList inList)
            {
                Phase = inPhase;
                Yield = YieldPhase.None;

                Next = inList.Head;
                Counter = inList.Count;
            }

            public void Start(YieldPhase inYield, ref YieldList inList)
            {
                Phase = (RoutinePhase) (-1);
                Yield = inYield;

                Next = inList.Head;
                Counter = inList.Count;
            }

            public void RemoveFromUpdate(Table inTable, RoutinePhase inPhase, Fiber inFiber)
            {
                if (Counter >= 0 && Next == inFiber.Index && Yield == YieldPhase.None && Phase == inPhase)
                {
                    Next = inTable.m_Entries[inFiber.Index].UpdateNext;
                    --Counter;
                }
            }

            public void RemoveFromYield(Table inTable, YieldPhase inPhase, Fiber inFiber)
            {
                if (Counter >= 0 && Next == inFiber.Index && Yield == inPhase)
                {
                    Next = inTable.m_Entries[inFiber.Index].YieldNext;
                    --Counter;
                }
            }

            public void Clear()
            {
                Phase = (RoutinePhase) (-1);
                Yield = YieldPhase.None;
                Next = -1;
                Counter = -1;
            }
        }

        #endregion

        public Table(Manager inManager)
        {
            m_Entries = new Entry[0];
            m_Manager = inManager;

            m_FreeList.Create();
            m_ActiveList.Create();

            m_LateUpdateList.Create();
            m_UpdateList.Create();
            m_FixedUpdateList.Create();
            m_ManualUpdateList.Create();
            m_CustomUpdateList.Create();
            m_ThinkUpdateList.Create();
            m_RealtimeUpdateList.Create();

            m_YieldFixedUpdateList.Create();
            m_YieldEndOfFrameList.Create();
            m_YieldLateUpdateList.Create();
            m_YieldUpdateList.Create();
            m_YieldCustomUpdateList.Create();
            m_YieldThinkUpdateList.Create();
            m_YieldRealtimeUpdateList.Create();
        }

        private Manager m_Manager;
        private Entry[] m_Entries;

        private MainList m_FreeList;
        private MainList m_ActiveList;

        private UpdateList m_LateUpdateList;
        private UpdateList m_UpdateList;
        private UpdateList m_FixedUpdateList;
        private UpdateList m_ManualUpdateList;
        private UpdateList m_CustomUpdateList;
        private UpdateList m_ThinkUpdateList;
        private UpdateList m_RealtimeUpdateList;

        private YieldList m_YieldFixedUpdateList;
        private YieldList m_YieldEndOfFrameList;
        private YieldList m_YieldLateUpdateList;
        private YieldList m_YieldUpdateList;
        private YieldList m_YieldCustomUpdateList;
        private YieldList m_YieldThinkUpdateList;
        private YieldList m_YieldRealtimeUpdateList;

        private UpdateStackFrame m_MainUpdate;
        private UpdateStackFrame m_NestedUpdate;

        /// <summary>
        /// Returns the Fiber at the given index.
        /// </summary>
        public Fiber this[Routine inRoutine]
        {
            get
            {
                uint val = (uint) inRoutine;
                if (val == 0)
                    return null;

                Fiber fiber = m_Entries[(int) (val & Table.INDEX_MASK)].Fiber;
                return (fiber.HasHandle(inRoutine) ? fiber : null);
            }
        }

        /// <summary>
        /// Total number of Fibers.
        /// </summary>
        public int TotalCapacity
        {
            get { return m_Entries.Length; }
        }

        /// <summary>
        /// Total number of Fibers not free.
        /// </summary>
        public int TotalRunning
        {
            get { return m_Entries.Length - m_FreeList.Count; }
        }

        /// <summary>
        /// Total number of Fibers in the active list.
        /// </summary>
        public int TotalActive
        {
            get { return m_ActiveList.Count; }
        }

        /// <summary>
        /// Returns the first active Fiber.
        /// </summary>
        public Fiber StartActive(ref int ioIndex)
        {
            if (m_ActiveList.Head == -1)
            {
                ioIndex = -1;
                return null;
            }

            ioIndex = m_Entries[m_ActiveList.Head].MainNext;
            return m_Entries[m_ActiveList.Head].Fiber;
        }

        /// <summary>
        /// Traverses to the next Fiber.
        /// </summary>
        public Fiber TraverseActive(ref int ioIndex)
        {
            if (ioIndex == -1)
                return null;

            Fiber fiber = m_Entries[ioIndex].Fiber;
            ioIndex = m_Entries[ioIndex].MainNext;
            return fiber;
        }

        #region Queries

        /// <summary>
        /// Runs a query and returns the first routine to pass.
        /// </summary>
        public Routine RunQueryFirst(ITableQuery inQuery, ITableOperation inOperation)
        {
            int totalLeft = m_ActiveList.Count;
            int currentNode = m_ActiveList.Head;
            while (currentNode != -1 && totalLeft-- > 0)
            {
                Entry e = m_Entries[currentNode];
                currentNode = e.MainNext;
                if (inQuery.Validate(e.Fiber))
                {
                    inOperation.Execute(e.Fiber);
                    return e.Fiber.GetHandle();
                }
            }
            return Routine.Null;
        }

        /// <summary>
        /// Runs a query on all routines.
        /// </summary>
        public void RunQueryAll(ITableQuery inQuery, ITableOperation inOperation)
        {
            int totalLeft = m_ActiveList.Count;
            int currentNode = m_ActiveList.Head;
            while (currentNode != -1 && totalLeft-- > 0)
            {
                Entry e = m_Entries[currentNode];
                currentNode = e.MainNext;
                if (inQuery.Validate(e.Fiber))
                    inOperation.Execute(e.Fiber);
            }
        }

        /// <summary>
        /// Runs a query on all routines.
        /// </summary>
        public void RunQueryAll(ITableQuery inQuery, ITableOperation inOperation, ref ICollection<Routine> ioRoutines)
        {
            int totalLeft = m_ActiveList.Count;
            int currentNode = m_ActiveList.Head;
            while (currentNode != -1 && totalLeft-- > 0)
            {
                Entry e = m_Entries[currentNode];
                currentNode = e.MainNext;

                if (inQuery.Validate(e.Fiber))
                {
                    inOperation.Execute(e.Fiber);
                    if (ioRoutines == null)
                        ioRoutines = new List<Routine>();
                    ioRoutines.Add(e.Fiber.GetHandle());
                }
            }
        }

        #endregion

        /// <summary>
        /// Disposes of all active Fibers.
        /// </summary>
        public void ClearAll()
        {
            int totalLeft = m_ActiveList.Count;
            int currentNode = m_ActiveList.Head;
            while (currentNode != -1 && totalLeft-- > 0)
            {
                Entry e = m_Entries[currentNode];
                currentNode = e.MainNext;
                e.Fiber.Dispose();
            }
        }

        /// <summary>
        /// Sets the number of entries in the table.
        /// </summary>
        public void SetCapacity(int inDesiredAmount)
        {
            int currentCount = m_Entries.Length;
            if (currentCount >= inDesiredAmount)
                return;
            if (inDesiredAmount > INDEX_MASK)
                throw new IndexOutOfRangeException("Routine limit exceeded. Cannot have more than " + (INDEX_MASK + 1).ToString() + " running concurrently.");

            m_Manager.Log("Expanded capacity to " + inDesiredAmount.ToString());

            Array.Resize(ref m_Entries, inDesiredAmount);
            m_FreeList.Count += inDesiredAmount - currentCount;

            for (int i = currentCount; i < inDesiredAmount; ++i)
            {
                Fiber fiber = new Fiber(m_Manager, (uint) i);
                m_Entries[i].Fiber = fiber;
                m_Entries[i].MainPrev = i - 1;
                m_Entries[i].MainNext = i + 1;

                m_Entries[i].UpdateNext = m_Entries[i].UpdatePrev = m_Entries[i].YieldNext = m_Entries[i].YieldPrev = -1;
            }

            bool bStartingNew = m_FreeList.Head == -1;

            // If we don't already have a free slot
            if (bStartingNew)
            {
                m_FreeList.Head = currentCount;
                m_Entries[currentCount].MainPrev = currentCount;
                m_Entries[currentCount].MainNext = -1;
            }

            int lastIndex = m_Entries[m_FreeList.Head].MainPrev;

            // Previous last should now point to the first new
            if (!bStartingNew)
            {
                m_Entries[lastIndex].MainNext = currentCount;
                m_Entries[currentCount].MainPrev = lastIndex;
            }
            else
            {
                m_Entries[m_FreeList.Head].MainNext = currentCount + 1;
            }

            // Last pointer should now point to the last new
            m_Entries[m_FreeList.Head].MainPrev = inDesiredAmount - 1;

            // Set last entry to point to none
            m_Entries[inDesiredAmount - 1].MainNext = -1;
        }

        /// <summary>
        /// Returns a free Fiber.
        /// </summary>
        public Fiber GetFreeFiber()
        {
            if (m_FreeList.Count == 0)
                SetCapacity(m_Entries.Length == 0 ? Manager.DEFAULT_CAPACITY : m_Entries.Length * 2);
            return RemoveFirst(ref m_FreeList);
        }

        /// <summary>
        /// Adds a Fiber to the active list.
        /// </summary>
        public void AddActiveFiber(Fiber inFiber)
        {
            AddLast(inFiber, ref m_ActiveList);
        }

        /// <summary>
        /// Removes a Fiber from the active list.
        /// </summary>
        public void RemoveActiveFiber(Fiber inFiber)
        {
            RemoveEntry(inFiber, ref m_ActiveList);
        }

        /// <summary>
        /// Adds a Fiber to the free list.
        /// </summary>
        public void AddFreeFiber(Fiber inFiber)
        {
            AddFirst(inFiber, ref m_FreeList);
        }

        #region Update Lists

        /// <summary>
        /// Updates all fibers in the given list.
        /// </summary>
        public void RunUpdate(RoutinePhase inUpdateMode)
        {
            switch (inUpdateMode)
            {
                case RoutinePhase.FixedUpdate:
                    RunUpdate(RoutinePhase.FixedUpdate, ref m_FixedUpdateList);
                    break;

                case RoutinePhase.Update:
                    RunUpdate(RoutinePhase.Update, ref m_UpdateList);
                    break;

                case RoutinePhase.LateUpdate:
                    RunUpdate(RoutinePhase.LateUpdate, ref m_LateUpdateList);
                    break;

                case RoutinePhase.CustomUpdate:
                    RunUpdate(RoutinePhase.CustomUpdate, ref m_CustomUpdateList);
                    break;

                case RoutinePhase.ThinkUpdate:
                    RunUpdate(RoutinePhase.ThinkUpdate, ref m_ThinkUpdateList);
                    break;

                case RoutinePhase.RealtimeUpdate:
                    RunUpdate(RoutinePhase.RealtimeUpdate, ref m_RealtimeUpdateList);
                    break;

                case RoutinePhase.Manual:
                    if (m_MainUpdate.Counter >= 0)
                        RunUpdateNested(RoutinePhase.Manual, ref m_ManualUpdateList);
                    else
                        RunUpdate(RoutinePhase.Manual, ref m_ManualUpdateList);
                    break;
            }
        }

        // Runs a top-level update on the given update list
        private void RunUpdate(RoutinePhase inUpdateMode, ref UpdateList ioList)
        {
            if (ioList.Dirty)
            {
                SortUpdateList(ref ioList);
                ioList.Dirty = false;
            }

            m_MainUpdate.Start(inUpdateMode, ref ioList);

            bool bPrevUpdating = ioList.Updating;
            ioList.Updating = true;
            {
                while (m_MainUpdate.Next != -1 && m_MainUpdate.Counter-- > 0)
                {
                    Entry e = m_Entries[m_MainUpdate.Next];
                    m_MainUpdate.Next = e.UpdateNext;
                    if (e.Fiber.PrepareUpdate(inUpdateMode))
                        e.Fiber.Update();
                }
            }
            ioList.Updating = bPrevUpdating;

            m_MainUpdate.Clear();
        }

        // Runs a nested update on the given update list (always manual)
        private void RunUpdateNested(RoutinePhase inUpdateMode, ref UpdateList ioList)
        {
            if (ioList.Dirty)
            {
                SortUpdateList(ref ioList);
                ioList.Dirty = false;
            }

            m_NestedUpdate.Start(inUpdateMode, ref ioList);

            bool bPrevUpdating = ioList.Updating;
            ioList.Updating = true;
            {
                while (m_NestedUpdate.Next != -1 && m_NestedUpdate.Counter-- > 0)
                {
                    Entry e = m_Entries[m_NestedUpdate.Next];
                    m_NestedUpdate.Next = e.UpdateNext;
                    if (e.Fiber.PrepareUpdate(inUpdateMode))
                        e.Fiber.Update();
                }
            }
            ioList.Updating = bPrevUpdating;

            m_NestedUpdate.Clear();
        }

        /// <summary>
        /// Marks an update list to be sorted at the next available time.
        /// </summary>
        public void SetUpdateListDirty(RoutinePhase inUpdate)
        {
            switch (inUpdate)
            {
                case RoutinePhase.FixedUpdate:
                    m_FixedUpdateList.Dirty = true;
                    break;

                case RoutinePhase.Update:
                    m_UpdateList.Dirty = true;
                    break;

                case RoutinePhase.LateUpdate:
                    m_LateUpdateList.Dirty = true;
                    break;

                case RoutinePhase.CustomUpdate:
                    m_CustomUpdateList.Dirty = true;
                    break;

                case RoutinePhase.ThinkUpdate:
                    m_ThinkUpdateList.Dirty = true;
                    break;

                case RoutinePhase.RealtimeUpdate:
                    m_RealtimeUpdateList.Dirty = true;
                    break;

                case RoutinePhase.Manual:
                    m_ManualUpdateList.Dirty = true;
                    break;
            }
        }

        /// <summary>
        /// Returns the total number of fibers for the given update list.
        /// </summary>
        public int GetUpdateCount(RoutinePhase inUpdate)
        {
            switch (inUpdate)
            {
                case RoutinePhase.FixedUpdate:
                    return m_FixedUpdateList.Count;

                case RoutinePhase.Update:
                    return m_UpdateList.Count;

                case RoutinePhase.Manual:
                    return m_ManualUpdateList.Count;

                case RoutinePhase.LateUpdate:
                    return m_LateUpdateList.Count;

                case RoutinePhase.CustomUpdate:
                    return m_CustomUpdateList.Count;

                case RoutinePhase.ThinkUpdate:
                    return m_ThinkUpdateList.Count;

                case RoutinePhase.RealtimeUpdate:
                    return m_RealtimeUpdateList.Count;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns if the given list is updating.
        /// </summary>
        public bool GetIsUpdating(RoutinePhase inUpdate)
        {
            switch (inUpdate)
            {
                case RoutinePhase.FixedUpdate:
                    return m_FixedUpdateList.Updating;

                case RoutinePhase.Update:
                    return m_UpdateList.Updating;

                case RoutinePhase.Manual:
                    return m_ManualUpdateList.Updating;

                case RoutinePhase.LateUpdate:
                    return m_LateUpdateList.Updating;

                case RoutinePhase.CustomUpdate:
                    return m_CustomUpdateList.Updating;

                case RoutinePhase.ThinkUpdate:
                    return m_ThinkUpdateList.Updating;

                case RoutinePhase.RealtimeUpdate:
                    return m_RealtimeUpdateList.Updating;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Adds a Fiber to the given update list.
        /// </summary>
        public void AddFiberToUpdateList(Fiber inFiber, RoutinePhase inUpdate)
        {
            switch (inUpdate)
            {
                case RoutinePhase.FixedUpdate:
                    AddLast(inFiber, ref m_FixedUpdateList);
                    m_FixedUpdateList.Dirty = true;
                    break;

                case RoutinePhase.Update:
                    AddLast(inFiber, ref m_UpdateList);
                    m_UpdateList.Dirty = true;
                    break;

                case RoutinePhase.LateUpdate:
                    AddLast(inFiber, ref m_LateUpdateList);
                    m_LateUpdateList.Dirty = true;
                    break;

                case RoutinePhase.CustomUpdate:
                    AddLast(inFiber, ref m_CustomUpdateList);
                    m_CustomUpdateList.Dirty = true;
                    break;

                case RoutinePhase.ThinkUpdate:
                    AddLast(inFiber, ref m_ThinkUpdateList);
                    m_ThinkUpdateList.Dirty = true;
                    break;

                case RoutinePhase.RealtimeUpdate:
                    AddLast(inFiber, ref m_RealtimeUpdateList);
                    m_RealtimeUpdateList.Dirty = true;
                    break;

                case RoutinePhase.Manual:
                    AddLast(inFiber, ref m_ManualUpdateList);
                    m_ManualUpdateList.Dirty = true;
                    break;
            }
        }

        /// <summary>
        /// Removes a Fiber from the given update list.
        /// </summary>
        public void RemoveFiberFromUpdateList(Fiber inFiber, RoutinePhase inUpdate)
        {
            m_MainUpdate.RemoveFromUpdate(this, inUpdate, inFiber);

            switch (inUpdate)
            {
                case RoutinePhase.FixedUpdate:
                    RemoveEntry(inFiber, ref m_FixedUpdateList);
                    break;

                case RoutinePhase.Update:
                    RemoveEntry(inFiber, ref m_UpdateList);
                    break;

                case RoutinePhase.LateUpdate:
                    RemoveEntry(inFiber, ref m_LateUpdateList);
                    break;

                case RoutinePhase.CustomUpdate:
                    RemoveEntry(inFiber, ref m_CustomUpdateList);
                    break;

                case RoutinePhase.ThinkUpdate:
                    RemoveEntry(inFiber, ref m_ThinkUpdateList);
                    break;

                case RoutinePhase.RealtimeUpdate:
                    RemoveEntry(inFiber, ref m_RealtimeUpdateList);
                    break;

                case RoutinePhase.Manual:
                    // Nested list is only involved in a manual update
                    m_NestedUpdate.RemoveFromUpdate(this, inUpdate, inFiber);

                    RemoveEntry(inFiber, ref m_ManualUpdateList);
                    break;
            }
        }

        #endregion

        #region Updating a Fiber manually

        /// <summary>
        /// Attempts to update the given Routine manually.
        /// </summary>
        public void RunManualUpdate(Fiber inFiber)
        {
            bool bPrevUpdating = m_ManualUpdateList.Updating;
            m_ManualUpdateList.Updating = true;
            inFiber.Update();
            m_ManualUpdateList.Updating = bPrevUpdating;
        }

        #endregion

        #region Yield Lists

        public void RunYieldUpdate(YieldPhase inUpdate)
        {
            switch (inUpdate)
            {
                case YieldPhase.WaitForEndOfFrame:
                    RunYieldUpdate(YieldPhase.WaitForEndOfFrame, ref m_YieldEndOfFrameList);
                    break;

                case YieldPhase.WaitForFixedUpdate:
                    RunYieldUpdate(YieldPhase.WaitForFixedUpdate, ref m_YieldFixedUpdateList);
                    break;

                case YieldPhase.WaitForLateUpdate:
                    RunYieldUpdate(YieldPhase.WaitForLateUpdate, ref m_YieldLateUpdateList);
                    break;

                case YieldPhase.WaitForUpdate:
                    RunYieldUpdate(YieldPhase.WaitForUpdate, ref m_YieldUpdateList);
                    break;

                case YieldPhase.WaitForCustomUpdate:
                    RunYieldUpdate(YieldPhase.WaitForCustomUpdate, ref m_YieldCustomUpdateList);
                    break;

                case YieldPhase.WaitForThinkUpdate:
                    RunYieldUpdate(YieldPhase.WaitForThinkUpdate, ref m_YieldThinkUpdateList);
                    break;

                case YieldPhase.WaitForRealtimeUpdate:
                    RunYieldUpdate(YieldPhase.WaitForRealtimeUpdate, ref m_YieldRealtimeUpdateList);
                    break;
            }
        }

        private void RunYieldUpdate(YieldPhase inUpdate, ref YieldList ioList)
        {
            if (ioList.Dirty)
            {
                SortYieldList(ref ioList);
                ioList.Dirty = false;
            }

            m_MainUpdate.Start(inUpdate, ref ioList);

            ioList.Updating = true;
            {
                while (m_MainUpdate.Next != -1 && m_MainUpdate.Counter-- > 0)
                {
                    Entry e = m_Entries[m_MainUpdate.Next];
                    m_MainUpdate.Next = e.YieldNext;
                    e.Fiber.Update(inUpdate);
                }
            }
            ioList.Updating = false;

            m_MainUpdate.Clear();
        }

        public void SetYieldListDirty(YieldPhase inUpdate)
        {
            switch (inUpdate)
            {
                case YieldPhase.WaitForEndOfFrame:
                    m_YieldEndOfFrameList.Dirty = true;
                    break;

                case YieldPhase.WaitForFixedUpdate:
                    m_YieldFixedUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForLateUpdate:
                    m_YieldLateUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForUpdate:
                    m_YieldUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForCustomUpdate:
                    m_YieldCustomUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForThinkUpdate:
                    m_YieldThinkUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForRealtimeUpdate:
                    m_YieldRealtimeUpdateList.Dirty = true;
                    break;
            }
        }

        public int GetYieldCount(YieldPhase inUpdate)
        {
            switch (inUpdate)
            {
                case YieldPhase.WaitForEndOfFrame:
                    return m_YieldEndOfFrameList.Count;

                case YieldPhase.WaitForFixedUpdate:
                    return m_YieldFixedUpdateList.Count;

                case YieldPhase.WaitForLateUpdate:
                    return m_YieldLateUpdateList.Count;

                case YieldPhase.WaitForUpdate:
                    return m_YieldUpdateList.Count;

                case YieldPhase.WaitForCustomUpdate:
                    return m_YieldCustomUpdateList.Count;

                case YieldPhase.WaitForThinkUpdate:
                    return m_YieldThinkUpdateList.Count;

                case YieldPhase.WaitForRealtimeUpdate:
                    return m_YieldRealtimeUpdateList.Count;

                default:
                    return 0;
            }
        }

        public bool GetIsUpdatingYield(YieldPhase inUpdate)
        {
            switch (inUpdate)
            {
                case YieldPhase.WaitForEndOfFrame:
                    return m_YieldEndOfFrameList.Updating;

                case YieldPhase.WaitForFixedUpdate:
                    return m_YieldFixedUpdateList.Updating;

                case YieldPhase.WaitForLateUpdate:
                    return m_YieldLateUpdateList.Updating;

                case YieldPhase.WaitForUpdate:
                    return m_YieldUpdateList.Updating;

                case YieldPhase.WaitForCustomUpdate:
                    return m_YieldCustomUpdateList.Updating;

                case YieldPhase.WaitForThinkUpdate:
                    return m_YieldThinkUpdateList.Updating;

                case YieldPhase.WaitForRealtimeUpdate:
                    return m_YieldRealtimeUpdateList.Updating;

                default:
                    return false;
            }
        }

        public void AddFiberToYieldList(Fiber inFiber, YieldPhase inUpdate)
        {
            switch (inUpdate)
            {
                case YieldPhase.WaitForEndOfFrame:
                    AddLast(inFiber, ref m_YieldEndOfFrameList);
                    m_YieldEndOfFrameList.Dirty = true;
                    break;

                case YieldPhase.WaitForFixedUpdate:
                    AddLast(inFiber, ref m_YieldFixedUpdateList);
                    m_FixedUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForLateUpdate:
                    AddLast(inFiber, ref m_YieldLateUpdateList);
                    m_YieldLateUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForUpdate:
                    AddLast(inFiber, ref m_YieldUpdateList);
                    m_YieldUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForCustomUpdate:
                    AddLast(inFiber, ref m_YieldCustomUpdateList);
                    m_YieldCustomUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForThinkUpdate:
                    AddLast(inFiber, ref m_YieldThinkUpdateList);
                    m_YieldThinkUpdateList.Dirty = true;
                    break;

                case YieldPhase.WaitForRealtimeUpdate:
                    AddLast(inFiber, ref m_YieldRealtimeUpdateList);
                    m_YieldRealtimeUpdateList.Dirty = true;
                    break;
            }
        }

        public void RemoveFiberFromYieldList(Fiber inFiber, YieldPhase inUpdate)
        {
            m_MainUpdate.RemoveFromYield(this, inUpdate, inFiber);

            // No need to check nested list, since the nested list will never be running a yield update
            // m_NestedUpdate.RemoveFromYield(this, inUpdate, inFiber);

            switch (inUpdate)
            {
                case YieldPhase.WaitForEndOfFrame:
                    RemoveEntry(inFiber, ref m_YieldEndOfFrameList);
                    break;

                case YieldPhase.WaitForFixedUpdate:
                    RemoveEntry(inFiber, ref m_YieldFixedUpdateList);
                    break;

                case YieldPhase.WaitForLateUpdate:
                    RemoveEntry(inFiber, ref m_YieldLateUpdateList);
                    break;

                case YieldPhase.WaitForUpdate:
                    RemoveEntry(inFiber, ref m_YieldUpdateList);
                    break;

                case YieldPhase.WaitForCustomUpdate:
                    RemoveEntry(inFiber, ref m_YieldCustomUpdateList);
                    break;

                case YieldPhase.WaitForThinkUpdate:
                    RemoveEntry(inFiber, ref m_YieldThinkUpdateList);
                    break;

                case YieldPhase.WaitForRealtimeUpdate:
                    RemoveEntry(inFiber, ref m_RealtimeUpdateList);
                    break;
            }
        }

        #endregion
    }
}
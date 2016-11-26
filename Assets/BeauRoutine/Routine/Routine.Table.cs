/*
 * Copyright (C) 2016. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Routine.Table.cs
 * Purpose: Stores all existing Fibers in a table and
 *          provides methods for iterating over active Fibers.
*/

using System;

namespace BeauRoutine
{
    public partial struct Routine
    {
        private sealed class FiberTable
        {
            private struct Entry
            {
                public Fiber Fiber;

                /// <summary>
                /// Previous entry index, or the index
                /// of the last entry, if this is the first.
                /// </summary>
                public int PrevIndex;

                /// <summary>
                /// Next entry index, or -1 if this is the last.
                /// </summary>
                public int NextIndex;
            }

            public FiberTable()
            {
                m_Entries = new Entry[0];
                m_FirstFree = m_FirstActive = -1;
                m_FreeCount = m_ActiveCount = 0;
            }

            private Entry[] m_Entries;

            private int m_FirstFree;
            private int m_FreeCount;

            private int m_FirstActive;
            private int m_ActiveCount;

            /// <summary>
            /// Returns the Fiber at the given index.
            /// </summary>
            public Fiber this[int inIndex]
            {
                get { return m_Entries[inIndex].Fiber; }
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
                get { return m_Entries.Length - m_FreeCount; }
            }

            /// <summary>
            /// Total number of Fibers in the active list.
            /// </summary>
            public int TotalActive
            {
                get { return m_ActiveCount; }
            }

            /// <summary>
            /// Returns the first active Fiber.
            /// </summary>
            public Fiber StartActive(ref int ioIndex)
            {
                if (m_FirstActive == -1)
                {
                    ioIndex = -1;
                    return null;
                }

                ioIndex = m_Entries[m_FirstActive].NextIndex;
                return m_Entries[m_FirstActive].Fiber;
            }

            /// <summary>
            /// Traverses to the next Fiber.
            /// </summary>
            public Fiber Traverse(ref int ioIndex)
            {
                if (ioIndex == -1)
                    return null;

                Fiber fiber = m_Entries[ioIndex].Fiber;
                ioIndex = m_Entries[ioIndex].NextIndex;
                return fiber;
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

                Array.Resize(ref m_Entries, inDesiredAmount);
                m_FreeCount += inDesiredAmount - currentCount;

                for (int i = currentCount; i < inDesiredAmount; ++i)
                {
                    Fiber fiber = new Fiber((uint)i);
                    m_Entries[i].Fiber = fiber;
                    m_Entries[i].PrevIndex = i - 1;
                    m_Entries[i].NextIndex = i + 1;
                }

                bool bStartingNew = m_FirstFree == -1;

                // If we don't already have a free slot
                if (bStartingNew)
                {
                    m_FirstFree = currentCount;
                    m_Entries[m_FirstFree].PrevIndex = currentCount;
                    m_Entries[m_FirstFree].NextIndex = -1;
                }

                int lastIndex = m_Entries[m_FirstFree].PrevIndex;

                // Previous last should now point to the first new
                if (!bStartingNew)
                {
                    m_Entries[lastIndex].NextIndex = currentCount;
                    m_Entries[currentCount].PrevIndex = lastIndex;
                }
                else
                {
                    m_Entries[m_FirstFree].NextIndex = currentCount + 1;
                }

                // Last pointer should now point to the last new
                m_Entries[m_FirstFree].PrevIndex = inDesiredAmount - 1;

                // Set last entry to point to none
                m_Entries[inDesiredAmount - 1].NextIndex = -1;
            }

            /// <summary>
            /// Returns a free Fiber.
            /// </summary>
            public Fiber GetFreeFiber()
            {
                if (m_FreeCount == 0)
                    SetCapacity(m_Entries.Length * 2);
                return RemoveFirst(ref m_FirstFree, ref m_FreeCount);
            }

            /// <summary>
            /// Adds a Fiber to the active list.
            /// </summary>
            public void AddActiveFiber(Fiber inFiber)
            {
                AddLast(inFiber, ref m_FirstActive, ref m_ActiveCount);
            }

            /// <summary>
            /// Removes a Fiber from the active list.
            /// </summary>
            public void RemoveActiveFiber(Fiber inFiber)
            {
                RemoveEntry(inFiber, ref m_FirstActive, ref m_ActiveCount);
            }

            /// <summary>
            /// Adds a Fiber to the free list.
            /// </summary>
            public void AddFreeFiber(Fiber inFiber)
            {
                AddFirst(inFiber, ref m_FirstFree, ref m_FreeCount);
            }

            // Adds the Fiber to the start of the given list
            private void AddFirst(Fiber inFiber, ref int ioFirst, ref int ioCount)
            {
                int fiberIndex = (int)inFiber.Index;

                // If there are no free fibers currently,
                // we can just add this as the first and only entry.
                if (ioFirst == -1)
                {
                    ioFirst = fiberIndex;
                    m_Entries[fiberIndex].NextIndex = -1;
                    m_Entries[fiberIndex].PrevIndex = fiberIndex;
                }
                else
                {
                    Entry firstEntry = m_Entries[ioFirst];

                    // Point back at the current last entry
                    m_Entries[fiberIndex].PrevIndex = firstEntry.PrevIndex;

                    // Point at the old first free entry
                    m_Entries[fiberIndex].NextIndex = ioFirst;

                    // Point the old first entry at the new first entry
                    m_Entries[ioFirst].PrevIndex = fiberIndex;

                    ioFirst = fiberIndex;
                }

                ++ioCount;
            }

            // Adds the Fiber to the end of the given list
            private void AddLast(Fiber inFiber, ref int ioFirst, ref int ioCount)
            {
                int fiberIndex = (int)inFiber.Index;

                // If there are no free fibers currently,
                // we can just add this as the first and only entry.
                if (ioFirst == -1)
                {
                    ioFirst = fiberIndex;
                    m_Entries[fiberIndex].NextIndex = -1;
                    m_Entries[fiberIndex].PrevIndex = fiberIndex;
                }
                else
                {
                    Entry firstEntry = m_Entries[ioFirst];

                    // Point the old last entry to this one
                    m_Entries[firstEntry.PrevIndex].NextIndex = fiberIndex;

                    // Point the new last entry at the previous one
                    m_Entries[fiberIndex].PrevIndex = firstEntry.PrevIndex;

                    // Point the new last entry at nothing
                    m_Entries[fiberIndex].NextIndex = -1;

                    // Point the first entry at this one
                    m_Entries[ioFirst].PrevIndex = fiberIndex;
                }

                ++ioCount;
            }

            // Removes the first Fiber from the given list.
            private Fiber RemoveFirst(ref int ioFirst, ref int ioCount)
            {
                int fiberIndex = ioFirst;

                if (fiberIndex == -1)
                    return null;

                int nextIndex = m_Entries[fiberIndex].NextIndex;
                int prevIndex = m_Entries[fiberIndex].PrevIndex;

                // If the table only has one entry,
                // just remove it and set the table to empty.
                if (nextIndex == -1)
                {
                    m_Entries[fiberIndex].PrevIndex = -1;
                    ioFirst = -1;
                    --ioCount;
                    return m_Entries[fiberIndex].Fiber;
                }

                // Point the next entry at the last entry
                m_Entries[nextIndex].PrevIndex = prevIndex;
                
                // Clear pointers in the current entry
                m_Entries[fiberIndex].NextIndex = -1;
                m_Entries[fiberIndex].PrevIndex = -1;

                // Point to the next entry as the first.
                ioFirst = nextIndex;
                --ioCount;

                return m_Entries[fiberIndex].Fiber;
            }

            // Removes an entry from the Fiber list.
            private void RemoveEntry(Fiber inFiber, ref int ioFirst, ref int ioCount)
            {
                int fiberIndex = (int)inFiber.Index;
                int nextIndex = m_Entries[fiberIndex].NextIndex;
                int prevIndex = m_Entries[fiberIndex].PrevIndex;

                // If the list is already empty, we can't do anything about it
                if (ioFirst == -1)
                    return;

                // Ensure the next entry is pointing back to our previous index.
                m_Entries[nextIndex == -1 ? ioFirst : nextIndex].PrevIndex = prevIndex;

                // If we're the first entry, the first entry is now our last
                if (fiberIndex == ioFirst)
                {
                    ioFirst = nextIndex;
                }
                else
                {
                    // Previous entry should point back to our next entry
                    m_Entries[prevIndex].NextIndex = nextIndex;
                }

                m_Entries[fiberIndex].NextIndex = -1;
                m_Entries[fiberIndex].PrevIndex = -1;

                --ioCount;
            }
        }
    }
}

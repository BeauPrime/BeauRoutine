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

                public override string ToString()
                {
                    return (Fiber.IsRunning() ? "ACTIVE " : "INACTIVE ") + "Prev: " + PrevIndex.ToString() + "; Next: " + NextIndex.ToString();
                }
            }

            public FiberTable()
            {
                m_Entries = new Entry[0];
                m_FreeHead = m_ActiveHead = -1;
                m_FreeCount = m_ActiveCount = 0;
            }

            private Entry[] m_Entries;

            private int m_FreeHead;
            private int m_FreeCount;

            private int m_ActiveHead;
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
                if (m_ActiveHead == -1)
                {
                    ioIndex = -1;
                    return null;
                }

                ioIndex = m_Entries[m_ActiveHead].NextIndex;
                return m_Entries[m_ActiveHead].Fiber;
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

                bool bStartingNew = m_FreeHead == -1;

                // If we don't already have a free slot
                if (bStartingNew)
                {
                    m_FreeHead = currentCount;
                    m_Entries[m_FreeHead].PrevIndex = currentCount;
                    m_Entries[m_FreeHead].NextIndex = -1;
                }

                int lastIndex = m_Entries[m_FreeHead].PrevIndex;

                // Previous last should now point to the first new
                if (!bStartingNew)
                {
                    m_Entries[lastIndex].NextIndex = currentCount;
                    m_Entries[currentCount].PrevIndex = lastIndex;
                }
                else
                {
                    m_Entries[m_FreeHead].NextIndex = currentCount + 1;
                }

                // Last pointer should now point to the last new
                m_Entries[m_FreeHead].PrevIndex = inDesiredAmount - 1;

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
                return RemoveFirst(ref m_FreeHead, ref m_FreeCount);
            }

            /// <summary>
            /// Adds a Fiber to the active list.
            /// </summary>
            public void AddActiveFiber(Fiber inFiber)
            {
                AddLast(inFiber, ref m_ActiveHead, ref m_ActiveCount);
            }

            /// <summary>
            /// Removes a Fiber from the active list.
            /// </summary>
            public void RemoveActiveFiber(Fiber inFiber)
            {
                RemoveEntry(inFiber, ref m_ActiveHead, ref m_ActiveCount);
            }

            /// <summary>
            /// Adds a Fiber to the free list.
            /// </summary>
            public void AddFreeFiber(Fiber inFiber)
            {
                AddFirst(inFiber, ref m_FreeHead, ref m_FreeCount);
            }

            // Adds the Fiber to the start of the given list
            private void AddFirst(Fiber inFiber, ref int ioHead, ref int ioCount)
            {
                int fiberIndex = (int)inFiber.Index;

                // If there are no free fibers currently,
                // we can just add this as the first and only entry.
                if (ioHead == -1)
                {
                    ioHead = fiberIndex;
                    m_Entries[fiberIndex].NextIndex = -1;
                    m_Entries[fiberIndex].PrevIndex = fiberIndex;
                }
                else
                {
                    Entry firstEntry = m_Entries[ioHead];

                    // Point back at the current last entry
                    m_Entries[fiberIndex].PrevIndex = firstEntry.PrevIndex;

                    // Point at the old first free entry
                    m_Entries[fiberIndex].NextIndex = ioHead;

                    // Point the old first entry at the new first entry
                    m_Entries[ioHead].PrevIndex = fiberIndex;

                    ioHead = fiberIndex;
                }

                ++ioCount;
            }

            // Adds the Fiber to the end of the given list
            private void AddLast(Fiber inFiber, ref int ioHead, ref int ioCount)
            {
                int fiberIndex = (int)inFiber.Index;

                // If there are no free fibers currently,
                // we can just add this as the first and only entry.
                if (ioHead == -1)
                {
                    ioHead = fiberIndex;
                    m_Entries[fiberIndex].NextIndex = -1;
                    m_Entries[fiberIndex].PrevIndex = fiberIndex;
                }
                else
                {
                    Entry firstEntry = m_Entries[ioHead];

                    // Point the old last entry to this one
                    m_Entries[firstEntry.PrevIndex].NextIndex = fiberIndex;

                    // Point the new last entry at the previous one
                    m_Entries[fiberIndex].PrevIndex = firstEntry.PrevIndex;

                    // Point the new last entry at nothing
                    m_Entries[fiberIndex].NextIndex = -1;

                    // Point the first entry at this one
                    m_Entries[ioHead].PrevIndex = fiberIndex;
                }

                ++ioCount;
            }

            // Removes the first Fiber from the given list.
            private Fiber RemoveFirst(ref int ioHead, ref int ioCount)
            {
                int fiberIndex = ioHead;

                if (fiberIndex == -1)
                    return null;

                int nextIndex = m_Entries[fiberIndex].NextIndex;
                int prevIndex = m_Entries[fiberIndex].PrevIndex;

                // If the table only has one entry,
                // just remove it and set the table to empty.
                if (nextIndex == -1)
                {
                    m_Entries[fiberIndex].PrevIndex = -1;
                    ioHead = -1;
                    --ioCount;
                    return m_Entries[fiberIndex].Fiber;
                }

                // Point the next entry at the last entry
                m_Entries[nextIndex].PrevIndex = prevIndex;
                
                // Clear pointers in the current entry
                m_Entries[fiberIndex].NextIndex = -1;
                m_Entries[fiberIndex].PrevIndex = -1;

                // Point to the next entry as the first.
                ioHead = nextIndex;
                --ioCount;

                return m_Entries[fiberIndex].Fiber;
            }

            // Removes an entry from the Fiber list.
            private void RemoveEntry(Fiber inFiber, ref int ioHead, ref int ioCount)
            {
                int fiberIndex = (int)inFiber.Index;
                int nextIndex = m_Entries[fiberIndex].NextIndex;
                int prevIndex = m_Entries[fiberIndex].PrevIndex;

                // If the list is already empty, we can't do anything about it
                if (ioHead == -1)
                    return;

                // Ensure the next entry is pointing back to our previous index.
                m_Entries[nextIndex == -1 ? ioHead : nextIndex].PrevIndex = prevIndex;

                // If we're the first entry, the first entry is now our last
                if (fiberIndex == ioHead)
                {
                    ioHead = nextIndex;
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

            #region Merge Sort

            public void SortActiveList()
            {
                MergeSort(ref m_ActiveHead);
            }

            private void MergeSort(ref int ioHead)
            {
                if (ioHead == -1 || m_Entries[ioHead].NextIndex == -1)
                    return;

                int listA;
                int listB;

                SplitLists(ioHead, out listA, out listB);

                MergeSort(ref listA);
                MergeSort(ref listB);
                
                ioHead = InPlaceSortedMerge(listA, listB);
            }

            // Merging algorithm source: http://stackoverflow.com/a/11273262
            private int InPlaceSortedMerge(int inHeadA, int inHeadB)
            {
                int outputResult = -1;
                int newPtr = outputResult;

                // Store the tail so we can write the correct tail pointer for the merged list.
                int aTail = inHeadA == -1 ? -1 : m_Entries[inHeadA].PrevIndex;
                int bTail = inHeadB == -1 ? -1 : m_Entries[inHeadB].PrevIndex;

                int aPtr = inHeadA;
                int bPtr = inHeadB;

                while(aPtr != -1 && bPtr != -1)
                {
                    if (m_Entries[aPtr].Fiber.CompareTo(m_Entries[bPtr].Fiber) <= 0)
                    {
                        if (outputResult == -1)
                        {
                            outputResult = aPtr;
                            newPtr = outputResult;
                        }
                        else
                        {
                            m_Entries[newPtr].NextIndex = aPtr;
                            m_Entries[aPtr].PrevIndex = newPtr;

                            newPtr = m_Entries[newPtr].NextIndex;
                        }
                        aPtr = m_Entries[aPtr].NextIndex;
                    }
                    else
                    {
                        if (outputResult == -1)
                        {
                            outputResult = bPtr;
                            newPtr = outputResult;
                        }
                        else
                        {
                            m_Entries[newPtr].NextIndex = bPtr;
                            m_Entries[bPtr].PrevIndex = newPtr;

                            newPtr = m_Entries[newPtr].NextIndex;
                        }
                        bPtr = m_Entries[bPtr].NextIndex;
                    }
                }

                if (aPtr != -1)
                {
                    if (outputResult == -1)
                    {
                        outputResult = aPtr;
                    }
                    else
                    {
                        m_Entries[newPtr].NextIndex = aPtr;
                        m_Entries[aPtr].PrevIndex = newPtr;

                        m_Entries[outputResult].PrevIndex = aTail;
                    }
                }
                else if (bPtr != -1)
                {
                    if (outputResult == -1)
                    {
                        outputResult = bPtr;
                    }
                    else
                    {
                        m_Entries[newPtr].NextIndex = bPtr;
                        m_Entries[bPtr].PrevIndex = newPtr;

                        m_Entries[outputResult].PrevIndex = bTail;
                    }
                }

                return outputResult;
            }

            private void SplitLists(int inHead, out int outHeadA, out int outHeadB)
            {
                if (inHead == -1 || m_Entries[inHead].NextIndex == -1)
                {
                    outHeadA = inHead;
                    outHeadB = -1;
                    return;
                }

                int tail = m_Entries[inHead].PrevIndex;

                int halfPtr = inHead;
                int fullPtr = m_Entries[inHead].NextIndex;
                while(fullPtr != -1)
                {
                    fullPtr = m_Entries[fullPtr].NextIndex;
                    if (fullPtr == -1)
                        break;
                    halfPtr = m_Entries[halfPtr].NextIndex;
                    fullPtr = m_Entries[fullPtr].NextIndex;
                }

                outHeadA = inHead;
                outHeadB = m_Entries[halfPtr].NextIndex;

                // Truncate list A
                m_Entries[halfPtr].NextIndex = -1;
                m_Entries[outHeadA].PrevIndex = halfPtr;

                // List B is already truncated
                // Make sure the first entry is pointing at the original tail
                if (outHeadB != -1)
                    m_Entries[outHeadB].PrevIndex = tail;
            }

            #endregion
        }
    }
}

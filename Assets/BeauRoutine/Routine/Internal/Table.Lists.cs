/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    2 March 2018
 * 
 * File:    Table.Lists.cs
 * Purpose: Methods for updating the main and update lists.
*/

namespace BeauRoutine.Internal
{
    /// <summary>
    /// Collection of Fibers.
    /// </summary>
    internal sealed partial class Table
    {
        #region Main List

        // Adds the Fiber to the start of the given list
        private void AddFirst(Fiber inFiber, ref MainList ioList)
        {
            int fiberIndex = (int)inFiber.Index;

            // If there are no free fibers currently,
            // we can just add this as the first and only entry.
            if (ioList.Head == -1)
            {
                ioList.Head = fiberIndex;
                m_Entries[fiberIndex].MainNext = -1;
                m_Entries[fiberIndex].MainPrev = fiberIndex;
            }
            else
            {
                Entry firstEntry = m_Entries[ioList.Head];

                // Point back at the current last entry
                m_Entries[fiberIndex].MainPrev = firstEntry.MainPrev;

                // Point at the old first free entry
                m_Entries[fiberIndex].MainNext = ioList.Head;

                // Point the old first entry at the new first entry
                m_Entries[ioList.Head].MainPrev = fiberIndex;

                ioList.Head = fiberIndex;
            }

            ++ioList.Count;
        }

        // Adds the Fiber to the end of the given list
        private void AddLast(Fiber inFiber, ref MainList ioList)
        {
            int fiberIndex = (int)inFiber.Index;

            // If there are no free fibers currently,
            // we can just add this as the first and only entry.
            if (ioList.Head == -1)
            {
                ioList.Head = fiberIndex;
                m_Entries[fiberIndex].MainNext = -1;
                m_Entries[fiberIndex].MainPrev = fiberIndex;
            }
            else
            {
                Entry firstEntry = m_Entries[ioList.Head];

                // Point the old last entry to this one
                m_Entries[firstEntry.MainPrev].MainNext = fiberIndex;

                // Point the new last entry at the previous one
                m_Entries[fiberIndex].MainPrev = firstEntry.MainPrev;

                // Point the new last entry at nothing
                m_Entries[fiberIndex].MainNext = -1;

                // Point the first entry at this one
                m_Entries[ioList.Head].MainPrev = fiberIndex;
            }

            ++ioList.Count;
        }

        // Removes the first Fiber from the given list.
        private Fiber RemoveFirst(ref MainList ioList)
        {
            int fiberIndex = ioList.Head;

            if (fiberIndex == -1)
                return null;

            int nextIndex = m_Entries[fiberIndex].MainNext;
            int prevIndex = m_Entries[fiberIndex].MainPrev;

            // If the table only has one entry,
            // just remove it and set the table to empty.
            if (nextIndex == -1)
            {
                m_Entries[fiberIndex].MainPrev = -1;
                ioList.Head = -1;
                --ioList.Count;
                return m_Entries[fiberIndex].Fiber;
            }

            // Point the next entry at the last entry
            m_Entries[nextIndex].MainPrev = prevIndex;

            // Clear pointers in the current entry
            m_Entries[fiberIndex].MainNext = -1;
            m_Entries[fiberIndex].MainPrev = -1;

            // Point to the next entry as the first.
            ioList.Head = nextIndex;
            --ioList.Count;

            return m_Entries[fiberIndex].Fiber;
        }

        // Removes an entry from the Fiber list.
        private void RemoveEntry(Fiber inFiber, ref MainList ioList)
        {
            int fiberIndex = (int)inFiber.Index;
            int nextIndex = m_Entries[fiberIndex].MainNext;
            int prevIndex = m_Entries[fiberIndex].MainPrev;

            // If the list is already empty, we can't do anything about it
            if (ioList.Head == -1)
                return;

            // Ensure the next entry is pointing back to our previous index.
            m_Entries[nextIndex == -1 ? ioList.Head : nextIndex].MainPrev = prevIndex;

            // If we're the first entry, the first entry is now our last
            if (fiberIndex == ioList.Head)
            {
                ioList.Head = nextIndex;
            }
            else
            {
                // Previous entry should point back to our next entry
                m_Entries[prevIndex].MainNext = nextIndex;
            }

            m_Entries[fiberIndex].MainNext = -1;
            m_Entries[fiberIndex].MainPrev = -1;

            --ioList.Count;
        }

        #endregion

        #region Update List

        // Adds the Fiber to the start of the given list
        private void AddFirst(Fiber inFiber, ref UpdateList ioList)
        {
            int fiberIndex = (int)inFiber.Index;

            // If there are no free fibers currently,
            // we can just add this as the first and only entry.
            if (ioList.Head == -1)
            {
                ioList.Head = fiberIndex;
                m_Entries[fiberIndex].UpdateNext = -1;
                m_Entries[fiberIndex].UpdatePrev = fiberIndex;
            }
            else
            {
                Entry firstEntry = m_Entries[ioList.Head];

                // Point back at the current last entry
                m_Entries[fiberIndex].UpdatePrev = firstEntry.UpdatePrev;

                // Point at the old first free entry
                m_Entries[fiberIndex].UpdateNext = ioList.Head;

                // Point the old first entry at the new first entry
                m_Entries[ioList.Head].UpdatePrev = fiberIndex;

                ioList.Head = fiberIndex;
            }

            ++ioList.Count;
        }

        // Adds the Fiber to the end of the given list
        private void AddLast(Fiber inFiber, ref UpdateList ioList)
        {
            int fiberIndex = (int)inFiber.Index;

            // If there are no free fibers currently,
            // we can just add this as the first and only entry.
            if (ioList.Head == -1)
            {
                ioList.Head = fiberIndex;
                m_Entries[fiberIndex].UpdateNext = -1;
                m_Entries[fiberIndex].UpdatePrev = fiberIndex;
            }
            else
            {
                Entry firstEntry = m_Entries[ioList.Head];

                // Point the old last entry to this one
                m_Entries[firstEntry.UpdatePrev].UpdateNext = fiberIndex;

                // Point the new last entry at the previous one
                m_Entries[fiberIndex].UpdatePrev = firstEntry.UpdatePrev;

                // Point the new last entry at nothing
                m_Entries[fiberIndex].UpdateNext = -1;

                // Point the first entry at this one
                m_Entries[ioList.Head].UpdatePrev = fiberIndex;
            }

            ++ioList.Count;
        }

        // Removes the first Fiber from the given list.
        private Fiber RemoveFirst(ref UpdateList ioList)
        {
            int fiberIndex = ioList.Head;

            if (fiberIndex == -1)
                return null;

            int nextIndex = m_Entries[fiberIndex].UpdateNext;
            int prevIndex = m_Entries[fiberIndex].UpdatePrev;

            // If the table only has one entry,
            // just remove it and set the table to empty.
            if (nextIndex == -1)
            {
                m_Entries[fiberIndex].UpdatePrev = -1;
                ioList.Head = -1;
                --ioList.Count;
                return m_Entries[fiberIndex].Fiber;
            }

            // Point the next entry at the last entry
            m_Entries[nextIndex].UpdatePrev = prevIndex;

            // Clear pointers in the current entry
            m_Entries[fiberIndex].UpdateNext = -1;
            m_Entries[fiberIndex].UpdatePrev = -1;

            // Point to the next entry as the first.
            ioList.Head = nextIndex;
            --ioList.Count;

            return m_Entries[fiberIndex].Fiber;
        }

        // Removes an entry from the Fiber list.
        private void RemoveEntry(Fiber inFiber, ref UpdateList ioList)
        {
            int fiberIndex = (int)inFiber.Index;
            int nextIndex = m_Entries[fiberIndex].UpdateNext;
            int prevIndex = m_Entries[fiberIndex].UpdatePrev;

            // If the list is already empty, we can't do anything about it
            if (ioList.Head == -1)
                return;

            // Ensure the next entry is pointing back to our previous index.
            m_Entries[nextIndex == -1 ? ioList.Head : nextIndex].UpdatePrev = prevIndex;

            // If we're the first entry, the first entry is now our last
            if (fiberIndex == ioList.Head)
            {
                ioList.Head = nextIndex;
            }
            else
            {
                // Previous entry should point back to our next entry
                m_Entries[prevIndex].UpdateNext = nextIndex;
            }

            m_Entries[fiberIndex].UpdateNext = -1;
            m_Entries[fiberIndex].UpdatePrev = -1;

            --ioList.Count;
        }

        #endregion

        #region Yield List

        // Adds the Fiber to the start of the given list
        private void AddFirst(Fiber inFiber, ref YieldList ioList)
        {
            int fiberIndex = (int)inFiber.Index;

            // If there are no free fibers currently,
            // we can just add this as the first and only entry.
            if (ioList.Head == -1)
            {
                ioList.Head = fiberIndex;
                m_Entries[fiberIndex].YieldNext = -1;
                m_Entries[fiberIndex].YieldPrev = fiberIndex;
            }
            else
            {
                Entry firstEntry = m_Entries[ioList.Head];

                // Point back at the current last entry
                m_Entries[fiberIndex].YieldPrev = firstEntry.YieldPrev;

                // Point at the old first free entry
                m_Entries[fiberIndex].YieldNext = ioList.Head;

                // Point the old first entry at the new first entry
                m_Entries[ioList.Head].YieldPrev = fiberIndex;

                ioList.Head = fiberIndex;
            }

            ++ioList.Count;
        }

        // Adds the Fiber to the end of the given list
        private void AddLast(Fiber inFiber, ref YieldList ioList)
        {
            int fiberIndex = (int)inFiber.Index;

            // If there are no free fibers currently,
            // we can just add this as the first and only entry.
            if (ioList.Head == -1)
            {
                ioList.Head = fiberIndex;
                m_Entries[fiberIndex].YieldNext = -1;
                m_Entries[fiberIndex].YieldPrev = fiberIndex;
            }
            else
            {
                Entry firstEntry = m_Entries[ioList.Head];

                // Point the old last entry to this one
                m_Entries[firstEntry.YieldPrev].YieldNext = fiberIndex;

                // Point the new last entry at the previous one
                m_Entries[fiberIndex].YieldPrev = firstEntry.YieldPrev;

                // Point the new last entry at nothing
                m_Entries[fiberIndex].YieldNext = -1;

                // Point the first entry at this one
                m_Entries[ioList.Head].YieldPrev = fiberIndex;
            }

            ++ioList.Count;
        }

        // Removes the first Fiber from the given list.
        private Fiber RemoveFirst(ref YieldList ioList)
        {
            int fiberIndex = ioList.Head;

            if (fiberIndex == -1)
                return null;

            int nextIndex = m_Entries[fiberIndex].YieldNext;
            int prevIndex = m_Entries[fiberIndex].YieldPrev;

            // If the table only has one entry,
            // just remove it and set the table to empty.
            if (nextIndex == -1)
            {
                m_Entries[fiberIndex].YieldPrev = -1;
                ioList.Head = -1;
                --ioList.Count;
                return m_Entries[fiberIndex].Fiber;
            }

            // Point the next entry at the last entry
            m_Entries[nextIndex].YieldPrev = prevIndex;

            // Clear pointers in the current entry
            m_Entries[fiberIndex].YieldNext = -1;
            m_Entries[fiberIndex].YieldPrev = -1;

            // Point to the next entry as the first.
            ioList.Head = nextIndex;
            --ioList.Count;

            return m_Entries[fiberIndex].Fiber;
        }

        // Removes an entry from the Fiber list.
        private void RemoveEntry(Fiber inFiber, ref YieldList ioList)
        {
            int fiberIndex = (int)inFiber.Index;
            int nextIndex = m_Entries[fiberIndex].YieldNext;
            int prevIndex = m_Entries[fiberIndex].YieldPrev;

            // If the list is already empty, we can't do anything about it
            if (ioList.Head == -1)
                return;

            // Ensure the next entry is pointing back to our previous index.
            m_Entries[nextIndex == -1 ? ioList.Head : nextIndex].YieldPrev = prevIndex;

            // If we're the first entry, the first entry is now our last
            if (fiberIndex == ioList.Head)
            {
                ioList.Head = nextIndex;
            }
            else
            {
                // Previous entry should point back to our next entry
                m_Entries[prevIndex].YieldNext = nextIndex;
            }

            m_Entries[fiberIndex].YieldNext = -1;
            m_Entries[fiberIndex].YieldPrev = -1;

            --ioList.Count;
        }

        #endregion

        #region Merge Sort Update
        
        private void SortUpdateList(ref UpdateList ioList)
        {
            MergeSort_UpdateList(ref ioList.Head);
        }

        private void MergeSort_UpdateList(ref int ioHead)
        {
            if (ioHead == -1 || m_Entries[ioHead].UpdateNext == -1)
                return;

            int listA;
            int listB;

            SplitLists_UpdateList(ioHead, out listA, out listB);

            MergeSort_UpdateList(ref listA);
            MergeSort_UpdateList(ref listB);

            ioHead = InPlaceSortedMerge_UpdateList(listA, listB);
        }

        // Merging algorithm source: http://stackoverflow.com/a/11273262
        private int InPlaceSortedMerge_UpdateList(int inHeadA, int inHeadB)
        {
            int outputResult = -1;
            int newPtr = outputResult;

            // Store the tail so we can write the correct tail pointer for the merged list.
            int aTail = inHeadA == -1 ? -1 : m_Entries[inHeadA].UpdatePrev;
            int bTail = inHeadB == -1 ? -1 : m_Entries[inHeadB].UpdatePrev;

            int aPtr = inHeadA;
            int bPtr = inHeadB;

            while (aPtr != -1 && bPtr != -1)
            {
                if (m_Entries[aPtr].Fiber.Priority >= m_Entries[bPtr].Fiber.Priority)
                {
                    if (outputResult == -1)
                    {
                        outputResult = aPtr;
                        newPtr = outputResult;
                    }
                    else
                    {
                        m_Entries[newPtr].UpdateNext = aPtr;
                        m_Entries[aPtr].UpdatePrev = newPtr;

                        newPtr = m_Entries[newPtr].UpdateNext;
                    }
                    aPtr = m_Entries[aPtr].UpdateNext;
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
                        m_Entries[newPtr].UpdateNext = bPtr;
                        m_Entries[bPtr].UpdatePrev = newPtr;

                        newPtr = m_Entries[newPtr].UpdateNext;
                    }
                    bPtr = m_Entries[bPtr].UpdateNext;
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
                    m_Entries[newPtr].UpdateNext = aPtr;
                    m_Entries[aPtr].UpdatePrev = newPtr;

                    m_Entries[outputResult].UpdatePrev = aTail;
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
                    m_Entries[newPtr].UpdateNext = bPtr;
                    m_Entries[bPtr].UpdatePrev = newPtr;

                    m_Entries[outputResult].UpdatePrev = bTail;
                }
            }

            return outputResult;
        }

        private void SplitLists_UpdateList(int inHead, out int outHeadA, out int outHeadB)
        {
            if (inHead == -1 || m_Entries[inHead].UpdateNext == -1)
            {
                outHeadA = inHead;
                outHeadB = -1;
                return;
            }

            int tail = m_Entries[inHead].UpdatePrev;

            int halfPtr = inHead;
            int fullPtr = m_Entries[inHead].UpdateNext;
            while (fullPtr != -1)
            {
                fullPtr = m_Entries[fullPtr].UpdateNext;
                if (fullPtr == -1)
                    break;
                halfPtr = m_Entries[halfPtr].UpdateNext;
                fullPtr = m_Entries[fullPtr].UpdateNext;
            }

            outHeadA = inHead;
            outHeadB = m_Entries[halfPtr].UpdateNext;

            // Truncate list A
            m_Entries[halfPtr].UpdateNext = -1;
            m_Entries[outHeadA].UpdatePrev = halfPtr;

            // List B is already truncated
            // Make sure the first entry is pointing at the original tail
            if (outHeadB != -1)
                m_Entries[outHeadB].UpdatePrev = tail;
        }

        #endregion
    
        #region Merge Sort Yield
        
        private void SortYieldList(ref YieldList ioList)
        {
            MergeSort_YieldList(ref ioList.Head);
        }

        private void MergeSort_YieldList(ref int ioHead)
        {
            if (ioHead == -1 || m_Entries[ioHead].YieldNext == -1)
                return;

            int listA;
            int listB;

            SplitLists_YieldList(ioHead, out listA, out listB);

            MergeSort_YieldList(ref listA);
            MergeSort_YieldList(ref listB);

            ioHead = InPlaceSortedMerge_YieldList(listA, listB);
        }

        // Merging algorithm source: http://stackoverflow.com/a/11273262
        private int InPlaceSortedMerge_YieldList(int inHeadA, int inHeadB)
        {
            int outputResult = -1;
            int newPtr = outputResult;

            // Store the tail so we can write the correct tail pointer for the merged list.
            int aTail = inHeadA == -1 ? -1 : m_Entries[inHeadA].YieldPrev;
            int bTail = inHeadB == -1 ? -1 : m_Entries[inHeadB].YieldPrev;

            int aPtr = inHeadA;
            int bPtr = inHeadB;

            while (aPtr != -1 && bPtr != -1)
            {
                if (m_Entries[aPtr].Fiber.Priority >= m_Entries[bPtr].Fiber.Priority)
                {
                    if (outputResult == -1)
                    {
                        outputResult = aPtr;
                        newPtr = outputResult;
                    }
                    else
                    {
                        m_Entries[newPtr].YieldNext = aPtr;
                        m_Entries[aPtr].YieldPrev = newPtr;

                        newPtr = m_Entries[newPtr].YieldNext;
                    }
                    aPtr = m_Entries[aPtr].YieldNext;
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
                        m_Entries[newPtr].YieldNext = bPtr;
                        m_Entries[bPtr].YieldPrev = newPtr;

                        newPtr = m_Entries[newPtr].YieldNext;
                    }
                    bPtr = m_Entries[bPtr].YieldNext;
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
                    m_Entries[newPtr].YieldNext = aPtr;
                    m_Entries[aPtr].YieldPrev = newPtr;

                    m_Entries[outputResult].YieldPrev = aTail;
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
                    m_Entries[newPtr].YieldNext = bPtr;
                    m_Entries[bPtr].YieldPrev = newPtr;

                    m_Entries[outputResult].YieldPrev = bTail;
                }
            }

            return outputResult;
        }

        private void SplitLists_YieldList(int inHead, out int outHeadA, out int outHeadB)
        {
            if (inHead == -1 || m_Entries[inHead].YieldNext == -1)
            {
                outHeadA = inHead;
                outHeadB = -1;
                return;
            }

            int tail = m_Entries[inHead].YieldPrev;

            int halfPtr = inHead;
            int fullPtr = m_Entries[inHead].YieldNext;
            while (fullPtr != -1)
            {
                fullPtr = m_Entries[fullPtr].YieldNext;
                if (fullPtr == -1)
                    break;
                halfPtr = m_Entries[halfPtr].YieldNext;
                fullPtr = m_Entries[fullPtr].YieldNext;
            }

            outHeadA = inHead;
            outHeadB = m_Entries[halfPtr].YieldNext;

            // Truncate list A
            m_Entries[halfPtr].YieldNext = -1;
            m_Entries[outHeadA].YieldPrev = halfPtr;

            // List B is already truncated
            // Make sure the first entry is pointing at the original tail
            if (outHeadB != -1)
                m_Entries[outHeadB].YieldPrev = tail;
        }

        #endregion
    }
}

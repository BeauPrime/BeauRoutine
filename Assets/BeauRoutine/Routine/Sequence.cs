/*
 * Copyright (C) 2016-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    21 Nov 2016
 * 
 * File:    Sequence.cs
 * Purpose: Allows for the sequencing of coroutines and actions
 *          without needing to create custom coroutines
 *          every time.
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace BeauRoutine
{
    /// <summary>
    /// Construct routines as a sequence.
    /// </summary>
    public sealed class Sequence : IEnumerator, IDisposable
    {
        private enum NodeType : byte
        {
            Value,
            Routine,
            Action,
            YieldIEnumerator,
            YieldResult
        }

        private struct Node
        {
            public NodeType Type;
            public object Value;

            public Node(NodeType inType, object inValue)
            {
                Type = inType;
                Value = inValue;
            }
        }

        private object m_Current;
        private int m_Index;
        private List<Node> m_Nodes;

        private Sequence()
        {
            m_Index = -1;
            m_Nodes = new List<Node>();
            m_Current = null;
        }

        /// <summary>
        /// Executes a routine.
        /// </summary>
        public Sequence Then(IEnumerator inNext)
        {
            AddNode(NodeType.Routine, inNext);
            return this;
        }

        /// <summary>
        /// Executes an action.
        /// </summary>
        public Sequence Then(Action inAction)
        {
            AddNode(NodeType.Action, inAction);
            return this;
        }

        /// <summary>
        /// Executes the routines in parallel.
        /// </summary>
        public Sequence Combine(params IEnumerator[] inNexts)
        {
            AddNode(NodeType.Routine, Routine.Combine(inNexts));
            return this;
        }

        /// <summary>
        /// Waits for the given number of seconds.
        /// </summary>
        public Sequence Wait(float inSeconds)
        {
            AddNode(NodeType.Value, inSeconds);
            return this;
        }

        /// <summary>
        /// Waits until the next frame.
        /// </summary>
        public Sequence Wait()
        {
            AddNode(NodeType.Value, null);
            return this;
        }

        /// <summary>
        /// Yields the result of the given function.
        /// </summary>
        public Sequence Yield(Func<object> inFunction)
        {
            AddNode(NodeType.YieldResult, inFunction);
            return this;
        }

        /// <summary>
        /// Yields the IEnumerator from the given function.
        /// </summary>
        public Sequence Yield(Func<IEnumerator> inFunction)
        {
            AddNode(NodeType.YieldIEnumerator, inFunction);
            return this;
        }

        /// <summary>
        /// Yields the given value.
        /// </summary>
        public Sequence Yield(object inValue)
        {
            AddNode(NodeType.Value, inValue);
            return this;
        }

        #region Nodes

        private void AddNode(NodeType inType, object inValue)
        {
            if (m_Index != -1)
                throw new InvalidOperationException("Cannot add to a Sequence while it's in operation!");
            m_Nodes.Add(new Node(inType, inValue));
        }

        #endregion

        #region IRoutineEnumerator

        object IEnumerator.Current
        {
            get { return m_Current; }
        }

        bool IEnumerator.MoveNext()
        {
            m_Current = null;
            while(true)
            {
                ++m_Index;
                if (m_Index >= m_Nodes.Count)
                    return false;
                Node n = m_Nodes[m_Index];
                switch(n.Type)
                {
                    case NodeType.Value:
                    case NodeType.Routine:
                        m_Current = n.Value;
                        return true;
                    case NodeType.Action:
                        ((Action)n.Value).Invoke();
                        break;
                    case NodeType.YieldIEnumerator:
                        m_Current = ((Func<IEnumerator>)n.Value).Invoke();
                        return true;
                    case NodeType.YieldResult:
                        m_Current = ((Func<object>)n.Value).Invoke();
                        return true;
                }
            }
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            for(int i = m_Index + 1; i < m_Nodes.Count; ++i)
            {
                switch(m_Nodes[i].Type)
                {
                    case NodeType.Routine:
                        ((IDisposable)m_Nodes[i].Value).Dispose();
                        break;
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates a new sequence.
        /// </summary>
        static public Sequence Create()
        {
            return new Sequence();
        }

        /// <summary>
        /// Creates a new sequence starting with the given routine.
        /// </summary>
        static public Sequence Create(IEnumerator inStart)
        {
            return new Sequence().Then(inStart);
        }
        
        /// <summary>
        /// Creates a new sequence staring with the given routines.
        /// </summary>
        static public Sequence Create(params IEnumerator[] inRoutines)
        {
            Sequence s = new Sequence();
            for (int i = 0; i < inRoutines.Length; ++i)
                s.Then(inRoutines[i]);
            return s;
        }

        /// <summary>
        /// Creates a new sequence starting with the given routine.
        /// </summary>
        static public Sequence Create(Action inStart)
        {
            return new Sequence().Then(inStart);
        }

        public override string ToString()
        {
            return "Sequence (" + m_Index.ToString() + " / " + m_Nodes.Count.ToString() + ")";
        }
    }
}

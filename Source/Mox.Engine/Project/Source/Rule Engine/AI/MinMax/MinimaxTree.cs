// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mox.AI
{
    /// <summary>
    /// Interface of a minmax tree.
    /// </summary>
    public interface IMinimaxTree
    {
        #region Properties

        /// <summary>
        /// Current depth of the tree.
        /// </summary>
        int Depth
        {
            get;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a new node in the minimax tree.
        /// </summary>
        /// <param name="isMaximizing">Whether the new node is a maximizing node.</param>
        /// <param name="result">The result to associated with the node.</param>
        /// <param name="debugInfo">Debug info, if wanted.</param>
        void BeginNode(bool isMaximizing, object result, string debugInfo);

        /// <summary>
        /// Ends the current node.
        /// </summary>
        /// <returns>False if the search can be beta-cutoff.</returns>
        bool EndNode();

        /// <summary>
        /// Evaluates the current node (must be a leaf node).
        /// </summary>
        /// <param name="value"></param>
        void Evaluate(float value);

        /// <summary>
        /// Discards the current node so it's not taken into account.
        /// </summary>
        void Discard();

        #endregion
    }

    /// <summary>
    /// A minimax tree
    /// </summary>
    public class MinimaxTree : IMinimaxTree
    {
        #region Inner Types

        public sealed class Node
        {
            public readonly bool IsMaximizing;
            public readonly Node Parent;

            public float Alpha = StartingMinValue;
            public float Beta = StartingMaxValue;
            public object m_result;
            public bool HasResult;

            private bool m_initialized;

#if DEBUG
            public List<object> Choices;
#endif

            public object Result
            {
                get { return m_result; }
                set 
                {
                    m_result = value;
                    HasResult = true;
                }
            }

            public Node(bool isMaximizing, Node parent)
            {
                IsMaximizing = isMaximizing;
                Parent = parent;

                if (Parent != null)
                {
                    Parent.Initialize(IsMaximizing != Parent.IsMaximizing);
                }
            }

            internal void Initialize(bool swap)
            {
                if (Parent != null)
                {
                    Parent.Initialize(IsMaximizing != Parent.IsMaximizing);

                    if (!m_initialized)
                    {
                        if (swap)
                        {
                            Alpha = Parent.Beta;
                            Beta = Parent.Alpha;
                        }
                        else
                        {
                            Alpha = Parent.Alpha;
                            Beta = Parent.Beta;
                        }
                    }
                }
                m_initialized = true;
            }

            [Conditional("DEBUG")]
            internal void Assert_is_initialized()
            {
                Debug.Assert(m_initialized, "Node is not initialized");
            }
        }

        #endregion

        #region Constants

        internal const float StartingMinValue = float.MinValue;
        internal const float StartingMaxValue = float.MaxValue;

        public const float MinValue = StartingMinValue;
        public const float MaxValue = StartingMaxValue;

        #endregion

        #region Variables

        private readonly Stack<Node> m_nodes = new Stack<Node>();

#if DEBUG
        private int m_numEvaluations;
#endif

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public MinimaxTree()
        {
            Node node = new Node(true, null);
            m_nodes.Push(node);

            node.Alpha = StartingMinValue;
            EnableDebugInfo = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Current minimax node.
        /// </summary>
        public Node CurrentNode
        {
            get { return m_nodes.Peek(); }
        }

        /// <summary>
        /// Current depth of the tree.
        /// </summary>
        public int Depth
        {
            get { return m_nodes.Count; }
        }

#if DEBUG
        public int NumEvaluations
        {
            get { return m_numEvaluations; }
        }
#endif

        public bool EnableDebugInfo
        {
            get;
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a new node in the minimax tree.
        /// </summary>
        /// <param name="maximizingNode">Whether the new node is a maximizing node.</param>
        /// <param name="result">The result to associated with the node.</param>
        public void BeginNode(bool maximizingNode, object result)
        {
            BeginNode(maximizingNode, result, null);
        }

        /// <summary>
        /// Begins a new node in the minimax tree.
        /// </summary>
        /// <param name="isMaximizing">Whether the new node is a maximizing node.</param>
        /// <param name="result">The result to associated with the node.</param>
        /// <param name="debugInfo">Debug info, if wanted.</param>
        public void BeginNode(bool isMaximizing, object result, string debugInfo)
        {
            Debug_BeginNode(isMaximizing, result, debugInfo);

            Node parentNode = CurrentNode;

            Node node = new Node(isMaximizing, parentNode);

#if !DEBUG
            if (m_nodes.Count <= 1)
#endif
            {
                node.Result = result;
            }

            m_nodes.Push(node);
        }

        /// <summary>
        /// Ends the current node.
        /// </summary>
        /// <returns>False if the search can be beta-cutoff.</returns>
        public bool EndNode()
        {
            Debug.Assert(m_nodes.Count > 1, "Cannot end the root node");

            Node disposedNode = m_nodes.Pop();
            Node nodeToUpdate = CurrentNode;

            float oldAlpha = nodeToUpdate.Alpha;
            float oldBeta = nodeToUpdate.Beta;

            bool result = Update(nodeToUpdate, disposedNode, m_nodes.Count == 1);
            Debug_EndNode(!result, oldAlpha, oldBeta, nodeToUpdate);
            return result;
        }

        /// <summary>
        /// Evaluates the current node (must be a leaf node).
        /// </summary>
        /// <remarks>
        /// Evaluation must always be done relatively to the maximizing player (bigger number means situation favors maximizing player)
        /// </remarks>
        /// <param name="value"></param>
        public void Evaluate(float value)
        {
#if DEBUG
            m_numEvaluations++;
#endif
            Debug_Evaluate(value);
            CurrentNode.Alpha = value;
        }

        /// <summary>
        /// Discards the current node so it's not taken into account.
        /// </summary>
        public void Discard()
        {
            Debug_Discard();
            float value = CurrentNode.Parent.IsMaximizing ? MinValue : MaxValue;
            CurrentNode.Alpha = value;
        }

        #region Implementation

        private static bool Update(Node nodeToUpdate, Node evaluatedNode, bool rootNode)
        {
            nodeToUpdate.Assert_is_initialized();

            int sign = evaluatedNode.IsMaximizing ? -1 : +1;

            bool updated = Compare(ref nodeToUpdate.Alpha, evaluatedNode.Alpha, sign);
            if (updated)
            {
#if DEBUG
                nodeToUpdate.Choices = evaluatedNode.Choices ?? new List<object>();
                nodeToUpdate.Choices.Add(evaluatedNode.Result);
#endif
                if (rootNode)
                {
                    nodeToUpdate.Result = evaluatedNode.Result;
                }
            }

            if (evaluatedNode.IsMaximizing != nodeToUpdate.IsMaximizing)
            {
                return nodeToUpdate.Alpha.CompareTo(nodeToUpdate.Beta) * sign > 0;
            }

            // Never cutoff when both are max or min
            return true;
        }

        private static bool Compare(ref float a, float b, int compareResult)
        {
            if (a.CompareTo(b) == compareResult)
            {
                a = b;
                return true;
            }

            return false;
        }

        #endregion

        #endregion

        #region Debug

        private readonly StringBuilder m_debugInfo = new StringBuilder();

        public string DebugInfo
        {
            get { return m_debugInfo.ToString(); }
        }

        [Conditional("DEBUG")]
        private void DebugWrite(string msg)
        {
            DebugWrite(msg, 0);
        }

        [Conditional("DEBUG")]
        private void DebugWrite(string msg, int depthOffset)
        {
            if (Configuration.Debug_Minimax_tree)
#pragma warning disable 162
            {
                if (EnableDebugInfo)
                {
                    string indent = new string(' ', (Depth + depthOffset - 1)*4);
                    //Debug.WriteLine(indent + msg);
                    m_debugInfo.AppendLine(indent + msg);
                }
            }
#pragma warning restore 162
        }

        private static string Format_AlphaBeta(float value)
        {
            if (value >= MaxValue)
            {
                return "+infinite";
            }
            else if (value <= MinValue)
            {
                return "-infinite";
            }
            else
            {
                return value.ToString();
            }
        }

        private static string Format_Choice(object choice)
        {
            if (ReferenceEquals(choice, null))
            {
                return "[null]";
            }

            return choice.ToString();
        }

        [Conditional("DEBUG")]
        private void Debug_BeginNode(bool maximizing, object choice, string debugInfo)
        {
            DebugWrite(string.Format("Trying {1} {2} choice {0}", Format_Choice(choice), maximizing ? "maximizing" : "minimizing", debugInfo));
            DebugWrite("{");
        }

        [Conditional("DEBUG")]
        private void Debug_EndNode(bool cutoff, float oldAlpha, float oldBeta, Node updatedNode)
        {
            if (oldAlpha != updatedNode.Alpha || oldBeta != updatedNode.Beta)
            {
                DebugWrite(string.Format("Updating node (currently [{0}, {1}]) to [{2}, {3}]", Format_AlphaBeta(oldAlpha), Format_AlphaBeta(oldBeta), Format_AlphaBeta(updatedNode.Alpha), Format_AlphaBeta(updatedNode.Beta)), 1);
            }
            else
            {
                DebugWrite(string.Format("Leaving node unchanged [{0}, {1}]", Format_AlphaBeta(updatedNode.Alpha), Format_AlphaBeta(updatedNode.Beta)), 1);
            }

            if (cutoff)
            {
                DebugWrite("Cutting off", 1);
            }

            DebugWrite("}");
        }

        [Conditional("DEBUG")]
        private void Debug_Evaluate(float value)
        {
            Throw.ArgumentOutOfRangeIf(value < MinValue || value > MaxValue, "Invalid evaluation value", "value");

            DebugWrite(string.Format("Evaluating to {0}", value));
        }

        [Conditional("DEBUG")]
        private void Debug_Discard()
        {
            DebugWrite("Discarding...");
        }

        #endregion
    }
}

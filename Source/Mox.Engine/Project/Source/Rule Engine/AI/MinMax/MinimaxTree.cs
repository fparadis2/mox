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
using System.Text;
using Mox.Flow;

namespace Mox.AI
{
    /// <summary>
    /// A minimax tree      
    /// </summary>
    public class MinimaxTree : IMinimaxTree
    {
        #region Inner Types

        public sealed class Node
        {
            public readonly Node Parent;

            public float Alpha = StartingMinValue;
            public float Beta = StartingMaxValue;
            public object m_result;
            public bool HasResult;

            private bool m_initialized;
            private bool m_isMaximizing;

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

            public bool IsMaximizing
            {
                get
                {
                    Assert_is_initialized();
                    return m_isMaximizing;
                }
                private set { m_isMaximizing = value; }
            }

            public Node(Node parent)
            {
                IsMaximizing = true;
                Parent = parent;
            }

            internal Node()
            {
                IsMaximizing = true;
                m_initialized = true;
            }

            internal void Initialize(bool isMaximizing)
            {
                Debug.Assert(!m_initialized);
                IsMaximizing = isMaximizing;

                if (isMaximizing != Parent.IsMaximizing)
                {
                    Alpha = Parent.Beta;
                    Beta = Parent.Alpha;
                }
                else
                {
                    Alpha = Parent.Alpha;
                    Beta = Parent.Beta;
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
            Node node = new Node();
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
        /// <param name="result">The result to associated with the node.</param>
        /// <param name="debugInfo">Debug info, if wanted.</param>
        public void BeginNode(object result, string debugInfo = null)
        {
            Debug_BeginNode(result, debugInfo);

            Node parentNode = CurrentNode;

            parentNode.Assert_is_initialized();

            Node node = new Node(parentNode);

#if !DEBUG
            if (m_nodes.Count <= 1)
#endif
            {
                node.Result = result;
            }

            m_nodes.Push(node);
        }

        public void InitializeNode(bool isMaximizing)
        {
            CurrentNode.Initialize(isMaximizing);
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
            CurrentNode.Parent.Assert_is_initialized();

            Debug_Discard();
            float value = CurrentNode.Parent.IsMaximizing ? MinValue : MaxValue;
            CurrentNode.Alpha = value;
        }

        public bool ConsiderTranspositionTable(int hash)
        {
            return true;
        }

        public bool TryGetBestResult(out object result)
        {
            Debug.Assert(Depth == 1);
            result = CurrentNode.Result;
            return CurrentNode.HasResult;
        }

        public float GetBestValue()
        {
            Debug.Assert(Depth == 1);
            return CurrentNode.Alpha;
        }

        #region Implementation

        private static bool Update(Node nodeToUpdate, Node evaluatedNode, bool rootNode)
        {
            nodeToUpdate.Assert_is_initialized();

            int sign = nodeToUpdate.IsMaximizing ? -1 : +1;

            if (nodeToUpdate.Alpha.CompareTo(evaluatedNode.Alpha) == sign)
            {
                nodeToUpdate.Alpha = evaluatedNode.Alpha;
#if DEBUG
                nodeToUpdate.Choices = evaluatedNode.Choices ?? new List<object>();
                nodeToUpdate.Choices.Add(evaluatedNode.Result);
#endif
                if (rootNode)
                {
                    nodeToUpdate.Result = evaluatedNode.Result;
                }
            }

            if (nodeToUpdate.Parent != null && nodeToUpdate.Parent.IsMaximizing != nodeToUpdate.IsMaximizing)
            {
                return nodeToUpdate.Alpha.CompareTo(nodeToUpdate.Beta) * sign > 0;
            }

            // Never cutoff when both are max or min
            return true;
        }

        #endregion

        #endregion

        #region Debug

        private readonly StringBuilder m_debugInfo = new StringBuilder();

        public string DebugInfo
        {
            get { return m_debugInfo.ToString(); }
        }

        // For debug purposes
        public Game Game { get; set; }

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

        private string Format_Choice(object choice)
        {
            if (ReferenceEquals(choice, null))
            {
                return "[null]";
            }

            if (Game != null && choice is IChoiceResult)
            {
                return ((IChoiceResult) choice).ToString(Game);
            }

            return choice.ToString();
        }

        [Conditional("DEBUG")]
        private void Debug_BeginNode(object choice, string debugInfo)
        {
            DebugWrite(string.Format("Trying {1} choice {0}", Format_Choice(choice), debugInfo));
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

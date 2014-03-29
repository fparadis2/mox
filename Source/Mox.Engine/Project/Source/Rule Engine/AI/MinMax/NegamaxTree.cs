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
using Microsoft.SqlServer.Server;
using Mox.Flow;

namespace Mox.AI
{
    /// <summary>
    /// A negamax tree
    /// </summary>
    /// <remarks>
    /// http://en.wikipedia.org/wiki/Negamax
    /// http://homepages.cwi.nl/~paulk/theses/Carolus.pdf
    /// http://web.archive.org/web/20070822204120/www.seanet.com/~brucemo/topics/hashing.htm
    /// </remarks>
    public class NegamaxTree : IMinimaxTree
    {
        #region Constants

        public const float MinValue = float.MinValue;
        public const float MaxValue = float.MaxValue;

        #endregion

        #region Variables

        private readonly Stack<Node> m_nodes = new Stack<Node>();
        private readonly TranspositionTable m_transpositionTable = new TranspositionTable();

#if DEBUG
        private int m_numEvaluations;
#endif

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public NegamaxTree()
        {
            Node node = new Node();
            m_nodes.Push(node);

            Debug_Indent(node.IsMaximizing);

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

            Node node = new Node(CurrentNode);

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
            Debug_Indent(isMaximizing);
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

            UpdateTranspositionTable(nodeToUpdate, disposedNode);

            float oldAlpha = nodeToUpdate.Alpha;
            float oldBeta = nodeToUpdate.Beta;

            bool result = Update(nodeToUpdate, disposedNode, m_nodes.Count == 1);
            Debug_EndNode(!result, oldAlpha, oldBeta, nodeToUpdate, disposedNode);
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
            Debug_Indent(CurrentNode.Parent.IsMaximizing);
            Debug_Evaluate(value);
            CurrentNode.BestValue = CurrentNode.Parent.ComparisonSign * value;
        }

        /// <summary>
        /// Discards the current node so it's not taken into account.
        /// </summary>
        public void Discard()
        {
            Debug_Indent(CurrentNode.Parent.IsMaximizing);
            Debug_Discard();
            CurrentNode.BestValue = MinValue;
        }

        /// <summary>
        /// Checks if the game hash has been seen before.
        /// If so, returns false and this node evaluation can end early.
        /// </summary>
        public bool ConsiderTranspositionTable(int hash)
        {
            var currentNode = CurrentNode;

#if DEBUG
            Throw.InvalidOperationIf(!currentNode.IsInitialized, "Must initialize node before considering TTable");
#endif

            TranspositionTableEntry entry;
            if (m_transpositionTable.TryLookup(hash, out entry) && entry.Depth <= Depth)
            {
                Throw.InvalidOperationIf(currentNode.IsMaximizing != entry.IsMaximizing, "Not supposed to find entries of different color in TTable");

                switch (entry.Type)
                {
                    case TranspositionTableEntryType.Exact:
                        Debug_TranspositionTable_ExactHit(hash, entry.Value);
                        currentNode.BestValue = entry.Value;
                        return false;
                    case TranspositionTableEntryType.LowerBound:
                        Debug_TranspositionTable_LowerBound(hash, entry.Value, currentNode.Alpha);
                        currentNode.Alpha = Math.Max(currentNode.Alpha, entry.Value);
                        break;
                    case TranspositionTableEntryType.UpperBound:
                        Debug_TranspositionTable_UpperBound(hash, entry.Value, currentNode.Beta);
                        currentNode.Beta = Math.Min(currentNode.Beta, entry.Value);
                        break;
                    
                    default:
                        throw new NotImplementedException();
                }

                if (currentNode.Alpha >= currentNode.Beta)
                {
                    currentNode.BestValue = entry.Value;
                    return false;
                }
            }

            currentNode.Hash = hash;
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
            return CurrentNode.BestValue;
        }

        private void UpdateTranspositionTable(Node nodeToUpdate, Node evaluatedNode)
        {
            if (!evaluatedNode.Hash.HasValue)
                return;

            int hash = evaluatedNode.Hash.Value;
            var bestValue = evaluatedNode.BestValue;

            var entry = new TranspositionTableEntry()
            {
                Depth = Depth + 1, // Node is already popped
                Value = bestValue,
                IsMaximizing = evaluatedNode.IsMaximizing,
                Type = TranspositionTableEntryType.Exact
            };

            if (bestValue <= evaluatedNode.OriginalAlpha)
            {
                entry.Value = evaluatedNode.BestValue;
                entry.Type = TranspositionTableEntryType.LowerBound;
            }
            else if (bestValue >= evaluatedNode.Beta)
            {
                entry.Value = evaluatedNode.BestValue;
                entry.Type = TranspositionTableEntryType.UpperBound;
            }

            m_transpositionTable.Store(hash, entry);
        }

        private static bool Update(Node nodeToUpdate, Node evaluatedNode, bool rootNode)
        {
            var value = evaluatedNode.GetBestValueWithRegardsTo(nodeToUpdate);

            if (value > nodeToUpdate.BestValue)
            {
                nodeToUpdate.BestValue = value;

#if DEBUG
                nodeToUpdate.Choices = evaluatedNode.Choices ?? new List<object>();
                nodeToUpdate.Choices.Add(evaluatedNode.Result);
#endif
                if (rootNode)
                {
                    nodeToUpdate.Result = evaluatedNode.Result;
                }
            }

            nodeToUpdate.Alpha = Math.Max(nodeToUpdate.Alpha, value);
            return nodeToUpdate.Alpha < nodeToUpdate.Beta;
        }

        #endregion

        #region Debug

        private readonly StringBuilder m_debugInfo = new StringBuilder();
        private readonly StringBuilder m_indent = new StringBuilder();

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
                    m_debugInfo.AppendLine(m_indent + msg);
                }
            }
#pragma warning restore 162
        }

        [Conditional("DEBUG")]
        private void Debug_Indent(bool isMaximizing)
        {
            if (isMaximizing)
                m_indent.Append("+   ");
            else
                m_indent.Append("-   ");
        }

        [Conditional("DEBUG")]
        private void Debug_Unindent()
        {
            m_indent.Remove(m_indent.Length - 4, 4);
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
        }

        [Conditional("DEBUG")]
        private void Debug_EndNode(bool cutoff, float oldAlpha, float oldBeta, Node updatedNode, Node endedNode)
        {
            Debug_Unindent();

            string transpositionTableUpdate = null;

            if (endedNode.Hash.HasValue)
            {
                var bestValue = endedNode.BestValue;

                if (bestValue <= endedNode.OriginalAlpha)
                {
                    transpositionTableUpdate = string.Format(" - updated hash [{0}] with lowerbound (alpha) value {1} >= best value {2}", endedNode.Hash.Value, endedNode.OriginalAlpha, bestValue);
                }
                else if (bestValue >= endedNode.Beta)
                {
                    transpositionTableUpdate = string.Format(" - updated hash [{0}] with upperbound (beta) value {1} <= best value {2}", endedNode.Hash.Value, endedNode.Beta, bestValue);
                }
                else
                {
                    transpositionTableUpdate = string.Format(" - updated hash [{0}] with exact value {1}", endedNode.Hash.Value, bestValue);
                }
            }

            if (oldAlpha != updatedNode.Alpha || oldBeta != updatedNode.Beta)
            {
                DebugWrite(string.Format("Updating node (currently [{0}, {1}]) to [{2}, {3}]{4}", Format_AlphaBeta(oldAlpha), Format_AlphaBeta(oldBeta), Format_AlphaBeta(updatedNode.Alpha), Format_AlphaBeta(updatedNode.Beta), transpositionTableUpdate), 1);
            }
            else
            {
                DebugWrite(string.Format("Leaving node unchanged [{0}, {1}]{2}", Format_AlphaBeta(updatedNode.Alpha), Format_AlphaBeta(updatedNode.Beta), transpositionTableUpdate), 1);
            }

            if (cutoff)
            {
                DebugWrite("Cutting off", 1);
            }
        }

        [Conditional("DEBUG")]
        private void Debug_Evaluate(float value)
        {
            Throw.ArgumentOutOfRangeIf(value < MinValue || value > MaxValue, "Invalid evaluation value", "value");

            DebugWrite(string.Format("Evaluating to {0}", Format_AlphaBeta(value)));
        }

        [Conditional("DEBUG")]
        private void Debug_Discard()
        {
            DebugWrite("Discarding...");
        }

        [Conditional("DEBUG")]
        private void Debug_TranspositionTable_ExactHit(int hash, float value)
        {
            DebugWrite(string.Format("Node with hash [{0}] has already been seen before and evaluated to {1}", hash, Format_AlphaBeta(value)));
        }

        [Conditional("DEBUG")]
        private void Debug_TranspositionTable_LowerBound(int hash, float value, float referenceValue)
        {
            if (value > referenceValue)
                DebugWrite(string.Format("Node with hash [{0}] has already been seen before and its lower bound (alpha) was {1} (updating from {2})", hash, Format_AlphaBeta(value), Format_AlphaBeta(referenceValue)));
        }

        [Conditional("DEBUG")]
        private void Debug_TranspositionTable_UpperBound(int hash, float value, float referenceValue)
        {
            if (value < referenceValue)
                DebugWrite(string.Format("Node with hash [{0}] has already been seen before and its upper bound (beta) was {1} (updating from {2})", hash, Format_AlphaBeta(value), Format_AlphaBeta(referenceValue)));
        }

        #endregion

        #region Inner Types

        public sealed class Node
        {
            public readonly Node Parent;

            public float Alpha = MinValue;
            public float Beta = MaxValue;
            public float OriginalAlpha = MinValue;
            public object m_result;

            // Needed for transposition table
            public float BestValue = MinValue;

            private bool? m_isMaximizing;

#if DEBUG
            public List<object> Choices;
#endif

            public bool IsMaximizing
            {
                get { return m_isMaximizing.Value; }
            }

            public bool HasResult
            {
                get; 
                private set; 
            }

            public object Result
            {
                get { return m_result; }
                set
                {
                    m_result = value;
                    HasResult = true;
                }
            }

            public int? Hash
            {
                get;
                set;
            }

            public int ComparisonSign
            {
                get { return m_isMaximizing.Value ? +1 : -1; }
            }

            public bool IsInitialized
            {
                get { return m_isMaximizing.HasValue; }
            }

            public Node(Node parent)
            {
                Parent = parent;
                Debug.Assert(Parent.m_isMaximizing.HasValue, "Parent is supposed to be already initialized");
            }

            // Root node
            internal Node()
            {
                m_isMaximizing = true;
            }

            internal void Initialize(bool isMaximizing)
            {
                Debug.Assert(!m_isMaximizing.HasValue, "Already initialized");
                Debug.Assert(Parent.m_isMaximizing.HasValue, "Parent is supposed to be already initialized");
                m_isMaximizing = isMaximizing;

                if (isMaximizing == Parent.m_isMaximizing.Value)
                {
                    Alpha = Parent.Alpha;
                    Beta = Parent.Beta;
                }
                else
                {
                    Alpha = -Parent.Beta;
                    Beta = -Parent.Alpha;
                }

                OriginalAlpha = Alpha;
            }

            internal float GetBestValueWithRegardsTo(Node node)
            {
                var value = BestValue;

                if (m_isMaximizing.HasValue && (m_isMaximizing.Value != node.m_isMaximizing.Value))
                    value = -value;

                return value;
            }
        }

        private enum TranspositionTableEntryType : byte
        {
            Exact,
            LowerBound,
            UpperBound
        }

        private struct TranspositionTableEntry
        {
            public float Value;
            public bool IsMaximizing;
            public TranspositionTableEntryType Type;
            public int Depth;
        }

        private class TranspositionTable
        {
            private readonly Dictionary<int, TranspositionTableEntry> m_entries = new Dictionary<int, TranspositionTableEntry>();

            public bool TryLookup(int hash, out TranspositionTableEntry entry)
            {
                return m_entries.TryGetValue(hash, out entry);
            }

            public void Store(int hash, TranspositionTableEntry entry)
            {
                m_entries[hash] = entry;
            }
        }

        #endregion
    }
}

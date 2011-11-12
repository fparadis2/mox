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
using Mox.Flow;

namespace Mox.AI
{
    /// <summary>
    /// Partitions a single AI run into smaller units of computation suitable for multi-threaded usage.
    /// </summary>
    /// <remarks>Also used in single-threaded AI to get the same results whatever the method used.</remarks>
    public class MinMaxPartitioner
    {
        #region Inner Types

        private class WorkOrder : IWorkOrder
        {
            #region Variables

            private readonly IEvaluationStrategy m_evaluationStrategy;
            private readonly object m_choiceResult;
            private readonly MinimaxTree m_tree;
            private readonly ICancellable m_cancellable;
            private readonly Choice m_choice;

            #endregion

            #region Constructor

            public WorkOrder(IEvaluationStrategy evaluationStrategy, Choice choice, object choiceResult, ICancellable cancellable)
            {
                Throw.IfNull(evaluationStrategy, "evaluationStrategy");
                Throw.IfNull(choice, "choice");

                m_evaluationStrategy = evaluationStrategy;
                m_choice = choice;
                m_choiceResult = choiceResult;
                m_cancellable = cancellable;

                m_tree = new MinimaxTree();
            }

            #endregion

            #region Properties

            public IEvaluationStrategy EvaluationStrategy
            {
                get { return m_evaluationStrategy; }
            }

            public MinimaxTree Tree
            {
                get { return m_tree; }
            }

            public Choice Choice
            {
                get { return m_choice; }
            }

            public object ChoiceResult
            {
                get { return m_choiceResult; }
            }

            #endregion

            #region Methods

            public void Evaluate(Game game)
            {
                EvaluationStrategy.Evaluate(game, this, m_cancellable);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly IDispatchStrategy m_dispatchStrategy;
        private readonly IEvaluationStrategy m_evaluationStrategy;

        #endregion

        #region Constructor

        public MinMaxPartitioner(IDispatchStrategy dispatchStrategy, IEvaluationStrategy evaluationStrategy)
        {
            Throw.IfNull(dispatchStrategy, "dispatchStrategy");
            Throw.IfNull(evaluationStrategy, "evaluationStrategy");

            m_dispatchStrategy = dispatchStrategy;
            m_evaluationStrategy = evaluationStrategy;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Partitions the given choices through the dispatcher and returns the aggregated result.
        /// </summary>
        /// <returns></returns>
        public AIResult Execute(Choice choice, ICollection<object> choiceResults, ICancellable cancellable)
        {
            Throw.IfNull(choiceResults, "choiceResults");
            Throw.InvalidArgumentIf(choiceResults.Count == 0, "No choices!", "choiceResults");

            if (choiceResults.Count == 1)
            {
                return new AIResult
                {
                    Result = choiceResults.First(),
                    DriverType = m_evaluationStrategy.DriverType
                };
            }

            var workOrders = CreateWorkOrders(choice, choiceResults, cancellable);

            Dispatch(workOrders);
            m_dispatchStrategy.Wait();
            return Aggregate(workOrders, choice.DefaultValue);
        }

        private IEnumerable<WorkOrder> CreateWorkOrders(Choice choice, ICollection<object> choiceResults, ICancellable cancellable)
        {
            Debug.Assert(choiceResults.Count > 1);

            List<WorkOrder> workOrders = new List<WorkOrder>();

            foreach (object choiceResult in choiceResults)
            {
                workOrders.Add(new WorkOrder(m_evaluationStrategy, choice, choiceResult, cancellable));
            }

            return workOrders;
        }

        private void Dispatch(IEnumerable<WorkOrder> workOrders)
        {
            foreach (WorkOrder order in workOrders)
            {
                m_dispatchStrategy.Dispatch(order);
            }
        }

        private AIResult Aggregate(IEnumerable<WorkOrder> workOrders, object defaultChoice)
        {
            AIResult result = new AIResult();
            MinimaxTree mergeTree = new MinimaxTree
            {
                EnableDebugInfo = false
            };

            foreach (WorkOrder workOrder in workOrders)
            {
                if (IsValid(workOrder))
                {
                    mergeTree.BeginNode(true, workOrder.ChoiceResult);

#if DEBUG
                    result.NumEvaluations += workOrder.Tree.NumEvaluations;
#endif
                    mergeTree.Evaluate(workOrder.Tree.CurrentNode.Alpha);

                    mergeTree.EndNode();
                }
            }

            result.DriverType = m_evaluationStrategy.DriverType;
            result.PredictedScore = mergeTree.CurrentNode.Alpha;

            result.Result = mergeTree.CurrentNode.HasResult ? mergeTree.CurrentNode.Result : defaultChoice;

#pragma warning disable 162
            if (Configuration.Debug_Minimax_tree)
            {
                result.MinMaxTreeDebugInfo = AggregateDebugInfo(workOrders);
            }
#pragma warning restore 162

            return result;
        }

        private static string AggregateDebugInfo(IEnumerable<WorkOrder> workOrders)
        {
            StringBuilder sb = new StringBuilder();

            int i = 0;
            foreach (WorkOrder workOrder in workOrders)
            {
                sb.AppendFormat("### WorkOrder {0} ###", i++);
                sb.AppendLine();
                sb.AppendLine(workOrder.Tree.DebugInfo);
            }

            return sb.ToString();
        }

        private static bool IsValid(IWorkOrder order)
        {
            return order.Tree.CurrentNode.HasResult;
        }

        #endregion
    }
}

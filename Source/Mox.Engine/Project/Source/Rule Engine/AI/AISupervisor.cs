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
    /// Evaluates the best move available by combining all the AI parts (high-level).
    /// </summary>
    /// <remarks>
    /// This is the entry point for the min/max AI.
    /// </remarks>
    public class AISupervisor : IDisposable, IChoiceDecisionMaker
    {
        #region Inner Types

        private class NoTimeout : ICancellable
        {
            public bool Cancel
            {
                get { return false; }
            }
        }

        private class Timeout : ICancellable
        {
            private readonly Stopwatch m_stopWatch = Stopwatch.StartNew();
            private readonly TimeSpan m_timeout;

            public Timeout(TimeSpan timeout)
            {
                m_timeout = timeout;
            }

            public bool Cancel
            {
                get
                {
#if DEBUG
                    if (Debugger.IsAttached)
                    {
                        return false;
                    }
#endif

                    return m_stopWatch.Elapsed > m_timeout;
                }
            }
        }

        #endregion

        #region Variables

        private readonly AIParameters m_parameters = new AIParameters();
        private readonly IDispatchStrategy m_dispatchStrategy;

        private int m_choiceNumber;

        #endregion

        #region Constructor

        public AISupervisor(Game game)
        {
            m_dispatchStrategy = CreateDispatchStrategy(game);
        }

        private static IDispatchStrategy CreateDispatchStrategy(Game game)
        {
#pragma warning disable 162
            if (Configuration.AI_Multithreaded)
            {
                return new MultiThreadedDispatchStrategy(game);
            }

            return new SingleThreadedDispatchStrategy(game);
#pragma warning restore 162
        }

        public void Dispose()
        {
            DisposableHelper.SafeDispose(m_dispatchStrategy as IDisposable);
        }

        #endregion

        #region Properties
        
        public AIParameters Parameters
        {
            get { return m_parameters; }
        }

        #endregion

        #region Methods

        public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
        {
            m_choiceNumber++;

            var choiceEnumeratorProvider = new AttributedChoiceEnumeratorProvider(Parameters);
            var choiceEnumerator = choiceEnumeratorProvider.GetEnumerator(choice);

            var player = choice.Player.Resolve(sequencer.Game);

            using (IMinMaxAlgorithm algorithm = CreateAlgorithm(player))
            {
                DebugInfo debugInfo = new DebugInfo(player, algorithm);

                var evaluationStrategy = new EvaluationStrategy(sequencer, algorithm, choiceEnumeratorProvider);

                ICollection<object> choiceResults = choiceEnumerator.EnumerateChoices(sequencer.Game, choice).ToArray();

                AIResult result = Evaluate(evaluationStrategy, Parameters.DriverType, Parameters, choice, choiceResults);

#pragma warning disable 162
                if (Configuration.Debug_Minimax_tree)
                {
                    Debug.WriteLine(result.MinMaxTreeDebugInfo);
                }

                if (Configuration.Validate_Minimax_drivers)
                {
                    Trace.Assert(Configuration.Debug_Minimax_tree);
                    Trace.Assert(result.DriverType == Parameters.DriverType);
                    ValidateMinMaxDrivers(evaluationStrategy, Parameters, choice, choiceResults, result);
                }
#pragma warning restore 162

                debugInfo.AnalyzeResult(result, m_choiceNumber, choice);

                return result.Result;
            }
        }

        private AIResult Evaluate(EvaluationStrategy evaluationStrategy, AIParameters.MinMaxDriverType driverType, AIParameters parameters, Choice choice, ICollection<object> choiceResults)
        {
            evaluationStrategy.DriverType = driverType;

            MinMaxPartitioner partitioner = new MinMaxPartitioner(m_dispatchStrategy, evaluationStrategy, parameters);
            return partitioner.Execute(choice, choiceResults, CreateCancellable());
        }

        private ICancellable CreateCancellable()
        {
            return Parameters.GlobalAITimeout == TimeSpan.Zero ? (ICancellable)new NoTimeout() : new Timeout(Parameters.GlobalAITimeout);
        }

        #region Validation

        private void ValidateMinMaxDrivers(EvaluationStrategy strategy, AIParameters parameters, Choice choice, ICollection<object> choiceResults, AIResult expectedResult)
        {
            foreach (var otherDriverType in GetOtherDriverTypes(expectedResult.DriverType))
            {
                AIResult otherResult = Evaluate(strategy, otherDriverType, parameters, choice, choiceResults);

                ValidateAreEqual(expectedResult, otherResult, r => r.Result, "result");
                ValidateAreEqual(expectedResult, otherResult, r => r.PredictedScore, "predicted score");
                ValidateAreEqual(expectedResult, otherResult, r => r.MinMaxTreeDebugInfo, "minmax tree");
            }
        }

        private static void ValidateAreEqual<T>(AIResult expected, AIResult actual, Func<AIResult, T> provider, string things)
        {
            T expectedValue = provider(expected);
            T actualValue = provider(actual);

            if (!Equals(expectedValue, actualValue))
            {
                throw new InvalidProgramException(string.Format("Expected {0} {4} ({1}) but got {2} ({3}).", expectedValue, expected.DriverType, actualValue, actual.DriverType, things));
            }
        }

        private static IEnumerable<AIParameters.MinMaxDriverType> GetOtherDriverTypes(AIParameters.MinMaxDriverType type)
        {
            foreach (AIParameters.MinMaxDriverType driverType in Enum.GetValues(typeof(AIParameters.MinMaxDriverType)))
            {
                if (driverType != type)
                {
                    yield return driverType;
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates an algorithm to be used.
        /// </summary>
        /// <returns></returns>
        public virtual IMinMaxAlgorithm CreateAlgorithm(Player maximizingPlayer)
        {
            return new DefaultMinMaxAlgorithm(maximizingPlayer, Parameters);
        }

        private class DebugInfo
        {
            #region Variables

            private readonly Player m_player;
            private readonly IMinMaxAlgorithm m_algorithm;
            private readonly Stopwatch m_timer;

            #endregion

            #region Constructor

            public DebugInfo(Player player, IMinMaxAlgorithm algorithm)
            {
                m_player = player;
                m_algorithm = algorithm;
                m_timer = Stopwatch.StartNew();
            }

            #endregion

            #region Methods

            public void AnalyzeResult(AIResult result, int choiceNumber, Choice choice)
            {
                if (Configuration.Debug_AI_choices)
#pragma warning disable 162
                {
                    OutputDebug(result, choiceNumber, choice);
                }
#pragma warning restore 162
            }

            private void AppendTimeInfo(StringBuilder stringBuilder, long evaluations)
            {
                double elapsedSeconds = m_timer.Elapsed.TotalSeconds;
                if (evaluations != 0 && elapsedSeconds != 0)
                {
                    stringBuilder.AppendFormat(" in {0:0.##} seconds ({1:0.##} evals/s)", elapsedSeconds, evaluations / elapsedSeconds);
                }
            }

            private void AppendChoiceInfo(StringBuilder builder, AIResult result, int choiceNumber, Choice choice)
            {
                string resultString;

                object choiceResult = result.Result;
                if (ReferenceEquals(choiceResult, null))
                {
                    resultString = "[null]";
                }
                else if (choiceResult is IChoiceResult)
                {
                    resultString = ((IChoiceResult)choiceResult).ToString(m_player.Manager);
                }
                else
                {
                    resultString = choiceResult.ToString();
                }

                builder.AppendFormat("{0}: When asked [{1}], AI '{2}' returned {3} ", choiceNumber, choice.GetType().Name, m_player.Name, resultString);
            }

            private void AppendScoreInfo(StringBuilder builder, AIResult result)
            {
                float actualScore = m_algorithm.ComputeHeuristic(m_player.Manager, true);

                if (result.NumEvaluations > 0)
                {
                    builder.AppendFormat("(predicted {0}, scored {1})", result.PredictedScore, actualScore);
                }
                else
                {
                    builder.AppendFormat("(only choice, scored {0})", actualScore);
                }
            }

            private void OutputDebug(AIResult result, int choiceNumber, Choice choice)
            {
                StringBuilder stringBuilder = new StringBuilder();

                AppendChoiceInfo(stringBuilder, result, choiceNumber, choice);
                AppendScoreInfo(stringBuilder, result);
                AppendTimeInfo(stringBuilder, result.NumEvaluations);
                Trace.WriteLine(stringBuilder.ToString());
            }

            #endregion
        }

        #endregion
    }
}

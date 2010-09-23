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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mox.Flow;

namespace Mox.AI
{
    public class RecursiveMinMaxDriver<TController> : MinMaxDriver<TController>
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        protected RecursiveMinMaxDriver(Context context, IEnumerable<object> rootChoices)
            : base(context, rootChoices)
        {
        }

        #endregion

        #region Methods

        public static RecursiveMinMaxDriver<TController> CreateRootController(Game game, IMinimaxTree minmaxTree, IMinMaxAlgorithm algorithm, IChoiceResolverProvider choiceResolverProvider, params object[] choices)
        {
            Context context = new Context(minmaxTree, algorithm, choiceResolverProvider, game);
            return new RecursiveMinMaxDriver<TController>(context, choices);
        }

        public static RecursiveMinMaxDriver<TController> CreateController(Game game, IMinimaxTree minmaxTree, IMinMaxAlgorithm algorithm, IChoiceResolverProvider choiceResolverProvider)
        {
            Context context = new Context(minmaxTree, algorithm, choiceResolverProvider, game);
            return new RecursiveMinMaxDriver<TController>(context, null);
        }

        protected override void TryAllChoices(Sequencer<TController> sequencer, bool maximizingPlayer, IEnumerable<object> choices, string debugInfo)
        {
            Debug.Assert(choices.Count() > 0);

            using (Sequencer<TController> forkedSequencer = sequencer.Fork())
            {
                foreach (object choice in choices)
                {
                    using (Sequencer<TController> clonedSequencer = forkedSequencer.Clone())
                    {
                        IChoiceScope choiceScope = BeginChoice(clonedSequencer.Game, maximizingPlayer, choice, debugInfo);

                        TryChoice(clonedSequencer);

                        if (!choiceScope.End())
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void TryChoice(Sequencer<TController> sequencer)
        {
            // Rerun with the added choice
            SequencerResult result = sequencer.RunOnce(Controller);

            // Continue with the next parts
            Recurse(sequencer, result);
        }

        private void Recurse(Sequencer<TController> sequencer, SequencerResult lastResult)
        {
            if (lastResult == SequencerResult.Retry)
            {
                Discard();
            }
            else if (lastResult != SequencerResult.Stop)
            {
                if (!EvaluateIf(sequencer.Game, sequencer.IsEmpty))
                {
                    sequencer.Run(Controller);

                    EvaluateIf(sequencer.Game, !HasConsumedChoice);
                }
            }
        }

        #endregion
    }
}
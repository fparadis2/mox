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
using System.Diagnostics;

using Mox.Flow;
using Mox.Transactions;

namespace Mox.AI
{
    internal class EvaluationStrategy : IEvaluationStrategy
    {
        #region Variables

        private readonly NewSequencer m_sequencer;
        private readonly IMinMaxAlgorithm m_algorithm;
        private readonly IChoiceEnumeratorProvider m_choiceEnumeratorProvider;

        private readonly int m_seed;

        #endregion

        #region Constructor

        public EvaluationStrategy(NewSequencer sequencer, IMinMaxAlgorithm algorithm, IChoiceEnumeratorProvider choiceEnumeratorProvider)
        {
            m_sequencer = sequencer;
            m_algorithm = algorithm;
            m_choiceEnumeratorProvider = choiceEnumeratorProvider;

            m_seed = sequencer.Game.Random.Next();
        }

        #endregion

        #region Properties

        public AIParameters.MinMaxDriverType DriverType
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public void Evaluate(Game game, IWorkOrder workOrder, ICancellable cancellable)
        {
            using (game.UpgradeController(new ObjectController(game)))
            using (game.UseRandom(Random.New(m_seed)))
            using (BeginRollbackTransaction(game))
            {
                NewSequencer sequencer = PrepareSequencer(game);
                Debug.Assert(sequencer.Game == game);

                AIEvaluationContext context = new AIEvaluationContext(workOrder.Tree, m_algorithm, m_choiceEnumeratorProvider.Clone());
                
                NewMinMaxDriver driver = CreateDriver(context, cancellable);

                driver.RunWithChoice(sequencer, workOrder.Choice, workOrder.ChoiceResult);
            }
        }

        private static NewMinMaxDriver CreateDriver(AIEvaluationContext context, ICancellable cancellable)
        {
            // Iterative is broken since sequencer/choice refactor
            // but recursive is now much more efficient in stack space so not a problem anymore

            return new NewRecursiveMinMaxDriver(context, cancellable);
        }

        private NewSequencer PrepareSequencer(Game game)
        {
            if (m_sequencer.Game == game)
            {
                return m_sequencer.Clone();
            }

            return m_sequencer.Clone(game);
        }

        private static IDisposable BeginRollbackTransaction(Game game)
        {
            const string Token = "EvaluationStrategy";

            game.Controller.BeginTransaction(Token);

            return new DisposableHelper(() => game.Controller.EndTransaction(true, Token));
        }

        #endregion
    }
}
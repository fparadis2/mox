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
using System.Reflection;
using Mox.Flow;

namespace Mox.AI
{
    internal class EvaluationStrategy<TController> : IEvaluationStrategy
    {
        #region Variables

        private readonly Sequencer<TController> m_sequencer;
        private readonly ControllerAccess m_controllerAccess;
        private readonly IMinMaxAlgorithm m_algorithm;
        private readonly IChoiceResolverProvider m_choiceResolverProvider;
        private readonly MethodBase m_method;
        private readonly object[] m_args;

        private readonly int m_seed;

        #endregion

        #region Constructor

        public EvaluationStrategy(Sequencer<TController> sequencer, ControllerAccess controllerAccess, IMinMaxAlgorithm algorithm, IChoiceResolverProvider choiceResolverProvider, MethodBase method, object[] args)
        {
            m_sequencer = sequencer;
            m_controllerAccess = controllerAccess;
            m_algorithm = algorithm;
            m_choiceResolverProvider = choiceResolverProvider;
            m_args = args;
            m_method = method;

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
            Sequencer<TController> sequencer;
            using (game.ChangeControlMode(ReplicationControlMode.Master))
            using (game.UseRandom(Random.New(m_seed)))
            using (PrepareSequencer(game, out sequencer))
            {
                Debug.Assert(sequencer.Game == game);

                IChoiceResolverProvider resolverProvider = m_choiceResolverProvider.Clone();

                MinMaxDriver<TController> driver = CreateDriver(game, workOrder.Tree, resolverProvider, workOrder.Choice);

                driver.Run(m_method, m_args, sequencer, m_controllerAccess, cancellable);
            }
        }

        private MinMaxDriver<TController> CreateDriver(Game game, IMinimaxTree tree, IChoiceResolverProvider resolverProvider, object choice)
        {
            switch (DriverType)
            {
                case AIParameters.MinMaxDriverType.Iterative:
                    return IterativeMinMaxDriver<TController>.CreateRootController(game, tree, m_algorithm, resolverProvider, choice);
                case AIParameters.MinMaxDriverType.Recursive:
                    return RecursiveMinMaxDriver<TController>.CreateRootController(game, tree, m_algorithm, resolverProvider, choice);
                default:
                    throw new NotImplementedException();
            }
        }

        private IDisposable PrepareSequencer(Game game, out Sequencer<TController> sequencer)
        {
            if (m_sequencer.Game == game)
            {
                sequencer = m_sequencer;
                return null;
            }

            sequencer = m_sequencer.Clone(game);
            return new DisposableHelper(sequencer.Dispose);
        }

        #endregion
    }
}
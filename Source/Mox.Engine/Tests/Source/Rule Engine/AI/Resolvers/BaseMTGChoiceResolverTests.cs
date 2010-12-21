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
using System.Reflection;
using Mox.Flow;
using NUnit.Framework;

namespace Mox.AI.Resolvers
{
    public abstract class BaseMTGChoiceResolverTests : BaseGameTests
    {
        #region Variables

        internal BaseMTGChoiceResolver m_choiceResolver;
        protected IMinMaxAlgorithm m_algorithm;
        protected MTGPart.Context m_context;
        
        protected AIParameters m_parameters;
        private IGameController m_controller;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_parameters = new AIParameters();
            m_controller = m_mockery.StrictMock<IGameController>();
            m_context = new Part<IGameController>.Context(new Sequencer<IGameController>(new Flow.Parts.MainPart(m_playerA), m_game), m_controller, ControllerAccess.Multiple);
            m_algorithm = m_mockery.StrictMock<IMinMaxAlgorithm>();

            m_choiceResolver = CreateResolver();
            m_choiceResolver.Parameters = m_parameters;
            m_choiceResolver.SessionData = AISessionData.Create();
        }

        internal abstract BaseMTGChoiceResolver CreateResolver();

        protected MethodBase GetMethod()
        {
            return typeof(IGameController).GetMethod(m_choiceResolver.ExpectedMethodName);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_GetContext()
        {
            // Context is always the first argument
            Assert.AreSame(m_context, m_choiceResolver.GetContext<IGameController>(GetMethod(), new object[] { m_context, m_playerA }));
        }

        [Test]
        public void Test_SetContext()
        {
            var replicatedGame = m_game.Replicate();

            Player otherPlayerA = Resolvable<Player>.Resolve(replicatedGame, m_playerA);

            m_context = new Part<IGameController>.Context(m_context.Sequencer.Clone(replicatedGame), m_controller, ControllerAccess.Multiple);

            object[] args = new object[2];
            args[1] = m_playerA;
            m_choiceResolver.SetContext(GetMethod(), args, m_context);

            Assert.AreEqual(m_context, args[0]);
            Assert.AreEqual(otherPlayerA, args[1]); // Also sets the player
        }

        [Test]
        public void Test_GetPlayer()
        {
            Assert.AreEqual(m_playerA, m_choiceResolver.GetPlayer(GetMethod(), new[] { null, m_playerA }));
        }

        #endregion
    }
}

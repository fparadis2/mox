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
using System.Linq;

using NUnit.Framework;
using Rhino.Mocks;

using Mox.Flow.Phases;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class SequenceTurnTests : PartTestBase
    {
        #region Variables

        private static readonly Property<int> TurnDataProperty = Property<int>.RegisterAttachedProperty("MyTurnDataProperty", typeof(SequenceTurnTests), PropertyFlags.None, 4);

        private SequenceTurn m_part;
        private ITurnFactory m_turnFactory;
        private Phase m_mockPhase1;
        private Phase m_mockPhase2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_turnFactory = m_mockery.StrictMock<ITurnFactory>();
            m_mockPhase1 = m_mockery.StrictMock<Phase>(Mox.Phases.End);
            m_mockPhase2 = m_mockery.StrictMock<Phase>(Mox.Phases.Combat);

            m_part = new SequenceTurn(m_playerA, m_turnFactory);
        }

        #endregion

        #region Utilities

        private void Expect_CreateTurn(params Phase[] phases)
        {
            Turn turn = new Turn();
            phases.ForEach(turn.Phases.Add);
            Expect.Call(m_turnFactory.CreateTurn()).Return(turn);
        }

        private void Assert_Phases_Are_Scheduled(params Phase[] phases)
        {
            Assert.AreEqual(phases.Length, m_lastContext.ScheduledParts.Count());

            int i = phases.Length - 1;
            foreach (var part in m_lastContext.ScheduledParts)
            {
                Phase phase = phases[i--];

                Assert.IsNotNull(part);
                Assert.IsInstanceOf<SequencePhase>(part);
                Assert.AreEqual(phase, ((SequencePhase)part).Phase);
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_turnFactory, m_part.TurnFactory);
        }

        [Test]
        public void Test_Sequence_sequences_all_the_phases_of_the_turn()
        {
            Expect_CreateTurn(m_mockPhase1, m_mockPhase2);

            Assert.IsNull(Execute(m_part));

            Assert_Phases_Are_Scheduled(m_mockPhase1, m_mockPhase2);
        }

        [Test]
        public void Test_Default_constructor_uses_the_default_turn_factory()
        {
            m_part = new SequenceTurn(m_playerA);
            Assert.IsNotNull(m_part.TurnFactory);
            Assert.IsInstanceOf<DefaultTurnFactory>(m_part.TurnFactory);
        }

        [Test]
        public void Test_Executes_increments_the_turn_number()
        {
            m_game.State.CurrentTurn = 0;

            Expect_CreateTurn();
            Execute(m_part);

            Assert.AreEqual(1, m_game.State.CurrentTurn);
        }

        [Test]
        public void Test_Executes_sets_the_active_player()
        {
            m_game.State.ActivePlayer = m_playerB;

            Expect_CreateTurn();
            Execute(m_part);

            Assert.AreEqual(m_playerA, m_game.State.ActivePlayer);
        }

        [Test]
        public void Test_Executes_resets_the_turn_data()
        {
            m_game.TurnData.SetValue(TurnDataProperty, 10);

            m_game.State.ActivePlayer = m_playerB;

            Expect_CreateTurn();
            Execute(m_part);

            Assert.AreEqual(4, m_game.TurnData.GetValue(TurnDataProperty));
        }

        #endregion
    }
}

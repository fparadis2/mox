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
using Mox.Flow;
using Mox.Flow.Parts;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class PhaseTests : BaseStepTests<Step>
    {
        #region Variables

        private Phase m_phase;

        private Step m_mockStep1;
        private Step m_mockStep2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_phase = new Phase(Mox.Phases.Combat);

            m_mockStep1 = m_mockery.StrictMock<Step>(Steps.Draw);
            m_mockStep2 = m_mockery.StrictMock<Step>(Steps.EndOfCombat);
        }

        #endregion

        #region Utilities

        private MTGPart SequencePhase(Player player)
        {
            MTGPart result = null;
            m_mockery.Test(() => result = m_phase.Sequence(m_sequencerTester.Context, player));
            return result;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_Values()
        {
            Assert.AreEqual(Mox.Phases.Combat, m_phase.Type);
        }

        [Test]
        public void Test_Contains_no_steps_by_default()
        {
            Assert.Collections.IsEmpty(m_phase.Steps);
            Assert.IsFalse(m_phase.Steps.IsReadOnly);
        }

        [Test]
        public void Test_A_phase_with_no_steps_gives_priority_to_active_player()
        {
            Assert.IsNull(SequencePhase(m_playerA));

            PlayUntilAllPlayersPassAndTheStackIsEmpty part = GetScheduledPart<PlayUntilAllPlayersPassAndTheStackIsEmpty>();
            Assert.IsNotNull(part);
            Assert.AreEqual(m_playerA, part.GetPlayer(m_sequencerTester.Context));
        }

        [Test]
        public void Test_A_phase_with_steps_sequences_the_steps()
        {
            m_phase.Steps.Add(m_mockStep1);
            m_phase.Steps.Add(m_mockStep2);

            Assert.IsNull(SequencePhase(m_playerA));

            Assert.AreEqual(2, m_sequencerTester.Context.ScheduledParts.Count());
            SequenceStep sequenceStep1 = (SequenceStep)m_sequencerTester.Context.ScheduledParts.Skip(1).First();
            Assert.AreEqual(m_mockStep1, sequenceStep1.Step);

            SequenceStep sequenceStep2 = (SequenceStep)m_sequencerTester.Context.ScheduledParts.First();
            Assert.AreEqual(m_mockStep2, sequenceStep2.Step);
        }

        [Test]
        public void Test_When_being_sequenced_phase_sets_the_current_phase_in_the_game_state()
        {
            Assert.IsNull(SequencePhase(m_playerA));
            Assert.AreEqual(m_phase.Type, m_game.State.CurrentPhase);
        }

        #endregion
    }
}

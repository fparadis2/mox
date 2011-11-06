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
using NUnit.Framework;

using Mox.Flow.Parts;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class StepTests : BaseStepTests<Step>
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();
            m_step = new Step(Steps.Draw);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_Values()
        {
            Assert.AreEqual(Steps.Draw, m_step.Type);
        }

        [Test]
        public void Test_By_default_step_gives_priority_to_active_player()
        {
            Assert.IsNull(SequenceStep(m_playerA));

            PlayUntilAllPlayersPassAndTheStackIsEmpty part = GetScheduledPart<PlayUntilAllPlayersPassAndTheStackIsEmpty>();
            Assert.IsNotNull(part);
            Assert.AreEqual(m_playerA, part.GetPlayer(m_game));
        }

        [Test]
        public void Test_When_being_sequenced_step_sets_the_current_step_in_the_game_state()
        {
            SequenceStep(m_playerA);

            Assert.AreEqual(m_step.Type, m_game.State.CurrentStep);
        }

        #endregion
    }
}

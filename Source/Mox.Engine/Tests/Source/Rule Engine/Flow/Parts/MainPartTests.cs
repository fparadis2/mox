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
using System.Linq;

using NUnit.Framework;
using Rhino.Mocks.Interfaces;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class MainPartTests : PartTestBase<MainPart>
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_part = new MainPart(m_playerA);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Execute_schedules_a_turn()
        {
            Execute(m_part);

            Assert.AreEqual(1, m_sequencerTester.Context.ScheduledParts.Count());
            SequenceTurn sequenceTurn = (SequenceTurn)m_sequencerTester.Context.ScheduledParts.First();
            Assert.AreEqual(m_playerA, sequenceTurn.GetPlayer(m_sequencerTester.Context));
        }

        [Test]
        public void Test_Execute_returns_another_main_for_the_next_player()
        {
            MTGPart result = (MTGPart)Execute(m_part);

            Assert.IsInstanceOf<MainPart>(result);
            Assert.AreEqual(m_playerB, result.GetPlayer(m_sequencerTester.Context));

            result = (MTGPart)Execute(result);

            Assert.IsInstanceOf<MainPart>(result);
            Assert.AreEqual(m_playerA, result.GetPlayer(m_sequencerTester.Context));
        }

        #endregion
    }
}

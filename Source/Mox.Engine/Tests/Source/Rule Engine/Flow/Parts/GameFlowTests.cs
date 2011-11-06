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

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class GameFlowTests : PartTestBase
    {
        #region Variables

        private GameFlow m_part;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_part = new GameFlow(m_playerA);
        }

        #endregion

        #region Utilities

        #endregion

        #region Tests

        [Test]
        public void Test_Execute_schedules_a_mulligan_part_for_each_player_and_then_schedules_a_main_part()
        {
            Assert.IsNull(Execute(m_part));

            List<PlayerPart> parts = new List<PlayerPart>(m_lastContext.ScheduledParts.Cast<PlayerPart>());
            Assert.AreEqual(3, parts.Count);

            Assert.IsInstanceOf<DrawInitialCards>(parts[2]);
            Assert.AreEqual(m_playerA, parts[2].GetPlayer(m_game));

            Assert.IsInstanceOf<DrawInitialCards>(parts[1]);
            Assert.AreEqual(m_playerB, parts[1].GetPlayer(m_game));

            Assert.IsInstanceOf<MainPart>(parts[0]);
            Assert.AreEqual(m_playerA, parts[0].GetPlayer(m_game));
        }

        #endregion
    }
}

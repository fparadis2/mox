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
using System.Text;
using NUnit.Framework;

namespace Mox.AI.Functional
{
    [TestFixture]
    public class ArtifactsFunctionalTests : AIFunctionalTests
    {
        #region Tests

        [Test]
        public void Test_The_AI_can_play_Icy_manipulator()
        {
            AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Icy Manipulator");

            SetupGame();

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);
        }

        #endregion
    }
}

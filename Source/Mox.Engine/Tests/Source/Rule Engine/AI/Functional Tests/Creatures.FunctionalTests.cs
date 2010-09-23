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
    public class CreaturesFunctionalTests : AIFunctionalTests
    {
        #region Tests

        [Test]
        public void Test_The_AI_will_play_creatures_in_general_if_able()
        {
            Card creature = AddCard(m_playerA, m_game.Zones.Hand, "10E", "Dross Crocodile");

            SetupGame();

            m_playerA.ManaPool[Color.Black] = 10;

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);

            Assert.AreEqual(m_game.Zones.Battlefield, creature.Zone);
        }

        [Test]
        public void Test_AI_Will_play_Angel_of_Mercy_when_possible()
        {
            Card creature = AddCard(m_playerA, m_game.Zones.Hand, "10E", "Angel of Mercy");

            SetupGame();

            m_playerA.ManaPool[Color.White] = 5;

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);

            Assert.Collections.IsEmpty(m_playerA.Hand);
            Assert.AreEqual(m_game.Zones.Battlefield, creature.Zone);
            Assert.AreEqual(23, m_playerA.Life);
        }

        [Test]
        public void Test_AI_Can_handle_having_a_bogardan_firefiend_in_play_with_shock_in_hand()
        {
            // When Bogardan Firefiend is put into a graveyard from play, it deals 2 damage to target creature.
            Card creature = AddCard(m_playerA, m_game.Zones.Battlefield, "10E", "Bogardan Firefiend");
            Card shock = AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");

            SetupGame();

            m_playerA.ManaPool[Color.Red] = 5;

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);
        }

        #endregion
    }
}

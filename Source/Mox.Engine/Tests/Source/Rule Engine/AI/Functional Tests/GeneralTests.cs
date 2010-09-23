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
    public class GeneralTests : AIFunctionalTests
    {
        #region Tests

        [Test]
        public void Test_Stress_test()
        {
            const int numCards = 4;

            foreach (Player player in m_game.Players)
            {
                for (int i = 0; i < numCards; i++)
                {
                    AddCard(player, m_game.Zones.Battlefield, "10E", "Mountain");
                }

                for (int i = 0; i < numCards + 2; i++)
                {
                    AddCard(player, m_game.Zones.Battlefield, "10E", "Plains");
                }

                for (int i = 0; i < 3; i++)
                {
                    AddCard(player, m_game.Zones.Hand, "10E", "Shock");
                }

                for (int i = 0; i < 2; i++)
                {
                    AddCard(player, m_game.Zones.Hand, "10E", "Wild Griffin");
                }

                for (int i = 0; i < 2; i++)
                {
                    AddCard(player, m_game.Zones.Battlefield, "10E", "Wild Griffin");
                }

                for (int i = 0; i < 2; i++)
                {
                    AddCard(player, m_game.Zones.Battlefield, "10E", "Nightmare");
                }
            }

            SetupGame();

            using (Profile())
            {
                Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);
            }
        }

        #endregion
    }
}

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

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class DrawInitialCardsTests : PartTestBase
    {
        #region Variables

        private DrawInitialCards m_drawCards;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_drawCards = new DrawInitialCards(m_playerA);
        }

        #endregion

        #region Utilities

        private void InitializeGame()
        {
            foreach (Player player in Player.Enumerate(m_playerA, false))
            {
                for (int i = 0; i < 40; i++)
                {
                    Card card = CreateCard(player, "B");
                    card.Zone = m_game.Zones.Library;
                }
            }
        }

        private void Expect_Mulligan(Player player, bool result)
        {
            m_sequencerTester.Expect_Player_Mulligan(player, result);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_DrawInitialCards_asks_players_for_mulligans_in_order()
        {
            InitializeGame();

            using (OrderedExpectations)
            {
                Expect_Mulligan(m_playerA, true);
                Expect_Mulligan(m_playerA, true);
                Expect_Mulligan(m_playerA, false);
            }

            m_sequencerTester.Run(m_drawCards);

            Assert.AreEqual(5, m_playerA.Hand.Count);
        }

        [Test]
        public void Test_A_player_cannot_mulligan_anymore_when_his_hand_is_empty()
        {
            InitializeGame();

            m_playerA.MaximumHandSize = 10;

            using (OrderedExpectations)
            {
                for (int i = 0; i < 10; i++)
                {
                    Expect_Mulligan(m_playerA, true);
                }
            }

            m_sequencerTester.Run(m_drawCards);

            Assert.AreEqual(0, m_playerA.Hand.Count);
        }

        #endregion
    }
}

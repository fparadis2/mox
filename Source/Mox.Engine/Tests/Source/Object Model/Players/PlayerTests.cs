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

namespace Mox
{
    [TestFixture]
    public class PlayerTests : BaseGameTests
    {
        #region Variables

        private Player m_player;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_player = m_game.CreatePlayer();
            m_player.Name = "John";
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Players_Manager_is_the_game()
        {
            Assert.AreEqual(m_game, m_playerA.Manager);
        }

        [Test]
        public void Test_Players_have_different_indices()
        {
            Assert.AreNotEqual(m_playerA.Index, m_player.Index);
        }

        [Test]
        public void Test_ToString_uses_name()
        {
            Assert.AreEqual("[Player: John]", m_player.ToString());
        }

        [Test]
        public void Test_Enumerate_invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new List<Player>(Player.Enumerate(null, false)); });
            Assert.Throws<ArgumentException>(delegate { new List<Player>(Player.Enumerate(new Player(), false)); });
        }

        [Test]
        public void Test_Enumerate_enumerates_all_the_players_of_the_game_in_order_and_stops_after_an_iteration()
        {
            CreateGame(3);

            Assert.Collections.AreEqual(new Player[] { m_playerA, m_playerB, m_playerC }, Player.Enumerate(m_playerA, false));
            Assert.Collections.AreEqual(new Player[] { m_playerB, m_playerC, m_playerA }, Player.Enumerate(m_playerB, false));
            Assert.Collections.AreEqual(new Player[] { m_playerC, m_playerA, m_playerB }, Player.Enumerate(m_playerC, false));
        }

        [Test]
        public void Test_Enumerate_enumerates_all_the_players_of_the_game_in_order_and_continues_indefinitly()
        {
            CreateGame(3);

            Assert.Collections.AreEqual(new Player[] { m_playerA, m_playerB, m_playerC, m_playerA, m_playerB, m_playerC }, Player.Enumerate(m_playerA, true).Take(6));
            Assert.Collections.AreEqual(new Player[] { m_playerB, m_playerC, m_playerA, m_playerB, m_playerC, m_playerA }, Player.Enumerate(m_playerB, true).Take(6));
            Assert.Collections.AreEqual(new Player[] { m_playerC, m_playerA, m_playerB, m_playerC, m_playerA, m_playerB }, Player.Enumerate(m_playerC, true).Take(6));
        }

        [Test]
        public void Test_GetNextPlayer_invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => Player.GetNextPlayer(null));
            Assert.Throws<ArgumentException>(() => Player.GetNextPlayer(new Player()));
        }

        [Test]
        public void Test_GetNextPlayer_returns_the_next_player_in_the_game()
        {
            CreateGame(3);

            Assert.AreEqual(m_playerB, Player.GetNextPlayer(m_playerA));
            Assert.AreEqual(m_playerC, Player.GetNextPlayer(m_playerB));
            Assert.AreEqual(m_playerA, Player.GetNextPlayer(m_playerC));
        }

        [Test]
        public void Test_Can_access_the_cards_in_control_of_the_player_in_the_given_zone()
        {
            m_card.Zone = m_game.Zones.Library; Assert.Collections.AreEqual(new[] { m_card }, m_playerA.Library);
            m_card.Zone = m_game.Zones.Battlefield; Assert.Collections.AreEqual(new[] { m_card }, m_playerA.Battlefield);
            m_card.Zone = m_game.Zones.Exile; Assert.Collections.AreEqual(new[] { m_card }, m_playerA.Exile);
            m_card.Zone = m_game.Zones.PhasedOut; Assert.Collections.AreEqual(new[] { m_card }, m_playerA.PhasedOut);

            Assert.Collections.IsEmpty(m_playerA.Graveyard);
        }

        [Test]
        public void Test_player_has_a_mana_pool()
        {
            Assert.IsNotNull(m_playerA.ManaPool);
        }

        [Test]
        public void Test_Can_get_set_MaximumHandSize()
        {
            Assert.AreEqual(7, m_player.MaximumHandSize);
            m_player.MaximumHandSize = 10;
            Assert.AreEqual(10, m_player.MaximumHandSize);
        }

        #endregion
    }
}

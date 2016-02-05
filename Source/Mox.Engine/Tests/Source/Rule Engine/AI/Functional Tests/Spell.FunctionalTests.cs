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

namespace Mox.AI.Functional
{
    [TestFixture]
    public class SpellFunctionalTests : AIFunctionalTests
    {
        #region Tests

        [Test]
        public void Test_AI_Will_not_play_Shock_when_not_enough_mana()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");

            SetupGame();

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);

            Assert.AreEqual(20, m_playerB.Life);
        }

        [Test]
        public void Test_AI_Will_play_Shock_when_possible()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");

            SetupGame();

            m_playerA.ManaPool[Color.Red] = 1;

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);

            Assert.Collections.IsEmpty(m_playerA.Hand);

            Assert.AreEqual(20, m_playerA.Life, "Don't shock yourself, dude");
            Assert.AreEqual(18, m_playerB.Life);
        }

        [Test]
        public void Test_AI_Will_play_a_land_to_play_Shock_when_possible()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");

            SetupGame();

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);

            Assert.Collections.IsEmpty(m_playerA.Hand);

            Assert.AreEqual(20, m_playerA.Life, "Don't shock yourself, dude");
            Assert.AreEqual(18, m_playerB.Life);
        }

        [Test]
        public void Test_AI_Will_play_Shock_when_possible_alternate_order()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");

            SetupGame();

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);

            Assert.Collections.IsEmpty(m_playerA.Hand);

            Assert.AreEqual(20, m_playerA.Life, "Eeeeh, don't shock yourself dude");
            Assert.AreEqual(18, m_playerB.Life);
        }

        [Test]
        public void Test_AI_Will_not_play_Shock_when_everything_is_lost()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Battlefield, "10E", "Mountain");

            SetupGame();

            m_playerA.Life = 2;
            m_playerB.Life = 20;

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);

            Assert.That(m_playerA.Hand.Any());
        }

        [Test]
        public void Test_AIs_shocking_each_other()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");

            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");

            SetupGame();

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);

            Assert.Collections.IsEmpty(m_playerA.Hand);
            Assert.Collections.IsEmpty(m_playerB.Hand);

            Assert.AreEqual(18, m_playerA.Life);
            Assert.AreEqual(18, m_playerB.Life);
        }

        [Test]
        public void Test_AIs_shocking_each_other_with_optional_mountain()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");

            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");

            SetupGame();

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, false);

            Assert.AreEqual(18, m_playerA.Life);
            Assert.AreEqual(18, m_playerB.Life);
        }

        [Test]
        public void Test_AIs_cannot_play_more_than_one_land_per_turn()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");

            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");

            SetupGame();

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, true);

            Assert.AreEqual(18, m_playerA.Life);
            Assert.AreEqual(18, m_playerB.Life);
        }

        [Test]
        public void Test_Shocking_showdown()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");

            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");

            SetupGame();

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, false);

            Assert.AreEqual(16, m_playerA.Life);
            Assert.AreEqual(16, m_playerB.Life);
        }

        [Test, Ignore("Too long")]
        public void Test_Shocking_shootout_stress_test()
        {
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");

            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Shock");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");
            AddCard(m_playerB, m_game.Zones.Hand, "10E", "Mountain");

            SetupGame();

            Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, false);

            Assert.AreEqual(14, m_playerA.Life);
            Assert.AreEqual(14, m_playerB.Life);
        }

        [Test, Ignore("Too long")]
        public void Test_Shocking_shootout_stress_test_2()
        {
            const int numCards = 4;

            foreach (Player player in m_game.Players)
            {
                for (int i = 0; i < numCards; i++)
                {
                    AddCard(player, m_game.Zones.Hand, "10E", "Shock");
                }

                for (int i = 0; i < numCards; i++)
                {
                    AddCard(player, m_game.Zones.Battlefield, "10E", "Mountain");
                }
            }

            SetupGame();

            using (Profile())
            {
                Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, false);
            }

            Assert.Collections.IsEmpty(m_playerA.Hand);
            Assert.Collections.IsEmpty(m_playerB.Hand);

            Assert.AreEqual(20 - numCards * 2, m_playerA.Life);
            Assert.AreEqual(20 - numCards * 2, m_playerB.Life);
        }

        [Test, Ignore("Too long")]
        public void Test_Shocking_shootout_stress_test_3()
        {
            const int numCards = 7;

            for (int i = 0; i < numCards; i++)
            {
                AddCard(m_playerA, m_game.Zones.Hand, "10E", "Shock");
            }

            for (int i = 0; i < numCards; i++)
            {
                AddCard(m_playerA, m_game.Zones.Hand, "10E", "Mountain");
            }

            SetupGame();

            using (Profile())
            {
                Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, false);
            }

            Assert.Collections.IsEmpty(m_playerA.Hand);
            Assert.Collections.IsEmpty(m_playerB.Hand);

            Assert.AreEqual(20, m_playerA.Life);
            Assert.AreEqual(20 - numCards * 2, m_playerB.Life);
        }

        [Test, Ignore("Too long")]
        public void Test_Shocking_shootout_stress_test_4()
        {
            const int numCards = 7;

            foreach (Player player in m_game.Players)
            {
                for (int i = 0; i < numCards; i++)
                {
                    AddCard(player, m_game.Zones.Hand, "10E", "Shock");
                }

                for (int i = 0; i < numCards + 3; i++)
                {
                    AddCard(player, m_game.Zones.Hand, "10E", "Mountain");
                }
            }

            SetupGame();

            using (Profile())
            {
                Play_until_all_players_pass_and_the_stack_is_empty(m_playerA, false);
            }

            Assert.Collections.IsEmpty(m_playerA.Hand);
            Assert.Collections.IsEmpty(m_playerB.Hand);

            Assert.AreEqual(20 - numCards * 2, m_playerA.Life);
            Assert.AreEqual(20 - numCards * 2, m_playerB.Life);
        }

        #endregion
    }
}

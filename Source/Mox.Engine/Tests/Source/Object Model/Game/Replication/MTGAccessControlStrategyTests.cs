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
using Mox.Abilities;
using NUnit.Framework;

namespace Mox.Replication
{
    [TestFixture]
    public class MTGAccessControlStrategyTestsTests : BaseGameTests
    {
        #region Variables

        private MTGAccessControlStrategy m_strategy;
        private MockAbility m_mockAbility;

        private List<KeyValuePair<Player, bool>> m_expectedVisibilityChanges;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_strategy = new MTGAccessControlStrategy(m_game);
            m_expectedVisibilityChanges = new List<KeyValuePair<Player, bool>>();

            m_mockAbility = m_game.CreateAbility<MockAbility>(m_card);

            // Start with a fully visible card.
            m_card.Zone = m_game.Zones.Battlefield;
        }

        #endregion

        #region Utilities

        private void Expect_UserAccessChange(Player player, bool newVisibility)
        {
            KeyValuePair<Player, bool> visibilityChange = new KeyValuePair<Player,bool>(player, newVisibility);
            Assert.IsFalse(m_expectedVisibilityChanges.Any(pair => pair.Key == player), "There is already another expectation for that player");
            m_expectedVisibilityChanges.Add(visibilityChange);
        }

        private void Assert_Triggers_UserAccessChanged(Object obj, System.Action action)
        {
            EventSink<UserAccessChangedEventArgs<Player>> sink = new EventSink<UserAccessChangedEventArgs<Player>>(m_strategy);
            m_strategy.UserAccessChanged += sink;

            sink.Callback += delegate(object sender, UserAccessChangedEventArgs<Player> e)
            {
                Assert.AreEqual(m_strategy, sender);
                if (Equals(obj, e.Object))
                {
                    Assert.IsTrue(m_expectedVisibilityChanges.Any(pair => pair.Key == e.User), "Received unexpected visibility change event for player " + GetPlayerName(e.User));
                    KeyValuePair<Player, bool> expectedEvent = m_expectedVisibilityChanges.First(pair => pair.Key == e.User);
                    Assert.AreEqual(expectedEvent.Value, e.Access[UserAccess.Read], "Expected object's visibility to become {0} for player {1}", expectedEvent.Value, GetPlayerName(e.User));
                    m_expectedVisibilityChanges.Remove(expectedEvent);
                }
            };

            action();

            if (m_expectedVisibilityChanges.Count > 0)
            {
                KeyValuePair<Player, bool> first = m_expectedVisibilityChanges.First();
                Assert.Fail("Expected user access change to {0} for player {1}", first.Value, GetPlayerName(first.Key));
            }
        }

        private static string GetPlayerName(Player player)
        {
            return player == null ? "<Spectator>" : player.Name;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new MTGAccessControlStrategy(null); });
        }

        [Test]
        public void Test_Invalid_IsVisible_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_strategy.GetUserAccess(m_playerA, null));
        }

        #region Players

        [Test]
        public void Test_Players_are_always_visible_to_one_another()
        {
            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerA, m_playerA));
            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerB, m_playerA));
            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(null, m_playerA));
        }

        #endregion

        #region Cards

        [Test]
        public void Test_Cards_are_visible_to_everyone_when_they_are_in_public_zones()
        {
            foreach (Zone zone in new[] { m_game.Zones.Battlefield, m_game.Zones.PhasedOut, m_game.Zones.Graveyard, m_game.Zones.Exile, m_game.Zones.Stack })
            {
                m_card.Zone = zone;

                Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerA, m_card));
                Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerB, m_card));
                Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(null, m_card));
            }
        }

        [Test]
        public void Test_Cards_are_visible_only_to_their_owner_when_in_hand()
        {
            m_card.Zone = m_game.Zones.Hand;

            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerA, m_card));
            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(m_playerB, m_card));
            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(null, m_card));
        }

        [Test]
        public void Test_Cards_are_visible_to_nobody_when_in_library()
        {
            m_card.Zone = m_game.Zones.Library;

            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(m_playerA, m_card));
            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(m_playerB, m_card));
            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(null, m_card));
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_an_invisible_zone_to_a_visible_zone()
        {
            m_card.Zone = m_game.Zones.Library;

            Expect_UserAccessChange(m_playerA, true);
            Expect_UserAccessChange(m_playerB, true);
            Expect_UserAccessChange(null, true);
            Assert_Triggers_UserAccessChanged(m_card, () => m_card.Zone = m_game.Zones.Battlefield);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_a_visible_zone_to_an_invisible_zone()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            Expect_UserAccessChange(m_playerA, false);
            Expect_UserAccessChange(m_playerB, false);
            Expect_UserAccessChange(null, false);
            Assert_Triggers_UserAccessChanged(m_card, () => m_card.Zone = m_game.Zones.Library);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_Library_to_Hand()
        {
            m_card.Zone = m_game.Zones.Library;

            Expect_UserAccessChange(m_playerA, true);
            Assert_Triggers_UserAccessChanged(m_card, () => m_card.Zone = m_game.Zones.Hand);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_Hand_to_Library()
        {
            m_card.Zone = m_game.Zones.Hand;

            Expect_UserAccessChange(m_playerA, false);
            Assert_Triggers_UserAccessChanged(m_card, () => m_card.Zone = m_game.Zones.Library);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_Hand_to_Play()
        {
            m_card.Zone = m_game.Zones.Hand;

            Expect_UserAccessChange(m_playerB, true);
            Expect_UserAccessChange(null, true);
            Assert_Triggers_UserAccessChanged(m_card, () => m_card.Zone = m_game.Zones.Battlefield);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_Play_to_Hand()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            Expect_UserAccessChange(m_playerB, false);
            Expect_UserAccessChange(null, false);
            Assert_Triggers_UserAccessChanged(m_card, () => m_card.Zone = m_game.Zones.Hand);
        }

        #endregion

        #region Abilities

        [Test]
        public void Test_Abilities_are_visible_if_they_have_a_visible_Source()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerA, m_mockAbility));
            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerB, m_mockAbility));
            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(null, m_mockAbility));

            m_card.Zone = m_game.Zones.Hand;

            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerA, m_mockAbility));
            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(m_playerB, m_mockAbility));
            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(null, m_mockAbility));

            m_card.Zone = m_game.Zones.Library;

            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(m_playerA, m_mockAbility));
            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(m_playerB, m_mockAbility));
            Assert.AreEqual(UserAccess.None, m_strategy.GetUserAccess(null, m_mockAbility));
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_an_ability_Source_changes_from_Play_to_Hand()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            Expect_UserAccessChange(m_playerB, false);
            Expect_UserAccessChange(null, false);
            Assert_Triggers_UserAccessChanged(m_mockAbility, () => m_card.Zone = m_game.Zones.Hand);
        }

        #endregion

        #region Game State

        [Test]
        public void Test_gameState_is_always_visible_to_everybody()
        {
            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerA, m_game.State));
            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(m_playerB, m_game.State));
            Assert.AreEqual(UserAccess.Read, m_strategy.GetUserAccess(null, m_game.State));
        }

        #endregion

        #endregion
    }
}

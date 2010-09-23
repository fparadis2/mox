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

namespace Mox.Replication
{
    [TestFixture]
    public class MTGGameVisibilityStrategyTests : BaseGameTests
    {
        #region Variables

        private MTGGameVisibilityStrategy m_strategy;

        private List<KeyValuePair<Player, bool>> m_expectedVisibilityChanges;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_strategy = new MTGGameVisibilityStrategy(m_game);
            m_expectedVisibilityChanges = new List<KeyValuePair<Player, bool>>();

            // Start with a fully visible card.
            m_card.Zone = m_game.Zones.Battlefield;
        }

        #endregion

        #region Utilities

        private void Expect_VisibilityChange(Player player, bool newVisibility)
        {
            KeyValuePair<Player, bool> visibilityChange = new KeyValuePair<Player,bool>(player, newVisibility);
            Assert.IsFalse(m_expectedVisibilityChanges.Any(pair => pair.Key == player), "There is already another expectation for that player");
            m_expectedVisibilityChanges.Add(visibilityChange);
        }

        private void Assert_Triggers_ObjectVisibilityChanged(Object obj, System.Action action)
        {
            EventSink<VisibilityChangedEventArgs> sink = new EventSink<VisibilityChangedEventArgs>(m_strategy);
            m_strategy.ObjectVisibilityChanged += sink;

            sink.Callback += delegate(object sender, VisibilityChangedEventArgs e)
            {
                Assert.AreEqual(m_strategy, sender);
                if (Equals(obj, e.Object))
                {
                    Assert.IsTrue(m_expectedVisibilityChanges.Any(pair => pair.Key == e.Player), "Received unexpected visibility change event for player " + GetPlayerName(e.Player));
                    KeyValuePair<Player, bool> expectedEvent = m_expectedVisibilityChanges.First(pair => pair.Key == e.Player);
                    Assert.AreEqual(expectedEvent.Value, e.Visibility, "Expected object's visibility to become {0} for player {1}", expectedEvent.Value, GetPlayerName(e.Player));
                    m_expectedVisibilityChanges.Remove(expectedEvent);
                }
            };

            action();

            if (m_expectedVisibilityChanges.Count > 0)
            {
                KeyValuePair<Player, bool> first = m_expectedVisibilityChanges.First();
                Assert.Fail("Expected visibility change to {0} for player {1}", first.Value, GetPlayerName(first.Key));
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
            Assert.Throws<ArgumentNullException>(delegate { new MTGGameVisibilityStrategy(null); });
        }

        [Test]
        public void Test_Invalid_IsVisible_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_strategy.IsVisible(null, m_playerA));
        }

        #region Players

        [Test]
        public void Test_Players_are_always_visible_to_one_another()
        {
            Assert.IsTrue(m_strategy.IsVisible(m_playerA, m_playerA));
            Assert.IsTrue(m_strategy.IsVisible(m_playerA, m_playerB));
            Assert.IsTrue(m_strategy.IsVisible(m_playerA, null));
        }

        #endregion

        #region Cards

        [Test]
        public void Test_Cards_are_visible_to_everyone_when_they_are_in_public_zones()
        {
            foreach (Zone zone in new[] { m_game.Zones.Battlefield, m_game.Zones.PhasedOut, m_game.Zones.Graveyard, m_game.Zones.Exile, m_game.Zones.Stack })
            {
                m_card.Zone = zone;

                Assert.IsTrue(m_strategy.IsVisible(m_card, m_playerA));
                Assert.IsTrue(m_strategy.IsVisible(m_card, m_playerB));
                Assert.IsTrue(m_strategy.IsVisible(m_card, null));
            }
        }

        [Test]
        public void Test_Cards_are_visible_only_to_their_owner_when_in_hand()
        {
            m_card.Zone = m_game.Zones.Hand;

            Assert.IsTrue(m_strategy.IsVisible(m_card, m_playerA));
            Assert.IsFalse(m_strategy.IsVisible(m_card, m_playerB));
            Assert.IsFalse(m_strategy.IsVisible(m_card, null));
        }

        [Test]
        public void Test_Cards_are_visible_to_nobody_when_in_library()
        {
            m_card.Zone = m_game.Zones.Library;

            Assert.IsFalse(m_strategy.IsVisible(m_card, m_playerA));
            Assert.IsFalse(m_strategy.IsVisible(m_card, m_playerB));
            Assert.IsFalse(m_strategy.IsVisible(m_card, null));
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_an_invisible_zone_to_a_visible_zone()
        {
            m_card.Zone = m_game.Zones.Library;

            Expect_VisibilityChange(m_playerA, true);
            Expect_VisibilityChange(m_playerB, true);
            Expect_VisibilityChange(null, true);
            Assert_Triggers_ObjectVisibilityChanged(m_card, () => m_card.Zone = m_game.Zones.Battlefield);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_a_visible_zone_to_an_invisible_zone()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            Expect_VisibilityChange(m_playerA, false);
            Expect_VisibilityChange(m_playerB, false);
            Expect_VisibilityChange(null, false);
            Assert_Triggers_ObjectVisibilityChanged(m_card, () => m_card.Zone = m_game.Zones.Library);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_Library_to_Hand()
        {
            m_card.Zone = m_game.Zones.Library;

            Expect_VisibilityChange(m_playerA, true);
            Assert_Triggers_ObjectVisibilityChanged(m_card, () => m_card.Zone = m_game.Zones.Hand);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_Hand_to_Library()
        {
            m_card.Zone = m_game.Zones.Hand;

            Expect_VisibilityChange(m_playerA, false);
            Assert_Triggers_ObjectVisibilityChanged(m_card, () => m_card.Zone = m_game.Zones.Library);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_Hand_to_Play()
        {
            m_card.Zone = m_game.Zones.Hand;

            Expect_VisibilityChange(m_playerB, true);
            Expect_VisibilityChange(null, true);
            Assert_Triggers_ObjectVisibilityChanged(m_card, () => m_card.Zone = m_game.Zones.Battlefield);
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_a_card_changes_from_Play_to_Hand()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            Expect_VisibilityChange(m_playerB, false);
            Expect_VisibilityChange(null, false);
            Assert_Triggers_ObjectVisibilityChanged(m_card, () => m_card.Zone = m_game.Zones.Hand);
        }

        #endregion

        #region Abilities

        [Test]
        public void Test_Abilities_are_visible_if_they_have_a_visible_Source()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            Assert.IsTrue(m_strategy.IsVisible(m_mockAbility, m_playerA));
            Assert.IsTrue(m_strategy.IsVisible(m_mockAbility, m_playerB));
            Assert.IsTrue(m_strategy.IsVisible(m_mockAbility, null));

            m_card.Zone = m_game.Zones.Hand;

            Assert.IsTrue(m_strategy.IsVisible(m_mockAbility, m_playerA));
            Assert.IsFalse(m_strategy.IsVisible(m_mockAbility, m_playerB));
            Assert.IsFalse(m_strategy.IsVisible(m_mockAbility, null));

            m_card.Zone = m_game.Zones.Library;

            Assert.IsFalse(m_strategy.IsVisible(m_mockAbility, m_playerA));
            Assert.IsFalse(m_strategy.IsVisible(m_mockAbility, m_playerB));
            Assert.IsFalse(m_strategy.IsVisible(m_mockAbility, null));
        }

        [Test]
        public void Test_ObjectVisibilityChanged_is_triggered_when_an_ability_Source_changes_from_Play_to_Hand()
        {
            m_card.Zone = m_game.Zones.Battlefield;

            Expect_VisibilityChange(m_playerB, false);
            Expect_VisibilityChange(null, false);
            Assert_Triggers_ObjectVisibilityChanged(m_mockAbility, () => m_card.Zone = m_game.Zones.Hand);
        }

        #endregion

        #region Game State

        [Test]
        public void Test_gameState_is_always_visible_to_everybody()
        {
            Assert.IsTrue(m_strategy.IsVisible(m_game.State, m_playerA));
            Assert.IsTrue(m_strategy.IsVisible(m_game.State, m_playerB));
            Assert.IsTrue(m_strategy.IsVisible(m_game.State, null));
        }

        #endregion

        #endregion
    }
}

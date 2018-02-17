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

namespace Mox.Abilities
{
    [TestFixture]
    public class DiscardCostTests : CostTestsBase
    {
        #region Variables

        private Filter m_filter;
        private Card m_card2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_card2 = CreateCard(m_playerA);

            m_card.Zone = m_game.Zones.Hand;
            m_card2.Zone = m_game.Zones.Hand;

            Assert.AreEqual(2, m_playerA.Hand.Count);

            m_filter = HandFilter.Any;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Execute_asks_the_player_to_discard_a_single_card()
        {
            var cost = new DiscardCost(1, m_filter);

            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card, m_card2 }, m_card, TargetContextType.Discard);
            Execute(cost, true);

            Assert.Collections.AreEquivalent(new[] { m_card2 }, m_playerA.Hand);
            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
        }

        [Test]
        public void Test_Player_can_cancel_discarding_a_single_card()
        {
            var cost = new DiscardCost(1, m_filter);

            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card, m_card2 }, null, TargetContextType.Discard);
            Execute(cost, false);

            Assert.Collections.AreEquivalent(new[] { m_card, m_card2 }, m_playerA.Hand);
        }

        [Test]
        public void Test_Execute_asks_the_player_to_discard_multiple_cards()
        {
            var cost = new DiscardCost(2, m_filter);

            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card, m_card2 }, m_card, TargetContextType.Discard);
            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card2 }, m_card2, TargetContextType.Discard);
            Execute(cost, true);

            Assert.Collections.IsEmpty(m_playerA.Hand);
            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, m_card2.Zone);
        }

        [Test]
        public void Test_Execute_respects_the_filter()
        {
            m_card.Type = Type.Creature;
            m_card2.Type = Type.Artifact | Type.Creature;

            var cost = new DiscardCost(1, m_filter & CardFilter.OfType(Type.Artifact));

            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card2 }, m_card2, TargetContextType.Discard);
            Execute(cost, true);

            Assert.Collections.AreEquivalent(new[] { m_card }, m_playerA.Hand);
            Assert.AreEqual(m_game.Zones.Graveyard, m_card2.Zone);
        }

        [Test]
        public void Test_Player_can_cancel_discarding_multiple_cards()
        {
            var cost = new DiscardCost(2, m_filter);

            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card, m_card2 }, m_card, TargetContextType.Discard);
            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card2 }, null, TargetContextType.Discard);
            Execute(cost, false);
        }

        [Test]
        public void Test_Player_is_asked_until_he_gives_a_correct_choice()
        {
            var cost = new DiscardCost(2, m_filter);

            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card, m_card2 }, m_card, TargetContextType.Discard);
            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card2 }, m_card, TargetContextType.Discard);
            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card2 }, CreateCard(m_playerA), TargetContextType.Discard);
            m_sequencer.Expect_Player_Target(m_playerA, true, new[] { m_card2 }, m_card2, TargetContextType.Discard);
            Execute(cost, true);

            Assert.Collections.IsEmpty(m_playerA.Hand);
        }

        [Test]
        public void Test_Cost_returns_false_if_the_player_doesnt_have_enough_cards_to_discard()
        {
            var cost = new DiscardCost(99, m_filter);
            Execute(cost, false);

            Assert.Collections.AreEquivalent(new[] { m_card, m_card2 }, m_playerA.Hand);
        }

        [Test]
        public void Test_Cost_to_discard_whole_hand_is_always_ok()
        {
            var cost = new DiscardHandCost();
            Execute(cost, true);

            Assert.Collections.IsEmpty(m_playerA.Hand);
        }

        #endregion
    }
}

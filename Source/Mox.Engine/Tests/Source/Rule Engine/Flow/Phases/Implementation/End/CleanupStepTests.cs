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
using Rhino.Mocks;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class CleanupStepTests : BaseStepTests<CleanupStep>
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_step = new CleanupStep();

            m_sequencerTester.MockPlayerController(m_playerA);
        }

        #endregion

        #region Utilities

        private Card CreateCardAndAddToPlayerHand(Player owner)
        {
            Card card = CreateCard(owner);
            card.Zone = m_game.Zones.Hand;
            return card;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(Steps.Cleanup, m_step.Type);
        }

        [Test]
        public void Test_Nothing_to_do_if_hand_size_is_smaller_than_maximum_hand_size()
        {
            m_playerA.MaximumHandSize = 10;

            CreateCardAndAddToPlayerHand(m_playerA);
            CreateCardAndAddToPlayerHand(m_playerA);
            CreateCardAndAddToPlayerHand(m_playerA);
            CreateCardAndAddToPlayerHand(m_playerA);
            CreateCardAndAddToPlayerHand(m_playerA);

            RunStep(m_playerA);
        }

        [Test]
        public void Test_If_active_players_hand_contains_more_cards_than_his_maximum_hand_size_he_must_discard_cards()
        {
            m_playerA.MaximumHandSize = 3;
            
            Card card1 = CreateCardAndAddToPlayerHand(m_playerA);
            Card card2 = CreateCardAndAddToPlayerHand(m_playerA);
            Card card3 = CreateCardAndAddToPlayerHand(m_playerA);
            Card card4 = CreateCardAndAddToPlayerHand(m_playerA);
            Card card5 = CreateCardAndAddToPlayerHand(m_playerA);

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card2, card3, card4, card5 }, card1, TargetContextType.Discard);
                m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card2, card3, card4, card5 }, card3, TargetContextType.Discard);
            }

            RunStep(m_playerA);

            Assert.Collections.AreEquivalent(new[] { card2, card4, card5 }, m_playerA.Hand);
            Assert.AreEqual(m_game.Zones.Graveyard, card1.Zone);
            Assert.AreEqual(m_game.Zones.Graveyard, card3.Zone);
        }

        [Test]
        public void Test_Active_player_continues_to_discard_until_he_gives_a_correct_card()
        {
            m_playerA.MaximumHandSize = 1;

            Card card1 = CreateCardAndAddToPlayerHand(m_playerA);
            Card card2 = CreateCardAndAddToPlayerHand(m_playerA);

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card2 }, null, TargetContextType.Discard);
                m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card2 }, m_card, TargetContextType.Discard);
                m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card2 }, card1, TargetContextType.Discard);
            }

            RunStep(m_playerA);
        }

        [Test]
        public void Test_Damage_dealt_on_permanents_is_removed_after_discarding()
        {
            m_playerA.MaximumHandSize = 1;

            Card card1 = CreateCardAndAddToPlayerHand(m_playerA);
            Card card2 = CreateCardAndAddToPlayerHand(m_playerA);

            card2.Damage = 10;

            using (m_mockery.Ordered())
            {
                m_sequencerTester.Expect_Player_Target(m_playerA, false, new[] { card1, card2 }, card1, TargetContextType.Discard);
            }

            RunStep(m_playerA);

            Assert.AreEqual(0, card2.Damage);
        }

        [Test]
        public void Test_Triggers_EndOfTurn_event()
        {
            m_game.AssertTriggers<Events.EndOfTurnEvent>(() => RunStep(m_playerA), e =>
            { 
                Assert.AreEqual(m_playerA, e.ActivePlayer); 
            });
        }

        #endregion
    }
}

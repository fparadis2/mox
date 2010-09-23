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
using Rhino.Mocks;

namespace Mox
{
    [TestFixture]
    public class GameActionsTests : BaseGameTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();
        }

        #endregion

        #region Tests

        #region DrawCards

        [Test]
        public void Test_DrawCards()
        {
            Card otherCard = CreateCard(m_playerA); otherCard.Zone = m_game.Zones.Library;

            Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library); // Sanity check

            m_playerA.DrawCards(1);

            Assert.Collections.AreEqual(new[] { m_card }, m_playerA.Library);
            Assert.Collections.AreEqual(new[] { otherCard }, m_playerA.Hand);
        }

        [Test]
        public void Test_DrawCards_can_draw_multiple_cards()
        {
            Card otherCard = CreateCard(m_playerA); otherCard.Zone = m_game.Zones.Library;

            Assert.Collections.AreEqual(new[] { m_card, otherCard }, m_playerA.Library); // Sanity check

            m_playerA.DrawCards(2);

            Assert.Collections.IsEmpty(m_playerA.Library);
            Assert.Collections.AreEquivalent(new[] { m_card, otherCard }, m_playerA.Hand);
        }

        [Test]
        public void Test_Cannot_draw_more_cards_than_available()
        {
            Assert.Less(m_playerA.Library.Count, 2);
            Assert.Throws<InvalidOperationException>(() => m_playerA.DrawCards(2));
        }

        [Test]
        public void DrawCards_triggers_the_DrawCard_event_for_each_drawn_card()
        {
            Card otherCard = CreateCard(m_playerA);

            m_playerA.Library.MoveToTop(new[] {m_card, otherCard});

            int times = 0;
            m_game.AssertTriggers<Events.DrawCardEvent>(() => m_playerA.DrawCards(2), e =>
            {
                Assert.AreEqual(m_playerA, e.Player);
                Assert.AreEqual(times++ == 0 ? otherCard : m_card, e.Card);
            }, 2);
        }

        #endregion

        #region Discard

        [Test]
        public void Discard_moves_the_card_into_the_players_graveyard()
        {
            m_card.Zone = m_game.Zones.Hand;

            m_playerA.Discard(m_card);

            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
        }

        [Test]
        public void Discard_triggers_the_PlayerDiscarded_event()
        {
            m_card.Zone = m_game.Zones.Hand;

            m_game.AssertTriggers<Events.PlayerDiscardedEvent>(() => m_playerA.Discard(m_card), e =>
            {
                Assert.AreEqual(m_playerA, e.Player);
                Assert.AreEqual(m_card, e.Card);
            });
        }

        #endregion

        #endregion
    }
}

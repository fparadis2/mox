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
using Mox.Flow;
using NUnit.Framework;

namespace Mox.AI.Resolvers
{
    [TestFixture]
    public class TargetResolverTests : BaseMTGChoiceResolverTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        internal override BaseMTGChoiceResolver CreateResolver()
        {
            return new TargetResolver();
        }

        #endregion

        #region Methods

        private object[] ConstructArguments(bool allowCancel, int[] possibleTargets)
        {
            return new object[] { m_context, m_playerA, new TargetContext(allowCancel, possibleTargets, TargetContextType.Normal) };
        }

        private IEnumerable<ITargetable> Resolve(params ITargetable[] targets)
        {
            return m_choiceResolver.ResolveChoices(GetMethod(), ConstructArguments(false, targets.Select(t => t.Identifier).ToArray()))
                                   .Select(c => m_game.GetObjectByIdentifier<ITargetable>((int)c));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_ResolveChoices_returns_all_possible_targets()
        {
            var targets = new ITargetable[] {m_playerA, m_playerB, m_card};
            Assert.Collections.AreEqual(targets, Resolve(targets));
        }

        [Test]
        public void Test_Default_choice_is_the_first_possible_target()
        {
            var targets = new [] { m_playerA.Identifier, m_playerB.Identifier, m_card.Identifier };
            Assert.AreEqual(m_playerA.Identifier, m_choiceResolver.GetDefaultChoice(GetMethod(), ConstructArguments(false, targets)));
        }

        #endregion

        #region Optimizations

        #region Only try distinct targets

        private Card CreateCard(string cardName, Zone zone)
        {
            Card card = CreateCard(m_playerA, cardName);
            card.Zone = zone;
            return card;
        }

        [Test]
        public void Test_Duplicate_target_cards_are_only_returned_once()
        {
            Card card = CreateCard("MyCard", m_game.Zones.Battlefield);
            Card identicalCard = CreateCard("MyCard", m_game.Zones.Battlefield);
            Card differentCard = CreateCard("MyCard", m_game.Zones.Battlefield);

            differentCard.Power = 10;

            Assert.Collections.AreEqual(new ITargetable[] {m_playerA, m_playerB, card, differentCard}, Resolve(m_playerA, m_playerB, card, identicalCard, differentCard));
        }

        #endregion

        #endregion
    }
}

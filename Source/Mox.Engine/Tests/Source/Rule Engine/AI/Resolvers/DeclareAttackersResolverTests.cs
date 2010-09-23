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

namespace Mox.AI.Resolvers
{
    [TestFixture]
    public class DeclareAttackersResolverTests : BaseMTGChoiceResolverTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        internal override BaseMTGChoiceResolver CreateResolver()
        {
            return new DeclareAttackersResolver();
        }

        #endregion

        #region Utilities

        private List<DeclareAttackersResult> GetChoices(Player player, params Card[] legalAttackers)
        {
            return m_choiceResolver.ResolveChoices(GetMethod(), new object[] { m_context, player, new DeclareAttackersContext(legalAttackers) }).Cast<DeclareAttackersResult>().ToList();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Returns_empty_when_there_is_nothing_to_do()
        {
            List<DeclareAttackersResult> results = GetChoices(m_playerA);

            Assert.AreEqual(1, results.Count);
            Assert.Collections.IsEmpty(results[0].GetAttackers(m_game));
        }

        [Test]
        public void Test_Returns_both_choices_when_one_possible_attacker()
        {
            List<DeclareAttackersResult> results = GetChoices(m_playerA, m_card);

            Assert.AreEqual(2, results.Count);
            Assert.Collections.AreEqual(new[] { m_card }, results[0].GetAttackers(m_game));
            Assert.Collections.IsEmpty(results[1].GetAttackers(m_game));
        }

        [Test]
        public void Test_Returns_all_combinations_when_multiple_attackers()
        {
            Card cardA = CreateCard(m_playerA); cardA.Power = 1;
            Card cardB = CreateCard(m_playerA); cardB.Power = 2;
            Card cardC = CreateCard(m_playerA); cardC.Power = 3;

            List<DeclareAttackersResult> results = GetChoices(m_playerA, cardA, cardB, cardC);

            Assert.AreEqual(8, results.Count);

            Assert.That(results.Any(r => r.GetAttackers(m_game).Count() == 0));

            Assert.That(results.Any(r => r.GetAttackers(m_game).IsEquivalent(new[] { cardA })));
            Assert.That(results.Any(r => r.GetAttackers(m_game).IsEquivalent(new[] { cardB })));
            Assert.That(results.Any(r => r.GetAttackers(m_game).IsEquivalent(new[] { cardC })));

            Assert.That(results.Any(r => r.GetAttackers(m_game).IsEquivalent(new[] { cardA, cardB })));
            Assert.That(results.Any(r => r.GetAttackers(m_game).IsEquivalent(new[] { cardB, cardC })));
            Assert.That(results.Any(r => r.GetAttackers(m_game).IsEquivalent(new[] { cardC, cardA })));

            Assert.That(results.Any(r => r.GetAttackers(m_game).IsEquivalent(new[] { cardA, cardB, cardC })));
        }

        [Test]
        public void Test_Default_choice_is_to_not_attack()
        {
            DeclareAttackersResult result = (DeclareAttackersResult)m_choiceResolver.GetDefaultChoice(GetMethod(), null);
            Assert.Collections.IsEmpty(result.GetAttackers(m_game));
        }

        #endregion
    }
}

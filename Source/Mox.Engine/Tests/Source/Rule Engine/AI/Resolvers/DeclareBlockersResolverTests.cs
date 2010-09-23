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
    public class DeclareBlockersResolverTests : BaseMTGChoiceResolverTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        internal override BaseMTGChoiceResolver CreateResolver()
        {
            return new DeclareBlockersResolver();
        }

        #endregion

        #region Utilities

        private List<DeclareBlockersResult> GetChoices(Player player, IEnumerable<Card> attackers, IEnumerable<Card> legalBlockers)
        {
            return m_choiceResolver.ResolveChoices(GetMethod(), new object[] { m_context, player, new DeclareBlockersContext(attackers, legalBlockers) }).Cast<DeclareBlockersResult>().ToList();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Returns_empty_when_there_is_nothing_to_do()
        {
            List<DeclareBlockersResult> results = GetChoices(m_playerA, new[] { m_card }, new Card[0]);

            Assert.AreEqual(1, results.Count);
            Assert.Collections.IsEmpty(results[0].Blockers);
        }

        [Test]
        public void Test_Returns_both_choices_when_one_possible_block()
        {
            Card blocking = CreateCard(m_playerA);

            List<DeclareBlockersResult> results = GetChoices(m_playerA, new[] { m_card }, new[] { blocking });

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(1, results[0].Blockers.Count);
            Assert.That(IsBlock(results[0], m_card, blocking));
            Assert.Collections.IsEmpty(results[1].Blockers);
        }

        [Test]
        public void Test_Returns_all_combinations_when_multiple_attackers_and_blockers()
        {
            Card attacker1 = CreateCard(m_playerA);
            Card attacker2 = CreateCard(m_playerA);
            Card blocker1 = CreateCard(m_playerA);
            Card blocker2 = CreateCard(m_playerA);

            List<DeclareBlockersResult> results = GetChoices(m_playerA, new[] { attacker1, attacker2 }, new[] { blocker1, blocker2 });

            Assert.AreEqual(9, results.Count);

            Assert.That(results.Any(r => r.Blockers.Count == 0));

            Assert.That(results.Any(r => IsBlock(r, attacker1, blocker1) && !IsBlock(r, attacker2, blocker2)));
            Assert.That(results.Any(r => !IsBlock(r, attacker1, blocker1) && IsBlock(r, attacker2, blocker2)));
            Assert.That(results.Any(r => IsBlock(r, attacker1, blocker1) && IsBlock(r, attacker2, blocker2)));

            Assert.That(results.Any(r => IsBlock(r, attacker1, blocker2) && !IsBlock(r, attacker2, blocker1)));
            Assert.That(results.Any(r => !IsBlock(r, attacker1, blocker2) && IsBlock(r, attacker2, blocker1)));
            Assert.That(results.Any(r => IsBlock(r, attacker1, blocker2) && IsBlock(r, attacker2, blocker1)));

            Assert.That(results.Any(r => IsBlock(r, attacker1, blocker2) && IsBlock(r, attacker1, blocker1)));
            Assert.That(results.Any(r => IsBlock(r, attacker2, blocker2) && IsBlock(r, attacker2, blocker1)));
        }

        private static bool IsBlock(DeclareBlockersResult result, Card attacker, Card blocker)
        {
            return result.Blockers.Any(pair => pair.BlockedCreatureId == attacker.Identifier && pair.BlockingCreatureId == blocker.Identifier);
        }

        [Test]
        public void Test_Default_choice_is_to_not_block()
        {
            DeclareBlockersResult result = (DeclareBlockersResult)m_choiceResolver.GetDefaultChoice(GetMethod(), null);
            Assert.Collections.IsEmpty(result.Blockers);
        }

        #endregion
    }
}

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

namespace Mox.AI.ChoiceEnumerators
{
    internal class DeclareAttackersChoiceEnumerator : ChoiceEnumerator
    {
        #region Overrides of ChoiceEnumerator

        /// <summary>
        /// Returns the possible choices for the choice context.
        /// </summary>
        public override IEnumerable<object> EnumerateChoices(Game game, Choice choice)
        {
            Player player = choice.Player.Resolve(game);
            DeclareAttackersContext attackInfo = ((DeclareAttackersChoice)choice).AttackContext;

            var attackers = attackInfo.LegalAttackers.Select(a => player.Manager.GetObjectByIdentifier<Card>(a)).ToList();

            var attackCombinations = EnumerateAttackerCombinations(attackers);

            //attackCombinations = Sort(attackCombinations);

            foreach (var combination in attackCombinations)
            {
                yield return new DeclareAttackersResult(combination.ToArray());
            }
        }

        private static IEnumerable<IEnumerable<Card>> Sort(IEnumerable<IEnumerable<Card>> combinations)
        {
            return combinations.OrderByDescending(Score);
        }

        private static int Score(IEnumerable<Card> cards)
        {
            return cards.Aggregate(0, (s, c) => s + c.Toughness + c.Power);
        }

        private static IEnumerable<IEnumerable<Card>> EnumerateAttackerCombinations(IList<Card> attackers)
        {
            IList<EquivalentGroup> groups = Group(attackers);
            var groupCounts = groups.Select(g => g.Count).ToList();

            for (int numAttackers = attackers.Count; numAttackers > 0; numAttackers--)
            {
                var combinations = groupCounts.EnumerateGroupCombinations(numAttackers);
                foreach (var result in MapToAttackers(numAttackers, groups, combinations))
                {
                    yield return result;
                }
            }

            yield return new Card[0];
        }

        private static IEnumerable<IEnumerable<Card>> MapToAttackers(int size, IList<EquivalentGroup> groups, IEnumerable<IList<int>> combinations)
        {
            foreach (var combination in combinations)
            {
                int[] usages = new int[groups.Count];
                List<Card> cards = new List<Card>(size);

                foreach (int index in combination)
                {
                    cards.Add(groups[index][usages[index]++]);
                }

                yield return cards;
            }
        }

        private static IList<EquivalentGroup> Group(IEnumerable<Card> attackers)
        {
            List<EquivalentGroup> groups = new List<EquivalentGroup>();

            foreach (Card card in attackers)
            {
                Card attacker = card;
                if (!groups.Any(g => g.Consider(attacker)))
                {
                    groups.Add(new EquivalentGroup(card));
                }
            }

            return groups;
        }

        #region Equivalence Grouping

        private class EquivalentGroup
        {
            private readonly List<Card> m_cards = new List<Card>();

            public EquivalentGroup(Card card)
            {
                m_cards.Add(card);
            }

            public Card this[int index]
            {
                get { return m_cards[index]; }
            }

            public int Count
            {
                get { return m_cards.Count; }
            }

            public bool Consider(Card card)
            {
                if (m_cards[0].IsEquivalentTo(card))
                {
                    m_cards.Add(card);
                    return true;
                }

                return false;
            }
        }

        #endregion

        #endregion
    }
}

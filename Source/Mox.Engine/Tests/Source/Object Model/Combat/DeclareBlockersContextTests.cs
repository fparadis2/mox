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
using System.Collections.Generic;

namespace Mox
{
    [TestFixture]
    public class DeclareBlockersContextTests : BaseGameTests
    {
        #region Variables

        private DeclareBlockersContext m_context;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_context = new DeclareBlockersContext(new[] { CreateCard(m_playerA) }, new[] { m_card });
        }

        #endregion

        #region Utilities

        private DeclareBlockersResult CreateResult(Card attacker, params Card[] blockers)
        {
            List<DeclareBlockersResult.BlockingCreature> list = new List<DeclareBlockersResult.BlockingCreature>();

            foreach (Card blocker in blockers)
            {
                list.Add(new DeclareBlockersResult.BlockingCreature(blocker, attacker));
            }

            return new DeclareBlockersResult(list.ToArray());
        }

        #endregion

        #region Tests

        [Test]
        public void Test_construction_values()
        {
            Assert.Collections.AreEqual(new[] { m_card.Identifier }, m_context.LegalBlockers);
        }

        [Test]
        public void Test_IsEmpty_returns_true_if_there_is_no_legal_blocker()
        {
            Assert.IsTrue(new DeclareBlockersContext(new[] { m_card }, new Card[0]).IsEmpty);
            Assert.IsFalse(m_context.IsEmpty);
        }

        [Test]
        public void Test_FromPlayer_returns_a_context_where_legal_blockers_are_all_the_untapped_creature_cards_on_the_battlefield_controlled_by_the_player()
        {
            List<Card> creatures = new List<Card>();
            for (int i = 0; i < 7; i++)
            {
                creatures.Add(CreateCard(m_playerA));

            }

            creatures.ForEach(c =>
            {
                c.Type = Type.Creature;
                c.Zone = m_game.Zones.Battlefield;
            });

            creatures[2].Zone = m_game.Zones.Exile; // not on battlefield
            creatures[3].Type = Type.Enchantment; // not a creature
            creatures[4].Tapped = true; // tapped
            creatures[5].Controller = m_playerB; // not controlled by player
            m_game.CreateAbility<CannotBlockAbility>(creatures[6]);

            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);

            DeclareBlockersContext context = DeclareBlockersContext.ForPlayer(m_playerA);
            Assert.Collections.AreEqual(creatures.Take(2).Select(c => c.Identifier), context.LegalBlockers);
            Assert.Collections.AreEqual(new[] { m_card.Identifier }, context.Attackers);
        }

        [Test]
        public void Test_Is_serializable()
        {
            Assert.IsSerializable(m_context);
        }

        [Test]
        public void Test_IsValid_returns_true_only_if_each_blocker_is_in_the_list_of_legal_blockers_and_each_blocked_creature_in_the_list_of_attackers()
        {
            Card attacker = CreateCard(m_playerA);

            Card otherCard = CreateCard(m_playerA);
            m_context = new DeclareBlockersContext(new[] { attacker }, new[] { m_card, otherCard });

            Assert.IsTrue(m_context.IsValid(CreateResult(attacker, m_card)));
            Assert.IsTrue(m_context.IsValid(CreateResult(attacker, otherCard)));
            Assert.IsTrue(m_context.IsValid(CreateResult(attacker, m_card, otherCard)));
            Assert.IsTrue(m_context.IsValid(CreateResult(attacker, otherCard, m_card)));

            Assert.IsFalse(m_context.IsValid(CreateResult(attacker, CreateCard(m_playerA))));
            Assert.IsFalse(m_context.IsValid(CreateResult(attacker, m_card, CreateCard(m_playerA))));
            Assert.IsFalse(m_context.IsValid(CreateResult(CreateCard(m_playerA), m_card)));
        }

        #endregion
    }
}

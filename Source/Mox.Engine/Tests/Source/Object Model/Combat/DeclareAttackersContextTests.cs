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

using Mox.Abilities;

namespace Mox
{
    [TestFixture]
    public class DeclareAttackersContextTests : BaseGameTests
    {
        #region Variables

        private DeclareAttackersContext m_context;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_context = new DeclareAttackersContext(new[] { m_card });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_construction_values()
        {
            Assert.Collections.AreEqual(new[] { m_card.Identifier }, m_context.LegalAttackers);
        }

        [Test]
        public void Test_IsEmpty_returns_true_if_there_is_no_legal_attacker()
        {
            Assert.IsTrue(new DeclareAttackersContext(new Card[0]).IsEmpty);
            Assert.IsFalse(m_context.IsEmpty);
        }

        [Test]
        public void Test_FromPlayer_returns_a_context_where_legal_attackers_are_all_the_untapped_creature_cards_on_the_battlefield_controlled_by_the_player()
        {
            List<Card> creatures = new List<Card>();

            for (int i = 0; i < 8; i++)
            {
                creatures.Add(CreateCard(m_playerA));
            }

            creatures.ForEach(c =>
            {
                c.Type = Type.Creature;
                c.Zone = m_game.Zones.Battlefield;
                c.HasSummoningSickness = false;
            });

            creatures[2].Zone = m_game.Zones.Exile; // not on battlefield
            creatures[3].Type = Type.Enchantment; // not a creature
            creatures[4].Tapped = true; // tapped
            creatures[5].Controller = m_playerB; // not controlled by player
            m_game.CreateAbility<CannotAttackAbility>(creatures[6]); // cannot attack
            creatures[7].HasSummoningSickness = true; // summoning sickness

            DeclareAttackersContext context = DeclareAttackersContext.ForPlayer(m_playerA);
            Assert.Collections.AreEqual(creatures.Take(2).Select(c => c.Identifier), context.LegalAttackers);
        }

        [Test]
        public void Test_Is_serializable()
        {
            Assert.IsSerializable(m_context);
        }

        [Test]
        public void Test_IsValid_returns_true_only_if_each_attacker_is_in_the_list_of_legal_attackers()
        {
            Card otherCard = CreateCard(m_playerA);
            m_context = new DeclareAttackersContext(new[] { m_card, otherCard });

            Assert.IsTrue(m_context.IsValid(new DeclareAttackersResult(m_card)));
            Assert.IsTrue(m_context.IsValid(new DeclareAttackersResult(otherCard)));
            Assert.IsTrue(m_context.IsValid(new DeclareAttackersResult(m_card, otherCard)));
            Assert.IsTrue(m_context.IsValid(new DeclareAttackersResult(otherCard, m_card)));

            Assert.IsFalse(m_context.IsValid(new DeclareAttackersResult(CreateCard(m_playerA))));
            Assert.IsFalse(m_context.IsValid(new DeclareAttackersResult(m_card, CreateCard(m_playerA))));
        }

        #endregion
    }
}

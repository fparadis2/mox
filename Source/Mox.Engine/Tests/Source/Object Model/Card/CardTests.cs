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
using System.Runtime.Remoting;
using Mox.Rules;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class CardTests : BaseGameTests
    {
        #region Variables

        #endregion

        #region Setup

        #endregion

        #region Utilities

        private int CountOfCards
        {
            get { return m_game.Cards.Count; }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_playerA, m_card.Owner);
            Assert.AreEqual(m_playerA, m_card.Controller);
            Assert.AreEqual("My Card", m_card.Name);
        }

        [Test]
        public void Test_Cannot_create_a_card_with_a_null_owner()
        {
            Assert.Throws<ArgumentNullException>(() => CreateCard(null));
        }

        [Test]
        public void Test_Cannot_create_a_card_with_an_owner_from_another_game()
        {
            Assert.Throws<ArgumentException>(() => m_game.CreateCard(new Game().CreatePlayer(), new CardIdentifier { Card = "B" }));
        }

        [Test]
        public void Test_Cannot_create_a_card_with_an_invalid_name()
        {
            Assert.Throws<ArgumentException>(() => m_game.CreateCard(m_playerA, new CardIdentifier()));
        }

        [Test]
        public void Test_Creation_is_undoable()
        {
            int initialCount = CountOfCards;

            Assert.IsUndoRedoable(m_game.Controller,
                () => Assert.AreEqual(initialCount, CountOfCards),
                () => CreateCard(m_playerA),
                () => Assert.AreEqual(initialCount + 1, CountOfCards));
        }

        [Test]
        public void Test_Cards_Manager_is_the_game()
        {
            Assert.AreEqual(m_game, m_card.Manager);
        }

        [Test]
        public void Test_Card_is_automatically_in_library_after_construction()
        {
            Assert.AreEqual(m_game.Zones.Library, m_card.Zone);
        }

        [Test]
        public void Test_Can_change_the_controller_of_a_card()
        {
            // Must be in a zone where it's allowed
            m_card.Zone = m_game.Zones.Battlefield;

            m_card.Controller = m_playerB;
            Assert.AreSame(m_playerB, m_card.Controller);
        }

        [Test]
        public void Test_Cannot_change_the_controller_of_a_card_to_null()
        {
            Assert.Throws<ArgumentNullException>(delegate { m_card.Controller = null; });
        }

        [Test]
        public void Test_Can_get_set_Cards_super_type()
        {
            Card card = CreateCard(m_playerA);
            Assert.AreEqual(SuperType.None, card.SuperType);
            card.SuperType = SuperType.Legendary | SuperType.Snow;
            Assert.AreEqual(SuperType.Legendary | SuperType.Snow, card.SuperType);
        }

        [Test]
        public void Test_Can_get_set_Cards_type()
        {
            Card card = CreateCard(m_playerA);
            Assert.AreEqual(Type.None, card.Type);
            card.Type = Type.Artifact | Type.Creature;
            Assert.AreEqual(Type.Artifact | Type.Creature, card.Type);
        }

        [Test]
        public void Test_Card_has_subtypes()
        {
            Card card = CreateCard(m_playerA);
            Assert.Collections.IsEmpty(card.SubTypes.ToList());
            card.SubTypes |= SubType.Aura;
            Assert.Collections.AreEqual(new[] { SubType.Aura }, card.SubTypes.ToList());
        }

        [Test]
        public void Test_Can_get_set_Cards_color()
        {
            Card card = CreateCard(m_playerA);
            Assert.AreEqual(Color.None, card.Color);
            card.Color = Color.White | Color.Red;
            Assert.AreEqual(Color.White | Color.Red, card.Color);
        }

        [Test]
        public void Test_Can_set_whether_a_card_is_tapped_or_not()
        {
            Assert.IsFalse(m_card.Tapped);
            m_card.Tapped = true;
            Assert.IsTrue(m_card.Tapped);
        }

        [Test]
        public void Test_Can_change_the_cards_zone()
        {
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.AreEqual(m_game.Zones.Battlefield, m_card.Zone);
        }

        [Test]
        public void Test_Cannot_set_a_null_zone()
        {
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.Throws<ArgumentNullException>(delegate { m_card.Zone = null; });
        }

        [Test]
        public void Test_Can_access_all_abilities_of_the_card()
        {
            m_game.Abilities.Clear();

            Ability ability = m_game.CreateAbility<PlayCardAbility>(m_card);
            Assert.Collections.AreEquivalent(new [] { ability }, m_card.Abilities);

            m_game.Abilities.Remove(ability);
            Assert.Collections.IsEmpty(m_card.Abilities);
        }

        [Test]
        public void Test_Can_change_Power_and_toughness_on_cards()
        {
            Assert.AreEqual(0, m_card.Power);
            Assert.AreEqual(0, m_card.Toughness);

            m_card.Power = 10;
            Assert.AreEqual(10, m_card.Power);

            m_card.Toughness = 5;
            Assert.AreEqual(5, m_card.Toughness);
        }

        [Test]
        public void Test_Can_change_Damage_on_cards()
        {
            Assert.AreEqual(0, m_card.Damage);
            m_card.Damage = 10;
            Assert.AreEqual(10, m_card.Damage);
        }

        [Test]
        public void Test_Card_is_not_attached_by_default()
        {
            Assert.IsNull(m_card.AttachedTo);
        }

        [Test]
        public void Test_Card_ceases_to_be_attached_if_it_quits_the_battlefield()
        {
            m_card.SubTypes |= SubType.Aura;

            Card other = CreateCard(m_playerA);
            other.Type = Type.Creature;
            other.Zone = m_card.Zone = m_game.Zones.Battlefield;

            m_card.Attach(other);
            Assert.AreEqual(other, m_card.AttachedTo);

            m_card.Zone = m_game.Zones.Exile;
            Assert.IsNull(m_card.AttachedTo);
        }

        [Test]
        public void Test_Cards_the_same_sub_type_are_considered_equivalent()
        {
            Card card1 = CreateCard(m_playerA);
            card1.SubTypes = new SubTypes(SubType.Angel, SubType.Swamp);

            Card card2 = CreateCard(m_playerA);
            card2.SubTypes = new SubTypes(SubType.Angel, SubType.Swamp);

            Assert.That(card1.IsEquivalentTo(card2));
            Assert.That(card2.IsEquivalentTo(card1));

            Card card3 = CreateCard(m_playerA);
            card3.SubTypes = new SubTypes(SubType.Swamp, SubType.Angel);

            Assert.That(card1.IsEquivalentTo(card3));
            Assert.That(card3.IsEquivalentTo(card1));
        }

        #endregion

        #region Summoning Sickness

        [Test]
        public void Test_Cards_dont_have_summoning_sickness_by_default()
        {
            m_card.Type = Type.Creature;

            Assert.IsFalse(m_card.HasSummoningSickness);
        }

        [Test]
        public void Test_Can_set_and_remove_summoning_sickness()
        {
            m_card.Type = Type.Creature;

            m_card.HasSummoningSickness = true;
            Assert.IsTrue(m_card.HasSummoningSickness);
            m_card.HasSummoningSickness = false;
            Assert.IsFalse(m_card.HasSummoningSickness);
        }

        [Test]
        public void Test_Cards_with_haste_dont_have_summoning_sickness()
        {
            m_card.Type = Type.Creature;
            m_card.HasSummoningSickness = true;
            m_game.CreateAbility<HasteAbility>(m_card);
            Assert.IsFalse(m_card.HasSummoningSickness);
        }

        [Test]
        public void Test_Non_creatures_never_have_sickness()
        {
            m_card.HasSummoningSickness = true;

            m_card.Type = Type.Artifact;
            Assert.IsFalse(m_card.HasSummoningSickness);

            m_card.Type = Type.Creature; // Can put it back and get sickness back
            Assert.IsTrue(m_card.HasSummoningSickness);
        }

        [Test]
        public void Test_A_card_that_comes_into_play_has_sickness()
        {
            m_card.Type = Type.Creature;
            Assert.IsFalse(m_card.HasSummoningSickness);

            m_card.Zone = m_game.Zones.Battlefield;

            Assert.IsTrue(m_card.HasSummoningSickness);
        }

        [Test]
        public void Test_HasSummoningSickness_always_returns_false_when_bypassing()
        {
            m_card.Type = Type.Creature;
            m_card.HasSummoningSickness = true;

            using (SummoningSickness.Bypass())
            {
                Assert.IsFalse(m_card.HasSummoningSickness);
            }
        }

        #endregion
    }
}

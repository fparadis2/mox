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

using Mox.Abilities;

namespace Mox
{
    [TestFixture]
    public class CardExtensionsTests : BaseGameTests
    {
        #region Variables

        private new Card m_card;

        #endregion

        #region Setup

        public override void Setup()
        {
            base.Setup();

            m_card = CreateCard(m_playerA);
        }

        #endregion

        #region Utilities

        private void Test_TypeMethod(Type type, Func<Card, bool> test, bool expectedResult)
        {
            m_card.Type = type;
            Assert.AreEqual(expectedResult, test(m_card));
        }

        private enum VisibleTo
        {
            Nobody,
            Owner,
            Everybody,
        }

        private void Test_Visibility(Zone zone, VisibleTo expectedResults)
        {
            m_card.Zone = zone;

            bool ownerVisible = (expectedResults != VisibleTo.Nobody);
            bool opponentVisible = (expectedResults == VisibleTo.Everybody);

            Assert.AreEqual(ownerVisible, m_card.IsVisible(m_playerA));
            Assert.AreEqual(opponentVisible, m_card.IsVisible(m_playerB));
            Assert.AreEqual(opponentVisible, m_card.IsVisible(null));
        }

        #endregion

        #region Tests

        #region Is

        #region Super Type

        [Test]
        public void Test_Is_SuperType_returns_true_if_the_card_is_all_of_the_specified_super_types()
        {
            Assert.IsFalse(m_card.Is(SuperType.Legendary));

            m_card.SuperType = SuperType.Legendary;
            Assert.IsTrue(m_card.Is(SuperType.Legendary));
            Assert.IsFalse(m_card.Is(SuperType.Legendary | SuperType.World));

            m_card.SuperType = SuperType.Legendary | SuperType.Snow;
            Assert.IsTrue(m_card.Is(SuperType.Legendary));
            Assert.IsTrue(m_card.Is(SuperType.Legendary | SuperType.Snow));
            Assert.IsFalse(m_card.Is(SuperType.Legendary | SuperType.World));
        }

        [Test]
        public void Test_IsAny_SuperType_returns_true_if_the_card_is_of_any_of_the_specified_super_type()
        {
            Assert.IsFalse(m_card.IsAny(SuperType.Legendary));

            m_card.SuperType = SuperType.Legendary;
            Assert.IsTrue(m_card.IsAny(SuperType.Legendary));
            Assert.IsTrue(m_card.IsAny(SuperType.Legendary | SuperType.World));

            m_card.SuperType = SuperType.Legendary | SuperType.Snow;
            Assert.IsTrue(m_card.IsAny(SuperType.Legendary));
            Assert.IsTrue(m_card.IsAny(SuperType.Legendary | SuperType.Snow));
            Assert.IsTrue(m_card.IsAny(SuperType.Legendary | SuperType.World));
            Assert.IsFalse(m_card.IsAny(SuperType.Basic | SuperType.World));
        }

        [Test]
        public void Test_IsExactly_SuperType_returns_true_if_the_card_is_exactly_of_the_specified_super_type()
        {
            Assert.IsFalse(m_card.IsExactly(SuperType.Legendary));

            m_card.SuperType = SuperType.Legendary;
            Assert.IsTrue(m_card.IsExactly(SuperType.Legendary));
            Assert.IsFalse(m_card.IsExactly(SuperType.Legendary | SuperType.World));

            m_card.SuperType = SuperType.Legendary | SuperType.Snow;
            Assert.IsFalse(m_card.IsExactly(SuperType.Legendary));
            Assert.IsFalse(m_card.IsExactly(SuperType.Legendary | SuperType.World));
            Assert.IsTrue(m_card.IsExactly(SuperType.Legendary | SuperType.Snow));
        }

        #endregion

        #region Type

        [Test]
        public void Test_Is_Type_returns_true_if_the_card_is_all_of_the_specified_type()
        {
            Assert.IsFalse(m_card.Is(Type.Creature));

            m_card.Type = Type.Creature;
            Assert.IsTrue(m_card.Is(Type.Creature));
            Assert.IsFalse(m_card.Is(Type.Creature | Type.Land));

            m_card.Type = Type.Creature | Type.Artifact;
            Assert.IsTrue(m_card.Is(Type.Creature));
            Assert.IsTrue(m_card.Is(Type.Creature | Type.Artifact));
            Assert.IsFalse(m_card.Is(Type.Creature | Type.Land));
        }

        [Test]
        public void Test_IsAny_Type_returns_true_if_the_card_is_of_any_of_the_specified_type()
        {
            Assert.IsFalse(m_card.IsAny(Type.Creature));

            m_card.Type = Type.Creature;
            Assert.IsTrue(m_card.IsAny(Type.Creature));
            Assert.IsTrue(m_card.IsAny(Type.Creature | Type.Land));

            m_card.Type = Type.Creature | Type.Artifact;
            Assert.IsTrue(m_card.IsAny(Type.Creature));
            Assert.IsTrue(m_card.IsAny(Type.Creature | Type.Artifact));
            Assert.IsTrue(m_card.IsAny(Type.Creature | Type.Land));
            Assert.IsFalse(m_card.IsAny(Type.Planeswalker | Type.Sorcery));
        }

        [Test]
        public void Test_IsExactly_Type_returns_true_if_the_card_is_exactly_of_the_specified_type()
        {
            Assert.IsFalse(m_card.IsExactly(Type.Creature));
            
            m_card.Type = Type.Creature;
            Assert.IsTrue(m_card.IsExactly(Type.Creature));
            Assert.IsFalse(m_card.IsExactly(Type.Creature | Type.Land));

            m_card.Type = Type.Creature | Type.Artifact;
            Assert.IsFalse(m_card.IsExactly(Type.Creature));
            Assert.IsFalse(m_card.IsExactly(Type.Creature | Type.Land));
            Assert.IsTrue(m_card.IsExactly(Type.Creature | Type.Artifact));
        }

        #endregion

        #region SubType

        [Test]
        public void Test_IsAll_SubType_returns_true_if_the_card_is_all_of_the_specified_subtypes()
        {
            Assert.IsFalse(m_card.IsAll(SubType.Mutant));

            m_card.SubTypes = new SubTypes(SubType.Mutant);
            Assert.IsTrue(m_card.IsAll(SubType.Mutant));
            Assert.IsFalse(m_card.IsAll(SubType.Monk, SubType.Mutant));

            m_card.SubTypes = new SubTypes(SubType.Monk, SubType.Mutant);
            Assert.IsTrue(m_card.IsAll(SubType.Mutant));
            Assert.IsTrue(m_card.IsAll(SubType.Monk, SubType.Mutant));
            Assert.IsFalse(m_card.IsAll(SubType.Minotaur, SubType.Mutant));
        }

        [Test]
        public void Test_IsAny_SubType_returns_true_if_the_card_is_of_any_of_the_specified_subtypes()
        {
            Assert.IsFalse(m_card.IsAny(SubType.Mutant));

            m_card.SubTypes = new SubTypes(SubType.Mutant);
            Assert.IsTrue(m_card.IsAny(SubType.Mutant));
            Assert.IsTrue(m_card.IsAny(SubType.Monk, SubType.Mutant));

            m_card.SubTypes = new SubTypes(SubType.Monk, SubType.Mutant);
            Assert.IsTrue(m_card.IsAny(SubType.Mutant));
            Assert.IsTrue(m_card.IsAny(SubType.Monk, SubType.Mutant));
            Assert.IsTrue(m_card.IsAny(SubType.Minotaur, SubType.Mutant));
            Assert.IsFalse(m_card.IsAny(SubType.Minotaur, SubType.Merfolk));
        }

        #endregion

        #region Color

        [Test]
        public void Test_Is_Color_returns_true_if_the_card_is_at_least_one_of_the_specified_color()
        {
            Assert.IsFalse(m_card.Is(Color.Red));

            m_card.Color = Color.Red;
            Assert.IsTrue(m_card.Is(Color.Red));
            Assert.IsFalse(m_card.Is(Color.Red | Color.Blue));

            m_card.Color = Color.Red | Color.Blue;
            Assert.IsTrue(m_card.Is(Color.Red));
            Assert.IsTrue(m_card.Is(Color.Red | Color.Blue));
            Assert.IsFalse(m_card.Is(Color.Red | Color.Green));
        }

        [Test]
        public void Test_IsAny_Color_returns_true_if_the_card_is_of_any_of_the_specified_color()
        {
            Assert.IsFalse(m_card.IsAny(Color.Red));

            m_card.Color = Color.Red;
            Assert.IsTrue(m_card.IsAny(Color.Red));
            Assert.IsTrue(m_card.IsAny(Color.Red | Color.Blue));

            m_card.Color = Color.Red | Color.Blue;
            Assert.IsTrue(m_card.IsAny(Color.Red));
            Assert.IsTrue(m_card.IsAny(Color.Red | Color.Blue));
            Assert.IsTrue(m_card.IsAny(Color.Red | Color.Green));
            Assert.IsFalse(m_card.IsAny(Color.Black | Color.White));
        }

        [Test]
        public void Test_IsExactly_returns_true_if_the_card_is_exactly_of_the_specified_color()
        {
            Assert.IsFalse(m_card.IsExactly(Color.Red));

            m_card.Color = Color.Red;
            Assert.IsTrue(m_card.IsExactly(Color.Red));
            Assert.IsFalse(m_card.IsExactly(Color.Red | Color.Blue));

            m_card.Color = Color.Red | Color.Blue;
            Assert.IsFalse(m_card.IsExactly(Color.Red));
            Assert.IsFalse(m_card.IsExactly(Color.Red | Color.Black));
            Assert.IsTrue(m_card.IsExactly(Color.Red | Color.Blue));
        }

        #endregion

        #region Misc

        [Test]
        public void Test_IsPermanent_returns_true_for_permanent_cards()
        {
            Func<Card, bool> test = card => card.IsPermanent();

            Test_TypeMethod(Type.Artifact, test, true);
            Test_TypeMethod(Type.Creature, test, true);
            Test_TypeMethod(Type.Enchantment, test, true);
            Test_TypeMethod(Type.Instant, test, false);
            Test_TypeMethod(Type.Land, test, true);
            Test_TypeMethod(Type.None, test, false);
            Test_TypeMethod(Type.Planeswalker, test, true);
            Test_TypeMethod(Type.Sorcery, test, false);
            Test_TypeMethod(Type.Tribal, test, false);
        }

        [Test]
        public void Test_IsVisible_returns_true_when_in_visible_zones()
        {
            Test_Visibility(m_game.Zones.Battlefield, VisibleTo.Everybody);
            Test_Visibility(m_game.Zones.Exile, VisibleTo.Everybody);
            Test_Visibility(m_game.Zones.Graveyard, VisibleTo.Everybody);
            Test_Visibility(m_game.Zones.Hand, VisibleTo.Owner);
            Test_Visibility(m_game.Zones.Library, VisibleTo.Nobody);
            Test_Visibility(m_game.Zones.PhasedOut, VisibleTo.Everybody);
            Test_Visibility(m_game.Zones.Stack, VisibleTo.Everybody);
        }

        #endregion

        #endregion

        #region Actions

        #region Attach

        private Card CreateAura(Player owner)
        {
            Card card = CreateCard(owner);
            card.SubTypes |= SubType.Aura;
            card.Zone = m_game.Zones.Battlefield;
            return card;
        }

        private Card CreateCreature(Player owner)
        {
            Card card = CreateCard(owner);
            card.Type = Type.Creature;
            card.Zone = m_game.Zones.Battlefield;
            return card;
        }

        [Test]
        public void Test_Attach_attaches_the_card_to_the_target()
        {
            Card target = CreateCreature(m_playerA);
            Card attached = CreateAura(m_playerB);

            attached.Attach(target);

            Assert.AreEqual(target, attached.AttachedTo);
        }

        [Test]
        public void Test_Attaching_null_means_detach()
        {
            Card target = CreateCreature(m_playerA);
            Card attached = CreateAura(m_playerB);

            attached.Attach(target);
            attached.Attach(null);

            Assert.AreEqual(null, attached.AttachedTo);
        }

        #endregion

        #region Destroy

        [Test]
        public void Test_ToDestroy_is_to_send_to_the_graveyard()
        {
            m_card.Type = Type.Artifact;
            m_card.Zone = m_game.Zones.Battlefield;
            m_card.Damage = 10;

            m_card.Destroy();

            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
            Assert.AreEqual(0, m_card.Damage);
        }

        [Test]
        public void Test_Cannot_destroy_a_non_permanent()
        {
            m_card.Type = Type.Instant;
            Assert.Throws<ArgumentException>(() => m_card.Destroy());
        }

        #endregion

        #region ReturnToHand

        [Test]
        public void Test_ReturnToHand_returns_the_card_to_its_owners_hand()
        {
            m_card.Zone = m_game.Zones.Library;
            m_card.Controller = m_playerB;

            m_card.ReturnToHand();
            m_card.ReturnToHand(); // Does nothing the other times..

            Assert.AreEqual(m_game.Zones.Hand, m_card.Zone);
            Assert.AreEqual(m_playerA, m_card.Controller);
        }

        #endregion

        #region Sacrifice

        [Test]
        public void Test_Sacrifice_moves_the_card_to_the_graveyard()
        {
            m_card.Zone = m_game.Zones.Battlefield;
            m_card.Sacrifice();
            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
        }

        #endregion

        #region Tap / Untap

        [Test]
        public void Test_Tap_taps_a_card()
        {
            m_card.Type = Type.Artifact;
            m_card.Zone = m_game.Zones.Battlefield;
            m_card.Tapped = false;

            m_card.Tap();
            m_card.Tap(); // Can do it twice

            Assert.IsTrue(m_card.Tapped);
        }

        [Test]
        public void Test_Untap_untaps_a_card()
        {
            m_card.Type = Type.Artifact;
            m_card.Zone = m_game.Zones.Battlefield;
            m_card.Tapped = true;

            m_card.Untap();
            m_card.Untap(); // Can do it twice

            Assert.IsFalse(m_card.Tapped);
        }

        [Test]
        public void Test_Cannot_tap_or_untap_a_card_that_is_not_on_the_battlefield()
        {
            m_card.Zone = m_game.Zones.Library;

            Assert.Throws<ArgumentException>(() => m_card.Tap());
            Assert.Throws<ArgumentException>(() => m_card.Untap());
        }

        #endregion

        #endregion

        #region Abilities

        [Test]
        public void Test_HasAbility_returns_true_if_the_card_has_at_least_one_ability_of_the_given_type()
        {
            Assert.IsFalse(m_card.HasAbility<FlyingAbility>());

            m_game.CreateAbility<FlyingAbility>(m_card);
            Assert.IsTrue(m_card.HasAbility<FlyingAbility>());

            m_game.CreateAbility<FlyingAbility>(m_card);
            Assert.IsTrue(m_card.HasAbility<FlyingAbility>());
        }

        #endregion

        #endregion
    }
}

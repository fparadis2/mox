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

namespace Mox.AI.Resolvers
{
    [TestFixture]
    public class PayManaResolverTests : BaseMTGChoiceResolverTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_card.Zone = m_game.Zones.Battlefield;
        }

        internal override BaseMTGChoiceResolver CreateResolver()
        {
            return new PayManaResolver();
        }

        #endregion

        #region Utilities

        private List<object> GetChoices(ManaCost manaCost)
        {
            List<object> choices = new List<object>();
            m_mockery.Test(() => choices.AddRange(m_choiceResolver.ResolveChoices(GetMethod(), new object[] { m_context, m_playerA, manaCost })));
            return choices;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Always_possible_to_pass()
        {
            m_game.Abilities.Clear();

            Assert.That(GetChoices(ManaCost.Empty).Contains(null));
        }

        [Test]
        public void Test_Default_choice_is_to_pass()
        {
            Assert.IsNull(m_choiceResolver.GetDefaultChoice(GetMethod(), null));
        }

        [Test]
        public void Test_Every_Mana_playable_ability_is_included_as_a_choice()
        {
            m_mockAbility.MockedIsManaAbility = true;
            m_mockAbility.Expect_CanPlay();

            Card otherCard = CreateCard(m_playerA, "Other Card");
            CreateMockAbility(otherCard, AbilityType.Normal);

            List<object> choices = GetChoices(ManaCost.Empty);
            Assert.AreEqual(2, choices.Count);

            Assert.That(choices.Contains(null));

            PlayAbility playAbility = (PlayAbility)choices.First(choice => choice is PlayAbility);
            Assert.IsNotNull(playAbility);
            Assert.AreEqual(m_mockAbility, playAbility.Ability.Resolve(m_game));
        }

        [Test]
        public void Test_If_Mana_is_not_available_no_payment_is_returned()
        {
            m_game.Abilities.Clear();

            ManaCost manaCost = new ManaCost(0, ManaSymbol.R);

            Assert.IsFalse(GetChoices(manaCost).Any(choice => choice is PayManaAction));
        }

        [Test]
        public void Test_If_Mana_is_available_a_payment_is_returned_and_no_other_actions_are_returned()
        {
            ManaCost manaCost = new ManaCost(0, ManaSymbol.R);
            m_playerA.ManaPool[Color.Red] = 1;

            List<object> choices = GetChoices(manaCost);
            Assert.AreEqual(1, choices.Count);

            PayManaAction payMana = (PayManaAction)choices.First(choice => choice is PayManaAction);
            Assert.IsNotNull(payMana);
            Assert.Collections.AreEqual(new[] { Color.Red }, payMana.Payment.Payments);
        }

        [Test]
        public void Test_If_Mana_is_available_a_payment_is_returned_with_colorless()
        {
            ManaCost manaCost = new ManaCost(1);
            m_playerA.ManaPool[Color.Red] = 1;

            PayManaAction payMana = (PayManaAction)GetChoices(manaCost).First(choice => choice is PayManaAction);
            Assert.IsNotNull(payMana);
            Assert.Collections.AreEqual(new[] { Color.Red }, payMana.Payment.Payments);
        }

        [Test]
        public void Test_Will_not_try_mana_abilities_that_cannot_help_with_cost()
        {
            MockAbility manaAbility1 = CreateMockAbility(m_card, AbilityType.Normal);
            manaAbility1.MockedIsManaAbility = true;
            manaAbility1.MockProperty = 1;
            manaAbility1.Expect_CanPlay();

            MockAbility manaAbility2 = CreateMockAbility(m_card, AbilityType.Normal);
            manaAbility2.MockedManaOutcome = ManaAbilityOutcome.OfColor(Color.White);
            manaAbility2.MockProperty = 2;
            manaAbility2.Expect_CanPlay();

            MockAbility manaAbility3 = CreateMockAbility(m_card, AbilityType.Normal);
            manaAbility3.MockedManaOutcome = ManaAbilityOutcome.OfColor(Color.Red);
            manaAbility3.MockProperty = 3;
            manaAbility3.Expect_CanPlay();

            MockAbility manaAbility4 = CreateMockAbility(m_card, AbilityType.Normal);
            manaAbility4.MockedManaOutcome = ManaAbilityOutcome.OfColor(Color.None); // Cannot be used to pay cost
            manaAbility4.MockProperty = 4;
            manaAbility4.Expect_CanPlay();

            ManaCost manaCost = ManaCost.Parse("RW");

            List<object> choices = GetChoices(manaCost);
            Assert.AreEqual(4, choices.Count);

            Assert.That(choices.Contains(null));
            Assert.That(choices.OfType<PlayAbility>().Any(pa => pa.Ability.Resolve(m_game) == manaAbility1));
            Assert.That(choices.OfType<PlayAbility>().Any(pa => pa.Ability.Resolve(m_game) == manaAbility2));
            Assert.That(choices.OfType<PlayAbility>().Any(pa => pa.Ability.Resolve(m_game) == manaAbility3));
        }

        [Test]
        public void Test_Will_not_try_mana_abilities_that_cannot_help_with_cost_with_mana_already_in_pool()
        {
            MockAbility manaAbility1 = CreateMockAbility(m_card, AbilityType.Normal);
            manaAbility1.MockedIsManaAbility = true;
            manaAbility1.MockProperty = 1;
            manaAbility1.Expect_CanPlay();

            MockAbility manaAbility2 = CreateMockAbility(m_card, AbilityType.Normal);
            manaAbility2.MockedManaOutcome = ManaAbilityOutcome.OfColor(Color.White);
            manaAbility2.MockProperty = 2;
            manaAbility2.Expect_CanPlay();

            MockAbility manaAbility3 = CreateMockAbility(m_card, AbilityType.Normal);
            manaAbility3.MockedManaOutcome = ManaAbilityOutcome.OfColor(Color.Red); // Cannot be used to pay remaining cost
            manaAbility3.MockProperty = 3;
            manaAbility3.Expect_CanPlay();

            MockAbility manaAbility4 = CreateMockAbility(m_card, AbilityType.Normal);
            manaAbility4.MockedManaOutcome = ManaAbilityOutcome.OfColor(Color.None); // Cannot be used to pay remaining cost
            manaAbility4.MockProperty = 4;
            manaAbility4.Expect_CanPlay();

            ManaCost manaCost = ManaCost.Parse("1RW");

            m_playerA.ManaPool[Color.None] = 1;
            m_playerA.ManaPool[Color.Red] = 1;

            List<object> choices = GetChoices(manaCost);
            Assert.AreEqual(3, choices.Count);

            Assert.That(choices.Contains(null));
            Assert.That(choices.OfType<PlayAbility>().Any(pa => pa.Ability.Resolve(m_game) == manaAbility1));
            Assert.That(choices.OfType<PlayAbility>().Any(pa => pa.Ability.Resolve(m_game) == manaAbility2));
        }

        #endregion
    }
}

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
using Rhino.Mocks;

namespace Mox.AI.Resolvers
{
    [TestFixture]
    public class GivePriorityResolverTests : BaseMTGChoiceResolverTests
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
            return new GivePriorityResolver();
        }

        #endregion

        #region Utilities

        private List<object> GetChoices()
        {
            return GetChoices(m_playerA);
        }

        private List<object> GetChoices(Player player)
        {
            List<object> choices = null;
            m_mockery.Test(() =>
            {
                choices = GetChoicesWithoutMock(player);
            });
            return choices;
        }

        private List<object> GetChoicesWithoutMock(Player player)
        {
            return new List<object>(m_choiceResolver.ResolveChoices(GetMethod(), new object[] { m_context, player }));
        }

        private void Assert_Choices_contain_ability(IEnumerable<object> choices, Ability ability)
        {
            Assert.IsNotNull(choices.OfType<PlayAbility>().Single(pa => pa.Ability.Resolve(m_game) == ability), "Ability {0} was not in choices", ability);
        }

        private void Assert_Choices_dont_contain_ability(IEnumerable<object> choices, Ability ability)
        {
            Assert.IsNull(choices.OfType<PlayAbility>().FirstOrDefault(pa => pa.Ability.Resolve(m_game) == ability), "Ability {0} was in choices", ability);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Always_possible_to_pass()
        {
            m_game.Abilities.Clear();
            Assert.That(GetChoices().Contains(null));
        }

        [Test]
        public void Test_Default_choice_is_to_pass()
        {
            m_game.Abilities.Clear();
            Assert.IsNull(m_choiceResolver.GetDefaultChoice(GetMethod(), null));
        }

        [Test]
        public void Test_Every_playable_ability_is_included_as_a_choice()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Normal;
            m_mockAbility.Expect_CanPlay();

            List<object> choices = GetChoices();
            PlayAbility playAbility = (PlayAbility)choices.First(choice => choice is PlayAbility);
            Assert.IsNotNull(playAbility);

            // Passing is still valid
            Assert.That(choices.Contains(null));
        }

        [Test]
        public void Test_Non_playable_ability_are_not_included_as_a_choice()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Normal;
            m_mockAbility.Expect_CannotPlay();

            Assert.IsFalse(GetChoices().Any(choice => choice is PlayAbility));
        }

        [Test]
        public void Test_Mana_abilities_are_not_considered()
        {
            m_mockAbility.MockedIsManaAbility = true;

            Assert.IsFalse(GetChoices().Any(choice => choice is PlayAbility));
        }

        [Test]
        public void Test_Stop_playing_spells_when_the_stack_is_not_empty()
        {
            Card newCard = CreateCard(m_playerB);
            newCard.Zone = m_game.Zones.Battlefield;

            MockAbility abilityB = CreateMockAbility(newCard, AbilityType.Normal);

            foreach (MockAbility ability in new[] { m_mockAbility, abilityB })
            {
                ability.Expect_CanPlay().Repeat.Any();
                ability.Implementation.Replay();
            }

            m_parameters.MaximumSpellStackDepth = 2;

            m_game.SpellStack.Push(new Spell(m_game, m_mockAbility, m_playerA));

            Assert.IsTrue(GetChoicesWithoutMock(m_playerB).Any(choice => choice is PlayAbility));

            m_game.SpellStack.Push(new Spell(m_game, m_mockAbility, m_playerB));

            Assert.IsFalse(GetChoicesWithoutMock(m_playerA).Any(choice => choice is PlayAbility));
        }

        [Test]
        public void Test_Stop_playing_spells_when_we_control_the_top_spell_on_the_stack()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Normal;
            m_mockAbility.Expect_CanPlay().Repeat.Any();
            m_mockAbility.Implementation.Replay();

            m_parameters.MaximumSpellStackDepth = 10;

            m_game.SpellStack.Push(new Spell(m_game, m_mockAbility, m_playerA));

            Assert.IsFalse(GetChoicesWithoutMock(m_playerA).Any(choice => choice is PlayAbility));
        }

        #region Optimizations

        #region Only try distinct cards/abilities

        private MockAbility CreateCardWithAbility(string cardName, Zone zone)
        {
            Card card = CreateCard(m_playerA, cardName);
            card.Zone = zone;
            return CreateMockAbility(card, AbilityType.Normal);
        }

        [Test]
        public void Test_Duplicate_cards_in_hand_are_only_returned_once()
        {
            m_game.Cards.Clear();

            MockAbility ability = CreateCardWithAbility("MyCard", m_game.Zones.Hand);
            MockAbility identicalAbility = CreateCardWithAbility("MyCard", m_game.Zones.Hand);
            MockAbility otherAbility = CreateCardWithAbility("MyOtherCard", m_game.Zones.Hand);

            ability.Expect_CanPlay();
            identicalAbility.Expect_CanPlay();
            otherAbility.Expect_CanPlay();

            var choices = GetChoices();
            Assert_Choices_contain_ability(choices, ability);
            Assert_Choices_dont_contain_ability(choices, identicalAbility);
            Assert_Choices_contain_ability(choices, otherAbility);

            // Passing is still valid
            Assert.That(choices.Contains(null));
        }

        [Test]
        public void Test_Duplicate_abilities_in_battlefield_are_only_returned_once_if_cards_and_abilities_are_equal()
        {
            m_game.Cards.Clear();

            MockAbility ability = CreateCardWithAbility("MyCard", m_game.Zones.Battlefield);
            MockAbility identicalAbility = CreateCardWithAbility("MyCard", m_game.Zones.Battlefield);
            MockAbility abilityWithDifferentProperties = CreateCardWithAbility("MyCard", m_game.Zones.Battlefield);
            MockAbility abilityWithDifferentCard = CreateCardWithAbility("MyCard", m_game.Zones.Battlefield);

            abilityWithDifferentProperties.MockProperty = 3;
            abilityWithDifferentCard.Source.Power = 10;

            ability.Expect_CanPlay();
            identicalAbility.Expect_CanPlay();
            abilityWithDifferentProperties.Expect_CanPlay();
            abilityWithDifferentCard.Expect_CanPlay();

            var choices = GetChoices();
            Assert_Choices_contain_ability(choices, ability);
            Assert_Choices_dont_contain_ability(choices, identicalAbility);
            Assert_Choices_contain_ability(choices, abilityWithDifferentProperties);
            Assert_Choices_contain_ability(choices, abilityWithDifferentCard);

            // Passing is still valid
            Assert.That(choices.Contains(null));
        }

        #endregion

        #region Dont try when stack is empty and everyone pass

        private static void Assert_IsPassing(ICollection<object> choices)
        {
            Assert.AreEqual(1, choices.Count);
            Assert.IsNull(choices.First());
        }

        private static void Assert_IsNotPassing(ICollection<object> choices)
        {
            Assert.Greater(choices.Count, 1);
        }

        [Test]
        public void Test_GivePriority_will_always_return_null_when_the_stack_is_not_empty_and_all_the_players_passed_in_succession()
        {
            m_mockAbility.MockedAbilityType = AbilityType.Normal;
            m_mockery.Test(() => m_game.SpellStack.Push(new Spell(m_game, m_mockAbility, m_playerA)));

            m_game.Abilities.Clear();
            m_parameters.MaximumSpellStackDepth = 0;

            Assert_IsPassing(GetChoices(m_playerA));
            Assert_IsPassing(GetChoices(m_playerB));

            m_parameters.MaximumSpellStackDepth = 10;                               // Whatever this number, the AI will pass afterwards until the stack becomes empty
            MockAbility newAbility = CreateMockAbility(m_card, AbilityType.Normal); // Even if there's an available ability

            Assert_IsPassing(GetChoices(m_playerA));
            Assert_IsPassing(GetChoices(m_playerB));

            m_game.SpellStack.Pop();

            newAbility.Expect_CanPlay();
            Assert_IsNotPassing(GetChoices(m_playerA));
        }

        #endregion

        #endregion

        #endregion
    }
}

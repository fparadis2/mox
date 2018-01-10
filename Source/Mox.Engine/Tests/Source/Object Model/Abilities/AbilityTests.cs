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

using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Mox.Abilities
{
    [TestFixture]
    public class AbilityTests : BaseGameTests
    {
        #region Variables

        private MockAbility m_ability;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_ability = m_game.CreateAbility<MockAbility>(m_card);
        }

        #endregion

        #region Utilities

        private bool CanPlay()
        {
            var context = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal);
            return m_ability.CanPlay(context);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Cannot_create_an_ability_with_a_null_source()
        {
            Assert.Throws<ArgumentNullException>(() => m_game.CreateAbility<MockAbility>(null));
        }

        [Test]
        public void Test_Can_associate_an_ability_with_a_source()
        {
            Assert.AreEqual(m_card, m_ability.Source);
            Assert.AreEqual(m_card.Controller, m_ability.Controller);
        }

        [Test]
        public void Test_Abilities_are_normal_by_default()
        {
            Assert.AreEqual(AbilityType.Normal, m_ability.AbilityType);
        }

        [Test]
        public void Test_Abilities_are_instant_speed_by_default()
        {
            Assert.AreEqual(AbilitySpeed.Instant, m_ability.AbilitySpeed);
        }

        [Test]
        public void Test_Abilities_can_only_played_by_the_controller_of_their_source()
        {
            Assert.IsTrue(m_ability.CanPlay(new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal)));
            Assert.IsFalse(m_ability.CanPlay(new AbilityEvaluationContext(m_playerB, AbilityEvaluationContextType.Normal)));
        }

        [Test]
        public void Test_During_mana_payment_only_mana_abilities_can_be_played()
        {
            AbilityEvaluationContext normalContext = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal);
            AbilityEvaluationContext manaContext = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.ManaPayment);

            m_ability.MockedAbilityType = AbilityType.Normal;
            m_ability.MockedIsManaAbility = true;

            Assert.IsTrue(m_ability.CanPlay(manaContext));
            Assert.IsTrue(m_ability.CanPlay(normalContext));

            m_ability.MockedAbilityType = AbilityType.Normal;
            m_ability.MockedIsManaAbility = false;

            Assert.IsFalse(m_ability.CanPlay(manaContext));
            Assert.IsTrue(m_ability.CanPlay(normalContext));
        }

        [Test]
        public void Test_Can_only_play_at_instant_speed_when_the_stack_is_not_empty()
        {
            m_game.SpellStack.Push(new Spell(m_mockAbility, m_playerA));

            m_ability.MockedAbilitySpeed = AbilitySpeed.Instant;
            Assert.IsTrue(CanPlay());

            m_ability.MockedAbilitySpeed = AbilitySpeed.Sorcery;
            Assert.IsFalse(CanPlay());
        }

        [Test]
        public void Test_Can_only_play_cards_at_sorcery_speed_on_the_main_phase_of_the_controllers_turn()
        {
            m_ability.MockedAbilitySpeed = AbilitySpeed.Sorcery;

            m_game.State.CurrentPhase = Phases.PrecombatMain;
            m_game.State.ActivePlayer = m_playerA;
            Assert.IsTrue(CanPlay());

            m_game.State.CurrentPhase = Phases.End;
            m_game.State.ActivePlayer = m_playerA;
            Assert.IsFalse(CanPlay());

            m_game.State.CurrentPhase = Phases.PostcombatMain;
            m_game.State.ActivePlayer = m_playerB;
            Assert.IsFalse(CanPlay());
        }

        [Test]
        public void Test_Can_play_at_instant_speed_on_all_phases()
        {
            m_ability.MockedAbilitySpeed = AbilitySpeed.Instant;

            m_game.State.CurrentPhase = Phases.End;
            m_game.State.ActivePlayer = m_playerA;
            Assert.IsTrue(CanPlay());

            m_game.State.CurrentPhase = Phases.End;
            m_game.State.ActivePlayer = m_playerA;
            Assert.IsTrue(CanPlay());

            m_game.State.CurrentPhase = Phases.PostcombatMain;
            m_game.State.ActivePlayer = m_playerB;
            Assert.IsTrue(CanPlay());
        }

        #endregion

        #region Inner Types

        private class MockAbility : Ability
        {
            public AbilityType? MockedAbilityType;
            public override AbilityType AbilityType => MockedAbilityType ?? base.AbilityType;

            public AbilitySpeed? MockedAbilitySpeed;
            public override AbilitySpeed AbilitySpeed => MockedAbilitySpeed ?? base.AbilitySpeed;

            public bool? MockedIsManaAbility;
            public override bool IsManaAbility => MockedIsManaAbility ?? base.IsManaAbility;
        }

        #endregion
    }
}

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

namespace Mox
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

        private IMethodOptions<object> Expect_Play(IEnumerable<Cost> costs)
        {
            return m_mockAbility.Expect_Play(costs);
        }

        private IMethodOptions<bool> Expect_CanExecute(Cost cost, ExecutionEvaluationContext context)
        {
            return Expect.Call(cost.CanExecute(m_game, context));
        }

        private bool CanPlay()
        {
            var context = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal);
            return m_mockAbility.CanPlay(context);
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
            Assert.AreEqual(AbilityType.Normal, m_mockAbility.AbilityType);
        }

        [Test]
        public void Test_Abilities_are_instant_speed_by_default()
        {
            Assert.AreEqual(AbilitySpeed.Instant, m_mockAbility.AbilitySpeed);
        }

        [Test]
        public void Test_Abilities_can_only_played_by_the_controller_of_their_source()
        {
            Expect_Play(null);

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_mockAbility.CanPlay(new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal)));
                Assert.IsFalse(m_mockAbility.CanPlay(new ExecutionEvaluationContext(m_playerB, EvaluationContextType.Normal)));
            });
        }

        [Test]
        public void Test_During_mana_payment_only_mana_abilities_can_be_played()
        {
            ExecutionEvaluationContext normalContext = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal);
            ExecutionEvaluationContext manaContext = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.ManaPayment);

            m_mockAbility.MockedAbilityType = AbilityType.Normal;
            m_mockAbility.MockedIsManaAbility = true;
            Expect_Play(null).Repeat.Twice();

            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_mockAbility.CanPlay(manaContext));
                Assert.IsTrue(m_mockAbility.CanPlay(normalContext));
            });

            m_mockAbility.MockedAbilityType = AbilityType.Normal;
            m_mockAbility.MockedIsManaAbility = false;
            Expect_Play(null);

            m_mockery.Test(delegate
            {
                Assert.IsFalse(m_mockAbility.CanPlay(manaContext));
                Assert.IsTrue(m_mockAbility.CanPlay(normalContext));
            });
        }

        [Test]
        public void Test_Abilities_can_only_be_played_if_costs_can_be_paid()
        {
            Expect_Play(new[] { new MockCost { IsValid = true } });
            m_mockery.Test(() => Assert.IsTrue(CanPlay()));
        }

        [Test]
        public void Test_Abilities_cannot_be_played_if_costs_cannot_be_paid()
        {
            Expect_Play(new[] { new MockCost { IsValid = false } });
            m_mockery.Test(() => Assert.IsFalse(CanPlay()));
        }

        [Test]
        public void Test_Can_only_play_at_instant_speed_when_the_stack_is_not_empty()
        {
            m_game.SpellStack.Push(new Spell(m_mockAbility, m_playerA));

            m_mockAbility.MockedAbilitySpeed = AbilitySpeed.Instant;
            Expect_Play(null);
            m_mockery.Test(() => Assert.IsTrue(CanPlay()));

            m_mockAbility.MockedAbilitySpeed = AbilitySpeed.Sorcery;
            m_mockery.Test(() => Assert.IsFalse(CanPlay()));
        }

        [Test]
        public void Test_Can_only_play_cards_at_sorcery_speed_on_the_main_phase_of_the_controllers_turn()
        {
            m_mockAbility.MockedAbilitySpeed = AbilitySpeed.Sorcery;

            m_game.State.CurrentPhase = Phases.PrecombatMain;
            m_game.State.ActivePlayer = m_playerA;
            Expect_Play(null);
            m_mockery.Test(() => Assert.IsTrue(CanPlay()));

            m_game.State.CurrentPhase = Phases.End;
            m_game.State.ActivePlayer = m_playerA;
            m_mockery.Test(() => Assert.IsFalse(CanPlay()));

            m_game.State.CurrentPhase = Phases.PostcombatMain;
            m_game.State.ActivePlayer = m_playerB;
            m_mockery.Test(() => Assert.IsFalse(CanPlay()));
        }

        [Test]
        public void Test_Can_play_at_instant_speed_on_all_phases()
        {
            m_mockAbility.MockedAbilitySpeed = AbilitySpeed.Instant;

            m_game.State.CurrentPhase = Phases.End;
            m_game.State.ActivePlayer = m_playerA;
            Expect_Play(null);
            m_mockery.Test(() => Assert.IsTrue(CanPlay()));

            m_game.State.CurrentPhase = Phases.End;
            m_game.State.ActivePlayer = m_playerA;
            Expect_Play(null);
            m_mockery.Test(() => Assert.IsTrue(CanPlay()));

            m_game.State.CurrentPhase = Phases.PostcombatMain;
            m_game.State.ActivePlayer = m_playerB;
            Expect_Play(null);
            m_mockery.Test(() => Assert.IsTrue(CanPlay()));
        }

        #endregion
    }
}

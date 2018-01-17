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
using Mox.Flow;
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class PlayCardAbilityTests : BaseGameTests
    {
        #region Inner Types

        private class MockPlayCardAbility : PlayCardAbility
        {
        }

        #endregion

        #region Variables

        private MockPlayCardAbility m_ability;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_card.Zone = m_game.Zones.Hand;
            m_ability = m_game.CreateAbility<MockPlayCardAbility>(m_card, SpellDefinition.Empty);

            m_game.State.CurrentPhase = Phases.PrecombatMain;
            m_game.State.ActivePlayer = m_playerA;
        }

        #endregion

        #region Utilities

        private bool CanPlay()
        {
            var context = new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal);
            return m_ability.CanPlay(context);
        }

        private void PlayAndResolve()
        {
            NewSequencerTester sequencer = new NewSequencerTester(m_mockery, m_game);
            sequencer.Run(new PushAbility(m_ability));
            sequencer.Run(new ResolveAbility(m_ability));
        }

        private void Resolve()
        {
            NewSequencerTester sequencer = new NewSequencerTester(m_mockery, m_game);
            sequencer.Run(new ResolveAbility(m_ability));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_PlayCardAbility_puts_permanent_cards_into_play_when_it_resolves()
        {
            m_card.Type = Type.Creature;
            Resolve();
            Assert.AreEqual(m_game.Zones.Battlefield, m_card.Zone);
        }

        [Test]
        public void Test_PlayCardAbility_puts_non_permanent_cards_into_graveyard_when_it_resolves()
        {
            m_card.Type = Type.Instant;
            Resolve();
            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
        }

        [Test]
        public void Test_PlayCardAbility_uses_the_stack_by_default()
        {
            Assert.IsTrue(m_ability.UseStack);
        }

        [Test]
        public void Test_PlayCardAbility_doesnt_use_the_stack_for_lands()
        {
            m_card.Type = Type.Land;
            Assert.IsFalse(m_ability.UseStack);
        }

        [Test]
        public void Test_Can_play_cards_from_the_hand()
        {
            m_card.Zone = m_game.Zones.Hand;
            Assert.IsTrue(CanPlay());
        }

        [Test]
        public void Test_Cannot_play_cards_from_anywhere_else_than_the_hand()
        {
            m_card.Zone = m_game.Zones.Library;
            Assert.IsFalse(CanPlay());

            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsFalse(CanPlay());
        }

        [Test]
        public void Test_Can_only_play_one_land_per_turn()
        {
            m_card.Type = Type.Land;

            Card secondLand = CreateCard(m_playerA);
            secondLand.Zone = m_game.Zones.Hand;
            secondLand.Type = Type.Land;

            Assert.IsTrue(CanPlay());

            PlayAndResolve();

            m_ability = m_game.CreateAbility<MockPlayCardAbility>(secondLand, SpellDefinition.Empty);
            Assert.IsFalse(CanPlay());
        }

        [Test]
        public void Test_Cards_come_into_play_with_summoning_sickness()
        {
            m_card.Type = Type.Creature;
            PlayAndResolve();
            Assert.IsTrue(m_card.HasSummoningSickness);
        }

        [Test]
        public void Test_PlayCardAbility_is_instant_speed_if_source_is_instant()
        {
            foreach (Type type in Enum.GetValues(typeof(Type)))
            {
                m_card.Type = type;
                Assert.AreEqual(type == Type.Instant ? AbilitySpeed.Instant : AbilitySpeed.Sorcery, m_ability.AbilitySpeed);
            }
        }

        [Test]
        public void Test_PlayCardAbility_is_instant_speed_if_source_has_flash()
        {
            m_card.Type = Type.Sorcery;
            m_game.CreateAbility<FlashAbility>(m_card);
            Assert.AreEqual(AbilitySpeed.Instant, m_ability.AbilitySpeed);
        }

        #endregion

        #region Mock Types

        private class PushAbility : Part
        {
            private readonly Resolvable<SpellAbility> m_ability;

            public PushAbility(SpellAbility ability)
            {
                m_ability = ability;
            }

            public override Part Execute(Context context)
            {
                var ability = m_ability.Resolve(context.Game);
                ability.Push(context, ability.Controller);
                return null;
            }
        }

        private class ResolveAbility : Part
        {
            private readonly Resolvable<SpellAbility> m_ability;

            public ResolveAbility(SpellAbility ability)
            {
                m_ability = ability;
            }

            public override Part Execute(Context context)
            {
                var ability = m_ability.Resolve(context.Game);
                ability.Resolve(context, ability.Controller);
                return null;
            }
        }

        #endregion
    }
}

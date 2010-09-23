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
using Mox.Rules;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox
{
    [TestFixture]
    public class PlayCardAbilityTests : BaseGameTests
    {
        #region Inner Types

        public class PlayCardAbilityImplementation
        {
            public virtual IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                yield break;
            }
        }

        private class MockPlayCardAbility : PlayCardAbility
        {
            internal PlayCardAbilityImplementation Implementation
            {
                get;
                set;
            }

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                return Implementation.PlaySpecific(spell);
            }
        }

        #endregion

        #region Variables

        private MockPlayCardAbility m_ability;
        private Spell m_spell;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_card.Zone = m_game.Zones.Hand;
            m_ability = m_game.CreateAbility<MockPlayCardAbility>(m_card);
            m_ability.Implementation = m_mockery.PartialMock<PlayCardAbilityImplementation>();
            m_spell = new Spell(m_game, m_ability, m_playerA);

            m_mockery.Replay(m_ability.Implementation);

            m_game.State.CurrentPhase = Phases.PrecombatMain;
            m_game.State.ActivePlayer = m_playerA;
        }

        #endregion

        #region Utilities

        protected IEnumerable<ImmediateCost> PlayAndResolveSpell(Spell spell, bool useStack)
        {
            return PlayAndResolveSpell(spell, useStack, null, null, delegate { });
        }

        protected IEnumerable<ImmediateCost> PlayAndResolveSpell(Spell spell, bool useStack, IEnumerable<ImmediateCost> additionalImmediateCosts, IEnumerable<DelayedCost> additionalDelayedCosts, SpellEffect additionalEffect)
        {
            m_mockery.BackToRecord(m_ability.Implementation);
            Expect.Call(m_ability.Implementation.PlaySpecific(null)).IgnoreArguments().Callback(delegate(Spell specificSpell)
            {
                Assert.AreEqual(spell.Ability, specificSpell.Ability);
                Assert.AreEqual(spell.Source, specificSpell.Source);
                Assert.AreEqual(spell.Game, specificSpell.Game);
                Assert.AreEqual(spell.Controller, specificSpell.Controller);

                specificSpell.Effect = additionalEffect;

                if (additionalDelayedCosts != null)
                {
                    additionalDelayedCosts.ForEach(specificSpell.DelayedCosts.Add);
                }

                return true;
            }).Return(additionalImmediateCosts);

            List<ImmediateCost> costs = null;
            m_mockery.Test(delegate 
            {
                costs = new List<ImmediateCost>(spell.Ability.Play(spell));
            });

            Assert.IsNotNull(spell.PreEffect);
            Assert.IsNotNull(spell.Effect);
            
            spell.PreEffect(spell, null);

            if (useStack)
            {
                Assert.AreEqual(m_game.Zones.Stack, spell.Ability.Source.Zone);
            }
            else
            {
                Assert.AreNotEqual(m_game.Zones.Stack, spell.Ability.Source.Zone);
            }

            spell.Effect(spell, null);
            Assert.AreEqual(useStack, m_spell.UseStack);

            return costs;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_PlayCardAbility_puts_permanent_cards_into_play_when_it_resolves()
        {
            m_card.Type = Type.Creature;
            PlayAndResolveSpell(m_spell, true);
            Assert.AreEqual(m_game.Zones.Battlefield, m_card.Zone);
        }

        [Test]
        public void Test_PlayCardAbility_puts_non_permanent_cards_into_graveyard_when_it_resolves()
        {
            m_card.Type = Type.Instant;
            PlayAndResolveSpell(m_spell, true);
            Assert.AreEqual(m_game.Zones.Graveyard, m_card.Zone);
        }

        [Test]
        public void Test_PlayCardAbility_uses_the_stack()
        {
            PlayAndResolveSpell(m_spell, true);
            Assert.IsTrue(m_spell.UseStack);
        }

        [Test]
        public void Test_PlayCardAbility_doesnt_use_the_stack_for_lands()
        {
            m_card.Type = Type.Land;
            PlayAndResolveSpell(m_spell, false);
            Assert.IsFalse(m_spell.UseStack);
        }

        [Test]
        public void Test_Can_play_cards_from_the_hand()
        {
            Assert.IsTrue(m_ability.CanPlay(m_playerA, new ExecutionEvaluationContext()));
        }

        [Test]
        public void Test_Cannot_play_cards_from_anywhere_else_than_the_hand()
        {
            m_card.Zone = m_game.Zones.Library;
            Assert.IsFalse(m_ability.CanPlay(m_playerA, new ExecutionEvaluationContext()));

            m_card.Zone = m_game.Zones.Battlefield;
            Assert.IsFalse(m_ability.CanPlay(m_playerA, new ExecutionEvaluationContext()));
        }

        [Test]
        public void Test_MustPayMana_is_there_is_a_mana_cost()
        {
            ManaCost manaCost = new ManaCost(3, ManaSymbol.RG);
            m_ability.ManaCost = manaCost;

            // Can always "play" mana costs
            Assert.IsTrue(m_ability.CanPlay(m_playerA, new ExecutionEvaluationContext()));

            PlayAndResolveSpell(m_spell, true).Enumerate();
            Assert.IsTrue(m_spell.DelayedCosts.Any(c => c is PayManaCost && ((PayManaCost)c).ManaCost == manaCost));
        }

        [Test]
        public void Test_Can_specialize_using_the_PlaySpecific_method()
        {
            int i = 0;
            ImmediateCost additionalImmediateCost = m_mockery.StrictMock<ImmediateCost>();
            DelayedCost additionalDelayedCost = m_mockery.StrictMock<DelayedCost>();
            SpellEffect additionalEffect = (s, c) => i = 1;

            IEnumerable<ImmediateCost> costs = PlayAndResolveSpell(m_spell, true, new[] { additionalImmediateCost }, new[] { additionalDelayedCost }, additionalEffect);
            Assert.Collections.Contains(additionalImmediateCost, costs);
            Assert.Collections.Contains(additionalDelayedCost, m_spell.DelayedCosts);
            Assert.AreEqual(1, i);
        }

        [Test]
        public void Test_Can_only_play_one_land_per_turn()
        {
            m_card.Type = Type.Land;

            Card secondLand = CreateCard(m_playerA);
            secondLand.Zone = m_game.Zones.Hand;
            secondLand.Type = Type.Land;

            Assert.IsTrue(m_ability.CanPlay(m_playerA, new ExecutionEvaluationContext()));

            PlayAndResolveSpell(m_spell, false);

            m_ability = m_game.CreateAbility<MockPlayCardAbility>(secondLand);
            m_ability.Implementation = m_mockery.PartialMock<PlayCardAbilityImplementation>();
            m_mockery.Replay(m_ability.Implementation);

            Assert.IsFalse(m_ability.CanPlay(m_playerA, new ExecutionEvaluationContext()));
        }

        [Test]
        public void Test_Cards_come_into_play_with_summoning_sickness()
        {
            m_card.Type = Type.Creature;
            PlayAndResolveSpell(m_spell, true);
            Assert.IsTrue(m_card.HasSummoningSickness());
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
    }
}

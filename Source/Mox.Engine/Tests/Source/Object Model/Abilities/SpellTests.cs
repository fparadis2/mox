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
    public class SpellTests : BaseGameTests
    {
        #region Variables

        private Spell m_spell;
        private object m_context;
        private Cost m_cost;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_context = new object();
            m_spell = new Spell(m_mockAbility, m_playerA, m_context);
            m_cost = m_mockery.StrictMock<Cost>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new Spell(null, m_playerA, m_context); });
            Assert.Throws<ArgumentNullException>(delegate { new Spell(m_mockAbility, null, m_context); });
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_game, m_spell.Game);
            Assert.AreEqual(m_card, m_spell.Source);
            Assert.AreEqual(m_mockAbility, m_spell.Ability);
            Assert.AreEqual(m_playerA, m_spell.Controller);
            Assert.AreEqual(m_context, m_spell.Context);
            Assert.Collections.IsEmpty(m_spell.Costs);

            Assert.IsNull(m_spell.EffectPart);
            Assert.IsTrue(m_spell.UseStack);
        }

        [Test]
        public void Test_Can_construct_a_spell_from_another_spell()
        {
            Spell oldSpell = m_spell;
            oldSpell.AddCost(m_cost);
            oldSpell.EffectPart = new MockPart();
            oldSpell.UseStack = true;

            m_spell = oldSpell.Resolve(oldSpell.Game, true);

            Assert.AreEqual(m_game, m_spell.Game);
            Assert.AreEqual(m_card, m_spell.Source);
            Assert.AreEqual(m_mockAbility, m_spell.Ability);
            Assert.AreEqual(m_playerA, m_spell.Controller);
            Assert.AreEqual(m_context, m_spell.Context);
            Assert.AreEqual(oldSpell.EffectPart, m_spell.EffectPart);
            Assert.AreEqual(oldSpell.UseStack, m_spell.UseStack);

            Assert.Collections.IsEmpty(m_spell.Costs); // Costs are not copied.
        }

        [Test]
        public void Test_Resolving_a_spell_with_the_same_game_returns_the_same_spell_if_not_forced()
        {
            Spell oldSpell = m_spell;
            m_spell = oldSpell.Resolve(oldSpell.Game, false);
            Assert.AreSame(m_spell, oldSpell);
        }

        [Test]
        public void Test_Can_construct_a_spell_from_another_spell_in_a_different_game()
        {
            Spell oldSpell = m_spell;
            oldSpell.AddCost(m_cost);
            oldSpell.EffectPart = new MockPart();
            oldSpell.UseStack = true;

            var replicatedGame = m_game.Replicate();

            m_spell = oldSpell.Resolve(replicatedGame, false);

            Card otherCard = Resolvable<Card>.Resolve(m_spell.Game, oldSpell.Source);
            Ability otherAbility = Resolvable<Ability>.Resolve(m_spell.Game, oldSpell.Ability);
            Player otherPlayer = Resolvable<Player>.Resolve(m_spell.Game, oldSpell.Controller);

            Assert.AreEqual(replicatedGame, m_spell.Game);
            Assert.AreEqual(otherCard, m_spell.Source);
            Assert.AreEqual(otherAbility, m_spell.Ability);
            Assert.AreEqual(otherPlayer, m_spell.Controller);
            Assert.AreEqual(oldSpell.Context, m_spell.Context);
            Assert.AreEqual(oldSpell.EffectPart, m_spell.EffectPart);
            Assert.AreEqual(oldSpell.UseStack, m_spell.UseStack);

            Assert.Collections.IsEmpty(m_spell.Costs); // Costs are not copied.
        }

        [Test]
        public void Test_Can_get_set_the_effect()
        {
            SpellEffect effect = delegate{};
            m_spell.Effect = effect;
            Assert.IsNotNull(m_spell.EffectPart);
        }

        [Test]
        public void Test_Can_get_set_the_effect_part()
        {
            var part = new MockPart();
            m_spell.EffectPart = part;
            Assert.AreEqual(part, m_spell.EffectPart);
        }

        [Test]
        public void Test_Can_get_set_whether_the_spell_will_use_the_stack()
        {
            m_spell.UseStack = false;
            Assert.IsFalse(m_spell.UseStack);

            m_spell.UseStack = true;
            Assert.IsTrue(m_spell.UseStack);
        }

        [Test]
        public void Test_Can_add_costs()
        {
            m_spell.AddCost(m_cost);
            Assert.Collections.Contains(m_cost, m_spell.Costs);
        }

        [Test]
        public void Test_SpellType_is_Ability_unless_it_its_ability_is_a_PlayCardAbility()
        {
            m_spell = new Spell(m_mockAbility, m_playerA);
            Assert.AreEqual(SpellType.Ability, m_spell.SpellType);

            m_spell = new Spell(new PlayCardAbility(), m_playerA);
            Assert.AreEqual(SpellType.Spell, m_spell.SpellType);
        }

        #endregion

        #region Inner Types

        private class MockPart : Part
        {
            #region Overrides of Part

            public override Part Execute(Context context)
            {
                return null;
            }

            #endregion
        }

        #endregion
    }
}

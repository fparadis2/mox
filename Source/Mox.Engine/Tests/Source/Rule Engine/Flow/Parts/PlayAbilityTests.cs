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
using System.Linq;

using NUnit.Framework;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class PlayAbilityTests : PartTestBase
    {
        #region Variables

        private PlayAbility m_part;
        private object m_context;

        private ImmediateCost m_immediateCost1;
        private ImmediateCost m_immediateCost2;

        private DelayedCost m_delayedCost1;
        private DelayedCost m_delayedCost2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_context = new object();

            m_part = new PlayAbility(m_playerA, m_mockAbility, m_context);

            m_immediateCost1 = m_mockery.StrictMock<ImmediateCost>();
            m_immediateCost2 = m_mockery.StrictMock<ImmediateCost>();

            m_delayedCost1 = m_mockery.StrictMock<DelayedCost>();
            m_delayedCost2 = m_mockery.StrictMock<DelayedCost>();
        }

        #endregion

        #region Utilities

        private void Run()
        {
            m_sequencerTester.Run(m_part);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new PlayAbility(null, m_mockAbility, null); });
        }

        [Test]
        public void Test_Playing_an_ability_pushes_a_spell_on_the_stack()
        {
            Spell createdSpell = null;

            m_mockAbility.Expect_Play(spell => createdSpell = spell);

            Run();

            Assert.AreEqual(m_mockAbility, createdSpell.Ability);
            Assert.AreEqual(m_playerA, createdSpell.Controller);
            Assert.AreEqual(m_card, createdSpell.Source);
            Assert.AreEqual(m_context, createdSpell.Context);

            Assert.AreEqual(1, m_game.SpellStack.Count());
            Assert.AreEqual(createdSpell, m_game.SpellStack.Peek());
        }

        [Test]
        public void Test_A_mana_ability_doesnt_use_the_stack_by_default()
        {
            m_mockAbility.MockedIsManaAbility = true;

            m_mockAbility.Expect_Play(spell => Assert.IsFalse(spell.UseStack));

            Run();

            Assert.AreEqual(0, m_game.SpellStack.Count());
        }

        [Test]
        public void Test_Each_cost_of_the_spell_is_paid_before_the_spell_is_pushed_on_the_stack()
        {
            using (OrderedExpectations)
            {
                Expect_Play_Ability_Raw(m_mockAbility, m_playerA, m_immediateCost1, m_immediateCost2);
            }

            Run();
        }

        [Test]
        public void Test_Each_delayed_cost_of_the_spell_is_paid_before_the_spell_is_pushed_on_the_stack()
        {
            using (OrderedExpectations)
            {
                Expect_Play_Ability_Delayed_Raw(m_mockAbility, m_playerA, m_delayedCost1, m_delayedCost2);
            }

            Run();
        }

        [Test]
        public void Test_If_an_immediate_cost_returns_false_when_executing_the_whole_ability_is_undone()
        {
            using (OrderedExpectations)
            {
                ImmediateCost[] costs = new[] { m_immediateCost1 };

                ISpellEffect spellEffect = m_mockery.StrictMock<ISpellEffect>();

                m_mockAbility.Expect_Play(costs, null, spell =>
                {
                    spell.PreEffect = (s, c) => spellEffect.DoPre();
                    spell.Effect = (s, c) => spellEffect.Do();
                });

                m_immediateCost1.Expect_Execute(m_playerA, false, () =>
                {
                    m_playerA.Life = 42;
                    Assert.AreEqual(42, m_playerA.Life);
                });
            }

            Run();

            Assert.AreNotEqual(42, m_playerA.Life);
        }

        [Test]
        public void Test_If_a_delayed_cost_returns_false_when_executing_the_whole_ability_is_undone()
        {
            using (OrderedExpectations)
            {
                ISpellEffect spellEffect = m_mockery.StrictMock<ISpellEffect>();

                m_mockAbility.Expect_Play(null, new[] { m_delayedCost1 }, spell =>
                {
                    spell.PreEffect = (s, c) => spellEffect.DoPre();
                    spell.Effect = (s, c) => spellEffect.Do();
                });

                m_delayedCost1.Expect_Execute(m_playerA, false, () =>
                {
                    m_playerA.Life = 42;
                    Assert.AreEqual(42, m_playerA.Life);
                });
            }

            Run();

            Assert.AreNotEqual(42, m_playerA.Life);
        }

        [Test]
        public void Test_PlayAbility_triggers_the_SpellPlayed_event()
        {
            Spell createdSpell = null;

            m_mockAbility.Expect_Play(spell => createdSpell = spell);

            m_game.AssertTriggers<Events.SpellPlayed>(Run, e =>
            {
                Assert.AreEqual(createdSpell, m_game.SpellStack.Peek()); // Already on the stack when triggered.
                Assert.AreEqual(createdSpell, e.Spell);
            });
        }

        #endregion
    }
}

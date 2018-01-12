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

using Mox.Abilities;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class PlayAbilityTests : PartTestBase
    {
        #region Variables

        private MockSpellAbility m_ability;
        private PlayAbility m_part;
        private object m_context;

        private MockCost m_cost1;
        private MockCost m_cost2;
        private MockAction m_action;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_context = new object();
            m_ability = m_game.CreateAbility<MockSpellAbility>(m_card);

            m_part = new PlayAbility(m_playerA, m_ability, m_context);

            m_cost1 = new MockCost();
            m_cost2 = new MockCost();
            m_action = new MockAction();

            var spellDefinition = m_ability.CreateSpellDefinition();
            spellDefinition.AddCost(m_cost1);
            spellDefinition.AddCost(m_cost2);
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
        public void Test_Playing_an_ability_pushes_a_spell_on_the_stack()
        {
            Assert.AreEqual(0, m_game.SpellStack2.Count);

            Run();

            Assert.AreEqual(1, m_game.SpellStack2.Count);
            var createdSpell = m_game.SpellStack2.Peek();

            Assert.AreEqual(m_ability, createdSpell.Ability);
            Assert.AreEqual(m_playerA, createdSpell.Controller);

#warning todo spell_v2 needed?
            //Assert.AreEqual(m_card, createdSpell.Source);
            //Assert.AreEqual(m_context, createdSpell.Context);
        }

        [Test]
        public void Test_Some_spells_dont_use_the_stack()
        {
            bool executed = false;
            m_action.Effect = () => executed = true;

            var spellDefinition = m_ability.CreateSpellDefinition();
            spellDefinition.AddAction(m_action);

            m_ability.MockedIsManaAbility = true;

            Run();

            Assert.AreEqual(0, m_game.SpellStack2.Count);
            Assert.That(executed);
        }

        [Test]
        public void Test_Each_cost_of_the_spell_is_paid_before_the_spell_is_pushed_on_the_stack()
        {
            Run();

            Assert.That(m_cost1.Executed);
            Assert.That(m_cost2.Executed);
        }

        [Test]
        public void Test_If_a_delayed_cost_returns_false_when_executing_the_whole_ability_is_undone()
        {
            m_cost1.ExecuteCallback = () =>
            {
                m_playerA.Life = 42;
                Assert.AreEqual(42, m_playerA.Life);
                return false;
            };

            Run();

            Assert.AreNotEqual(42, m_playerA.Life);
        }

        [Test]
        public void Test_PlayAbility_triggers_the_SpellPlayed_event()
        {
#warning todo spell_v2
            /*Spell createdSpell = null;

            m_mockAbility.Expect_Play(spell => createdSpell = spell);

            m_game.AssertTriggers<Events.SpellPlayed>(Run, e =>
            {
                Assert.AreEqual(createdSpell, m_game.SpellStack.Peek()); // Already on the stack when triggered.
                Assert.AreEqual(createdSpell, e.Spell);
            });*/
        }

        #endregion
    }
}

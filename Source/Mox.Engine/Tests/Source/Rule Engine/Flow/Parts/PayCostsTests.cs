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
using System.Collections.Generic;

using NUnit.Framework;
using Rhino.Mocks;

using Mox.Abilities;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class PayCostsTests : PartTestBase
    {
        #region Inner Types

        private class PayCostsProxy : Part
        {
            private readonly PayCosts m_payCosts;

            public PayCostsProxy(PayCosts payCosts)
            {
                Throw.IfNull(payCosts, "payCosts");
                m_payCosts = payCosts;
            }

            public override Part Execute(Context context)
            {
                // MUST begin a transaction for PayCosts to work properly
                context.Schedule(new BeginTransactionPart(PayCosts.TransactionToken));
                return m_payCosts;
            }
        }

        private class MockPayCosts : PayCosts
        {
            private readonly Resolvable<Spell2> m_spell;

            public MockPayCosts(Resolvable<Spell2> spell)
            {
                m_spell = spell;
            }

            protected override Spell2 GetSpell(Context context, out Part nextPart)
            {
                nextPart = null;
                return m_spell.Resolve(context.Game);
            }
        }

        #endregion

        #region Variables

        private Part m_part;

        private MockSpellAbility m_ability;
        private MockCost m_cost1;
        private MockCost m_cost2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_cost1 = new MockCost();
            m_cost2 = new MockCost();

            var spellDefinition = CreateSpellDefinition(m_card);
            spellDefinition.AddCost(m_cost1);
            spellDefinition.AddCost(m_cost2);

            m_ability = m_game.CreateAbility<MockSpellAbility>(m_card, spellDefinition);
            Spell2 spell = m_game.CreateSpell(m_ability, m_playerA);
            m_part = CreatePart(spell);
        }

        #endregion

        #region Utilities

        private void Run(bool expectedResult)
        {
            m_sequencerTester.Run(m_part);
            Assert.AreEqual(expectedResult, m_sequencerTester.Sequencer.PopArgument<bool>(PayCosts.ArgumentToken));
        }

        private static Part CreatePart(Spell2 spell)
        {
            return new PayCostsProxy(new MockPayCosts(spell));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Each_cost_of_the_spell_is_paid_before_the_spell_is_pushed_on_the_stack()
        {
            Run(true);
            Assert.That(m_cost1.Executed);
            Assert.That(m_cost2.Executed);
        }

        [Test]
        public void Test_If_a_cost_returns_false_when_executing_the_whole_ability_is_undone()
        {
            m_cost1.ExecuteCallback = () =>
            {
                m_playerA.Life = 42;
                Assert.AreEqual(42, m_playerA.Life);

                return false;
            };

            m_playerA.Life = 20;
            Run(false);
            Assert.AreEqual(20, m_playerA.Life);
        }

        #endregion
    }
}

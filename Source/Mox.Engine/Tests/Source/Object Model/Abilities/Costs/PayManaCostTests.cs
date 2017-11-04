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

namespace Mox
{
    [TestFixture]
    public class PayManaCostTests : CostTestsBase
    {
        #region Variables

        private PayManaCost m_cost;
        private MockAction m_mockAction;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_mockAction = new MockAction { ExpectedPlayer = m_playerA };

            m_cost = new PayManaCost(new ManaCost(2, ManaSymbol.R, ManaSymbol.W));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(new ManaCost(2, ManaSymbol.R, ManaSymbol.W), m_cost.ManaCost);
        }

        [Test]
        public void Test_Can_always_be_paid_in_non_usermode()
        {
            ExecutionEvaluationContext context = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal) { UserMode = false };
            Assert.IsTrue(m_cost.CanExecute(m_game, context));
        }

        [Test]
        public void Test_Can_be_paid_in_usermode_only_if_player_has_enough_mana_potential()
        {
            ExecutionEvaluationContext context = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal) { UserMode = true };
            Assert.IsFalse(m_cost.CanExecute(m_game, context));

            m_playerA.ManaPool.Red = 3;
            m_playerA.ManaPool.White = 1;

            context = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal) { UserMode = true };
            Assert.IsTrue(m_cost.CanExecute(m_game, context));
        }

        [Test]
        public void Test_If_the_player_passes_the_cost_is_cancelled()
        {
            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, null);
            Execute(m_cost, m_playerA, false);
        }

        [Test]
        public void Test_Nothing_to_do_if_the_cost_is_empty()
        {
            m_cost = new PayManaCost(new ManaCost(0));
            Execute(m_cost, m_playerA, true);
        }

        [Test]
        public void Test_Nothing_to_do_if_the_cost_is_null()
        {
            m_cost = new PayManaCost(null);
            Execute(m_cost, m_playerA, true);
        }

        [Test]
        public void Test_The_player_can_play_mana_actions_during_mana_payment_cost()
        {
            ExecutionEvaluationContext manaPaymentContext = new ExecutionEvaluationContext(m_playerA, EvaluationContextType.ManaPayment);

            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, m_mockAction);
            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, null);

            Execute(m_cost, m_playerA, false);
        }

        [Test]
        public void Test_The_player_can_pay_for_the_whole_mana()
        {
            m_playerA.ManaPool.Red = 1;
            m_playerA.ManaPool.Blue = 1;
            m_playerA.ManaPool.White = 1;
            m_playerA.ManaPool.Colorless = 1;

            var payment = ManaPayment.Prepare(m_cost.ManaCost);
            payment.Atoms[0] = new ManaPaymentAmount { White = 1 };
            payment.Atoms[1] = new ManaPaymentAmount { Red = 1 };
            payment.Generic = new ManaPaymentAmount { Blue = 1, Colorless = 1 };
                        
            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, new PayManaAction(payment));

            Execute(m_cost, m_playerA, true);

            Assert.AreEqual(0, m_playerA.ManaPool.Red);
            Assert.AreEqual(0, m_playerA.ManaPool.Blue);
            Assert.AreEqual(0, m_playerA.ManaPool.White);
            Assert.AreEqual(0, m_playerA.ManaPool.Colorless);
        }

        [Test]
        public void Test_Can_pay_in_multiple_payments()
        {
            // Repeating here for clarity
            m_cost = new PayManaCost(new ManaCost(2, ManaSymbol.R, ManaSymbol.W));

            m_playerA.ManaPool.Red = 1;
            m_playerA.ManaPool.Blue = 1;
            m_playerA.ManaPool.White = 1;
            m_playerA.ManaPool.Colorless = 1;

            var payment1 = ManaPayment.Prepare(m_cost.ManaCost);
            payment1.Atoms[1] = new ManaPaymentAmount { Red = 1 };
            payment1.Generic = new ManaPaymentAmount { Blue = 1 };

            var intermediateCost = new ManaCost(1, ManaSymbol.W);

            var payment2 = ManaPayment.Prepare(intermediateCost);
            payment2.Atoms[0] = new ManaPaymentAmount { White = 1 };
            payment2.Generic = new ManaPaymentAmount { Colorless = 1 };

            m_sequencer.Expect_Player_PayMana(m_playerA, m_cost.ManaCost, new PayManaAction(payment1));
            m_sequencer.Expect_Player_PayMana(m_playerA, intermediateCost, new PayManaAction(payment2));

            Execute(m_cost, m_playerA, true);

            Assert.AreEqual(0, m_playerA.ManaPool.Red);
            Assert.AreEqual(0, m_playerA.ManaPool.Blue);
            Assert.AreEqual(0, m_playerA.ManaPool.White);
            Assert.AreEqual(0, m_playerA.ManaPool.Colorless);
        }

        #endregion
    }
}

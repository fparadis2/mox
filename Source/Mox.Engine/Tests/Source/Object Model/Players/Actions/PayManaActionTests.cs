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

using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class PayManaActionTests : BaseGameTests
    {
        #region Variables

        private PayManaAction m_action;
        private NewSequencerTester m_sequencerTester;
        private ManaPaymentNew m_payment;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_payment = new ManaPaymentNew { Generic = new ManaPaymentAmount { Red = 1 } };

            m_action = new PayManaAction(m_payment);

            m_sequencerTester = new NewSequencerTester(m_mockery, m_game);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_payment, m_action.Payment);
        }

        [Test]
        public void Test_can_only_execute_during_mana_payment()
        {
            Assert.IsFalse(m_action.CanExecute(new ExecutionEvaluationContext(m_playerA, EvaluationContextType.Normal)));
            Assert.IsTrue(m_action.CanExecute(new ExecutionEvaluationContext(m_playerA, EvaluationContextType.ManaPayment)));
        }

        [Test]
        public void Test_can_never_truly_Execute_this_action()
        {
            Assert.Throws<InvalidProgramException>(() => m_action.Execute(m_sequencerTester.CreateContext(), m_playerA));
        }

        #endregion
    }
}

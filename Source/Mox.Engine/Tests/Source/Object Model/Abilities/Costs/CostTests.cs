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
    public class CostTests : BaseGameTests
    {
        #region Tests

        [Test]
        public void Test_CannotPlay_is_a_cost_that_can_never_be_executed()
        {
            ExecutionEvaluationContext context = new ExecutionEvaluationContext();

            foreach (EvaluationContextType type in Enum.GetValues(typeof(EvaluationContextType)))
            {
                context.Type = type;
                Assert.IsFalse(Cost.CannotPlay.CanExecute(m_game, context));
            }
            Assert.Throws<InvalidOperationException>(() => Cost.CannotPlay.Execute(null, null));
        }

        [Test]
        public void Test_Tap_is_a_TapCost_with_Tap_true()
        {
            TapCost tapCost = Cost.Tap(m_card);
            Assert.IsNotNull(tapCost, "tapCost");
            Assert.AreEqual(m_card, tapCost.Card);
            Assert.IsTrue(tapCost.DoTap);
        }

        [Test]
        public void Test_IsImmediate_returns_false_for_ordinary_costs()
        {
            Assert.IsFalse(Cost.IsImmediate(m_mockery.StrictMock<Cost>()));
        }

        [Test]
        public void Test_IsImmediate_returns_true_for_immediate_costs()
        {
            Assert.IsTrue(Cost.IsImmediate(m_mockery.StrictMock<ImmediateCost>()));
        }

        #endregion
    }
}

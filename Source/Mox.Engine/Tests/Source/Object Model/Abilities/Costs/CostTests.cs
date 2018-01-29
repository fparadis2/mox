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

namespace Mox.Abilities
{
    [TestFixture]
    public class CostTests : CostTestsBase
    {
        #region Tests

        [Test]
        public void Test_CannotPlay_is_a_cost_that_can_never_be_executed()
        {
            foreach (AbilityEvaluationContextType type in Enum.GetValues(typeof(AbilityEvaluationContextType)))
            {
                var context = new AbilityEvaluationContext(m_playerA, type);
                Assert.IsFalse(Cost.CannotPlay.CanExecute(m_ability, context));
            }
            Assert.Throws<InvalidOperationException>(() => Cost.CannotPlay.Execute(null, null));
        }

        #endregion
    }
}

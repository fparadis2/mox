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
using System.Text;
using NUnit.Framework;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class BeginningOfCombatStepTests : BaseStepTests<BeginningOfCombatStep>
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_step = new BeginningOfCombatStep();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(Steps.BeginningOfCombat, m_step.Type);
        }

        [Test]
        public void Test_CombatData_is_reset()
        {
            m_game.CombatData.Attackers = new DeclareAttackersResult(m_card);

            RunStep(m_playerA);

            Assert.That(m_game.CombatData.Attackers.IsEmpty);
        }

        #endregion
    }
}

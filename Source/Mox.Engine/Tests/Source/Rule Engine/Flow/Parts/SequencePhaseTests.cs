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
using System.Reflection;
using System.Text;

using NUnit.Framework;
using Rhino.Mocks;

using Is = Rhino.Mocks.Constraints.Is;

using Mox.Flow.Phases;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class SequencePhaseTests : PartTestBase<SequencePhase>
    {
        #region Variables

        private Phase m_phase;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_phase = m_mockery.StrictMock<Phase>(Mox.Phases.PrecombatMain);
            m_part = new SequencePhase(m_playerA, m_phase);
        }

        #endregion

        #region Utilities

        private void Expect_Sequence_Phase(Player player, MTGPart result)
        {
            Expect.Call(m_phase.Sequence(null, null))
                  .IgnoreArguments()
                  .Constraints(Is.Matching<MTGPart.Context>(m_sequencerTester.ValidateContext), Is.Equal(player))
                  .Return(result);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new SequencePhase(null, m_phase); });
            Assert.Throws<ArgumentNullException>(delegate { new SequencePhase(m_playerA, null); });
        }

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.AreEqual(m_phase, m_part.Phase);
        }

        [Test]
        public void Test_Execute_executes_the_step()
        {
            MTGPart result = m_mockery.StrictMock<MTGPart>(m_playerA);
            Expect_Sequence_Phase(m_playerA, result);
            Assert.AreEqual(result, Execute(m_part));
        }

        #endregion
    }
}

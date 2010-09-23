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
    public class SequenceStepTests : PartTestBase<SequenceStep>
    {
        #region Variables

        private Step m_step;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_step = m_mockery.StrictMock<Step>(Steps.Draw);
            m_part = new SequenceStep(m_playerA, m_step);
        }

        #endregion

        #region Utilities

        private void Expect_Sequence_Step(Player player, MTGPart result)
        {
            typeof(Step).GetMethod("SequenceImpl", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(m_step, new object[] { null, null });

            LastCall
                .IgnoreArguments()
                .Constraints(Is.Matching<MTGPart.Context>(m_sequencerTester.ValidateContext), Is.Equal(player))
                .Return(result);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new SequenceStep(null, m_step); });
            Assert.Throws<ArgumentNullException>(delegate { new SequenceStep(m_playerA, null); });
        }

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.AreEqual(m_step, m_part.Step);
            Assert.AreEqual(m_playerA, m_part.GetPlayer(m_sequencerTester.Context));
        }

        [Test]
        public void Test_Execute_executes_the_step()
        {
            MTGPart result = m_mockery.StrictMock<MTGPart>(m_playerA);
            Expect_Sequence_Step(m_playerA, result);
            Assert.AreEqual(result, Execute(m_part));
        }

        #endregion
    }
}

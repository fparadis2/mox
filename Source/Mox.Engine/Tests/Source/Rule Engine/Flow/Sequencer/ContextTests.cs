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

namespace Mox.Flow
{
    [TestFixture]
    public class ContextTests : BaseGameTests
    {
        #region Variables

        private NewPart.Context m_context;

        #endregion

        #region Setup

        public override void Setup()
        {
            base.Setup();

            var sequencer = new NewSequencer(m_game, m_mockery.StrictMock<NewPart>());
            m_context = new NewPart.Context(sequencer);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_game, m_context.Game);

            Assert.Collections.IsEmpty(m_context.ScheduledParts);
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new NewPart.Context(null); });
        }

        [Test]
        public void Test_Schedule_adds_a_scheduled_part()
        {
            var part1 = m_mockery.StrictMock<NewPart>();
            var part2 = m_mockery.StrictMock<NewPart>();

            m_context.Schedule(part1);
            m_context.Schedule(part2);

            Assert.Collections.AreEqual(new[] { part2, part1 }, m_context.ScheduledParts);
        }

        [Test]
        public void Test_Can_get_set_Stop()
        {
            Assert.IsFalse(m_context.Stop);
            m_context.Stop = true;
            Assert.IsTrue(m_context.Stop);
        }

        #endregion
    }
}

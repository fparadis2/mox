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

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class GivePriorityTests : PartTestBase
    {
        #region Variables

        private GivePriority m_part;
        private Action m_mockAction;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_mockAction = m_mockery.StrictMock<Action>();
            m_part = new GivePriority(m_playerA);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new GivePriority(null); });
        }

        [Test]
        public void Test_If_player_returns_null_it_returns_null()
        {
            m_sequencerTester.Expect_Player_GivePriority(m_playerA, null);
            
            Assert.IsNull(Execute(m_part));

            Assert.IsTrue(m_lastContext.PopArgument<bool>(GivePriority.ArgumentToken));
            Assert.Collections.IsEmpty(m_lastContext.ScheduledParts);
        }

        [Test]
        public void Test_If_player_returns_an_invalid_action_it_retries()
        {
            m_sequencerTester.Expect_Player_GivePriority_And_Play(m_playerA, m_mockAction);

            Assert.AreEqual(m_part, Execute(m_part));

            Assert.Collections.IsEmpty(m_lastContext.ScheduledParts);
        }

        [Test]
        public void Test_If_player_returns_a_valid_action_it_is_executed()
        {
            m_sequencerTester.Expect_Player_GivePriority_And_PlayInvalid(m_playerA, m_mockAction);

            Assert.IsNull(Execute(m_part));

            Assert.IsFalse(m_lastContext.PopArgument<bool>(GivePriority.ArgumentToken));
            Assert.Collections.IsEmpty(m_lastContext.ScheduledParts);
        }

        #endregion
    }
}

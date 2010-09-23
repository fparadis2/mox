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
using Rhino.Mocks;

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class PlayUntilAllPlayersPassTests : PartTestBase<PlayUntilAllPlayersPass>
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_part = new PlayUntilAllPlayersPass(m_playerA);
        }

        #endregion

        #region Utilities

        private void Run()
        {
            m_sequencerTester.Run(m_part);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new PlayUntilAllPlayersPass(null); });
        }

        [Test]
        public void Test_Execute_gives_priority_to_each_player_until_they_all_pass()
        {
            using (OrderedExpectations)
            {
                Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_Execute_gives_priority_to_each_player_until_they_all_pass_starting_with_the_given_player()
        {
            using (OrderedExpectations)
            {
                Expect_Everyone_passes_once(m_playerB);
            }

            m_part = new PlayUntilAllPlayersPass(m_playerB);
            Run();
        }

        [Test]
        public void Test_Execute_will_ask_each_player_until_everyone_passes_1()
        {
            using (OrderedExpectations)
            {
                Expect_Player_MockAction(m_playerA, m_mockAction);
                Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_Execute_will_ask_each_player_until_everyone_passes_2()
        {
            using (OrderedExpectations)
            {
                Expect_Player_MockAction(m_playerB, m_mockAction);
                Expect_Player_MockAction(m_playerB, null);
                Expect_Player_MockAction(m_playerA, m_mockAction);
                Expect_Player_MockAction(m_playerA, m_mockAction);
                Expect_Everyone_passes_once(m_playerA);
            }

            m_part = new PlayUntilAllPlayersPass(m_playerB);
            Run();
        }

        [Test]
        public void Test_Cannot_execute_an_action_that_cannot_be_executed()
        {
            using (OrderedExpectations)
            {
                Expect_Player_Invalid_MockAction(m_playerA, m_mockAction);
                Expect_Player_Invalid_MockAction(m_playerA, m_mockAction);
                Expect_Player_Invalid_MockAction(m_playerA, m_mockAction);
                Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_State_based_actions_and_triggered_abilities_are_checked_before_giving_priority()
        {
            Execute(m_part);

            Assert.AreEqual(3, m_sequencerTester.Context.ScheduledParts.Count());
            Assert.IsInstanceOf<CheckStateBasedActions>(m_sequencerTester.Context.ScheduledParts.Skip(2).First());
            Assert.IsInstanceOf<HandleTriggeredAbilities>(m_sequencerTester.Context.ScheduledParts.Skip(1).First());
        }

        #endregion
    }
}

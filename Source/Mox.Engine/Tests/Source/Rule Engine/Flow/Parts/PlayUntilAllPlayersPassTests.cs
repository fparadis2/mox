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

namespace Mox.Flow.Parts
{
    [TestFixture]
    public class PlayUntilAllPlayersPassTests : PartTestBase
    {
        #region Variables

        private PlayUntilAllPlayersPass m_part;
        private Action m_mockAction;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_mockAction = m_mockery.StrictMock<Action>();
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
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_Execute_gives_priority_to_each_player_until_they_all_pass_starting_with_the_given_player()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Everyone_passes_once(m_playerB);
            }

            m_part = new PlayUntilAllPlayersPass(m_playerB);
            Run();
        }

        [Test]
        public void Test_Execute_will_ask_each_player_until_everyone_passes_1()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority_And_Play(m_playerA, m_mockAction);
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_Execute_will_ask_each_player_until_everyone_passes_2()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority_And_Play(m_playerB, m_mockAction);
                m_sequencerTester.Expect_Player_GivePriority_And_Play(m_playerB, null);
                m_sequencerTester.Expect_Player_GivePriority_And_Play(m_playerA, m_mockAction);
                m_sequencerTester.Expect_Player_GivePriority_And_Play(m_playerA, m_mockAction);
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);
            }

            m_part = new PlayUntilAllPlayersPass(m_playerB);
            Run();
        }

        [Test]
        public void Test_Cannot_execute_an_action_that_cannot_be_executed()
        {
            using (OrderedExpectations)
            {
                m_sequencerTester.Expect_Player_GivePriority_And_PlayInvalid(m_playerA, m_mockAction);
                m_sequencerTester.Expect_Player_GivePriority_And_PlayInvalid(m_playerA, m_mockAction);
                m_sequencerTester.Expect_Player_GivePriority_And_PlayInvalid(m_playerA, m_mockAction);
                m_sequencerTester.Expect_Everyone_passes_once(m_playerA);
            }

            Run();
        }

        [Test]
        public void Test_State_based_actions_and_triggered_abilities_are_checked_before_giving_priority()
        {
            Execute(m_part);

            Assert.AreEqual(3, m_lastContext.ScheduledParts.Count());
            Assert.IsInstanceOf<CheckStateBasedActions>(m_lastContext.ScheduledParts.Skip(2).First());
            Assert.IsInstanceOf<HandleTriggeredAbilities>(m_lastContext.ScheduledParts.Skip(1).First());
        }

        #endregion
    }
}

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
    public class DeadGameInputTests : BaseGameTests
    {
        #region Variables

        private DeadGameInput m_input;
        private NewSequencer m_sequencer;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_input = new DeadGameInput();
            m_sequencer = new NewSequencerTester(m_mockery, m_game).Sequencer;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Always_makes_the_default_choice()
        {
            var choice = new MockChoice(m_playerA);
            Assert.AreEqual(3, m_input.MakeChoiceDecision(m_sequencer, choice));
        }

        #endregion

        #region Mock Types

        private class MockChoice : Choice
        {
            public MockChoice(Resolvable<Player> player)
                : base(player)
            {
            }

            #region Overrides of Choice

            public override object DefaultValue
            {
                get { return 3; }
            }

            #endregion
        }

        #endregion
    }
}

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
using Rhino.Mocks;

namespace Mox.Flow
{
    [TestFixture]
    public class MasterGameControllerTests : BaseGameTests
    {
        #region Variables

        private IChoiceDecisionMaker m_fallbackInput;
        private IClientInput m_clientController;

        private MasterGameInput m_masterInput;
        private NewSequencer m_sequencer;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_fallbackInput = m_mockery.StrictMock<IChoiceDecisionMaker>();
            m_clientController = m_mockery.StrictMock<IClientInput>();

            m_masterInput = new MasterGameInput(m_game, m_fallbackInput);
            m_sequencer = new NewSequencerTester(m_mockery, m_game).Sequencer;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new MasterGameInput(null, m_fallbackInput); });
            Assert.Throws<ArgumentNullException>(delegate { new MasterGameInput(m_game, null); });
        }

        [Test]
        public void Test_By_default_calls_are_routed_to_the_fallback_controller()
        {
            GivePriorityChoice choice = new GivePriorityChoice(m_playerA);

            Expect.Call(m_fallbackInput.MakeChoiceDecision(m_sequencer, choice)).Return("skrillex");
            m_mockery.Test(() => Assert.AreEqual("skrillex", m_masterInput.MakeChoiceDecision(m_sequencer, choice)));
        }

        [Test]
        public void Test_AssignClientInput_invalid_arguments()
        {
            Assert.Throws<ArgumentException>(() => m_masterInput.AssignClientInput(null, m_clientController));
            Assert.Throws<ArgumentException>(() => m_masterInput.AssignClientInput(m_playerA, null));
            Assert.Throws<ArgumentException>(() => m_masterInput.AssignClientInput(new Player(), m_clientController));
        }

        [Test]
        public void Test_AssignClientInput_assigns_a_client_input_to_a_specific_player()
        {
            m_masterInput.AssignClientInput(m_playerA, m_clientController);

            MulliganChoice choice = new MulliganChoice(m_playerA);
            
            Expect.Call(m_clientController.Mulligan()).Return(true);
            m_mockery.Test(() => Assert.AreEqual(true, m_masterInput.MakeChoiceDecision(m_sequencer, choice)));
        }

        [Test]
        public void Test_Can_overwrite_input_for_a_given_player()
        {
            IClientInput clientInput2 = m_mockery.StrictMock<IClientInput>();

            m_masterInput.AssignClientInput(m_playerA, m_clientController);
            m_masterInput.AssignClientInput(m_playerA, clientInput2);

            Action action = m_mockery.StrictMock<Action>();
            GivePriorityChoice choice = new GivePriorityChoice(m_playerA);

            Expect.Call(clientInput2.GivePriority()).Return(action);
            m_mockery.Test(() => Assert.AreEqual(action, m_masterInput.MakeChoiceDecision(m_sequencer, choice)));
        }

        [Test]
        public void Test_Unassign_invalid_arguments()
        {
            Assert.Throws<ArgumentException>(() => m_masterInput.Unassign(null));
            Assert.Throws<ArgumentException>(() => m_masterInput.Unassign(new Player()));
        }

        [Test]
        public void Test_Can_unassign_a_player()
        {
            m_masterInput.Unassign(m_playerB); // Unassigning an already unassigned player does nothing.

            m_masterInput.AssignClientInput(m_playerA, m_clientController);
            m_masterInput.Unassign(m_playerA);

            GivePriorityChoice choice = new GivePriorityChoice(m_playerA);

            Expect.Call(m_fallbackInput.MakeChoiceDecision(m_sequencer, choice)).Return(42);
            m_mockery.Test(() => Assert.AreEqual(42, m_masterInput.MakeChoiceDecision(m_sequencer, choice)));
        }

        #endregion
    }
}

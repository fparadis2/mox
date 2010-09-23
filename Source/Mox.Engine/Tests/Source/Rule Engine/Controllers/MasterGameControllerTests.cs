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

        private IGameController m_fallbackController;
        private MasterGameController m_masterController;
        private IGameController m_controller;
        private IClientController m_clientController;

        private SequencerTester m_sequencerTester;
        private Part<IGameController>.Context m_context;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_sequencerTester = new SequencerTester(m_mockery, m_game);
            m_context = m_sequencerTester.Context;

            m_fallbackController = m_mockery.StrictMock<IGameController>();
            m_controller = m_masterController = new MasterGameController(m_game, m_fallbackController);

            m_clientController = m_mockery.StrictMock<IClientController>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new MasterGameController(null, m_fallbackController); });
            Assert.Throws<ArgumentNullException>(delegate { new MasterGameController(m_game, null); });
        }

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.AreEqual(m_fallbackController, m_masterController.FallbackController);
        }

        [Test]
        public void Test_Default_fallback_controller_is_a_dead_controller()
        {
            m_masterController = new MasterGameController(m_game);
            Assert.IsInstanceOf<DeadGameController>(m_masterController.FallbackController);
        }

        [Test]
        public void Test_Cannot_dispatch_a_call_to_an_invalid_player()
        {
            Assert.Throws<ArgumentException>(() => m_controller.Mulligan(null, new Player()));
        }

        [Test]
        public void Test_By_default_calls_are_routed_to_the_fallback_controller()
        {
            Expect.Call(m_fallbackController.Mulligan(m_context, m_playerA)).Return(true);
            m_mockery.Test(() => Assert.IsTrue(m_controller.Mulligan(m_context, m_playerA)));
        }

        [Test]
        public void Test_AssignClientController_invalid_arguments()
        {
            Assert.Throws<ArgumentException>(() => m_masterController.AssignClientController(null, m_clientController));
            Assert.Throws<ArgumentException>(() => m_masterController.AssignClientController(m_playerA, null));
            Assert.Throws<ArgumentException>(() => m_masterController.AssignClientController(new Player(), m_clientController));
        }

        [Test]
        public void Test_AssignClientController_assigns_a_client_controller_to_a_specific_player()
        {
            m_masterController.AssignClientController(m_playerA, m_clientController);

            TargetContext targetInfo = new TargetContext(false, new[] { 1, 2, 3 }, TargetContextType.Normal);
            Expect.Call(m_clientController.Target(targetInfo)).Return(3);
            m_mockery.Test(() => Assert.AreEqual(3, m_controller.Target(m_context, m_playerA, targetInfo)));
        }

        [Test]
        public void Test_Can_overwrite_controller_for_a_given_player()
        {
            IClientController clientController2 = m_mockery.StrictMock<IClientController>();

            m_masterController.AssignClientController(m_playerA, m_clientController);
            m_masterController.AssignClientController(m_playerA, clientController2);

            Action action = m_mockery.StrictMock<Action>();
            Expect.Call(clientController2.PayMana(new ManaCost(3))).Return(action);
            m_mockery.Test(() => Assert.AreEqual(action, m_controller.PayMana(m_context, m_playerA, new ManaCost(3))));
        }

        [Test]
        public void Test_Unassign_invalid_arguments()
        {
            Assert.Throws<ArgumentException>(() => m_masterController.Unassign(null));
            Assert.Throws<ArgumentException>(() => m_masterController.Unassign(new Player()));
        }

        [Test]
        public void Test_Can_unassign_a_player()
        {
            m_masterController.Unassign(m_playerB); // Unassigning an already unassigned player does nothing.

            m_masterController.AssignClientController(m_playerA, m_clientController);
            m_masterController.Unassign(m_playerA);

            Action action = m_mockery.StrictMock<Action>();
            Expect.Call(m_fallbackController.GivePriority(m_context, m_playerA)).Return(action);
            m_mockery.Test(() => Assert.AreEqual(action, m_controller.GivePriority(m_context, m_playerA)));
        }

        #endregion
    }
}

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
using Mox.Flow;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI
{
    [TestFixture]
    public class UIPlayerControllerTests : BaseGameViewModelTests
    {
        #region Mock Types

        public interface IInteractionControllerImplementation
        {
            object DoInteraction(System.Type type);
        }

        private class MockInteractionController : InteractionController
        {
            private readonly IInteractionControllerImplementation m_implementation;

            public MockInteractionController(GameViewModel model, IInteractionControllerImplementation implementation)
                : base(model, null)
            {
                m_implementation = implementation;
            }

            protected override T BeginInteraction<T>(Action<T> initializer)
            {
                T interaction = base.BeginInteraction(initializer);

                object result = m_implementation.DoInteraction(interaction.GetType());
                interaction.End(result);

                return interaction;
            }
        }

        #endregion

        #region Variables

        private IInteractionControllerImplementation m_implementation;
        private InteractionController m_interactionController;

        private UIPlayerController m_playerController;

        #endregion

        #region Setup

        public override void Setup()
        {
            base.Setup();

            m_implementation = m_mockery.StrictMock<IInteractionControllerImplementation>();

            m_gameViewModel.MainPlayer = CreatePlayerViewModel();

            m_interactionController = new MockInteractionController(m_gameViewModel, m_implementation);
            m_playerController = new UIPlayerController(m_interactionController);
        }

        public override void Teardown()
        {
            m_interactionController.Dispose();

            base.Teardown();
        }

        #endregion

        #region Utilities

        private void Expect_Run_and_wait_for_interaction(string interactionTypeName, object result)
        {
            Expect
                .Call(m_implementation.DoInteraction(null))
                .IgnoreArguments()
                .Constraints(Rhino.Mocks.Constraints.Is.Matching<System.Type>(t => t.Name == interactionTypeName))
                .Return(result);
        }

        private void Assert_Returns<TResult>(TResult result, Func<TResult> action)
        {
            m_mockery.Test(() => Assert.AreEqual(result, action()));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Mulligan()
        {
            Expect_Run_and_wait_for_interaction("MulliganInteraction", true);
            Assert_Returns(true, m_playerController.Mulligan);
        }

        [Test]
        public void Test_GivePriority()
        {
            Action action = m_mockery.StrictMock<Action>();
            Expect_Run_and_wait_for_interaction("GivePriorityInteraction", action);
            Assert_Returns(action, m_playerController.GivePriority);
        }

        [Test]
        public void Test_PayMana()
        {
            Action action = m_mockery.StrictMock<Action>();
            Expect_Run_and_wait_for_interaction("PayManaInteraction", action);
            Assert_Returns(action, () => m_playerController.PayMana(new ManaCost(0)));
        }

        [Test]
        public void Test_Target()
        {
            Expect_Run_and_wait_for_interaction("TargetInteraction", 3);
            Assert_Returns(3, () => m_playerController.Target(new TargetContext(false, new int[0], TargetContextType.Normal)));
        }

        [Test]
        public void Test_DeclareAttackers()
        {
            DeclareAttackersResult result = new DeclareAttackersResult();
            Expect_Run_and_wait_for_interaction("DeclareAttackersInteraction", result);
            Assert_Returns(result, () => m_playerController.DeclareAttackers(new DeclareAttackersContext(new Card[0])));
        }

        #endregion
    }
}

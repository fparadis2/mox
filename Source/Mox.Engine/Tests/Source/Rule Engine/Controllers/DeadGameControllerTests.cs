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
    public class DeadGameControllerTests : BaseGameTests
    {
        #region Variables

        private DeadGameController m_controller;

        private SequencerTester m_sequencerTester;
        private Part<IGameController>.Context m_context;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_controller = new DeadGameController();

            m_sequencerTester = new SequencerTester(m_mockery, m_game);
            m_context = m_sequencerTester.Context;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_AskModalChoice_returns_the_default_choice()
        {
            ModalChoiceContext choiceContext = new ModalChoiceContext { DefaultChoice = ModalChoiceResult.No };

            Assert.AreEqual(ModalChoiceResult.No, m_controller.AskModalChoice(m_context, m_playerA, choiceContext));
        }

        [Test]
        public void Test_GivePriority_always_returns_null()
        {
            Assert.IsNull(m_controller.GivePriority(m_context, m_playerA));
        }

        [Test]
        public void Test_PayMana_always_returns_null()
        {
            Assert.IsNull(m_controller.PayMana(m_context, m_playerA, new ManaCost(0)));
            Assert.IsNull(m_controller.PayMana(m_context, m_playerA, new ManaCost(5, ManaSymbol.R)));
        }

        [Test]
        public void Test_Mulligan_always_returns_false()
        {
            Assert.IsFalse(m_controller.Mulligan(m_context, m_playerA));
        }

        [Test]
        public void Test_Target_cancels_if_possible()
        {
            Assert.AreEqual(ObjectManager.InvalidIdentifier, m_controller.Target(m_context, m_playerA, new TargetContext(true, new[] { 1, 2, 3 }, TargetContextType.Normal)));
        }

        [Test]
        public void Test_Target_returns_the_first_available_target_if_cannot_cancel()
        {
            Assert.AreEqual(1, m_controller.Target(m_context, m_playerA, new TargetContext(false, new[] { 1, 2, 3 }, TargetContextType.Normal)));
        }

        #endregion
    }
}

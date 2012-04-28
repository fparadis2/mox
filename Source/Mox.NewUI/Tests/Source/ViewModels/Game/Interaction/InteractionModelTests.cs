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

namespace Mox.UI.Game
{
    [TestFixture]
    public class InteractionModelTests : BaseGameViewModelTests
    {
        #region Variables

        private InteractionModel m_interactionModel;
        private UserChoiceInteractionModel m_userChoice;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_interactionModel = m_gameViewModel.Interaction;
            m_userChoice = new UserChoiceInteractionModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_set_the_user_choice()
        {
            Assert.IsFalse(m_interactionModel.IsUserInteractionVisible);

            m_interactionModel.UserChoiceInteraction = m_userChoice;
            Assert.AreEqual(m_userChoice, m_interactionModel.UserChoiceInteraction);

            Assert.IsTrue(m_interactionModel.IsUserInteractionVisible);
        }

        [Test]
        public void Test_The_SelectChoiceCommand_triggers_the_UserChoiceSelected_event()
        {
            EventSink<ItemEventArgs<UserChoiceModel>> sink = new EventSink<ItemEventArgs<UserChoiceModel>>(m_interactionModel);
            m_interactionModel.UserChoiceSelected += sink;

            UserChoiceModel model = new UserChoiceModel();
            m_interactionModel.UserChoiceInteraction = new UserChoiceInteractionModel(model);
            Assert.EventCalledOnce(sink, () => m_interactionModel.SelectChoice(model));
            Assert.AreEqual(model, sink.LastEventArgs.Item);
        }

        [Test]
        public void Test_Cannot_set_an_unrelated_choice_as_the_selected_choice()
        {
            EventSink<ItemEventArgs<UserChoiceModel>> sink = new EventSink<ItemEventArgs<UserChoiceModel>>(m_interactionModel);
            m_interactionModel.UserChoiceSelected += sink;

            Assert.Throws<ArgumentException>(() => m_interactionModel.SelectChoice(new UserChoiceModel()));

            Assert.AreEqual(0, sink.TimesCalled);
        }

        #endregion
    }
}

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
    public class UserChoiceInteractionModelTests
    {
        #region Variables

        private UserChoiceInteractionModel m_userChoiceInteractionModel;
        private UserChoiceModel m_userChoiceModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_userChoiceInteractionModel = new UserChoiceInteractionModel();
            m_userChoiceModel = new UserChoiceModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_Text()
        {
            Assert.AreEqual(null, m_userChoiceInteractionModel.Text);
            m_userChoiceInteractionModel.Text = "New Value";
            Assert.AreEqual("New Value", m_userChoiceInteractionModel.Text);
        }

        [Test]
        public void Test_Can_add_and_remove_choices()
        {
            m_userChoiceInteractionModel.Choices.Add(m_userChoiceModel);
            Assert.Collections.Contains(m_userChoiceModel, m_userChoiceInteractionModel.Choices);
            Assert.IsTrue(m_userChoiceInteractionModel.Choices.Remove(m_userChoiceModel));
        }

        [Test]
        public void Test_Params_constructor()
        {
            m_userChoiceInteractionModel = new UserChoiceInteractionModel(m_userChoiceModel);
            Assert.Collections.Contains(m_userChoiceModel, m_userChoiceInteractionModel.Choices);
        }

        [Test]
        public void Test_YesNo_returns_a_yes_no_model()
        {
            m_userChoiceInteractionModel = UserChoiceInteractionModel.YesNo("Hello");
            Assert.AreEqual("Hello", m_userChoiceInteractionModel.Text);
            Assert.AreEqual(2, m_userChoiceInteractionModel.Choices.Count);

            Assert.AreEqual("Yes", m_userChoiceInteractionModel.Choices[0].Text);
            Assert.AreEqual(UserChoiceType.Yes, m_userChoiceInteractionModel.Choices[0].Type);

            Assert.AreEqual("No", m_userChoiceInteractionModel.Choices[1].Text);
            Assert.AreEqual(UserChoiceType.No, m_userChoiceInteractionModel.Choices[1].Type);
        }

        [Test]
        public void Test_Cancel_returns_a_cancel_model()
        {
            m_userChoiceInteractionModel = UserChoiceInteractionModel.Cancel("Hello");
            Assert.AreEqual("Hello", m_userChoiceInteractionModel.Text);
            Assert.AreEqual(1, m_userChoiceInteractionModel.Choices.Count);

            Assert.AreEqual("Cancel", m_userChoiceInteractionModel.Choices[0].Text);
            Assert.AreEqual(UserChoiceType.Cancel, m_userChoiceInteractionModel.Choices[0].Type);
        }

        [Test]
        public void Test_Cancel_returns_a_cancel_model_with_cancel_button_text()
        {
            m_userChoiceInteractionModel = UserChoiceInteractionModel.Cancel("Hello", "MyText");
            Assert.AreEqual("Hello", m_userChoiceInteractionModel.Text);
            Assert.AreEqual(1, m_userChoiceInteractionModel.Choices.Count);

            Assert.AreEqual("MyText", m_userChoiceInteractionModel.Choices[0].Text);
            Assert.AreEqual(UserChoiceType.Cancel, m_userChoiceInteractionModel.Choices[0].Type);
        }

        #endregion
    }
}

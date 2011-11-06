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
    public class ModalChoiceContextTests : BaseGameTests
    {
        #region Variables

        private ModalChoiceContext m_context;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_context = new ModalChoiceContext();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_Choices()
        {
            Assert.Collections.IsEmpty(m_context.Choices);

            m_context.Choices.Add(ModalChoiceResult.No);
            m_context.Choices.Add(ModalChoiceResult.Yes);

            Assert.Collections.AreEqual(new[] { ModalChoiceResult.No, ModalChoiceResult.Yes }, m_context.Choices);
        }

        [Test]
        public void Test_Can_get_set_DefaultChoice()
        {
            Assert.AreNotEqual(ModalChoiceResult.No, m_context.DefaultChoice, "Sanity check");

            m_context.DefaultChoice = ModalChoiceResult.No;
            Assert.AreEqual(ModalChoiceResult.No, m_context.DefaultChoice);
        }

        [Test]
        public void Test_Can_get_set_Question()
        {
            m_context.Question = "My Question";
            Assert.AreEqual("My Question", m_context.Question);
        }

        [Test]
        public void Test_Can_get_set_Importance()
        {
            Assert.AreEqual(ModalChoiceImportance.Critical, m_context.Importance);

            m_context.Importance = ModalChoiceImportance.Important;
            Assert.AreEqual(ModalChoiceImportance.Important, m_context.Importance);
        }

        [Test]
        public void Test_IsSerializable()
        {
            Assert.IsSerializable(m_context);
        }

        [Test]
        public void Test_YesNo_returns_an_appropriate_ModalChoiceContext()
        {
            ModalChoiceContext context = ModalChoiceContext.YesNo("Hello",  ModalChoiceResult.Yes);
            Assert.AreEqual("Hello", context.Question);
            Assert.AreEqual(ModalChoiceImportance.Important, context.Importance);
            Assert.AreEqual(ModalChoiceResult.Yes, context.DefaultChoice);
            Assert.Collections.AreEqual(new[] { ModalChoiceResult.Yes, ModalChoiceResult.No }, context.Choices);

            context = ModalChoiceContext.YesNo("Hello", ModalChoiceResult.No, ModalChoiceImportance.Trivial);
            Assert.AreEqual("Hello", context.Question);
            Assert.AreEqual(ModalChoiceImportance.Trivial, context.Importance);
            Assert.AreEqual(ModalChoiceResult.No, context.DefaultChoice);
            Assert.Collections.AreEqual(new[] { ModalChoiceResult.No, ModalChoiceResult.Yes }, context.Choices);
        }

        #endregion
    }
}

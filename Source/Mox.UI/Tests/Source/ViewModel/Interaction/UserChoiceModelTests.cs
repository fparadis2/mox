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

namespace Mox.UI
{
    [TestFixture]
    public class UserChoiceModelTests
    {
        #region Variables

        private UserChoiceModel m_userChoiceModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_userChoiceModel = new UserChoiceModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_Text()
        {
            Assert.AreEqual(null, m_userChoiceModel.Text);
            m_userChoiceModel.Text = "New Value";
            Assert.AreEqual("New Value", m_userChoiceModel.Text);
        }

        [Test]
        public void Test_Can_get_set_Type()
        {
            Assert.AreEqual(UserChoiceType.None, m_userChoiceModel.Type);
            m_userChoiceModel.Type = UserChoiceType.No;
            Assert.AreEqual(UserChoiceType.No, m_userChoiceModel.Type);
        }

        #endregion
    }
}

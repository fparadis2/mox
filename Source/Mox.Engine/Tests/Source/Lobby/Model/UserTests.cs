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

namespace Mox.Lobby2
{
    [TestFixture]
    public class UserTests
    {
        #region Variables

        private User m_user;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_user = new User("MyName");
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new User(null); });
            Assert.Throws<ArgumentNullException>(delegate { new User(string.Empty); });
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual("MyName", m_user.Name);
            Assert.IsFalse(m_user.IsAI);
        }

        [Test]
        public void Test_Equality_and_serialization()
        {
            User other = Assert.IsSerializable(m_user);
            Assert.AreCompletelyEqual(m_user, other);
        }

        [Test]
        public void Test_Can_get_AI_Users()
        {
            User aiUser = User.CreateAIUser();
            Assert.That(aiUser.IsAI);
        }

        #endregion
    }
}

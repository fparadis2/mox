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

namespace Mox.Network
{
    [TestFixture]
    public class ClientTests
    {
        #region Variables

        private Client m_client;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_client = new Client("MyName");
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new Client(null); });
            Assert.Throws<ArgumentNullException>(delegate { new Client(string.Empty); });
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual("MyName", m_client.Name);
        }

        [Test]
        public void Test_ToString()
        {
            Assert.AreEqual("[Client: MyName]", m_client.ToString());
        }

        #endregion
    }
}

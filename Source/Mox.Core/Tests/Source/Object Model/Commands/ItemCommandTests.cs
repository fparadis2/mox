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
using Rhino.Mocks;

namespace Mox.Transactions
{
    [TestFixture]
    public class ItemCommandTests
    {
        #region Variables

        private MockRepository m_mockery;

        private ItemCommand<object> m_command;
        private object m_object;

        #endregion

        #region Constructor

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_object = new object();
            m_command = m_mockery.StrictMock<ItemCommand<object>>(m_object);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreSame(m_object, m_command.Item);
        }

        #endregion
    }
}
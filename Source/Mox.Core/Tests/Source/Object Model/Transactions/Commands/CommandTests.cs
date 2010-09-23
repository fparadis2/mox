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
    public class CommandTests
    {
        #region Variables

        private MockRepository m_mockery;

        private Command m_command;
        private ISynchronizationContext m_syncContext;

        #endregion

        #region Constructor

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_command = m_mockery.PartialMock<Command>();
            m_syncContext = m_mockery.StrictMock<ISynchronizationContext>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_IsEmpty_is_false_by_default()
        {
            m_mockery.Test(() => Assert.IsFalse(m_command.IsEmpty));
        }

        [Test]
        public void Test_Dispose_does_nothing()
        {
            m_mockery.Test(() => m_command.Dispose());
        }

        #endregion
    }
}
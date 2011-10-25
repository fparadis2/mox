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
using Rhino.Mocks;

namespace Mox.Transactions
{
    [TestFixture]
    public class ReverseCommandTests
    {
        #region Variables

        private MockRepository m_mockery;

        private ObjectManager m_manager;
        private ICommand m_command;
        private ReverseCommand m_reverseCommand;

        #endregion

        #region Constructor

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_manager = new ObjectManager();
            m_command = m_mockery.StrictMock<ICommand>();
            m_reverseCommand = new ReverseCommand(m_command);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_IsEmpty_returns_the_same_value_as_inner_command()
        {
            Expect.Call(m_command.IsEmpty).Return(true);
            m_mockery.Test(() => Assert.That(m_reverseCommand.IsEmpty));
        }

        [Test]
        public void Test_Execute_unexecutes_the_inner_command()
        {
            m_command.Unexecute(m_manager);

            m_mockery.Test(() => m_reverseCommand.Execute(m_manager));
        }

        [Test]
        public void Test_Unexecute_executes_the_inner_command()
        {
            m_command.Execute(m_manager);

            m_mockery.Test(() => m_reverseCommand.Unexecute(m_manager));
        }

        #endregion
    }
}
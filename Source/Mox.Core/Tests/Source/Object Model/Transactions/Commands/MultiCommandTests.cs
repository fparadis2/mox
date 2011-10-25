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
    public class MultiCommandTests
    {
        #region Variables

        private MockRepository m_mockery;
        private ObjectManager m_manager;

        private MultiCommand m_multiCommand;

        private ICommand m_command1;
        private ICommand m_command2;

        private ISynchronizationContext m_syncContext;

        #endregion

        #region Constructor

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_manager = new MockObjectManager();

            m_multiCommand = new MultiCommand();

            m_command1 = m_mockery.StrictMock<ICommand>();
            m_command2 = m_mockery.StrictMock<ICommand>();

            m_multiCommand.Push(m_command1);
            m_multiCommand.Push(m_command2);

            m_syncContext = m_mockery.StrictMock<ISynchronizationContext>();
        }

        #endregion

        #region Utilities

        #endregion

        #region Tests

        [Test]
        public void Test_Cannot_push_a_null_command()
        {
            Assert.Throws<ArgumentNullException>(() => m_multiCommand.Push(null));
        }

        [Test]
        public void Test_Executing_the_command_executes_all_pushed_commands_in_order()
        {
            using (m_mockery.Ordered())
            {
                m_command1.Execute(m_manager);
                m_command2.Execute(m_manager);
            }

            m_mockery.Test(() => m_multiCommand.Execute(m_manager));
        }

        [Test]
        public void Test_Unexecuting_the_command_unexecutes_all_pushed_commands_in_reverse_order()
        {
            using (m_mockery.Ordered())
            {
                m_command2.Unexecute(m_manager);
                m_command1.Unexecute(m_manager);
            }

            m_mockery.Test(() => m_multiCommand.Unexecute(m_manager));
        }

        [Test]
        public void Test_the_command_is_empty_if_all_pushed_commands_are_empty()
        {
            Expect.Call(m_command1.IsEmpty).Return(true).Repeat.AtLeastOnce();
            Expect.Call(m_command2.IsEmpty).Return(true).Repeat.AtLeastOnce();

            m_mockery.Test(() => Assert.IsTrue(m_multiCommand.IsEmpty));
        }

        [Test]
        public void Test_the_command_is_not_empty_if_at_least_one_pushed_command_is_not_empty()
        {
            Expect.Call(m_command1.IsEmpty).Return(true).Repeat.Any();
            Expect.Call(m_command2.IsEmpty).Return(false).Repeat.AtLeastOnce();

            m_mockery.Test(() => Assert.IsFalse(m_multiCommand.IsEmpty));
        }

        [Test]
        public void Test_Commands_returns_the_commands_in_the_multi_command()
        {
            Assert.Collections.AreEqual(new[] { m_command1, m_command2 }, m_multiCommand.Commands);
            Assert.IsTrue(m_multiCommand.Commands.IsReadOnly);
            Assert.AreEqual(2, m_multiCommand.CommandCount);
        }

        [Test]
        public void Test_Is_always_public_and_has_no_associated_object()
        {
            Assert.IsNull(m_multiCommand.GetObject(m_manager));
            Assert.IsTrue(m_multiCommand.IsPublic);
        }

        [Test]
        public void Test_Synchronize_synchronizes_each_sub_command()
        {
            using (m_mockery.Ordered())
            {
                m_syncContext.Synchronize(m_command1);
                m_syncContext.Synchronize(m_command2);
            }

            m_mockery.Test(() => Assert.IsNull(m_multiCommand.Synchronize(m_syncContext)));
        }

        #endregion
    }
}
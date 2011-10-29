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
using Mox.Transactions;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Replication
{
    [TestFixture]
    public class CommandSynchronizerTests
    {
        #region Constants

        private const string UserKey = "TheUser";

        #endregion

        #region Inner Types

        private class MyObject : Object
        {
        }

        #endregion

        #region Variables

        private MockRepository m_mockery;

        private ObjectManager m_manager;
        private Object m_object;
        private CommandSynchronizer<string> m_synchronizer;
        private IAccessControlStrategy<string> m_accessControlStrategy;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_manager = new ObjectManager();
            m_object = m_manager.Create<MyObject>();
            m_accessControlStrategy = m_mockery.StrictMock<IAccessControlStrategy<string>>();
            m_synchronizer = new CommandSynchronizer<string>(m_manager, m_accessControlStrategy, UserKey);
        }

        #endregion

        #region Utilities

        private ICommand CreateCommand()
        {
            return m_mockery.StrictMock<ICommand>();
        }

        private ICommand CreateSynchronizableCommand()
        {
            return m_mockery.StrictMultiMock<ICommand>(typeof(ISynchronizableCommand));
        }

        private void Assert_PrepareImmediateSynchronization(IEnumerable<ICommand> expected, ICommand commandToSynchronize)
        {
            m_mockery.Test(() => Assert.Collections.AreEqual(expected, Flatten(new[] { m_synchronizer.PrepareImmediateSynchronization(commandToSynchronize) })));
        }

        private void Assert_PrepareDelayedSynchronization(Object obj, params ICommand[] commands)
        {
            Assert.Collections.AreEqual(commands, Flatten(new[] { m_synchronizer.PrepareDelayedSynchronization(obj) }));
        }

        private static IEnumerable<ICommand> Flatten(IEnumerable<ICommand> commands)
        {
            foreach (ICommand command in commands)
            {
                if (command is MultiCommand)
                {
                    foreach (ICommand subCommand in Flatten(((MultiCommand)command).Commands))
                    {
                        yield return subCommand;
                    }
                }
                else if (command != null)
                {
                    yield return command;
                }
            }
        }

        private static void Expect_IsPublic(ICommand command, bool result)
        {
            Expect.Call(((ISynchronizableCommand)command).IsPublic).Return(result);
        }

        private static void Expect_Synchronize(ICommand command, ICommand result)
        {
            Expect.Call(((ISynchronizableCommand)command).Synchronize()).Return(result);
        }

        private void Expect_GetObject(ICommand command, Object theObject)
        {
            Expect.Call(((ISynchronizableCommand)command).GetObject(m_manager)).Return(theObject);
        }

        private void Expect_GetUserAccess(Object theObject, string user, UserAccess access)
        {
            Expect.Call(m_accessControlStrategy.GetUserAccess(user, theObject)).Return(access);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Synchronize_returns_null_if_there_is_no_commands_to_synchronize()
        {
            Assert.IsNull(m_synchronizer.PrepareImmediateSynchronization(null));
        }

        [Test]
        public void Test_Synchronize_returns_a_multi_command_containing_the_simple_commands_given_during_synchronization()
        {
            ICommand command1 = CreateCommand();
            ICommand command2 = CreateCommand();

            Assert_PrepareImmediateSynchronization(new[] { command1, command2 }, new MultiCommand { command1, command2 });
        }

        [Test]
        public void Test_SynchronizableCommands_are_synchronized_if_they_are_public()
        {
            ICommand command = CreateSynchronizableCommand();
            ICommand result = CreateCommand();

            Expect_IsPublic(command, true);
            Expect_Synchronize(command, result);

            Assert_PrepareImmediateSynchronization(new[] { result }, command);
        }

        [Test]
        public void Test_SynchronizableCommands_are_synchronized_if_they_are_visible()
        {
            ICommand command = CreateSynchronizableCommand();
            ICommand result = CreateCommand();

            Expect_IsPublic(command, false);
            Expect_GetObject(command, m_object);
            Expect_GetUserAccess(m_object, UserKey, UserAccess.Read);
            Expect_Synchronize(command, result);

            Assert_PrepareImmediateSynchronization(new[] { result }, command);
        }

        [Test]
        public void Test_SynchronizableCommands_are_delay_synchronized_if_they_are_not_visible_and_private()
        {
            ICommand command = CreateSynchronizableCommand();
            ICommand result = CreateCommand();

            Expect_IsPublic(command, false);
            Expect_GetObject(command, m_object);
            Expect_GetUserAccess(m_object, UserKey, UserAccess.None);
            Expect_Synchronize(command, result);

            Assert_PrepareImmediateSynchronization(new ICommand[0], command);

            Assert_PrepareDelayedSynchronization(m_object, result);
            Assert_PrepareDelayedSynchronization(m_object); // Make sure the command is only returned once.
        }

        [Test]
        public void Test_SynchronizableCommands_are_considered_visible_if_they_have_no_object()
        {
            ICommand command = CreateSynchronizableCommand();
            ICommand result = CreateCommand();

            Expect_IsPublic(command, false);
            Expect_GetObject(command, null);
            Expect_Synchronize(command, result);

            Assert_PrepareImmediateSynchronization(new[] { result }, command);
        }

        #endregion
    }
}

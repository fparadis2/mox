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
using System.Diagnostics;
using System.Linq;
using Mox.Transactions;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Interfaces;

namespace Mox.Replication
{
    [TestFixture]
    public class CommandSynchronizerTests : BaseGameTests
    {
        #region Variables

        private CommandSynchronizer m_synchronizer;
        private IVisibilityStrategy m_visibilityStrategy;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_visibilityStrategy = m_mockery.StrictMock<IVisibilityStrategy>();
            m_synchronizer = new CommandSynchronizer();
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

        private void Assert_Synchronize(IEnumerable<ICommand> expected, IEnumerable<ICommand> commandsToSynchronize)
        {
            m_mockery.Test(() => Assert.Collections.AreEqual(expected, Flatten(new[] {m_synchronizer.Synchronize(m_game, m_visibilityStrategy, m_playerA, commandsToSynchronize)})));
        }

        private void Assert_Update(Object obj, params ICommand[] commands)
        {
            Assert.Collections.AreEqual(commands, Flatten(new[] { m_synchronizer.Update(obj) }));
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

        private static void Expect_Synchronize(ICommand command, ICommand result, params ICommand[] additionalCommands)
        {
            Expect.Call(((ISynchronizableCommand)command).Synchronize(null)).IgnoreArguments().Callback<ISynchronizationContext>(context =>
            {
                additionalCommands.ForEach(context.Synchronize);
                return true;
            }).Return(result);
        }

        private void Expect_GetObject(ICommand command, Object theObject)
        {
            Expect.Call(((ISynchronizableCommand)command).GetObject(m_game)).Return(theObject);
        }

        private void Expect_IsVisible(Object theObject, Player player, bool result)
        {
            Expect.Call(m_visibilityStrategy.IsVisible(theObject, player)).Return(result);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Synchronize_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_synchronizer.Synchronize(null, m_visibilityStrategy, m_playerA, new ICommand[0]));
            Assert.Throws<ArgumentNullException>(() => m_synchronizer.Synchronize(m_game, null, m_playerA, new ICommand[0]));
        }

        [Test]
        public void Test_Synchronize_returns_null_if_there_is_no_commands_to_synchronize()
        {
            Assert.IsNull(m_synchronizer.Synchronize(m_game, m_visibilityStrategy, m_playerA, null));
            Assert.IsNull(m_synchronizer.Synchronize(m_game, m_visibilityStrategy, m_playerA, new ICommand[0]));
        }

        [Test]
        public void Test_Synchronize_returns_a_multi_command_containing_the_simple_commands_given_during_synchronization()
        {
            ICommand command1 = CreateCommand();
            ICommand command2 = CreateCommand();

            Assert_Synchronize(new[] { command1, command2 }, new[] { command1, command2 });
        }

        [Test]
        public void Test_SynchronizableCommands_are_synchronized_if_they_are_public()
        {
            ICommand command = CreateSynchronizableCommand();
            ICommand result = CreateCommand();

            Expect_IsPublic(command, true);
            Expect_Synchronize(command, result);

            Assert_Synchronize(new[] { result }, new[] { command });
        }

        [Test]
        public void Test_SynchronizableCommands_are_synchronized_if_they_are_visible()
        {
            ICommand command = CreateSynchronizableCommand();
            ICommand result = CreateCommand();

            Expect_IsPublic(command, false);
            Expect_GetObject(command, m_card);
            Expect_IsVisible(m_card, m_playerA, true);
            Expect_Synchronize(command, result);

            Assert_Synchronize(new[] { result }, new[] { command });
        }

        [Test]
        public void Test_SynchronizableCommands_are_delay_synchronized_if_they_are_not_visible_and_private()
        {
            ICommand command = CreateSynchronizableCommand();
            ICommand result = CreateCommand();

            Expect_IsPublic(command, false);
            Expect_GetObject(command, m_card);
            Expect_IsVisible(m_card, m_playerA, false);
            Expect_Synchronize(command, result);

            Assert_Synchronize(new ICommand[0], new[] { command });

            Assert_Update(m_card, result);
            Assert_Update(m_card); // Make sure the command is only returned once.
        }

        [Test]
        public void Test_SynchronizableCommands_are_considered_visible_if_they_have_no_object()
        {
            ICommand command = CreateSynchronizableCommand();
            ICommand result = CreateCommand();

            Expect_IsPublic(command, false);
            Expect_GetObject(command, null);
            Expect_Synchronize(command, result);

            Assert_Synchronize(new[] { result }, new[] { command });
        }

        [Test]
        public void Test_SynchronizableCommands_can_register_sub_commands_in_the_context()
        {
            ICommand command = CreateSynchronizableCommand();
            ICommand result = CreateCommand();

            ICommand subcommand1 = CreateSynchronizableCommand();
            ICommand subcommandResult1 = CreateCommand();

            ICommand subcommand2 = CreateSynchronizableCommand();
            ICommand subcommandResult2 = CreateCommand();

            Expect_IsPublic(command, true);
            Expect_Synchronize(command, result, subcommand1, subcommand2);
            Expect_IsPublic(subcommand1, true);
            Expect_Synchronize(subcommand1, subcommandResult1);
            Expect_IsPublic(subcommand2, true);
            Expect_Synchronize(subcommand2, subcommandResult2);

            Assert_Synchronize(new[] { subcommandResult1, subcommandResult2, result }, new[] { command });
        }

        #endregion
    }
}

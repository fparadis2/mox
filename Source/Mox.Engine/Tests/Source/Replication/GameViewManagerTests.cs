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
    public class GameViewManagerTests : BaseGameTests
    {
        #region Inner Types

        public abstract class MockVisibilityStrategy : IVisibilityStrategy
        {
            #region Implementation of IVisibilityStrategy

            public abstract void Dispose();

            public virtual bool IsVisible(Object gameObject, Player player)
            {
                return false;
            }

            public event EventHandler<VisibilityChangedEventArgs> ObjectVisibilityChanged;

            public void OnObjectVisibilityChanged(VisibilityChangedEventArgs e)
            {
                ObjectVisibilityChanged.Raise(this, e);
            }

            #endregion
        }

        #endregion

        #region Variables

        private MockRepository m_commandMockery;

        private GameViewManager m_viewManager;
        private TransactionStack m_transactionStack;
        private MockVisibilityStrategy m_visibilityStrategy;
        private ICommandSynchronizer m_commandSynchronizer;

        private IGameListener m_listener;
        

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_commandMockery = new MockRepository();
            m_visibilityStrategy = m_mockery.PartialMock<MockVisibilityStrategy>();
            m_transactionStack = new TransactionStack(m_game);
            m_commandSynchronizer = m_mockery.StrictMock<ICommandSynchronizer>();
            m_viewManager = new GameViewManager(m_game, m_transactionStack, m_visibilityStrategy, m_commandSynchronizer);

            m_listener = m_mockery.StrictMock<IGameListener>();
        }

        public override void Teardown()
        {
            base.Teardown();

            m_commandMockery.VerifyAll();
        }

        #endregion

        #region Utilities

        private ICommand CreateCommand()
        {
            return CreateCommand(false);
        }

        private ICommand CreateCommand(bool empty)
        {
            ICommand command = m_commandMockery.StrictMock<ICommand>();
            SetupResult.For(command.IsEmpty).Return(empty);

            m_commandMockery.Replay(command);
            return command;
        }

        private void PushCommand(ICommand command)
        {
            m_transactionStack.Push(command);
        }

        private void RegisterListener()
        {
            m_mockery.Test(() => m_viewManager.Register(m_listener, m_playerA));
        }

        private IMethodOptions<ICommand> Expect_Synchronize_Command(params ICommand[] commands)
        {
            return Expect.Call(m_commandSynchronizer.Synchronize(null, null, null, null))
                         .IgnoreArguments()
                         .Callback<ObjectManager, IVisibilityStrategy, Player, IEnumerable<ICommand>>((manager, visibilityStrategy, player, enumerable) =>
            {
                Assert.AreEqual(m_game, manager);
                Assert.AreEqual(m_visibilityStrategy, visibilityStrategy);
                Assert.AreEqual(m_playerA, player);
                Assert.Collections.AreEqual(commands, Flatten(enumerable));
                return true;
            });
        }

        private IMethodOptions<ICommand> Expect_Update(Object theObject)
        {
            return Expect.Call(m_commandSynchronizer.Update(theObject));
        }

        private void Expect_Listener_Receives(ICommand command)
        {
            m_listener.Synchronize(command);
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
                else
                {
                    yield return command;
                }
            }
        }

        private void TriggerVisibilityChanged(Object obj, Player player, bool visible)
        {
            m_mockery.Test(() => m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(obj, player, visible)));
        }

        #endregion

        #region Tests

        #region General

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new GameViewManager(null, m_transactionStack, m_visibilityStrategy, m_commandSynchronizer); });
            Assert.Throws<ArgumentNullException>(delegate { new GameViewManager(m_game, null, m_visibilityStrategy, m_commandSynchronizer); });
            Assert.Throws<ArgumentNullException>(delegate { new GameViewManager(m_game, m_transactionStack, null, m_commandSynchronizer); });
            Assert.Throws<ArgumentNullException>(delegate { new GameViewManager(m_game, m_transactionStack, m_visibilityStrategy, null); });
        }

        [Test]
        public void Test_GameViewManager_disposes_its_visibility_strategy_upon_disposal()
        {
            m_visibilityStrategy.Dispose();

            m_mockery.Test(() => m_viewManager.Dispose());
        }

        #endregion

        #region Registration

        [Test]
        public void Test_Can_register_a_listener()
        {
            RegisterListener();
        }

        [Test]
        public void Test_Cannot_register_the_same_listener_twice()
        {
            RegisterListener();

            Assert.Throws<ArgumentException>(RegisterListener);
        }

        [Test]
        public void Test_Cannot_register_a_listener_with_an_invalid_player_identifier()
        {
            Assert.Throws<ArgumentException>(() => m_viewManager.Register(m_listener, new Game().CreatePlayer()));
        }

        #endregion

        #region Command synchronization

        [Test]
        public void Test_When_registering_all_commands_are_synchronized_and_the_result_is_sent_to_the_listener()
        {
            ICommand command1 = CreateCommand();
            ICommand command2 = CreateCommand();
            ICommand resultCommand = CreateCommand();

            PushCommand(command1);
            PushCommand(command2);

            Expect_Synchronize_Command(command1, command2).Return(resultCommand);
            Expect_Listener_Receives(resultCommand);

            RegisterListener();
        }

        [Test]
        public void Test_When_registering_all_commands_are_synchronized_and_the_result_is_not_sent_to_the_listener_if_it_is_empty()
        {
            ICommand command1 = CreateCommand();
            ICommand command2 = CreateCommand();
            ICommand resultCommand = CreateCommand(true);

            PushCommand(command1);
            PushCommand(command2);

            Expect_Synchronize_Command(command1, command2).Return(resultCommand);

            RegisterListener();
        }

        [Test]
        public void Test_When_registering_all_commands_are_synchronized_and_the_result_is_not_sent_to_the_listener_if_it_is_null()
        {
            ICommand command1 = CreateCommand();
            ICommand command2 = CreateCommand();

            PushCommand(command1);
            PushCommand(command2);

            Expect_Synchronize_Command(command1, command2).Return(null);

            RegisterListener();
        }

        [Test]
        public void Test_When_a_command_is_pushed_it_is_synchronized_with_listeners()
        {
            RegisterListener();

            ICommand command1 = CreateCommand();
            ICommand resultCommand = CreateCommand();

            Expect_Synchronize_Command(command1).Return(resultCommand);
            Expect_Listener_Receives(resultCommand);

            m_mockery.Test(() => PushCommand(command1));
        }

        [Test]
        public void Test_When_a_command_is_pushed_it_is_not_synchronized_with_listeners_if_the_result_is_empty()
        {
            RegisterListener();

            ICommand command1 = CreateCommand();
            ICommand resultCommand = CreateCommand(true);

            Expect_Synchronize_Command(command1).Return(resultCommand);

            m_mockery.Test(() => PushCommand(command1));
        }

        [Test]
        public void Test_When_a_command_is_pushed_it_is_not_synchronized_with_listeners_if_the_result_is_null()
        {
            RegisterListener();

            ICommand command1 = CreateCommand();

            Expect_Synchronize_Command(command1).Return(null);

            m_mockery.Test(() => PushCommand(command1));
        }

        #endregion

        #region Delayed command synchronization

        [Test]
        public void Test_When_a_object_becomes_visible_the_command_synchronizer_is_asked_to_update_it_for_the_listener_to_which_it_is_now_visible()
        {
            RegisterListener();

            ICommand command = CreateCommand();

            Expect_Update(m_card).Return(command);
            Expect_Listener_Receives(command);

            TriggerVisibilityChanged(m_card, m_playerA, true);
        }

        [Test]
        public void Test_When_a_object_becomes_visible_the_listener_is_not_synchronized_if_the_command_is_empty()
        {
            RegisterListener();

            ICommand command = CreateCommand(true);

            Expect_Update(m_card).Return(command);

            TriggerVisibilityChanged(m_card, m_playerA, true);
        }

        [Test]
        public void Test_When_a_object_becomes_visible_the_listener_is_not_synchronized_if_there_is_nothing_to_update()
        {
            RegisterListener();

            Expect_Update(m_card).Return(null);

            TriggerVisibilityChanged(m_card, m_playerA, true);
        }

        [Test]
        public void Test_When_a_object_becomes_visible_the_command_synchronizer_is_not_asked_to_update_it_for_listeners_not_involved()
        {
            RegisterListener();

            TriggerVisibilityChanged(m_card, m_playerB, true);
        }

        [Test]
        public void Test_When_a_object_becomes_invisible_nothing_is_updated()
        {
            RegisterListener();

            TriggerVisibilityChanged(m_card, m_playerA, false);
        }

        [Test]
        public void Test_Delayed_synchronization_is_only_received_just_before_normal_commands_could_be_synchronized()
        {
            RegisterListener();
            ICommand command = CreateCommand();

            Expect_Update(m_card).Return(command);
            Expect_Listener_Receives(command);

            ITransaction transaction = m_transactionStack.BeginTransaction(TransactionType.Atomic);
            {
                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(m_card, m_playerA, true));
            }
            m_mockery.Test(transaction.Dispose);
        }

        [Test]
        public void Test_Delayed_synchronization_is_only_received_once()
        {
            RegisterListener();
            ICommand command = CreateCommand();

            Expect_Update(m_card).Return(command);
            Expect_Listener_Receives(command);

            ITransaction transaction = m_transactionStack.BeginTransaction(TransactionType.Atomic);
            {
                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(m_card, m_playerA, true));
            }
            m_mockery.Test(delegate
            {
                transaction.Dispose();

                using (m_transactionStack.BeginTransaction())
                {
                }
            });
        }

        [Test]
        public void Test_Delayed_synchronization_is_not_received_if_cancelled_in_a_transaction()
        {
            RegisterListener();

            ITransaction transaction = m_transactionStack.BeginTransaction(TransactionType.Atomic);
            {
                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(m_card, m_playerA, true));
                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(m_card, m_playerA, false));
            }
            m_mockery.Test(transaction.Dispose);
        }

        [Test]
        public void Test_Delayed_synchronization_is_received_if_cancelled_and_then_happening_again_in_a_transaction()
        {
            RegisterListener();
            ICommand command = CreateCommand();

            Expect_Update(m_card).Return(command);
            Expect_Listener_Receives(command);

            ITransaction transaction = m_transactionStack.BeginTransaction(TransactionType.Atomic);
            {
                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(m_card, m_playerA, true));
                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(m_card, m_playerA, false));
                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(m_card, m_playerA, true));
            }
            m_mockery.Test(transaction.Dispose);
        }

        [Test]
        public void Test_Delayed_synchronization_is_received_only_once()
        {
            RegisterListener();
            ICommand command = CreateCommand();

            Expect_Update(m_card).Return(command);
            Expect_Listener_Receives(command);

            ITransaction transaction = m_transactionStack.BeginTransaction(TransactionType.Atomic);
            {
                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(m_card, m_playerA, true));
                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs(m_card, m_playerA, true));
            }
            m_mockery.Test(transaction.Dispose);
        }

        #endregion

        #region Transactions synchronization

        [Test]
        public void Test_When_a_transaction_is_ended_it_is_synchronized_with_listeners()
        {
            RegisterListener();

            ICommand command1 = CreateCommand();

            Expect_Synchronize_Command(command1).Return(command1);
            Expect_Listener_Receives(command1);

            ITransaction transaction = m_transactionStack.BeginTransaction();
            {
                PushCommand(command1);
            }
            m_mockery.Test(transaction.Dispose);
        }

        [Test]
        public void Test_Commands_in_non_atomic_transactions_are_synchronized_immediately()
        {
            RegisterListener();

            ICommand command1 = CreateCommand();

            using (m_mockery.Ordered())
            {
                m_listener.BeginTransaction(TransactionType.None);
                Expect_Synchronize_Command(command1).Return(command1);
                Expect_Listener_Receives(command1);
                m_listener.EndCurrentTransaction(false);
            }

            m_mockery.Test(delegate
            {
                using (m_transactionStack.BeginTransaction(TransactionType.None))
                {
                    PushCommand(command1);
                }
            });
        }

        #endregion

        #endregion
    }
}

//// Copyright (c) François Paradis
//// This file is part of Mox, a card game simulator.
//// 
//// Mox is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, version 3 of the License.
//// 
//// Mox is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
//using System;
//using System.Collections.Generic;
//using Mox.Transactions;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Rhino.Mocks.Interfaces;

//namespace Mox.Replication
//{
//    [TestFixture]
//    public class ReplicationSourceTests
//    {
//        #region Inner Types

//        public abstract class MockVisibilityStrategy : IVisibilityStrategy<string>
//        {
//            #region Implementation of IVisibilityStrategy

//            public abstract void Dispose();

//            public virtual bool IsVisible(Object gameObject, string key)
//            {
//                return false;
//            }

//            public event EventHandler<VisibilityChangedEventArgs<string>> ObjectVisibilityChanged;

//            public void OnObjectVisibilityChanged(VisibilityChangedEventArgs<string> e)
//            {
//                ObjectVisibilityChanged.Raise(this, e);
//            }

//            #endregion
//        }

//        private class MyObject : Object
//        {
//        }

//        #endregion

//        #region Variables

//        private MockRepository m_mockery;
//        private MockRepository m_commandMockery;

//        private ObjectManager m_manager;
//        private Object m_object;
//        private ReplicationSource<string> m_replicationSource;
//        private MockVisibilityStrategy m_visibilityStrategy;
//        private ICommandSynchronizer<string> m_commandSynchronizer;

//        private IReplicationClient m_client;

//        #endregion

//        #region Setup / Teardown

//        [SetUp]
//        public void Setup()
//        {
//            m_mockery = new MockRepository();
//            m_commandMockery = new MockRepository();

//            m_manager = new ObjectManager();
//            m_object = m_manager.Create<MyObject>();

//            m_visibilityStrategy = m_mockery.PartialMock<MockVisibilityStrategy>();
//            m_commandSynchronizer = m_mockery.StrictMock<ICommandSynchronizer<string>>();
//            m_replicationSource = new ReplicationSource<string>(m_manager, m_visibilityStrategy, m_commandSynchronizer);

//            //m_manager.TransactionStack.ClearUndoStack();
//            m_client = m_mockery.StrictMock<IReplicationClient>();
//        }

//        [TearDown]
//        public void Teardown()
//        {
//            m_commandMockery.VerifyAll();
//        }

//        #endregion

//        #region Utilities

//        private ICommand CreateCommand(bool empty = false)
//        {
//            ICommand command = m_commandMockery.StrictMock<ICommand>();
//            SetupResult.For(command.IsEmpty).Return(empty);

//            m_commandMockery.Replay(command);
//            return command;
//        }

//        private void PushCommand(ICommand command)
//        {
//#warning TODO
//            //m_manager.TransactionStack.Push(command);
//        }

//        private void RegisterListener(string key)
//        {
//            m_mockery.Test(() => m_replicationSource.Register(key, m_client));
//        }

//        private IMethodOptions<ICommand> Expect_Synchronize_Command(string expectedKey, params ICommand[] commands)
//        {
//            return Expect.Call(m_commandSynchronizer.Synchronize(null, null, null, null))
//                         .IgnoreArguments()
//                         .Callback<ObjectManager, IVisibilityStrategy<string>, string, IEnumerable<ICommand>>((manager, visibilityStrategy, key, enumerable) =>
//            {
//                Assert.AreEqual(m_manager, manager);
//                Assert.AreEqual(m_visibilityStrategy, visibilityStrategy);
//                Assert.AreEqual(expectedKey, key);
//                Assert.Collections.AreEqual(commands, Flatten(enumerable));
//                return true;
//            });
//        }

//        private IMethodOptions<ICommand> Expect_Update(Object theObject)
//        {
//            return Expect.Call(m_commandSynchronizer.Update(theObject));
//        }

//        private void Expect_Client_Receives(ICommand command)
//        {
//            m_client.Synchronize(command);
//        }

//        private static IEnumerable<ICommand> Flatten(IEnumerable<ICommand> commands)
//        {
//            foreach (ICommand command in commands)
//            {
//                if (command is MultiCommand)
//                {
//                    foreach (ICommand subCommand in Flatten(((MultiCommand)command).Commands))
//                    {
//                        yield return subCommand;
//                    }
//                }
//                else
//                {
//                    yield return command;
//                }
//            }
//        }

//        private void TriggerVisibilityChanged(Object obj, string key, bool visible)
//        {
//            m_mockery.Test(() => m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(obj, key, visible)));
//        }

//        #endregion

//        #region Tests

//        #region General

//        [Test]
//        public void Test_Invalid_construction_values()
//        {
//            Assert.Throws<ArgumentNullException>(delegate { new ReplicationSource<string>(null, m_visibilityStrategy, m_commandSynchronizer); });
//            Assert.Throws<ArgumentNullException>(delegate { new ReplicationSource<string>(m_manager, null, m_commandSynchronizer); });
//            Assert.Throws<ArgumentNullException>(delegate { new ReplicationSource<string>(m_manager, m_visibilityStrategy, null); });
//        }

//        [Test]
//        public void Test_GameViewManager_disposes_its_visibility_strategy_upon_disposal()
//        {
//            m_visibilityStrategy.Dispose();

//            m_mockery.Test(() => m_replicationSource.Dispose());
//        }

//        #endregion

//        #region Registration

//        [Test]
//        public void Test_Can_register_a_listener()
//        {
//            RegisterListener("MyKey");
//        }

//        [Test]
//        public void Test_Cannot_register_the_same_listener_twice()
//        {
//            RegisterListener("MyKey");

//            Assert.Throws<ArgumentException>(() => RegisterListener("MyKey"));
//        }

//        #endregion

//        #region Command synchronization

//        [Test]
//        public void Test_When_registering_all_commands_are_synchronized_and_the_result_is_sent_to_the_listener()
//        {
//            ICommand command1 = CreateCommand();
//            ICommand command2 = CreateCommand();
//            ICommand resultCommand = CreateCommand();

//            PushCommand(command1);
//            PushCommand(command2);

//            Expect_Synchronize_Command("TheKey", command1, command2).Return(resultCommand);
//            Expect_Client_Receives(resultCommand);

//            RegisterListener("TheKey");
//        }

//        [Test]
//        public void Test_When_registering_all_commands_are_synchronized_and_the_result_is_not_sent_to_the_listener_if_it_is_empty()
//        {
//            ICommand command1 = CreateCommand();
//            ICommand command2 = CreateCommand();
//            ICommand resultCommand = CreateCommand(true);

//            PushCommand(command1);
//            PushCommand(command2);

//            Expect_Synchronize_Command("TheKey", command1, command2).Return(resultCommand);

//            RegisterListener("TheKey");
//        }

//        [Test]
//        public void Test_When_registering_all_commands_are_synchronized_and_the_result_is_not_sent_to_the_listener_if_it_is_null()
//        {
//            ICommand command1 = CreateCommand();
//            ICommand command2 = CreateCommand();

//            PushCommand(command1);
//            PushCommand(command2);

//            Expect_Synchronize_Command("TheKey", command1, command2).Return(null);

//            RegisterListener("TheKey");
//        }

//        [Test]
//        public void Test_When_a_command_is_pushed_it_is_synchronized_with_listeners()
//        {
//            RegisterListener("TheKey");

//            ICommand command1 = CreateCommand();
//            ICommand resultCommand = CreateCommand();

//            Expect_Synchronize_Command("TheKey", command1).Return(resultCommand);
//            Expect_Client_Receives(resultCommand);

//            m_mockery.Test(() => PushCommand(command1));
//        }

//        [Test]
//        public void Test_When_a_command_is_pushed_it_is_not_synchronized_with_listeners_if_the_result_is_empty()
//        {
//            RegisterListener("TheKey");

//            ICommand command1 = CreateCommand();
//            ICommand resultCommand = CreateCommand(true);

//            Expect_Synchronize_Command("TheKey", command1).Return(resultCommand);

//            m_mockery.Test(() => PushCommand(command1));
//        }

//        [Test]
//        public void Test_When_a_command_is_pushed_it_is_not_synchronized_with_listeners_if_the_result_is_null()
//        {
//            RegisterListener("TheKey");

//            ICommand command1 = CreateCommand();

//            Expect_Synchronize_Command("TheKey", command1).Return(null);

//            m_mockery.Test(() => PushCommand(command1));
//        }

//        #endregion

//        #region Delayed command synchronization

//        [Test]
//        public void Test_When_a_object_becomes_visible_the_command_synchronizer_is_asked_to_update_it_for_the_listener_to_which_it_is_now_visible()
//        {
//            RegisterListener("TheKey");

//            ICommand command = CreateCommand();

//            Expect_Update(m_object).Return(command);
//            Expect_Client_Receives(command);

//            TriggerVisibilityChanged(m_object, "TheKey", true);
//        }

//        [Test]
//        public void Test_When_a_object_becomes_visible_the_listener_is_not_synchronized_if_the_command_is_empty()
//        {
//            RegisterListener("TheKey");

//            ICommand command = CreateCommand(true);

//            Expect_Update(m_object).Return(command);

//            TriggerVisibilityChanged(m_object, "TheKey", true);
//        }

//        [Test]
//        public void Test_When_a_object_becomes_visible_the_listener_is_not_synchronized_if_there_is_nothing_to_update()
//        {
//            RegisterListener("TheKey");

//            Expect_Update(m_object).Return(null);

//            TriggerVisibilityChanged(m_object, "TheKey", true);
//        }

//        [Test]
//        public void Test_When_a_object_becomes_visible_the_command_synchronizer_is_not_asked_to_update_it_for_listeners_not_involved()
//        {
//            RegisterListener("TheKey");

//            TriggerVisibilityChanged(m_object, "AnotherKey", true);
//        }

//        [Test]
//        public void Test_When_a_object_becomes_invisible_nothing_is_updated()
//        {
//            RegisterListener("TheKey");

//            TriggerVisibilityChanged(m_object, "TheKey", false);
//        }

//        [Test]
//        public void Test_Delayed_synchronization_is_only_received_just_before_normal_commands_could_be_synchronized()
//        {
//            RegisterListener("TheKey");
//            ICommand command = CreateCommand();

//            Expect_Update(m_object).Return(command);
//            Expect_Client_Receives(command);

//#warning TODO
//            //ITransaction transaction = m_manager.TransactionStack.BeginTransaction(TransactionType.Atomic);
//            {
//                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(m_object, "TheKey", true));
//            }
//            //m_mockery.Test(transaction.Dispose);
//        }

//        [Test]
//        public void Test_Delayed_synchronization_is_only_received_once()
//        {
//            RegisterListener("TheKey");
//            ICommand command = CreateCommand();

//            Expect_Update(m_object).Return(command);
//            Expect_Client_Receives(command);

//            var transactionStack = m_manager.TransactionStack;
//            ITransaction transaction = transactionStack.BeginTransaction(TransactionType.Atomic);
//            {
//                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(m_object, "TheKey", true));
//            }
//            m_mockery.Test(delegate
//            {
//                transaction.Dispose();

//                using (transactionStack.BeginTransaction())
//                {
//                }
//            });
//        }

//        [Test]
//        public void Test_Delayed_synchronization_is_not_received_if_cancelled_in_a_transaction()
//        {
//            RegisterListener("TheKey");

//            ITransaction transaction = m_manager.TransactionStack.BeginTransaction(TransactionType.Atomic);
//            {
//                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(m_object, "TheKey", true));
//                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(m_object, "TheKey", false));
//            }
//            m_mockery.Test(transaction.Dispose);
//        }

//        [Test]
//        public void Test_Delayed_synchronization_is_received_if_cancelled_and_then_happening_again_in_a_transaction()
//        {
//            RegisterListener("TheKey");
//            ICommand command = CreateCommand();

//            Expect_Update(m_object).Return(command);
//            Expect_Client_Receives(command);

//            ITransaction transaction = m_manager.TransactionStack.BeginTransaction(TransactionType.Atomic);
//            {
//                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(m_object, "TheKey", true));
//                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(m_object, "TheKey", false));
//                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(m_object, "TheKey", true));
//            }
//            m_mockery.Test(transaction.Dispose);
//        }

//        [Test]
//        public void Test_Delayed_synchronization_is_received_only_once()
//        {
//            RegisterListener("TheKey");
//            ICommand command = CreateCommand();

//            Expect_Update(m_object).Return(command);
//            Expect_Client_Receives(command);

//            ITransaction transaction = m_manager.TransactionStack.BeginTransaction(TransactionType.Atomic);
//            {
//                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(m_object, "TheKey", true));
//                m_visibilityStrategy.OnObjectVisibilityChanged(new VisibilityChangedEventArgs<string>(m_object, "TheKey", true));
//            }
//            m_mockery.Test(transaction.Dispose);
//        }

//        #endregion

//        #region Transactions synchronization

//        [Test]
//        public void Test_When_a_transaction_is_ended_it_is_synchronized_with_listeners()
//        {
//            RegisterListener("TheKey");

//            ICommand command1 = CreateCommand();

//            Expect_Synchronize_Command("TheKey", command1).Return(command1);
//            Expect_Client_Receives(command1);

//            ITransaction transaction = m_manager.TransactionStack.BeginTransaction();
//            {
//                PushCommand(command1);
//            }
//            m_mockery.Test(transaction.Dispose);
//        }

//        [Test]
//        public void Test_Commands_in_non_atomic_transactions_are_synchronized_immediately()
//        {
//            RegisterListener("TheKey");

//            ICommand command1 = CreateCommand();

//            using (m_mockery.Ordered())
//            {
//                m_client.BeginTransaction(TransactionType.None);
//                Expect_Synchronize_Command("TheKey", command1).Return(command1);
//                Expect_Client_Receives(command1);
//                m_client.EndCurrentTransaction(false);
//            }

//            m_mockery.Test(delegate
//            {
//                using (m_manager.TransactionStack.BeginTransaction(TransactionType.None))
//                {
//                    PushCommand(command1);
//                }
//            });
//        }

//        #endregion

//        #endregion
//    }
//}

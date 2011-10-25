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
using System.Diagnostics;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Transactions
{
    [TestFixture]
    public class TransactionStackTests
    {
        #region Variables

        private MockRepository m_mockery;
        private ObjectManager m_manager;

        private TransactionStack m_transactionStack;

        private ICommand m_command1;
        private ICommand m_command2;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_manager = new ObjectManager();

            m_transactionStack = new TransactionStack(m_manager);

            m_command1 = m_mockery.StrictMock<ICommand>();
            m_command2 = m_mockery.StrictMock<ICommand>();
        }

        #endregion

        #region Utilities

        private void Push(ICommand command)
        {
            Expect.Call(command.IsEmpty).Return(false);

            m_mockery.Test(() => m_transactionStack.Push(command));
        }

        #endregion

        #region Tests

        #region Basic tests

        [Test]
        public void Test_Is_not_in_a_transaction_by_default()
        {
            Assert.IsFalse(m_transactionStack.IsInTransaction);
        }

        [Test]
        public void Test_Can_begin_a_transaction()
        {
            using (ITransaction transaction = m_transactionStack.BeginTransaction())
            {
                Assert.IsNotNull(transaction);
                Assert.IsTrue(m_transactionStack.IsInTransaction);
            }

            Assert.IsFalse(m_transactionStack.IsInTransaction);
        }

        [Test]
        public void Test_Can_begin_multiple_inner_transactions()
        {
            using (m_transactionStack.BeginTransaction())
            using (m_transactionStack.BeginTransaction())
            using (m_transactionStack.BeginTransaction())
            {
                Assert.IsTrue(m_transactionStack.IsInTransaction);
            }

            Assert.IsFalse(m_transactionStack.IsInTransaction);
        }

        [Test, Conditional("DEBUG")]
        public void Test_Cannot_end_a_transaction_that_is_not_the_current_transaction()
        {
            using (ITransaction transaction = m_transactionStack.BeginTransaction())
            using (m_transactionStack.BeginTransaction())
            {
                Assert.Throws<InvalidOperationException>(transaction.Dispose);
            }
        }

        [Test]
        public void Test_Can_dispose_a_transaction_multiple_times()
        {
            using (ITransaction transaction = m_transactionStack.BeginTransaction())
            {
                transaction.Dispose();
                transaction.Dispose();
                Assert.IsFalse(m_transactionStack.IsInTransaction);
            }
        }

        [Test]
        public void Test_Cannot_dispose_the_transaction_stack_if_there_is_an_open_transaction()
        {
            using (m_transactionStack.BeginTransaction())
            {
                Assert.Throws<InvalidOperationException>(() => m_transactionStack.Dispose());
            }
        }

        [Test]
        public void Test_The_UndoStack_is_empty_by_default()
        {
            Assert.Collections.IsEmpty(m_transactionStack.UndoStack);
        }

        [Test]
        public void Test_Pushing_a_command_while_there_is_no_transaction_adds_it_directly_to_the_command_stack()
        {
            Expect.Call(m_command1.IsEmpty).Return(false);

            m_mockery.Test(delegate
            {
                m_transactionStack.Push(m_command1);
                Assert.Collections.Contains(m_command1, m_transactionStack.UndoStack);
            });
        }

        [Test]
        public void Test_Pushing_and_executing_a_command_while_there_is_no_transaction_adds_it_directly_to_the_command_stack()
        {
            Expect.Call(m_command1.IsEmpty).Return(false);
            m_command1.Execute(m_manager);

            m_mockery.Test(delegate
            {
                m_transactionStack.PushAndExecute(m_command1);
                Assert.Collections.Contains(m_command1, m_transactionStack.UndoStack);
            });
        }

        [Test]
        public void Test_The_command_is_not_pushed_if_execution_fails()
        {
            Expect.Call(m_command1.IsEmpty).Return(false);
            m_command1.Execute(m_manager); LastCall.Throw(new Exception());

            m_mockery.Test(delegate
            {
                try
                {
                    m_transactionStack.PushAndExecute(m_command1);
                }
                catch { }
                Assert.Collections.IsEmpty(m_transactionStack.UndoStack);
            });
        }

        [Test]
        public void Test_Pushing_an_empty_command_does_nothing()
        {
            Expect.Call(m_command1.IsEmpty).Return(true);

            m_mockery.Test(delegate
            {
                m_transactionStack.Push(m_command1);
                Assert.Collections.IsEmpty(m_transactionStack.UndoStack);
            });
        }

        [Test]
        public void Test_Pushing_and_executing_an_empty_command_does_nothing()
        {
            Expect.Call(m_command1.IsEmpty).Return(true);

            m_mockery.Test(delegate
            {
                m_transactionStack.PushAndExecute(m_command1);
                Assert.Collections.IsEmpty(m_transactionStack.UndoStack);
            });
        }

        [Test]
        public void Test_Cannot_push_a_null_command()
        {
            Assert.Throws<ArgumentNullException>(() => m_transactionStack.Push(null));
            Assert.Throws<ArgumentNullException>(() => m_transactionStack.PushAndExecute(null));
        }

        [Test]
        public void Test_Pushing_a_command_while_there_is_a_transaction_will_not_push_it_on_the_UndoStack_until_the_end_of_the_transaction()
        {
            Expect.Call(m_command1.IsEmpty).Return(false);

            m_mockery.Test(delegate
            {
                using (m_transactionStack.BeginTransaction())
                {
                    m_transactionStack.Push(m_command1);
                    Assert.Collections.IsEmpty(m_transactionStack.UndoStack);
                }
                Assert.Collections.CountEquals(1, m_transactionStack.UndoStack);
            });
        }

        [Test]
        public void Test_Pushing_and_executing_a_command_while_there_is_a_transaction_will_not_push_it_on_the_UndoStack_until_the_end_of_the_transaction()
        {
            Expect.Call(m_command1.IsEmpty).Return(false);
            m_command1.Execute(m_manager);

            m_mockery.Test(delegate
            {
                using (m_transactionStack.BeginTransaction())
                {
                    m_transactionStack.PushAndExecute(m_command1);
                    Assert.Collections.IsEmpty(m_transactionStack.UndoStack);
                }
                Assert.Collections.CountEquals(1, m_transactionStack.UndoStack);
            });
        }

        [Test]
        public void Test_All_commands_of_a_transaction_are_pushed_as_one_big_command()
        {
            using (m_transactionStack.BeginTransaction())
            {
                Push(m_command1);
                Push(m_command2);
            }
            Assert.Collections.CountEquals(1, m_transactionStack.UndoStack);
        }

        [Test]
        public void Test_Rollbacking_a_transaction_will_unexecute_every_command_in_the_transaction()
        {
            using (ITransaction transaction = m_transactionStack.BeginTransaction())
            {
                Push(m_command1);
                Push(m_command2);

                using (m_mockery.Ordered())
                {
                    m_command2.Unexecute(m_manager);
                    m_command1.Unexecute(m_manager);
                }

                m_mockery.Test(transaction.Rollback);
            }

            Assert.Collections.IsEmpty(m_transactionStack.UndoStack);
        }

        [Test, Conditional("DEBUG")]
        public void Test_Cannot_rollbacking_a_parent_transaction()
        {
            using (ITransaction transaction = m_transactionStack.BeginTransaction())
            using (m_transactionStack.BeginTransaction())
            {
                Assert.Throws<InvalidOperationException>(transaction.Rollback);
            }
        }

        [Test]
        public void Test_Cannot_rollback_on_a_disposed_transaction()
        {
            using (ITransaction transaction = m_transactionStack.BeginTransaction())
            {
                transaction.Dispose();
                Assert.Throws<ObjectDisposedException>(transaction.Rollback);
            }
        }

        [Test]
        public void Test_CommandPushed_is_triggered_when_a_command_is_pushed_when_not_in_transaction()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>(m_transactionStack);

            m_transactionStack.CommandPushed += sink;

            Assert.EventCalledOnce(sink, () => Push(m_command1));

            Assert.AreEqual(m_command1, sink.LastEventArgs.Command);
        }

        [Test]
        public void Test_CommandPushed_is_triggered_when_a_command_is_pushed_when_going_from_atomic_to_non_atomic()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>(m_transactionStack);

            m_transactionStack.CommandPushed += sink;

            ITransaction nonAtomicTransaction = m_transactionStack.BeginTransaction(TransactionType.None);
            {
                ITransaction transaction = m_transactionStack.BeginTransaction();
                {
                    Assert.EventNotCalled(sink, () => Push(m_command1));
                }
                Assert.EventCalledOnce(sink, transaction.Dispose);
            }
            Assert.EventNotCalled(sink, nonAtomicTransaction.Dispose);
        }

        [Test]
        public void Test_IsInAtomicTransaction_returns_false_if_there_is_no_transaction()
        {
            Assert.IsFalse(m_transactionStack.IsInAtomicTransaction);
        }

        [Test]
        public void Test_IsInAtomicTransaction_returns_false_if_there_is_non_atomic_transactions_on_the_stack()
        {
            using (m_transactionStack.BeginTransaction(TransactionType.None))
            using (m_transactionStack.BeginTransaction(TransactionType.None))
            using (m_transactionStack.BeginTransaction(TransactionType.None))
            {
                Assert.IsFalse(m_transactionStack.IsInAtomicTransaction);
            }
        }

        [Test]
        public void Test_IsInAtomicTransaction_returns_true_if_there_is_at_least_on_atomic_transaction_on_the_stack()
        {
            using (m_transactionStack.BeginTransaction(TransactionType.Atomic))
            using (m_transactionStack.BeginTransaction(TransactionType.None))
            using (m_transactionStack.BeginTransaction(TransactionType.Atomic))
            {
                Assert.IsTrue(m_transactionStack.IsInAtomicTransaction);
            }

            using (m_transactionStack.BeginTransaction(TransactionType.None))
            using (m_transactionStack.BeginTransaction(TransactionType.Atomic))
            using (m_transactionStack.BeginTransaction(TransactionType.None))
            {
                Assert.IsTrue(m_transactionStack.IsInAtomicTransaction);
            }
        }

        [Test]
        public void Test_The_TransactionStarted_event_is_triggered_whenever_a_transaction_is_started()
        {
            EventSink<TransactionStartedEventArgs> sink = new EventSink<TransactionStartedEventArgs>(m_transactionStack);

            m_transactionStack.TransactionStarted += sink;

            Assert.EventCalledOnce(sink, () => m_transactionStack.BeginTransaction(TransactionType.None)); Assert.AreEqual(TransactionType.None, sink.LastEventArgs.Type);
            Assert.EventCalledOnce(sink, () => m_transactionStack.BeginTransaction(TransactionType.Atomic)); Assert.AreEqual(TransactionType.Atomic, sink.LastEventArgs.Type);
        }

        [Test]
        public void Test_The_CurrentTransactionEnded_event_is_triggered_whenever_a_transaction_is_ended_normally()
        {
            EventSink<TransactionEndedEventArgs> sink = new EventSink<TransactionEndedEventArgs>(m_transactionStack);

            m_transactionStack.CurrentTransactionEnded += sink;

            ITransaction transaction1 = m_transactionStack.BeginTransaction(TransactionType.None);
            ITransaction transaction2 = m_transactionStack.BeginTransaction(TransactionType.None);

            Assert.EventCalledOnce(sink, transaction2.Dispose); Assert.IsFalse(sink.LastEventArgs.Rollbacked);
            Assert.EventCalledOnce(sink, transaction1.Dispose); Assert.IsFalse(sink.LastEventArgs.Rollbacked);
        }

        [Test]
        public void Test_The_CurrentTransactionEnded_event_is_triggered_whenever_the_current_transaction_is_rolled_back()
        {
            EventSink<TransactionEndedEventArgs> sink = new EventSink<TransactionEndedEventArgs>(m_transactionStack);

            m_transactionStack.CurrentTransactionEnded += sink;

            ITransaction transaction1 = m_transactionStack.BeginTransaction(TransactionType.None);
            ITransaction transaction2 = m_transactionStack.BeginTransaction(TransactionType.None);

            Assert.EventCalledOnce(sink, transaction2.Rollback); Assert.IsTrue(sink.LastEventArgs.Rollbacked);
            Assert.EventCalledOnce(sink, transaction1.Dispose); Assert.IsFalse(sink.LastEventArgs.Rollbacked);
        }

        [Test]
        public void Test_CurrentTransaction_returns_the_current_transaction()
        {
            Assert.IsNull(m_transactionStack.CurrentTransaction);

            using (ITransaction transaction1 = m_transactionStack.BeginTransaction())
            {
                Assert.AreEqual(transaction1, m_transactionStack.CurrentTransaction);
                using (ITransaction transaction2 = m_transactionStack.BeginTransaction())
                {
                    Assert.AreEqual(transaction2, m_transactionStack.CurrentTransaction);
                }
                Assert.AreEqual(transaction1, m_transactionStack.CurrentTransaction);
            }

            Assert.IsNull(m_transactionStack.CurrentTransaction);
        }

        [Test]
        public void Test_No_commands_are_pushed_while_the_stack_is_disabled()
        {
            Expect.Call(m_command1.IsEmpty).Return(false);

            m_mockery.Test(delegate
            {
                using (m_transactionStack.BeginTransaction(TransactionType.DisableStack))
                {
                    m_transactionStack.Push(m_command1);
                }

                Assert.Collections.IsEmpty(m_transactionStack.UndoStack);
            });
        }

        [Test]
        public void Test_No_commands_are_pushed_while_the_stack_is_disabled_but_they_are_still_executed()
        {
            Expect.Call(m_command1.IsEmpty).Return(false);
            m_command1.Execute(m_manager);

            m_mockery.Test(delegate
            {
                using (m_transactionStack.BeginTransaction(TransactionType.DisableStack))
                {
                    m_transactionStack.PushAndExecute(m_command1);
                }

                Assert.Collections.IsEmpty(m_transactionStack.UndoStack);
            });
        }

        [Test]
        public void Test_IsRollbacking_is_true_during_rollback()
        {
            using (m_mockery.Ordered())
            {
                Expect.Call(m_command1.IsEmpty).Return(false);
                m_command1.Execute(m_manager);

                m_command1.Unexecute(m_manager);
                LastCall.Callback<ObjectManager>(man =>
                {
                    Assert.IsTrue(m_transactionStack.IsRollbacking);
                    return true;
                });
            }

            m_mockery.Test(delegate
            {
                Assert.IsFalse(m_transactionStack.IsRollbacking);

                using (ITransaction transaction = m_transactionStack.BeginTransaction(TransactionType.DisableStack))
                {
                    Assert.IsFalse(m_transactionStack.IsRollbacking);

                    m_transactionStack.PushAndExecute(m_command1);

                    Assert.IsFalse(m_transactionStack.IsRollbacking);

                    transaction.Rollback();
                }

                Assert.IsFalse(m_transactionStack.IsRollbacking);
            });
        }

        #endregion

        #region Transient Scope tests

        [Test]
        public void Test_Cannot_create_a_transient_scope_while_one_is_active()
        {
            ITransientScope scope = m_transactionStack.CreateTransientScope();

            using (scope.Use())
            {
                Assert.Throws<InvalidOperationException>(() => m_transactionStack.CreateTransientScope());
            }
        }

        [Test]
        public void Test_During_a_Master_scope_a_rollbacked_user_transaction_is_flagged()
        {
            ITransientScope scope = m_transactionStack.CreateTransientScope();

            using (scope.Use())
            {
                ITransaction transaction = m_transactionStack.BeginTransaction();
                transaction.Rollback();

                Assert.That(scope.TransactionRolledback);
            }
        }

        [Test]
        public void Test_During_a_Master_scope_master_transactions_are_not_really_rolled_back()
        {
            using (m_transactionStack.CreateTransientScope().Use())
            {
                ITransaction transaction = m_transactionStack.BeginTransaction();

                Push(m_command1);
                Push(m_command2);

                m_mockery.Test(transaction.Rollback);
            }
        }

        [Test]
        public void Test_During_a_Master_scope_master_transactions_are_not_really_pushed()
        {
            using (m_transactionStack.CreateTransientScope().Use())
            {
                using (ITransaction transaction = m_transactionStack.BeginTransaction())
                {
                    Assert.AreNotEqual(m_transactionStack.CurrentTransaction, transaction);
                }
            }
        }

        [Test]
        public void Test_During_a_Master_scope_a_rollbacked_master_transaction_is_not_flagged()
        {
            ITransientScope scope = m_transactionStack.CreateTransientScope();

            using (scope.Use())
            {
                using (ITransaction transaction = m_transactionStack.BeginTransaction(TransactionType.Master))
                {
                    transaction.Rollback();
                }

                Assert.IsFalse(scope.TransactionRolledback);
            }
        }

        [Test]
        public void Test_During_a_Master_scope_master_transactions_are_rolled_back_as_normal()
        {
            using (m_transactionStack.CreateTransientScope().Use())
            {
                using (ITransaction transaction = m_transactionStack.BeginTransaction(TransactionType.Master))
                {
                    Push(m_command1);
                    Push(m_command2);

                    m_command2.Unexecute(m_manager);
                    m_command1.Unexecute(m_manager);

                    m_mockery.Test(transaction.Rollback);
                }
            }
        }

        [Test]
        public void Test_During_a_Master_scope_master_transactions_are_pushed_as_normal()
        {
            using (m_transactionStack.CreateTransientScope().Use())
            {
                using (ITransaction transaction = m_transactionStack.BeginTransaction(TransactionType.Master))
                {
                    Assert.AreEqual(m_transactionStack.CurrentTransaction, transaction);
                }
            }
        }

        [Test]
        public void Test_During_a_Master_scope_ending_a_previous_user_transaction_does_nothing()
        {
            ITransientScope scope = m_transactionStack.CreateTransientScope();

            using (ITransaction transaction = m_transactionStack.BeginTransaction())
            {
                using (scope.Use())
                {
                    transaction.Dispose();
                }

                Assert.AreEqual(transaction, m_transactionStack.CurrentTransaction);
            }
        }

        [Test]
        public void Test_During_a_Master_scope_rollbacking_a_previous_user_transaction_does_nothing_except_flag_the_scope()
        {
            ITransientScope scope = m_transactionStack.CreateTransientScope();

            using (ITransaction transaction = m_transactionStack.BeginTransaction())
            {
                using (scope.Use())
                {
                    transaction.Rollback();

                    Assert.That(scope.TransactionRolledback);
                }

                Assert.AreEqual(transaction, m_transactionStack.CurrentTransaction);
            }
        }

        [Test]
        public void Test_During_a_Master_scope_rollbacking_a_user_transaction_doesnt_dispose_it()
        {
            ITransientScope scope = m_transactionStack.CreateTransientScope();

            using (scope.Use())
            {
                ITransaction transaction = m_transactionStack.BeginTransaction();

                using (scope.Use())
                {
                    transaction.Rollback();
                }

                using (scope.Use())
                {
                    transaction.Rollback();
                }
            }
        }

        [Test]
        public void Test_A_master_scope_is_in_a_user_transaction_if_a_user_transaction_has_been_started()
        {
            ITransientScope scope = m_transactionStack.CreateTransientScope();

            Assert.IsFalse(scope.IsInUserTransaction);

            using (scope.Use())
            {
                Assert.IsFalse(scope.IsInUserTransaction);

                using (m_transactionStack.BeginTransaction())
                {
                    Assert.IsTrue(scope.IsInUserTransaction);

                    using (scope.Use())
                    {
                        Assert.IsTrue(scope.IsInUserTransaction);
                    }

                    Assert.IsTrue(scope.IsInUserTransaction);
                }

                Assert.IsFalse(scope.IsInUserTransaction);
            }

            Assert.IsFalse(scope.IsInUserTransaction);
        }

        #endregion

        #region EndActiveUserTransaction

        [Test]
        public void Test_EndActiveUserTransaction_ends_the_active_user_transaction_when_there_is_one()
        {
            using (ITransaction firstTransaction = m_transactionStack.BeginTransaction())
            {
                ITransaction secondTransaction = m_transactionStack.BeginTransaction(TransactionType.None, "Token");

                Assert.AreEqual(secondTransaction, m_transactionStack.CurrentTransaction);

                m_transactionStack.EndActiveUserTransaction(false, "Token");

                Assert.AreEqual(firstTransaction, m_transactionStack.CurrentTransaction);
            }
        }

        [Test]
        public void Test_EndActiveUserTransaction_asserts_that_the_current_transaction_is_a_user_transaction()
        {
            m_transactionStack.BeginTransaction(TransactionType.Master, "Token");
            Assert.Throws<InvalidOperationException>(() => m_transactionStack.EndActiveUserTransaction(false, "Token"));
        }

        [Test]
        public void Test_EndActiveUserTransaction_asserts_that_the_current_transaction_has_the_correct_token()
        {
            m_transactionStack.BeginTransaction(TransactionType.None, "Token");
            Assert.Throws<InvalidOperationException>(() => m_transactionStack.EndActiveUserTransaction(false, "AnotherToken"));
        }

        [Test]
        public void Test_EndActiveUserTransaction_asserts_that_there_is_a_current_transaction()
        {
            Assert.Throws<InvalidOperationException>(() => m_transactionStack.EndActiveUserTransaction(false, "Anything"));
        }

        [Test]
        public void Test_EndActiveUserTransaction_sets_the_TransientScope_Rollbacked_flag_if_in_a_transient_scope()
        {
            ITransientScope scope = m_transactionStack.CreateTransientScope();

            using (scope.Use())
            {
                m_transactionStack.EndActiveUserTransaction(false, "Anything");
                Assert.IsFalse(scope.TransactionRolledback);

                m_transactionStack.EndActiveUserTransaction(true, "Anything");
                Assert.IsTrue(scope.TransactionRolledback);
            }
        }

        #endregion

        #endregion
    }
}

using System;

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Transactions.Tests
{
    [TestFixture]
    public class ObjectControllerTests
    {
        #region Variables

        private MockRepository m_mockery;
        private ObjectManager m_manager;
        private ObjectController m_controller;

        private ICommand m_command1;
        private ICommand m_command2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_manager = new ObjectManager();
            m_controller = new ObjectController(m_manager);

            m_command1 = m_mockery.StrictMock<ICommand>();
            m_command2 = m_mockery.StrictMock<ICommand>();
        }

        #endregion

        #region Utilities

        private void Expect_Execute_Command(ICommand command, string message)
        {
            Expect.Call(command.IsEmpty).Return(false);
            command.Execute(m_manager);
            LastCall.Message("Executing " + message);
        }

        private void Expect_Unexecute_Command(ICommand command, string message)
        {
            command.Unexecute(m_manager);
            LastCall.Message("Unexecuting " + message);
        }

        private IDisposable BeginTransaction()
        {
            m_controller.BeginTransaction();

            return new DisposableHelper(() => m_controller.EndTransaction(false));
        }

        private IDisposable BeginAndRollbackTransaction()
        {
            m_controller.BeginTransaction();

            return new DisposableHelper(() => m_controller.EndTransaction(true));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Execute_executes_the_command()
        {
            Expect_Execute_Command(m_command1, "Command 1");

            using (m_mockery.Test())
            {
                m_controller.Execute(m_command1);
            }
        }

        [Test]
        public void Test_Empty_commands_are_ignored()
        {
            Expect.Call(m_command1.IsEmpty).Return(true);

            using (m_mockery.Test())
            {
                m_controller.Execute(m_command1);
            }
        }

        #region Transactions

        [Test]
        public void Test_EndTransaction_throws_if_token_doesnt_match_BeginTransaction()
        {
            m_controller.BeginTransaction("1");
            m_controller.BeginTransaction("2");
            Assert.Throws<InvalidOperationException>(() => m_controller.EndTransaction(false, "1"));
        }
        
        [Test]
        public void Test_Commands_are_still_executed_while_in_a_transaction()
        {
            Expect_Execute_Command(m_command1, "Command 1");

            using (m_mockery.Test())
            {
                using (BeginTransaction())
                {
                    m_controller.Execute(m_command1);
                }
            }
        }

        [Test]
        public void Test_Transactions_can_be_nested()
        {
            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1, "Command 1");
                Expect_Execute_Command(m_command2, "Command 2");
            }

            using (m_mockery.Test())
            {
                using (BeginTransaction())
                {
                    using (BeginTransaction())
                    {
                        m_controller.Execute(m_command1);
                    }

                    m_controller.Execute(m_command2);
                }
            }
        }

        [Test]
        public void Test_Transactions_can_be_rolled_back()
        {
            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1, "Command 1");
                Expect_Execute_Command(m_command2, "Command 2");

                Expect_Unexecute_Command(m_command2, "Command 2");
                Expect_Unexecute_Command(m_command1, "Command 1");
            }

            using (m_mockery.Test())
            {
                using (BeginAndRollbackTransaction())
                {
                    m_controller.Execute(m_command1);
                    m_controller.Execute(m_command2);
                }
            }
        }

        [Test]
        public void Test_Nested_transactions_are_also_rolled_back()
        {
            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1, "Command 1");
                Expect_Execute_Command(m_command2, "Command 2");

                Expect_Unexecute_Command(m_command2, "Command 2");
                Expect_Unexecute_Command(m_command1, "Command 1");
            }

            using (m_mockery.Test())
            {
                using (BeginAndRollbackTransaction())
                {
                    using (BeginTransaction())
                    {
                        m_controller.Execute(m_command1);
                        m_controller.Execute(m_command2);
                    }
                }
            }
        }

        [Test]
        public void Test_Rollbacking_a_parent_transaction_and_a_nested_transaction()
        {
            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1, "Command 1");
                Expect_Execute_Command(m_command2, "Command 2");

                Expect_Unexecute_Command(m_command2, "Command 2");
                Expect_Unexecute_Command(m_command1, "Command 1");
            }

            using (m_mockery.Test())
            {
                using (BeginAndRollbackTransaction())
                {
                    m_controller.Execute(m_command1);

                    using (BeginAndRollbackTransaction())
                    {
                        m_controller.Execute(m_command2);
                    }
                }
            }
        }

        #endregion

        #region Groups

        [Test]
        public void Test_BeginCommandGroup_starts_a_scope()
        {
            using (m_controller.BeginCommandGroup())
            { }
        }

        [Test]
        public void Test_Commands_are_still_executed_while_in_a_group()
        {
            Expect_Execute_Command(m_command1, "Command 1");

            using (m_mockery.Test())
            {
                using (m_controller.BeginCommandGroup())
                {
                    m_controller.Execute(m_command1);
                }
            }
        }

        [Test]
        public void Test_Command_groups_can_be_nested()
        {
            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1, "Command 1");
                Expect_Execute_Command(m_command2, "Command 2");
            }

            using (m_mockery.Test())
            {
                using (m_controller.BeginCommandGroup())
                {
                    using (m_controller.BeginCommandGroup())
                    {
                        m_controller.Execute(m_command1);
                    }

                    m_controller.Execute(m_command2);
                }
            }
        }

        #endregion

        #region Events

        [Test]
        public void Test_CommandExecuted_is_raised_when_a_command_is_executed()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_controller.CommandExecuted += sink;

            Expect_Execute_Command(m_command1, "Command 1");

            using (m_mockery.Test())
            {
                Assert.EventCalledOnce(sink, () => m_controller.Execute(m_command1));
            }

            Assert.AreEqual(m_command1, sink.LastEventArgs.Command);
        }

        [Test]
        public void Test_CommandExecuted_is_raised_during_a_transaction()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_controller.CommandExecuted += sink;

            Expect_Execute_Command(m_command1, "Command 1");
            Expect_Execute_Command(m_command2, "Command 2");

            using (m_mockery.Test())
            {
                using (BeginTransaction())
                {
                    Assert.EventCalledOnce(sink, () => m_controller.Execute(m_command1));
                    Assert.AreEqual(m_command1, sink.LastEventArgs.Command);

                    Assert.EventCalledOnce(sink, () => m_controller.Execute(m_command2));
                    Assert.AreEqual(m_command2, sink.LastEventArgs.Command);
                }
            }
        }

        [Test]
        public void Test_CommandExecuted_is_raised_with_inverse_commands_when_a_transaction_is_rolled_back()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_controller.CommandExecuted += sink;

            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1, "Command 1");
                Expect_Execute_Command(m_command2, "Command 2");

                Expect_Unexecute_Command(m_command2, "Command 2");
                Expect_Unexecute_Command(m_command1, "Command 1");
            }

            using (m_mockery.Test())
            {
                var transaction = BeginAndRollbackTransaction();
                {
                    Assert.EventCalledOnce(sink, () => m_controller.Execute(m_command1));
                    Assert.AreEqual(m_command1, sink.LastEventArgs.Command);

                    Assert.EventCalledOnce(sink, () => m_controller.Execute(m_command2));
                    Assert.AreEqual(m_command2, sink.LastEventArgs.Command);
                }
                Assert.EventCalledOnce(sink, transaction.Dispose);
            }

            using (m_mockery.Ordered())
            {
                Expect_Unexecute_Command(m_command2, "Command 2");
                Expect_Unexecute_Command(m_command1, "Command 1");
            }

            using (m_mockery.Test())
            {
                sink.LastEventArgs.Command.Execute(m_manager);
            }
        }

        [Test]
        public void Test_CommandExecuted_is_raised_with_nested_transaction_being_rolled_back()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_controller.CommandExecuted += sink;

            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1, "Command 1");
                Expect_Execute_Command(m_command2, "Command 2");

                Expect_Unexecute_Command(m_command2, "Command 2");
            }

            using (m_mockery.Test())
            {
                using (BeginTransaction())
                {
                    Assert.EventCalledOnce(sink, () => m_controller.Execute(m_command1));
                    Assert.AreEqual(m_command1, sink.LastEventArgs.Command);

                    var transaction = BeginAndRollbackTransaction();
                    {
                        Assert.EventCalledOnce(sink, () => m_controller.Execute(m_command2));
                        Assert.AreEqual(m_command2, sink.LastEventArgs.Command);
                    }
                    Assert.EventCalledOnce(sink, transaction.Dispose);
                }
            }

            Expect_Unexecute_Command(m_command2, "Command 2");

            using (m_mockery.Test())
            {
                sink.LastEventArgs.Command.Execute(m_manager);
            }
        }

        [Test]
        public void Test_CommandExecuted_is_raised_only_at_the_end_of_a_command_group()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_controller.CommandExecuted += sink;

            Expect_Execute_Command(m_command1, "Command 1");
            Expect_Execute_Command(m_command2, "Command 2");

            using (m_mockery.Test())
            {
                var group = m_controller.BeginCommandGroup();
                {
                    m_controller.Execute(m_command1);
                    m_controller.Execute(m_command2);
                }
                Assert.EventCalledOnce(sink, group.Dispose);
            }

            MultiCommand result = (MultiCommand)sink.LastEventArgs.Command;
            Assert.Collections.AreEqual(new[] { m_command1, m_command2 }, result.Commands);
        }

        [Test]
        public void Test_CommandExecuted_is_raised_at_the_end_of_a_group_nested_in_a_transaction()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_controller.CommandExecuted += sink;

            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1, "Command 1");
                Expect_Execute_Command(m_command2, "Command 2");

                Expect_Unexecute_Command(m_command2, "Command 2");
                Expect_Unexecute_Command(m_command1, "Command 1");
            }

            using (m_mockery.Test())
            {
                var transaction = BeginAndRollbackTransaction();
                {
                    m_controller.Execute(m_command1);

                    var group = m_controller.BeginCommandGroup();
                    {
                        m_controller.Execute(m_command2);
                    }
                    Assert.EventCalledOnce(sink, group.Dispose);
                    Assert.Collections.AreEqual(new[] { m_command2 }, ((MultiCommand)sink.LastEventArgs.Command).Commands);
                }
                Assert.EventCalledOnce(sink, transaction.Dispose);
            }

            using (m_mockery.Ordered())
            {
                Expect_Unexecute_Command(m_command2, "Command 2");
                Expect_Unexecute_Command(m_command1, "Command 1");
            }

            using (m_mockery.Test())
            {
                sink.LastEventArgs.Command.Execute(m_manager);
            }
        }

        [Test]
        public void Test_CommandExecuted_is_not_raised_when_rollbacking_a_transaction_nested_in_a_group()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_controller.CommandExecuted += sink;

            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1, "Command 1");
                Expect_Unexecute_Command(m_command1, "Command 1");
            }

            using (m_mockery.Test())
            {
                using (m_controller.BeginCommandGroup())
                {
                    var transaction = BeginAndRollbackTransaction();
                    {
                        m_controller.Execute(m_command1);
                    }
                    Assert.EventNotCalled(sink, transaction.Dispose);
                }
            }
        }

        #endregion

        #region CreateInitialSynchronizationCommand

        [Test]
        public void Test_CreateInitialSynchronizationCommand_initially_returns_an_empty_command()
        {
            var command = m_controller.CreateInitialSynchronizationCommand();
            Assert.That(command.IsEmpty);
        }

        [Test]
        public void Test_CreateInitialSynchronizationCommand_returns_a_multi_command_with_all_previously_executed_commands()
        {
            Expect_Execute_Command(m_command1, "Command 1");
            Expect_Execute_Command(m_command2, "Command 2");

            using (m_mockery.Test())
            {
                m_controller.Execute(m_command1);
                m_controller.Execute(m_command2);
            }

            MultiCommand command = (MultiCommand)m_controller.CreateInitialSynchronizationCommand();
            Assert.Collections.AreEqual(new[] { m_command1, m_command2 }, command.Commands);
        }

        [Test]
        public void Test_CreateInitialSynchronizationCommand_doesnt_take_pending_transactions_into_account()
        {
            Expect_Execute_Command(m_command1, "Command 1");
            Expect_Execute_Command(m_command2, "Command 2");

            using (m_mockery.Test())
            {
                m_controller.Execute(m_command1);

                using (BeginTransaction())
                {
                    m_controller.Execute(m_command2);

                    MultiCommand command = (MultiCommand)m_controller.CreateInitialSynchronizationCommand();
                    Assert.Collections.AreEqual(new[] { m_command1 }, command.Commands);
                }
            }
        }

        #endregion

        #endregion
    }
}
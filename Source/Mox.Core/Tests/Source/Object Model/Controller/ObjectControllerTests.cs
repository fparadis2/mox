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

        private void Expect_Execute_Command(ICommand command)
        {
            Expect.Call(command.IsEmpty).Return(false);
            command.Execute(m_manager);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Execute_executes_the_command()
        {
            Expect_Execute_Command(m_command1);

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
        public void Test_BeginTransaction_returns_a_transaction()
        {
            using (m_controller.BeginTransaction())
            {}
        }

        [Test]
        public void Test_Commands_are_still_executed_while_in_a_transaction()
        {
            Expect_Execute_Command(m_command1);

            using (m_mockery.Test())
            {
                using (m_controller.BeginTransaction())
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
                Expect_Execute_Command(m_command1);
                Expect_Execute_Command(m_command2);
            }

            using (m_mockery.Test())
            {
                using (m_controller.BeginTransaction())
                {
                    using (m_controller.BeginTransaction())
                    {
                        m_controller.Execute(m_command1);
                    }

                    m_controller.Execute(m_command2);
                }
            }
        }

        [Test]
        public void Test_Commands_can_be_rolled_back()
        {
            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1);
                Expect_Execute_Command(m_command2);

                m_command2.Unexecute(m_manager);
                m_command1.Unexecute(m_manager);
            }

            using (m_mockery.Test())
            {
                using (var transaction = m_controller.BeginTransaction())
                {
                    m_controller.Execute(m_command1);
                    m_controller.Execute(m_command2);

                    transaction.Rollback();
                }
            }
        }

        [Test]
        public void Test_Nested_transactions_are_also_rolled_back()
        {
            using (m_mockery.Ordered())
            {
                Expect_Execute_Command(m_command1);
                Expect_Execute_Command(m_command2);

                m_command2.Unexecute(m_manager);
                m_command1.Unexecute(m_manager);
            }

            using (m_mockery.Test())
            {
                using (var transaction = m_controller.BeginTransaction())
                {
                    using (m_controller.BeginTransaction())
                    {
                        m_controller.Execute(m_command1);
                        m_controller.Execute(m_command2);
                    }

                    transaction.Rollback();
                }
            }
        }

        [Test]
        public void Test_Cannot_execute_commands_once_rolled_back()
        {
            using (var transaction = m_controller.BeginTransaction())
            {
                transaction.Rollback();

                Assert.Throws<InvalidOperationException>(() => m_controller.Execute(m_command1));
            }
        }

        #endregion

        #region Events

        [Test]
        public void Test_CommandExecuted_is_raised_when_a_command_is_executed()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_controller.CommandExecuted += sink;

            Expect_Execute_Command(m_command1);

            using (m_mockery.Test())
            {
                Assert.EventCalledOnce(sink, () => m_controller.Execute(m_command1));
            }

            Assert.AreEqual(m_command1, sink.LastEventArgs.Command);
        }

        [Test]
        public void Test_CommandExecuted_is_raised_only_at_the_end_of_a_transaction()
        {
            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_controller.CommandExecuted += sink;

            Expect_Execute_Command(m_command1);
            Expect_Execute_Command(m_command2);

            using (m_mockery.Test())
            {
                var transaction = m_controller.BeginTransaction();
                {
                    m_controller.Execute(m_command1);
                    m_controller.Execute(m_command2);
                }
                Assert.EventCalledOnce(sink, transaction.Dispose);
            }

            MultiCommand result = (MultiCommand)sink.LastEventArgs.Command;
            Assert.Collections.AreEqual(new[] { m_command1, m_command2 }, result.Commands);
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
            Expect_Execute_Command(m_command1);
            Expect_Execute_Command(m_command2);

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
            Expect_Execute_Command(m_command1);
            Expect_Execute_Command(m_command2);

            using (m_mockery.Test())
            {
                m_controller.Execute(m_command1);

                using (m_controller.BeginTransaction())
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
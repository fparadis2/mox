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

        #region Tests

        [Test]
        public void Test_Execute_executes_the_command()
        {
            m_command1.Execute(m_manager);

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
            m_command1.Execute(m_manager);

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
                m_command1.Execute(m_manager);
                m_command2.Execute(m_manager);
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
                m_command1.Execute(m_manager);
                m_command2.Execute(m_manager);

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
                m_command1.Execute(m_manager);
                m_command2.Execute(m_manager);

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

        #endregion
    }
}

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
using Mox.Transactions;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Replication
{
    [TestFixture]
    public class ReplicationClientTests
    {
        #region Variables

        private MockRepository m_mockery;
        private ReplicationClient<ObjectManager> m_client;
        private ICommand m_command;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_client = new ReplicationClient<ObjectManager>();

            m_command = m_mockery.StrictMock<ICommand>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNotNull(m_client.Host);
        }

        [Test]
        public void Test_Replicate_executes_the_command_on_the_client_manager()
        {
            Expect.Call(m_command.IsEmpty).Return(false);
            m_command.Execute(m_client.Host);

            using (m_mockery.Test())
            {
                m_client.Replicate(m_command);
            }
        }

        [Test]
        public void Test_Replicate_doesnt_trigger_the_CommandExecuted_event()
        {
            Expect.Call(m_command.IsEmpty).Return(false);
            m_command.Execute(m_client.Host);

            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_client.CommandExecuted += sink;

            using (m_mockery.Test())
            {
                Assert.EventNotCalled(sink, () => m_client.Replicate(m_command));
            }
        }

        [Test]
        public void Test_Executing_a_command_on_the_replicated_host_triggers_the_CommandExecuted_event_and_executes_the_command()
        {
            Expect.Call(m_command.IsEmpty).Return(false);
            m_command.Execute(m_client.Host);

            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_client.CommandExecuted += sink;

            using (m_mockery.Test())
            {
                Assert.EventCalledOnce(sink, () => m_client.Host.Controller.Execute(m_command));
            }
        }

        [Test]
        public void Test_Executing_a_commands_on_the_replicated_host_while_in_a_group_executes_the_command_at_the_end_and_sends_the_event_at_the_end()
        {
            Expect.Call(m_command.IsEmpty).Return(false);
            m_command.Execute(m_client.Host);

            EventSink<CommandEventArgs> sink = new EventSink<CommandEventArgs>();
            m_client.CommandExecuted += sink;

            using (m_mockery.Test())
            {
                IDisposable group = m_client.Host.Controller.BeginCommandGroup();
                {
                    m_client.Host.Controller.Execute(m_command);
                }
                Assert.EventCalledOnce(sink, group.Dispose);
            }
        }

        [Test]
        public void Test_Cannot_start_a_transaction_directly_on_the_host_while_replicated()
        {
            using (m_mockery.Test())
            {
                Assert.Throws<InvalidOperationException>(() => m_client.Host.Controller.BeginTransaction("Any"));
            }
        }

        [Test]
        public void Test_Cannot_replicate_commands_while_the_host_has_been_upgraded()
        {
            IObjectController controller = m_mockery.StrictMock<IObjectController>();

            using (m_mockery.Test())
            {
                using (m_client.Host.UpgradeController(controller))
                {
                    Assert.Throws<InvalidProgramException>(() => m_client.Replicate(m_command));
                }
            }
        }

        #endregion
    }
}

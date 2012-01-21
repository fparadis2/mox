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
    public class ReplicationSourceTests
    {
        #region Constants

        private const string User1 = "User1";
        private const string User2 = "User2";

        #endregion

        #region Variables

        private MockRepository m_mockery;
        private MockRepository m_commandMockery;

        private ObjectManager m_manager;
        private Object m_object;
        private Object m_invisibleObject;
        private ReplicationSource<string> m_replicationSource;
        private MockAccessControlStrategy m_accessControlStrategy;

        private MockReplicationClient m_client;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_commandMockery = new MockRepository();

            m_manager = new ObjectManager();
            m_object = m_manager.Create<MyObject>();
            m_invisibleObject = m_manager.Create<MyObject>();

            m_accessControlStrategy = new MockAccessControlStrategy();
            m_accessControlStrategy.ChangeUserAccess(m_invisibleObject, User1, UserAccess.None);
            m_accessControlStrategy.ChangeUserAccess(m_invisibleObject, User2, UserAccess.None);

            m_manager.UpgradeController(new ObjectController(m_manager));
            m_replicationSource = new ReplicationSource<string>(m_manager, m_accessControlStrategy);

            m_client = new MockReplicationClient(m_mockery);
        }

        [TearDown]
        public void Teardown()
        {
            m_commandMockery.VerifyAll();
        }

        #endregion

        #region Utilities

        private ICommand ExecuteCommand()
        {
            var command = new MockCommand();
            m_manager.Controller.Execute(command);
            return command;
        }

        private ICommand ExecutePrivateCommand(Object owner)
        {
            var command = new MockSynchronizableCommand(owner);
            m_manager.Controller.Execute(command);
            return command;
        }

        private void RegisterListener(string user)
        {
            m_mockery.Test(() => m_replicationSource.Register(user, m_client));
        }

        private void Expect_Client_Replicate(params ICommand[] commands)
        {
            m_client.Expect_Replicate(commands);
        }

        private void ChangeUserAccess(Object obj, string user, UserAccess userAccess)
        {
            m_mockery.Test(() => m_accessControlStrategy.ChangeUserAccess(obj, user, userAccess));
        }

        #endregion

        #region Tests

        #region General

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new ReplicationSource<string>(null, m_accessControlStrategy); });
            Assert.Throws<ArgumentNullException>(delegate { new ReplicationSource<string>(m_manager, null); });
        }

        [Test]
        public void Test_ReplicationSource_disposes_its_visibility_strategy_upon_disposal()
        {
            m_accessControlStrategy.Dispose();

            m_mockery.Test(() => m_replicationSource.Dispose());
        }

        #endregion

        #region Registration

        [Test]
        public void Test_Can_register_a_listener()
        {
            RegisterListener(User1);
        }

        [Test]
        public void Test_Cannot_register_the_same_listener_twice()
        {
            RegisterListener(User1);

            Assert.Throws<ArgumentException>(() => RegisterListener(User1));
        }

        #endregion

        #region Command synchronization

        [Test]
        public void Test_When_registering_all_commands_are_synchronized_and_the_result_is_sent_to_the_listener()
        {
            ICommand command1 = ExecuteCommand();
            ICommand command2 = ExecuteCommand();

            Expect_Client_Replicate(command1, command2);

            RegisterListener(User1);
        }

        [Test]
        public void Test_When_registering_only_visible_commands_are_sent()
        {
            ExecutePrivateCommand(m_invisibleObject);
            ICommand command2 = ExecuteCommand();

            Expect_Client_Replicate(command2);

            RegisterListener(User1);
        }

        [Test]
        public void Test_When_a_command_is_pushed_it_is_synchronized_with_listeners()
        {
            RegisterListener(User1);

            ICommand command1 = new MockCommand();

            Expect_Client_Replicate(command1);

            m_mockery.Test(() => m_manager.Controller.Execute(command1));
        }

        [Test]
        public void Test_When_a_command_is_pushed_it_is_not_synchronized_with_listeners_if_it_is_not_visible()
        {
            RegisterListener(User1);

            ICommand command1 = ExecutePrivateCommand(m_invisibleObject);

            m_mockery.Test(() => m_manager.Controller.Execute(command1));
        }

        #endregion

        #region Delayed command synchronization

        [Test]
        public void Test_When_a_object_becomes_visible_all_commands_needed_to_synchronize_it_are_sent()
        {
            RegisterListener(User1);

            ICommand command1 = ExecutePrivateCommand(m_invisibleObject);
            ICommand command2 = ExecutePrivateCommand(m_invisibleObject);

            Expect_Client_Replicate(command1, command2);

            ChangeUserAccess(m_invisibleObject, User1, UserAccess.Read);
        }

        [Test]
        public void Test_When_a_object_becomes_visible_the_listener_is_not_synchronized_if_there_is_nothing_to_update()
        {
            RegisterListener(User1);

            ChangeUserAccess(m_invisibleObject, User1, UserAccess.Read);
        }

        [Test]
        public void Test_When_a_object_becomes_visible_the_command_synchronizer_is_not_asked_to_update_it_for_listeners_not_involved()
        {
            RegisterListener(User1);

            ExecutePrivateCommand(m_invisibleObject);

            ChangeUserAccess(m_invisibleObject, User2, UserAccess.Read);
        }

        [Test]
        public void Test_When_a_object_becomes_invisible_nothing_is_updated()
        {
            RegisterListener(User1);

            ChangeUserAccess(m_object, User1, UserAccess.None);
        }

        [Test]
        public void Test_Delayed_synchronization_is_received_only_once()
        {
            RegisterListener(User1);
            ICommand command = ExecutePrivateCommand(m_invisibleObject);

            Expect_Client_Replicate(command);

            ChangeUserAccess(m_invisibleObject, User1, UserAccess.Read);
            ChangeUserAccess(m_invisibleObject, User1, UserAccess.None);
            ChangeUserAccess(m_invisibleObject, User1, UserAccess.Read);
        }

        [Test]
        public void Test_Delayed_synchronization_is_received_once_per_client()
        {
            var client1 = new MockReplicationClient(m_mockery);
            var client2 = new MockReplicationClient(m_mockery);

            m_replicationSource.Register(User1, client1);
            m_replicationSource.Register(User2, client2);

            ICommand command = ExecutePrivateCommand(m_invisibleObject);

            client1.Expect_Replicate(command);
            client2.Expect_Replicate(command);

            using (m_mockery.Test())
            {
                m_accessControlStrategy.ChangeUserAccess(m_invisibleObject, User1, UserAccess.Read);
                m_accessControlStrategy.ChangeUserAccess(m_invisibleObject, User2, UserAccess.Read);
            }
        }

        [Test]
        public void Test_Delayed_synchronization_is_only_received_by_concerned_clients()
        {
            var client1 = new MockReplicationClient(m_mockery);
            var client2 = new MockReplicationClient(m_mockery);

            m_replicationSource.Register(User1, client1);
            m_replicationSource.Register(User2, client2);

            ICommand command = ExecutePrivateCommand(m_invisibleObject);

            client2.Expect_Replicate(command);

            ChangeUserAccess(m_invisibleObject, User2, UserAccess.Read);
        }

        #endregion

        #region Write access

        [Test]
        public void Test_A_listener_can_make_a_change_on_the_original_host()
        {
            RegisterListener(User1);

            var command = m_mockery.StrictMock<ICommand>();

            command.Execute(m_manager);

            using (m_mockery.Test())
            {
                m_client.Raise_CommandExecuted(command);
            }
        }

        #endregion

        #endregion

        #region Inner Types

        private class MockAccessControlStrategy : IAccessControlStrategy<string>
        {
            #region Variables

            private readonly List<UserAccessState> m_accesses = new List<UserAccessState>();

            #endregion

            #region Implementation of IVisibilityStrategy

            public void Dispose() {}

            public UserAccess GetUserAccess(string user, Object theObject)
            {
                foreach (UserAccessState state in m_accesses)
                {
                    if (state.User == user && state.Object == theObject.Identifier)
                    {
                        return state.Access;
                    }
                }

                return UserAccess.All;
            }

            public event EventHandler<UserAccessChangedEventArgs<string>> UserAccessChanged;

            private void OnUserAccessChanged(UserAccessChangedEventArgs<string> e)
            {
                UserAccessChanged.Raise(this, e);
            }

            public void ChangeUserAccess(Object theObject, string user, UserAccess userAccess)
            {
                Throw.IfEmpty(user, "user");

                m_accesses.RemoveAll(u => u.Object == theObject.Identifier && u.User == user);
                m_accesses.Add(new UserAccessState(theObject, user, userAccess));
                OnUserAccessChanged(new UserAccessChangedEventArgs<string>(theObject, user, userAccess));
            }

            #endregion

            #region Inner Types

            private class UserAccessState
            {
                public readonly int Object;
                public readonly string User;
                public readonly UserAccess Access;

                public UserAccessState(Object theObject, string user, UserAccess access)
                {
                    Object = theObject.Identifier;
                    Access = access;
                    User = user;
                }
            }
		 
        	#endregion
        }

        private class MockCommand : ICommand
        {
            #region Implementation of ICommand

            public bool IsEmpty
            {
                get { return false; }
            }

            public void Execute(ObjectManager objectManager)
            {
            }

            public void Unexecute(ObjectManager objectManager)
            {
            }

            #endregion
        }

        private class MockSynchronizableCommand : MockCommand, ISynchronizableCommand
        {
            #region Variables

            private readonly int m_objectIdentifier;

            public MockSynchronizableCommand(Object theObject)
            {
                m_objectIdentifier = theObject.Identifier;
            }

            #endregion

            #region Constructor

            #endregion

            #region Implementation of ISynchronizableCommand

            public bool IsPublic
            {
                get { return false; }
            }

            public Object GetObject(ObjectManager objectManager)
            {
                return objectManager.GetObjectByIdentifier<Object>(m_objectIdentifier);
            }

            public ICommand Synchronize()
            {
                return this;
            }

            #endregion
        }

        private class MyObject : Object
        {
        }

        private class MockReplicationClient : IReplicationClient
        {
            private readonly MockRepository m_mockery;
            private readonly IReplicationClient m_mockClient;

            public MockReplicationClient(MockRepository mockery)
            {
                m_mockery = mockery;
                m_mockClient = mockery.StrictMock<IReplicationClient>();
            }

            #region Implementation of IReplicationClient

            public void Replicate(ICommand command)
            {
                m_mockClient.Replicate(command);
            }

            public event EventHandler<CommandEventArgs> CommandExecuted;

            #endregion

            #region Utilities

            public void Expect_Replicate(params ICommand[] commands)
            {
                using (m_mockery.Ordered())
                {
                    m_mockClient.Replicate(null);
                    LastCall.IgnoreArguments().Callback<ICommand>(actual =>
                    {
                        Assert.Collections.AreEqual(Flatten(commands), Flatten(new[] { actual }));
                        return true;
                    });
                }
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

            public void Raise_CommandExecuted(ICommand command)
            {
                CommandExecuted.Raise(this, new CommandEventArgs(command));
            }

            #endregion
        }

        #endregion
    }
}

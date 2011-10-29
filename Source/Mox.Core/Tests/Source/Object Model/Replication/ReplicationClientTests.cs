﻿// Copyright (c) François Paradis
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
using System.IO;
using System.Linq;
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
        public void Test_Cannot_execute_a_command_directly_on_the_host_while_replicated()
        {
            using (m_mockery.Test())
            {
                Assert.Throws<InvalidOperationException>(() => m_client.Host.Controller.Execute(m_command));
            }
        }

        [Test]
        public void Test_Cannot_start_a_transaction_directly_on_the_host_while_replicated()
        {
            using (m_mockery.Test())
            {
                Assert.Throws<InvalidOperationException>(() => m_client.Host.Controller.BeginTransaction());
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

    [TestFixture]
    public class ReplicationClientTests_Old
    {
        #region Inner Types

        private class MyObject : Object
        {
            public static readonly Property<int> Property = Property<int>.RegisterProperty("Property", typeof(MyObject), PropertyFlags.Modifiable | PropertyFlags.Private);

            public int PropertyValue
            {
                get { return GetValue(Property); }
                set { SetValue(Property, value); }
            }
        }

        private class MyDerivedObject : MyObject
        {
        }

        private class MyObjectManager : ObjectManager
        {
            public TEffectInstance CreateGlobalEffect<TEffectInstance>(EffectBase effect)
                where TEffectInstance : EffectInstance, new()
            {
                return CreateEffect<TEffectInstance>(effect, null, ei => { });
            }

            public MyObject CreateObject(int value)
            {
                var obj = Create<MyObject>();
                SetObjectValue(obj, MyObject.Property, value);
                Objects.Add(obj);
                return obj;
            }
        }

        private class MockAccessControlStrategy : IAccessControlStrategy<string>
        {
            #region Variables

            private readonly ObjectManager m_manager;
            private readonly HashSet<string> m_invisibleKeys = new HashSet<string>();

            #endregion

            #region Constructor

            public MockAccessControlStrategy(ObjectManager manager)
            {
                m_manager = manager;
            }

            public void Dispose()
            {
            }

            #endregion

            #region Methods

            public UserAccess GetUserAccess(string user, Object gameObject)
            {
                return m_invisibleKeys.Contains(user) ? UserAccess.None : UserAccess.Read;
            }

            public void MakeVisible(string key)
            {
                m_invisibleKeys.Remove(key);
                OnUserAccessChanged(key, UserAccess.Read);
            }

            public void MakeInvisible(string key)
            {
                m_invisibleKeys.Add(key);
                OnUserAccessChanged(key, UserAccess.None);
            }

            public event EventHandler<UserAccessChangedEventArgs<string>> UserAccessChanged;

            private void OnUserAccessChanged(string key, UserAccess newAccess)
            {
                foreach (Object obj in m_manager.Objects)
                {
                    UserAccessChanged.Raise(this, new UserAccessChangedEventArgs<string>(obj, key, newAccess));
                }
            }

            #endregion
        }

        private class ReplicationTester : MarshalByRefObject
        {
            private readonly MyObjectManager m_host = new MyObjectManager();
            private readonly MockAccessControlStrategy m_accessControlStrategy;
            private readonly ReplicationSource<string> m_replicationSource;

            private MyObjectManager Host
            {
                get { return m_host; }
            }

            public ReplicationTester()
            {
                Host.CreateObject(42);

                m_accessControlStrategy = new MockAccessControlStrategy(Host);
                m_replicationSource = new ReplicationSource<string>(Host, m_accessControlStrategy);
            }

            public void Register(string key, IReplicationClient client)
            {
                m_replicationSource.Register(key, client);
            }

            public Resolvable<MyObject> CreateObject(int value)
            {
                return Host.CreateObject(value);
            }

            public Resolvable<MyObject> CreateDerivedObject(int value)
            {
                var obj = Host.Create<MyDerivedObject>();
                Host.Objects.Add(obj);
                obj.PropertyValue = value;
                return obj;
            }

            public void MakeInvisible(string key)
            {
                m_accessControlStrategy.MakeInvisible(key);
            }

            public void MakeVisible(string key)
            {
                m_accessControlStrategy.MakeVisible(key);
            }

            public void ChangeValue(Resolvable<MyObject> obj, int value, bool rollback)
            {
                using (ITransaction transaction = Host.Controller.BeginTransaction())
                {
                    obj.Resolve(Host).PropertyValue = value;

                    if (rollback)
                    {
                        transaction.Rollback();
                    }
                }
            }

            public void AddPlusOneEffect(Resolvable<MyObject> obj)
            {
                Host.CreateLocalEffect(obj.Resolve(Host), new PlusOneEffect());
            }

            public void AddPlusOneGlobalEffect()
            {
                Host.CreateGlobalEffect<MyGlobalEffectInstance>(new PlusOneEffect());
            }

            public void RemoveAllEffects()
            {
                foreach (EffectInstance effectInstance in Host.Objects.OfType<EffectInstance>().ToList())
                {
                    effectInstance.Remove();
                }
            }

            #region Inner Types

            [Serializable]
            private class PlusOneEffect : Effect<int>
            {
                public PlusOneEffect()
                    : base(MyObject.Property)
                {
                }

                public override int Modify(Object owner, int value)
                {
                    return value + 1;
                }
            }

            private class MyGlobalEffectInstance : GlobalEffectInstance
            {
                protected override IEnumerable<Object> InitObjects()
                {
                    foreach (Object obj in Manager.Objects)
                    {
                        if (AddAffectedObject(obj))
                        {
                            yield return obj;
                        }
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Variables

        private const string MyKey = "MyKey";

        private AppDomain m_testDomain;

        private ReplicationClient<MyObjectManager> m_client;
        private ReplicationTester m_tester;

        private MyObjectManager m_synchronizedHost;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_client = new ReplicationClient<MyObjectManager>();

            SetupClient();
        }

        [TearDown]
        public void Teardown()
        {
            TestDomain = null;
        }

        #endregion

        #region Utilities

        private AppDomain TestDomain
        {
            get { return m_testDomain; }
            set
            {
                if (m_testDomain != null)
                {
                    AppDomain.Unload(m_testDomain);
                }
                m_testDomain = value;
            }
        }

        private void SetupClient()
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(typeof(ReplicationTester).Assembly.CodeBase).Substring(6);
            TestDomain = AppDomain.CreateDomain("ReplicationClientTests Domain", null, setup);

            m_tester = (ReplicationTester)TestDomain.CreateInstanceAndUnwrap(typeof(ReplicationTester).Assembly.FullName, typeof(ReplicationTester).FullName);

            m_tester.Register(MyKey, m_client);
            m_synchronizedHost = m_client.Host;
        }

        #endregion

        #region Functional Tests

        [Test]
        public void Test_Initial_synchronization()
        {
            Assert.AreEqual(1, m_synchronizedHost.Objects.Count);
            Assert.AreEqual(42, m_synchronizedHost.Objects.OfType<MyObject>().Single().PropertyValue);
        }

        [Test]
        public void Test_Adding_objects_is_synchronized()
        {
            Resolvable<MyObject> createdObject = m_tester.CreateObject(30);

            Assert.AreEqual(2, m_synchronizedHost.Objects.Count);
            MyObject synchronizedObject = m_synchronizedHost.Objects.OfType<MyObject>().Skip(1).First();

            Assert.IsNotNull(synchronizedObject);
            Assert.AreSame(synchronizedObject, createdObject.Resolve(m_synchronizedHost));
            Assert.AreEqual(30, synchronizedObject.PropertyValue);
        }

        [Test]
        public void Test_Polymorphic_objects_are_correctly_replicated()
        {
            Resolvable<MyObject> createdObject = m_tester.CreateDerivedObject(30);

            Assert.AreEqual(2, m_synchronizedHost.Objects.Count);
            MyObject synchronizedObject = m_synchronizedHost.Objects.OfType<MyObject>().Skip(1).First();

            Assert.IsNotNull(synchronizedObject);
            Assert.IsInstanceOf<MyDerivedObject>(synchronizedObject);
            Assert.AreSame(synchronizedObject, createdObject.Resolve(m_synchronizedHost));
            Assert.AreEqual(30, synchronizedObject.PropertyValue);
        }

        [Test]
        public void Test_NonAtomic_transactions_are_correctly_replicated()
        {
            var createdObject = m_tester.CreateObject(10);
            m_tester.ChangeValue(createdObject, 99, false);
            Assert.AreEqual(99, createdObject.Resolve(m_synchronizedHost).PropertyValue);
        }

        [Test]
        public void Test_Rollbacking_NonAtomic_transactions_is_correctly_replicated()
        {
            var createdObject = m_tester.CreateObject(10);
            m_tester.ChangeValue(createdObject, 99, true);
            Assert.AreEqual(10, createdObject.Resolve(m_synchronizedHost).PropertyValue);
        }

        [Test]
        public void Test_Delayed_synchronization_works_with_identifier()
        {
            m_tester.MakeInvisible(MyKey);

            var createdObject = m_tester.CreateObject(10);
            Assert.AreEqual(0, createdObject.Resolve(m_synchronizedHost).PropertyValue);

            m_tester.MakeVisible(MyKey);
            Assert.AreEqual(10, createdObject.Resolve(m_synchronizedHost).PropertyValue);
        }

        [Test]
        public void Test_Effects_are_correctly_synchronized()
        {
            var createdObject = m_tester.CreateObject(10);
            Assert.AreEqual(10, createdObject.Resolve(m_synchronizedHost).PropertyValue);

            m_tester.AddPlusOneEffect(createdObject);
            Assert.AreEqual(11, createdObject.Resolve(m_synchronizedHost).PropertyValue);

            m_tester.RemoveAllEffects();
            Assert.AreEqual(10, createdObject.Resolve(m_synchronizedHost).PropertyValue);
        }

        [Test]
        public void Test_Global_Effects_are_correctly_synchronized()
        {
            var createdObject1 = m_tester.CreateObject(10);
            var createdObject2 = m_tester.CreateObject(15);
            
            m_tester.AddPlusOneGlobalEffect();

            var synchronizedObject1 = createdObject1.Resolve(m_synchronizedHost);
            var synchronizedObject2 = createdObject2.Resolve(m_synchronizedHost);

            Assert.AreEqual(11, synchronizedObject1.PropertyValue);
            Assert.AreEqual(16, synchronizedObject2.PropertyValue);

            m_tester.RemoveAllEffects();

            Assert.AreEqual(10, synchronizedObject1.PropertyValue);
            Assert.AreEqual(15, synchronizedObject2.PropertyValue);
        }

        [Test]
        public void Test_Identifiers_are_globally_unique()
        {
            var obj1 = m_tester.CreateObject(1).Resolve(m_synchronizedHost);

#warning todo
            //using (m_synchronizedHost.ChangeControlMode(ReplicationControlMode.Master))
            {
                var obj2 = m_synchronizedHost.CreateObject(12);

                Assert.AreNotSame(obj1, obj2);
                Assert.AreNotEqual(obj1.Identifier, obj2.Identifier);
            }
        }

        #endregion
    }
}

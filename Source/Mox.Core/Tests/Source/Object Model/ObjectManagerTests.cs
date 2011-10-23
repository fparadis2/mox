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

using NUnit.Framework;
using System.Collections.Generic;

namespace Mox
{
    [TestFixture]
    public class ObjectManagerTests
    {
        #region Inner Types

        private class MyObject : Object
        {
            public static readonly Property<int> Property = Property<int>.RegisterProperty("Property", typeof(MyObject));

            public int PropertyValue
            {
                get { return GetValue(Property); }
                set { SetValue(Property, value); }
            }

            public int InitCount;

            protected internal override void Init()
            {
                base.Init();
                InitCount++;
            }

            protected internal override void Uninit()
            {
                base.Init();
                InitCount--;
            }
        }

        private class AnotherObject : Object
        {
        }

        private class ObjectWithoutController : Object
        {
        }

        #endregion

        #region Variables

        private MockObjectManager m_manager;
        private MyObject m_myObject;
        private MyObject m_myObject2;

        private EventSink<PropertyChangedEventArgs> m_propertyChangedSink;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            BasicSetup();

            m_propertyChangedSink = new EventSink<PropertyChangedEventArgs>();
            m_manager.PropertyChanged += m_propertyChangedSink;
        }

        private void BasicSetup()
        {
            m_manager = new MockObjectManager();

            m_myObject = m_manager.Create<MyObject>();
            m_myObject2 = m_manager.Create<MyObject>();
        }

        #endregion

        #region Tests

        #region General

        [Test]
        public void Test_Can_access_the_transaction_stack()
        {
            Assert.IsNotNull(m_manager.TransactionStack);
        }

        [Test]
        public void Test_Create_a_new_object_of_the_given_type_with_a_unique_id()
        {
            Assert.IsNotNull(m_myObject);
            Assert.AreNotEqual(m_myObject.Identifier, m_manager.Create<MyObject>().Identifier);

            Assert.AreEqual(m_manager, m_myObject.Manager);
        }

        [Test]
        public void Test_GetObjectByIdentifier_returns_the_object_corresponding_to_the_given_identifier()
        {
            Assert.AreSame(m_myObject, m_manager.GetObjectByIdentifier<MyObject>(m_myObject.Identifier));
            Assert.Throws<KeyNotFoundException>(() => m_manager.GetObjectByIdentifier<MyObject>(9999));
        }

        [Test]
        public void Test_ContainsObject_returns_whether_an_object_with_the_given_identifier_is_present()
        {
            Assert.IsTrue(m_manager.ContainsObject(m_myObject.Identifier));
            Assert.IsFalse(m_manager.ContainsObject(9999));
        }

        #endregion

        #region Add

        [Test]
        public void Test_Can_add_objects_to_the_manager()
        {
            m_manager.Objects.Add(m_myObject);
            Assert.Collections.Contains(m_myObject, m_manager.Objects);
        }

        [Test]
        public void Test_Adding_an_object_calls_Init()
        {
            Assert.AreEqual(0, m_myObject.InitCount);
            m_manager.Objects.Add(m_myObject);
            Assert.AreEqual(1, m_myObject.InitCount);
        }

        [Test]
        public void Test_Add_is_undoable()
        {
            using (ITransaction transaction = m_manager.TransactionStack.BeginTransaction())
            {
                m_manager.Objects.Add(m_myObject);
                transaction.Rollback();

                Assert.IsFalse(m_manager.Objects.Contains(m_myObject));
                Assert.AreEqual(0, m_myObject.InitCount);
            }
        }

        [Test]
        public void Test_Cannot_add_the_same_item_twice()
        {
            m_manager.Objects.Add(m_myObject);
            Assert.Throws<ArgumentException>(() => m_manager.Objects.Add(m_myObject));
            Assert.AreEqual(1, m_myObject.InitCount);
        }

        [Test]
        public void Test_Cannot_add_an_item_from_another_manager()
        {
            Assert.Throws<ArgumentException>(() => m_manager.Objects.Add(new MockObjectManager().Create<MyObject>()));
        }

        [Test]
        public void Test_Cannot_add_a_null_item()
        {
            Assert.Throws<ArgumentNullException>(() => m_manager.Objects.Add(null));
        }

        [Test]
        public void Test_Can_add_an_object_for_which_there_is_no_controller()
        {
            Assert.DoesntThrow(() => m_manager.Objects.Add(m_manager.Create<ObjectWithoutController>()));
        }

        #endregion

        #region Remove

        [Test]
        public void Test_Asking_the_manager_to_remove_the_object_removes_it_from_the_given_collection()
        {
            m_manager.Objects.Add(m_myObject);
            Assert.IsTrue(m_manager.Objects.Remove(m_myObject));

            Assert.IsFalse(m_manager.Objects.Contains(m_myObject));
            Assert.Throws<KeyNotFoundException>(() => m_manager.GetObjectByIdentifier<Object>(m_myObject.Identifier));
        }

        [Test]
        public void Test_Removing_an_item_that_is_not_in_the_collection_does_nothing()
        {
            Assert.IsFalse(m_manager.Objects.Remove(m_myObject));
            Assert.AreEqual(0, m_myObject.InitCount);
        }

        [Test]
        public void Test_Remove_is_undoable()
        {
            m_manager.Objects.Add(m_myObject);

            using (ITransaction transaction = m_manager.TransactionStack.BeginTransaction())
            {
                Assert.AreEqual(1, m_myObject.InitCount);
                m_manager.Objects.Remove(m_myObject);
                Assert.AreEqual(0, m_myObject.InitCount);
                transaction.Rollback();
                Assert.AreEqual(1, m_myObject.InitCount);
            }
            Assert.IsTrue(m_manager.Objects.Contains(m_myObject));
            Assert.AreEqual(m_manager, m_myObject.Manager);
        }

        [Test]
        public void Test_Can_remove_an_object_with_Remove()
        {
            m_manager.Objects.Add(m_myObject);

            m_myObject.Remove();

            Assert.IsFalse(m_manager.Objects.Contains(m_myObject));
        }

        #endregion

        #region Clear

        [Test]
        public void Test_Clearing_the_objects()
        {
            m_manager.Objects.Add(m_myObject);
            m_manager.Objects.Add(m_myObject2);

            m_manager.Objects.Clear();
            Assert.Collections.IsEmpty(m_manager.Objects);
        }

        [Test]
        public void Test_Clearing_the_objects_calls_Uninit_on_all_objects()
        {
            m_manager.Objects.Add(m_myObject);
            m_manager.Objects.Add(m_myObject2);

            m_manager.Objects.Clear();

            Assert.AreEqual(0, m_myObject.InitCount);
            Assert.AreEqual(0, m_myObject2.InitCount);
        }

        [Test]
        public void Test_Clearing_an_empty_collection_does_nothing()
        {
            m_manager.Objects.Clear();
        }

        [Test]
        public void Test_Clear_is_undoable()
        {
            m_manager.Objects.Add(m_myObject);
            m_manager.Objects.Add(m_myObject2);

            using (ITransaction transaction = m_manager.TransactionStack.BeginTransaction())
            {
                m_manager.Objects.Clear();
                transaction.Rollback();
            }

            Assert.Collections.AreEquivalent(new Object[] { m_myObject, m_myObject2 }, m_manager.Objects);
        }

        #endregion

        #region Inventory

        [Test]
        public void Test_Can_register_an_inventory_and_get_the_list_of_objects_of_a_specific_type()
        {
            BasicSetup();
            ICollection<MyObject> typedObjects = m_manager.RegisterInventory<MyObject>();
            Assert.IsNotNull(typedObjects);
        }

        [Test]
        public void Test_inventory_list_is_synchronized_with_master_object_list()
        {
            BasicSetup();
            ICollection<MyObject> typedObjects = m_manager.RegisterInventory<MyObject>();

            m_manager.Objects.Add(m_myObject);
            Assert.Collections.Contains(m_myObject, typedObjects);

            m_manager.Objects.Remove(m_myObject);
            Assert.Collections.IsEmpty(typedObjects);

            Object other = m_manager.Create<AnotherObject>();
            m_manager.Objects.Add(other);
            Assert.Collections.IsEmpty(typedObjects);
        }

        [Test]
        public void Test_inventory_list_is_synchronized_even_with_transactions()
        {
            BasicSetup();
            ICollection<MyObject> typedObjects = m_manager.RegisterInventory<MyObject>();

            m_manager.Objects.Add(m_myObject);
            Assert.Collections.Contains(m_myObject, typedObjects);

            using (ITransaction transaction = m_manager.TransactionStack.BeginTransaction())
            {
                m_manager.Objects.Clear();
                Assert.Collections.IsEmpty(typedObjects);

                transaction.Rollback();
            }

            Assert.Collections.Contains(m_myObject, typedObjects);
        }

        [Test]
        public void Test_add_and_remove_on_the_inventory_list_is_synchronized()
        {
            BasicSetup();
            ICollection<MyObject> typedObjects = m_manager.RegisterInventory<MyObject>();

            typedObjects.Add(m_myObject);
            Assert.Collections.Contains(m_myObject, m_manager.Objects);
            Assert.Collections.Contains(m_myObject, typedObjects);

            Assert.IsTrue(typedObjects.Remove(m_myObject));
            Assert.Collections.IsEmpty(m_manager.Objects);
            Assert.Collections.IsEmpty(typedObjects);
        }

        [Test]
        public void Test_clear_on_the_inventory_list_is_synchronized()
        {
            BasicSetup();
            ICollection<MyObject> typedObjects = m_manager.RegisterInventory<MyObject>();

            Assert.Collections.IsEmpty(m_manager.Objects);
            Assert.Collections.IsEmpty(typedObjects);

            typedObjects.Add(m_myObject);
            typedObjects.Clear();

            Assert.Collections.IsEmpty(m_manager.Objects);
            Assert.Collections.IsEmpty(typedObjects);
        }

        #endregion

        #region SetObjectValue

        [Test]
        public void Test_Invalid_SetObjectValue_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_manager.SetObjectValue(null, MyObject.Property, 10));
            Assert.Throws<ArgumentNullException>(() => m_manager.SetObjectValue(m_myObject, null, 10));
            Assert.Throws<ArgumentException>(() => m_manager.SetObjectValue(new MockObjectManager().Create<MyObject>(), MyObject.Property, 10));
        }

        [Test]
        public void Test_SetObjectValue()
        {
            m_manager.SetObjectValue(m_myObject, MyObject.Property, 10);
            Assert.AreEqual(10, m_myObject.PropertyValue);
        }

        #endregion

        #region Events

        [Test]
        public void Test_PropertyChanged_is_triggered_when_a_property_changes()
        {
            m_manager.Objects.Add(m_myObject);

            Assert.EventCalledOnce(m_propertyChangedSink, () => m_myObject.PropertyValue = 10);
            Assert.AreEqual(m_myObject, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.Property, m_propertyChangedSink.LastEventArgs.Property);
            Assert.AreEqual(0, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(10, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        [Test]
        public void Test_Detaching_events()
        {
            m_manager.Objects.Add(m_myObject);

            m_manager.PropertyChanged -= m_propertyChangedSink;

            Assert.EventNotCalled(m_propertyChangedSink, () => m_myObject.PropertyValue = 10);
        }

        #endregion

        #region ControlMode

        [Test]
        public void Test_Mode_is_Master_by_default()
        {
            Assert.AreEqual(ReplicationControlMode.Master, m_manager.ControlMode);
        }

        [Test]
        public void Test_Can_change_mode_during_a_scope()
        {
            Assert.AreEqual(ReplicationControlMode.Master, m_manager.ControlMode);

            using (m_manager.ChangeControlMode(ReplicationControlMode.Slave))
            {
                Assert.AreEqual(ReplicationControlMode.Slave, m_manager.ControlMode);
            }

            Assert.AreEqual(ReplicationControlMode.Master, m_manager.ControlMode);
        }

        [Test]
        public void Test_Can_ensure_is_correct_mode()
        {
            m_manager.EnsureControlModeIs(ReplicationControlMode.Master);

            using (m_manager.ChangeControlMode(ReplicationControlMode.Slave))
            {
                m_manager.EnsureControlModeIs(ReplicationControlMode.Slave);
            }

            m_manager.EnsureControlModeIs(ReplicationControlMode.Master);
        }

        #endregion

        #endregion
    }
}

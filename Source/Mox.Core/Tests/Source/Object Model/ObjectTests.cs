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
using Mox.Transactions;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class ObjectTests
    {
        #region Inner Types

        private class MyObject : Object
        {
            #region Normal Properties

            public static readonly Property<int> SimpleProperty = Property<int>.RegisterProperty<MyObject>("Simple", o => o.m_simple);
            private int m_simple;

            public int Simple
            {
                get { return m_simple; }
                set { SetValue(SimpleProperty, value, ref m_simple); }
            }

            public static readonly Property<int> ReadOnlyProperty = Property<int>.RegisterProperty<MyObject>("ReadOnly", o => o.m_readOnly);
            private readonly int m_readOnly = 0;

            public int ReadOnly
            {
                get { return m_readOnly; }
            }

            public static readonly Property<int> PrivateProperty = Property<int>.RegisterProperty<MyObject>("Private", o => o.m_private, PropertyFlags.Private);
            private int m_private;

            public int Private
            {
                get { return m_private; }
                set { SetValue(PrivateProperty, value, ref m_private); }
            }


            public static readonly Property<int> DefaultValueProperty = Property<int>.RegisterProperty<MyObject>("DefaultValue", o => o.m_defaultValue);
            private int m_defaultValue = 10;

            public int DefaultValue
            {
                get { return m_defaultValue; }
                set { SetValue(DefaultValueProperty, value, ref m_defaultValue); }
            }

            #endregion

            #region Invalid Properties

            private const Property<int> m_nullProperty = null;

            public int Null
            {
                set { SetValue(m_nullProperty, value); }
            }

            public int OtherObjectProperty
            {
                set { SetValue(OtherObject.OtherObjectProperty, value); }
            }

            #endregion
        }

        private class OtherObject : Object
        {
            public static readonly Property<int> OtherObjectProperty = Property<int>.RegisterProperty<OtherObject>("OtherObjectProperty", o => o.m_property);
            private int m_property;

            private int Property
            {
                get { return m_property; }
                set { SetValue(OtherObjectProperty, value, ref m_property); }
            }
        }

        #endregion

        #region Variables

        private MockObjectManager m_manager;
        private MyObject m_object;
        private MyObject m_otherObject;

        private EventSink<PropertyChangingEventArgs> m_propertyChangingSink;
        private EventSink<PropertyChangedEventArgs> m_propertyChangedSink;

        private ICommand m_lastPushedCommand;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_manager = new MockObjectManager();
            m_manager.Controller.CommandExecuted += Controller_CommandExecuted;

            m_object = m_manager.CreateAndAdd<MyObject>();
            m_otherObject = m_manager.CreateAndAdd<MyObject>();

            m_propertyChangedSink = new EventSink<PropertyChangedEventArgs>();
            m_object.PropertyChanged += m_propertyChangedSink;

            m_propertyChangingSink = new EventSink<PropertyChangingEventArgs>();
            m_object.PropertyChanging += m_propertyChangingSink;
        }

        #endregion

        #region Utilities

        private ISynchronizableCommand GetTopSynchronizableCommand()
        {
            return (ISynchronizableCommand)m_lastPushedCommand;
        }

        void Controller_CommandExecuted(object sender, CommandEventArgs e)
        {
            m_lastPushedCommand = e.Command;
        }

        #endregion

        #region Tests

        #region Property Get/Set

        [Test]
        public void Default_value_of_a_property_is_the_initialization_value()
        {
            Assert.AreEqual(0, m_object.Simple);
        }

        [Test]
        public void Can_set_the_value_of_a_property()
        {
            m_object.Simple = 3;
            Assert.AreEqual(3, m_object.Simple);
        }

        [Test]
        public void Values_are_per_instance()
        {
            m_object.Simple = 3;
            m_otherObject.Simple = 10;

            Assert.AreEqual(3, m_object.Simple);
            Assert.AreEqual(10, m_otherObject.Simple);
        }

        [Test, Conditional("DEBUG")]
        public void Cannot_get_or_set_a_value_for_a_null_property()
        {
            Assert.Throws<Exception>(delegate { m_object.Null = 3; });
        }

        [Test]
        public void Cannot_set_a_value_for_a_property_belonging_to_another_object_if_its_not_attached()
        {
#if DEBUG
            Assert.Throws<ArgumentException>(delegate { m_object.OtherObjectProperty = 3; });
#else
            Assert.Throws<Exception>(delegate { m_object.OtherObjectProperty = 3; });
#endif
        }

        [Test]
        public void Test_Manager_can_set_the_value_of_a_read_only_property_when_the_property_has_not_been_added_yet()
        {
            MyObject obj = m_manager.Create<MyObject>();

            m_manager.SetObjectValue(obj, MyObject.ReadOnlyProperty, 10);
            Assert.AreEqual(10, obj.ReadOnly);
        }

        [Test]
        public void Test_Property_with_default_value_has_the_value_at_construction()
        {
            MyObject newObject = m_manager.Create<MyObject>();
            Assert.AreEqual(10, newObject.DefaultValue);
        }

        [Test]
        public void Test_Property_with_default_value_doesnt_trigger_any_events()
        {
            Assert.AreEqual(10, m_object.DefaultValue);

            Assert.AreEqual(0, m_propertyChangedSink.TimesCalled);
            Assert.AreEqual(0, m_propertyChangingSink.TimesCalled);
        }

        [Test]
        public void Test_Can_get_set_property_with_a_default_value()
        {
            m_object.DefaultValue = 3;
            Assert.AreEqual(3, m_object.DefaultValue);
        }

        [Test]
        public void Test_GetValue_returns_the_value()
        {
            m_object.Simple = 10;
            Assert.AreEqual(10, m_object.GetValue(MyObject.SimpleProperty));
        }

        #endregion

        #region Property Reset

        [Test]
        public void Test_ResetValue_resets_the_value_of_the_given_property()
        {
            m_object.DefaultValue = 99;
            m_object.ResetValue(MyObject.DefaultValueProperty);
            Assert.AreEqual(10, m_object.DefaultValue);
        }

        [Test]
        public void Test_Cannot_reset_a_read_only_property()
        {
            Assert.Throws<InvalidOperationException>(() => m_object.ResetValue(MyObject.ReadOnlyProperty));
        }

        [Test]
        public void Test_Reset_is_undoable()
        {
            m_object.DefaultValue = 99;

            Assert.IsUndoRedoable(m_manager.Controller,
                () => Assert.AreEqual(99, m_object.DefaultValue),
                () => m_object.ResetValue(MyObject.DefaultValueProperty),
                () => Assert.AreEqual(10, m_object.DefaultValue));
        }

        [Test]
        public void Test_Reset_is_atomic()
        {
            m_object.DefaultValue = 99;

            Assert.IsAtomic(m_manager.Controller, () => m_object.ResetValue(MyObject.DefaultValueProperty));
        }

        [Test]
        public void Test_ResetAllValues_resets_all_properties_to_their_default_value()
        {
            m_object.Simple = 99;
            m_object.DefaultValue = 99;

            m_object.ResetAllValues();

            Assert.AreEqual(0, m_object.Simple);
            Assert.AreEqual(10, m_object.DefaultValue);
        }

        [Test]
        public void Test_ResetAllValues_can_be_done_twice()
        {
            m_object.Simple = 99;
            m_object.DefaultValue = 99;

            m_object.ResetAllValues();
            m_object.ResetAllValues();

            Assert.AreEqual(0, m_object.Simple);
            Assert.AreEqual(10, m_object.DefaultValue);
        }

        [Test]
        public void Test_ResetAllValues_doesnt_modify_readonly_properties()
        {
            MyObject obj = m_manager.Create<MyObject>();

            m_manager.SetObjectValue(obj, MyObject.ReadOnlyProperty, 10);
            obj.ResetAllValues();
            Assert.AreEqual(10, obj.ReadOnly);
        }

        [Test]
        public void Test_ResetAllValues_is_undoable()
        {
            m_object.Simple = 99;
            m_object.DefaultValue = 99;

            Assert.IsUndoRedoable(m_manager.Controller,
                () =>
                {
                    Assert.AreEqual(99, m_object.Simple);
                    Assert.AreEqual(99, m_object.DefaultValue);
                },
                () => m_object.ResetAllValues(),
                () =>
                {
                    Assert.AreEqual(0, m_object.Simple);
                    Assert.AreEqual(10, m_object.DefaultValue);
                });
        }

        [Test]
        public void Test_ResetAllValues_is_atomic()
        {
            m_object.Simple = 99;
            m_object.DefaultValue = 99;

            Assert.IsAtomic(m_manager.Controller, () => m_object.ResetAllValues());
        }

        #endregion

        #region IsEquivalent

        [Test]
        public void Test_IsEquivalent_returns_false_for_objects_of_different_type()
        {
            MyObject myObject = m_manager.CreateAndAdd<MyObject>();
            OtherObject otherObject = m_manager.CreateAndAdd<OtherObject>();

            Assert.IsFalse(myObject.IsEquivalentTo(otherObject));
        }

        [Test]
        public void Test_Objects_are_always_equivalent_to_themselves()
        {
            MyObject myObject = m_manager.CreateAndAdd<MyObject>();
            Assert.IsTrue(myObject.IsEquivalentTo(myObject));

            OtherObject otherObject = m_manager.CreateAndAdd<OtherObject>();
            Assert.IsTrue(otherObject.IsEquivalentTo(otherObject));
        }

        [Test]
        public void Test_Objects_are_only_equivalent_if_they_have_the_same_property_values()
        {
            MyObject myObject1 = m_manager.CreateAndAdd<MyObject>();
            MyObject myObject2 = m_manager.CreateAndAdd<MyObject>();

            Assert.IsTrue(myObject1.IsEquivalentTo(myObject2));
            Assert.IsTrue(myObject2.IsEquivalentTo(myObject1));

            myObject1.Simple = 10;
            Assert.IsFalse(myObject1.IsEquivalentTo(myObject2));
            Assert.IsFalse(myObject2.IsEquivalentTo(myObject1));

            myObject2.Simple = 10;
            Assert.IsTrue(myObject1.IsEquivalentTo(myObject2));
            Assert.IsTrue(myObject2.IsEquivalentTo(myObject1));

            myObject1.ResetValue(MyObject.SimpleProperty);
            Assert.IsFalse(myObject1.IsEquivalentTo(myObject2));
            Assert.IsFalse(myObject2.IsEquivalentTo(myObject1));

            myObject2.ResetValue(MyObject.SimpleProperty);
            Assert.IsTrue(myObject1.IsEquivalentTo(myObject2));
            Assert.IsTrue(myObject2.IsEquivalentTo(myObject1));
        }

        [Test]
        public void Test_Objects_are_equivalent_even_if_properties_are_set_only_on_one_of_them_1()
        {
            MyObject myObject1 = m_manager.CreateAndAdd<MyObject>();
            MyObject myObject2 = m_manager.CreateAndAdd<MyObject>();

            myObject1.Simple = 10;
            myObject1.Simple = 0;
            Assert.IsTrue(myObject1.IsEquivalentTo(myObject2));
            Assert.IsTrue(myObject2.IsEquivalentTo(myObject1));
        }

        [Test]
        public void Test_Objects_are_equivalent_even_if_properties_are_set_only_on_one_of_them_2()
        {
            MyObject myObject1 = m_manager.CreateAndAdd<MyObject>();
            MyObject myObject2 = m_manager.CreateAndAdd<MyObject>();

            myObject2.Simple = 10;
            myObject2.Simple = 0;
            Assert.IsTrue(myObject1.IsEquivalentTo(myObject2));
            Assert.IsTrue(myObject2.IsEquivalentTo(myObject1));
        }

        [Test]
        public void Test_IsEquivalent_can_ignore_properties()
        {
            MyObject myObject1 = m_manager.CreateAndAdd<MyObject>();
            MyObject myObject2 = m_manager.CreateAndAdd<MyObject>();

            Assert.IsTrue(myObject1.IsEquivalentTo(myObject2, MyObject.DefaultValueProperty));
            Assert.IsTrue(myObject2.IsEquivalentTo(myObject1, MyObject.DefaultValueProperty));

            myObject1.DefaultValue = 66;
            Assert.IsTrue(myObject1.IsEquivalentTo(myObject2, MyObject.DefaultValueProperty));
            Assert.IsTrue(myObject2.IsEquivalentTo(myObject1, MyObject.DefaultValueProperty));

            myObject2.Simple = 66;
            Assert.IsFalse(myObject1.IsEquivalentTo(myObject2, MyObject.DefaultValueProperty));
            Assert.IsFalse(myObject2.IsEquivalentTo(myObject1, MyObject.DefaultValueProperty));
        }

        #endregion

        #region Events

        [Test]
        public void Test_PropertyChanging_is_triggered_when_a_property_changes()
        {
            Assert.EventCalledOnce(m_propertyChangingSink, () => m_object.Simple = 10);
            Assert.AreEqual(m_object, m_propertyChangingSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangingSink.LastEventArgs.Property);
            Assert.AreEqual(0, m_propertyChangingSink.LastEventArgs.OldValue);
            Assert.AreEqual(10, m_propertyChangingSink.LastEventArgs.NewValue);
            Assert.IsFalse(m_propertyChangingSink.LastEventArgs.Cancel);

            Assert.AreEqual(10, m_object.Simple);
        }

        [Test]
        public void Test_PropertyChanging_is_not_triggered_when_the_value_of_a_property_doesnt_change()
        {
            Assert.EventNotCalled(m_propertyChangingSink, () => m_object.Simple = 0);
        }

        [Test]
        public void Test_PropertyChanging_can_cancel_the_change()
        {
            m_propertyChangingSink.Callback += (sender, e) => e.Cancel = true;

            Assert.EventCalledOnce(m_propertyChangingSink, () => m_object.Simple = 10);

            Assert.AreEqual(m_object, m_propertyChangingSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangingSink.LastEventArgs.Property);
            Assert.AreEqual(0, m_propertyChangingSink.LastEventArgs.OldValue);
            Assert.AreEqual(10, m_propertyChangingSink.LastEventArgs.NewValue);
            Assert.IsTrue(m_propertyChangingSink.LastEventArgs.Cancel);

            Assert.AreEqual(0, m_propertyChangedSink.TimesCalled);
            Assert.AreEqual(0, m_object.Simple);
        }

        [Test]
        public void Test_PropertyChanged_is_triggered_when_a_property_changes()
        {
            Assert.EventCalledOnce(m_propertyChangedSink, () => m_object.Simple = 10);
            Assert.AreEqual(m_object, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangingSink.LastEventArgs.Property);
            Assert.AreEqual(0, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(10, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        [Test]
        public void Test_PropertyChanged_is_not_triggered_when_the_value_of_a_property_doesnt_change()
        {
            Assert.EventNotCalled(m_propertyChangedSink, () => m_object.Simple = 0);

            m_object.Simple = 10;
            m_object.Simple = 0;

            Assert.EventNotCalled(m_propertyChangedSink, () => m_object.ResetValue(MyObject.SimpleProperty));
            Assert.EventNotCalled(m_propertyChangedSink, () => m_object.ResetValue(MyObject.SimpleProperty));
        }

        [Test]
        public void Test_Detaching_events()
        {
            m_object.PropertyChanging -= m_propertyChangingSink;
            m_object.PropertyChanged -= m_propertyChangedSink;

            Assert.EventNotCalled(m_propertyChangingSink, () => m_object.Simple = 10);
            Assert.EventNotCalled(m_propertyChangedSink, () => m_object.Simple = 10);
        }

        [Test]
        public void Test_PropertyChanged_is_triggered_when_a_property_is_reset()
        {
            m_object.DefaultValue = 20;
            Assert.EventCalledOnce(m_propertyChangedSink, () => m_object.ResetValue(MyObject.DefaultValueProperty));
            Assert.AreEqual(m_object, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.DefaultValueProperty, m_propertyChangingSink.LastEventArgs.Property);
            Assert.AreEqual(20, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(10, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        #endregion

        #region Transactions

        [Test]
        public void Test_Setting_a_value_on_a_property_is_undoable()
        {
            Assert.IsUndoRedoable(m_manager.Controller,
                                  () => Assert.AreEqual(0, m_object.Simple),
                                  () => m_object.Simple = 3,
                                  () => Assert.AreEqual(3, m_object.Simple));
        }

        [Test]
        public void Test_Setting_the_same_value_does_nothing()
        {
            Assert.Produces(m_manager.Controller, () => m_object.Simple = 0, 0);
        }

        #endregion

        #region Synchronization of value setting commands

        [Test]
        public void Test_SetValue_commands_are_public_when_the_property_is_not_private()
        {
            m_object.Simple = 10;

            ISynchronizableCommand synchronizableCommmand = GetTopSynchronizableCommand();
            Assert.IsTrue(synchronizableCommmand.IsPublic);
        }

        [Test]
        public void Test_SetValue_commands_are_not_public_when_the_property_is_private()
        {
            m_object.Private = 10;

            ISynchronizableCommand synchronizableCommmand = GetTopSynchronizableCommand();
            Assert.IsFalse(synchronizableCommmand.IsPublic);
        }

        [Test]
        public void Test_Object_returns_the_object()
        {
            m_object.Simple = 10;

            ISynchronizableCommand synchronizableCommmand = GetTopSynchronizableCommand();
            Assert.AreEqual(m_object, synchronizableCommmand.GetObject(m_manager));
        }

        [Test]
        public void Test_Synchronize_returns_the_same_command()
        {
            m_object.Simple = 10;

            ISynchronizableCommand synchronizableCommmand = GetTopSynchronizableCommand();
            Assert.AreSame(synchronizableCommmand, synchronizableCommmand.Synchronize());
        }

        #endregion

        #endregion
    }
}

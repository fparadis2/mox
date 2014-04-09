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

namespace Mox
{
    [TestFixture]
    public class ObjectEffectsTests
    {
        #region Inner Types

        private class MyObject : Object
        {
            #region Normal Properties

            private int m_simple;
            public static readonly Property<int> SimpleProperty = Property<int>.RegisterProperty<MyObject>("Simple", o => o.m_simple, PropertyFlags.Modifiable);

            public int Simple
            {
                get { return m_simple; }
                set { SetValue(SimpleProperty, value, ref m_simple); }
            }

            private int m_defaultValue = 10;
            public static readonly Property<int> DefaultValueProperty = Property<int>.RegisterProperty<MyObject>("DefaultValue", o => o.m_defaultValue, PropertyFlags.Modifiable);

            public int DefaultValue
            {
                get { return m_defaultValue; }
                set { SetValue(DefaultValueProperty, value, ref m_defaultValue); }
            }

            #endregion
        }

        private class MyEffect : Effect<int>
        {
            public MyEffect(Property<int> property)
                : base(property)
            {
            }

            public override int Modify(Object owner, int value)
            {
                return value * 2 + 1;
            }
        }

        private class SortableEffect : Effect<int>
        {
            private readonly int m_sortOrder;
            private readonly int m_multiplier;

            public SortableEffect(int sortOrder, int multiplier)
                : base(MyObject.SimpleProperty)
            {
                m_sortOrder = sortOrder;
                m_multiplier = multiplier;
            }

            public override int Modify(Object owner, int value)
            {
                return value * m_multiplier + 1;
            }

            public override int CompareTo(EffectBase other)
            {
                return m_sortOrder.CompareTo(((SortableEffect)other).m_sortOrder);
            }
        }

        private class NullEffect : Effect<int>
        {
            public NullEffect(Property<int> property)
                : base(property)
            {
            }

            public override int Modify(Object owner, int value)
            {
                return value;
            }
        }

        #endregion

        #region Variables

        private MockObjectManager m_manager;
        private MyObject m_object;

        private EventSink<PropertyChangingEventArgs> m_propertyChangingSink;
        private EventSink<PropertyChangedEventArgs> m_propertyChangedSink;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_manager = new MockObjectManager();

            m_object = m_manager.Create<MyObject>();
            m_manager.Objects.Add(m_object);

            m_propertyChangedSink = new EventSink<PropertyChangedEventArgs>();
            m_object.PropertyChanged += m_propertyChangedSink;

            m_propertyChangingSink = new EventSink<PropertyChangingEventArgs>();
            m_object.PropertyChanging += m_propertyChangingSink;
        }

        #endregion

        #region Utilities

        private LocalEffectInstance AddEffect()
        {
            return m_manager.CreateLocalEffect(m_object, new MyEffect(MyObject.SimpleProperty));
        }

        private LocalEffectInstance AddDefaultValueEffect()
        {
            return m_manager.CreateLocalEffect(m_object, new MyEffect(MyObject.DefaultValueProperty));
        }

        private LocalEffectInstance AddNullEffect()
        {
            return m_manager.CreateLocalEffect(m_object, new NullEffect(MyObject.SimpleProperty));
        }

        private LocalEffectInstance AddSortableEffect(int sortOrder, int multiplier)
        {
            return m_manager.CreateLocalEffect(m_object, new SortableEffect(sortOrder, multiplier));
        }

        #endregion

        #region Tests

        #region No effect is active

        [Test]
        public void Test_Setting_a_value_while_no_effect_is_active_works()
        {
            m_object.Simple = 3;
            Assert.AreEqual(3, m_object.Simple);
            m_object.Simple = 4;
            Assert.AreEqual(4, m_object.Simple);
        }

        [Test]
        public void Test_Resetting_a_value_while_no_effect_is_active_works()
        {
            m_object.Simple = 3;
            Assert.AreEqual(3, m_object.Simple);
            m_object.ResetValue(MyObject.SimpleProperty);
            Assert.AreEqual(0, m_object.Simple);
        }

        [Test]
        public void Test_Resetting_a_value_with_default_value_while_no_effect_is_active_works()
        {
            m_object.DefaultValue = 3;
            Assert.AreEqual(3, m_object.DefaultValue);
            m_object.ResetValue(MyObject.DefaultValueProperty);
            Assert.AreEqual(10, m_object.DefaultValue);
        }

        [Test]
        public void Test_Setting_a_value_while_no_effect_is_active_triggers_PropertyChanged()
        {
            Assert.EventCalledOnce(m_propertyChangedSink, () => m_object.Simple = 4);
            Assert.AreEqual(m_object, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangedSink.LastEventArgs.Property);
            Assert.AreEqual(0, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(4, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        [Test]
        public void Test_Setting_a_value_with_default_value_while_no_effect_is_active_triggers_PropertyChanged()
        {
            Assert.EventCalledOnce(m_propertyChangedSink, () => m_object.DefaultValue = 4);
            Assert.AreEqual(m_object, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.DefaultValueProperty, m_propertyChangedSink.LastEventArgs.Property);
            Assert.AreEqual(10, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(4, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        [Test]
        public void Test_Reetting_a_value_while_no_effect_is_active_triggers_PropertyChanged()
        {
            m_object.Simple = 4;

            Assert.EventCalledOnce(m_propertyChangedSink, () => m_object.ResetValue(MyObject.SimpleProperty));
            Assert.AreEqual(m_object, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangedSink.LastEventArgs.Property);
            Assert.AreEqual(4, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(0, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        [Test]
        public void Test_Reetting_a_value_with_default_value_while_no_effect_is_active_triggers_PropertyChanged()
        {
            m_object.DefaultValue = 4;

            Assert.EventCalledOnce(m_propertyChangedSink, () => m_object.ResetValue(MyObject.DefaultValueProperty));
            Assert.AreEqual(m_object, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.DefaultValueProperty, m_propertyChangedSink.LastEventArgs.Property);
            Assert.AreEqual(4, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(10, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        #endregion

        #region While effect is active

        [Test]
        public void Test_Setting_a_value_while_an_effect_is_active_works()
        {
            AddEffect();

            m_object.Simple = 3;
            Assert.AreEqual(7, m_object.Simple);
            m_object.Simple = 4;
            Assert.AreEqual(9, m_object.Simple);
        }

        [Test]
        public void Test_Resetting_a_value_while_an_effect_is_active_works()
        {
            AddEffect();

            m_object.Simple = 3;
            Assert.AreEqual(7, m_object.Simple);
            m_object.ResetValue(MyObject.SimpleProperty);
            Assert.AreEqual(1, m_object.Simple);
        }

        #endregion

        #region Adding Effects

        [Test]
        public void Test_An_object_has_no_effects_by_default()
        {
            Assert.Collections.IsEmpty(m_object.AppliedEffects);
        }

        [Test]
        public void Test_Can_add_an_effect()
        {
            LocalEffectInstance effectInstance = AddEffect();
            Assert.Collections.Contains(effectInstance, m_object.AppliedEffects);
        }

        [Test]
        public void Test_Adding_an_effect_will_evaluate_it_immediatly()
        {
            AddEffect();
            Assert.AreEqual(1, m_object.Simple);
        }

        [Test]
        public void Test_Adding_an_effect_to_a_property_with_a_value_will_evaluate_it_immediatly()
        {
            m_object.Simple = 3;

            AddEffect();
            Assert.AreEqual(7, m_object.Simple);
        }

        [Test]
        public void Test_Adding_an_effect_to_a_default_value_property_will_evaluate_it_immediatly()
        {
            AddDefaultValueEffect();
            Assert.AreEqual(21, m_object.DefaultValue);
        }

        #endregion

        #region Removing effects

        [Test]
        public void Test_Can_remove_an_effect()
        {
            LocalEffectInstance instance = AddEffect();
            instance.Remove();
            Assert.Collections.IsEmpty(m_object.AppliedEffects);
        }

        [Test]
        public void Test_Removing_an_effect_will_evaluate_the_property_immediatly()
        {
            LocalEffectInstance instance = AddEffect();
            instance.Remove();
            Assert.AreEqual(0, m_object.Simple);
        }

        [Test]
        public void Test_Removing_an_effect_from_a_property_with_a_value_will_evaluate_it_immediatly()
        {
            m_object.Simple = 3;

            LocalEffectInstance instance = AddEffect();
            instance.Remove();
            Assert.AreEqual(3, m_object.Simple);
        }

        [Test]
        public void Test_Removing_an_effect_from_a_default_value_property_will_evaluate_it_immediatly()
        {
            LocalEffectInstance instance = AddDefaultValueEffect();
            instance.Remove();
            Assert.AreEqual(10, m_object.DefaultValue);
        }

        #endregion

        #region Events

        #region Changing

        [Test]
        public void Test_When_the_value_changes_because_adding_an_effect_the_PropertyChanged_event_is_triggered()
        {
            m_object.Simple = 3;

            Assert.EventCalledOnce(m_propertyChangedSink, () => AddEffect());
            Assert.AreEqual(m_object, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangedSink.LastEventArgs.Property);
            Assert.AreEqual(3, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(7, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        [Test]
        public void Test_When_the_value_changes_because_removing_an_effect_the_PropertyChanged_event_is_triggered()
        {
            m_object.Simple = 3;
            LocalEffectInstance effect = AddEffect();

            Assert.EventCalledOnce(m_propertyChangedSink, effect.Remove);
            Assert.AreEqual(m_object, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangedSink.LastEventArgs.Property);
            Assert.AreEqual(7, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(3, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        [Test]
        public void Test_When_the_value_changes_because_changing_the_base_value_the_PropertyChanging_event_is_triggered()
        {
            m_object.Simple = 3;
            AddEffect();

            Assert.EventCalledOnce(m_propertyChangingSink, () => m_object.Simple = 4);
            Assert.AreEqual(m_object, m_propertyChangingSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangingSink.LastEventArgs.Property);
            Assert.AreEqual(3, m_propertyChangingSink.LastEventArgs.OldValue);
            Assert.AreEqual(4, m_propertyChangingSink.LastEventArgs.NewValue);
        }

        [Test]
        public void Test_PropertyChanging_is_not_triggered_when_the_base_value_of_a_property_doesnt_change()
        {
            AddEffect();
            m_object.Simple = 3;

            Assert.AreNotEqual(3, m_object.Simple);

            Assert.EventNotCalled(m_propertyChangingSink, () => m_object.Simple = 3);
        }

        [Test]
        public void Test_PropertyChanging_is_not_triggered_when_the_base_value_of_a_property_with_default_value_doesnt_change()
        {
            AddDefaultValueEffect();

            Assert.AreNotEqual(10, m_object.DefaultValue);

            Assert.EventNotCalled(m_propertyChangingSink, () => m_object.DefaultValue = 10);
        }

        [Test]
        public void Test_Can_cancel_a_property_change()
        {
            m_object.Simple = 3;
            AddEffect();

            m_propertyChangingSink.Callback += (o, e) => e.Cancel = true;

            Assert.EventCalledOnce(m_propertyChangingSink, () => m_object.Simple = 4);
            Assert.AreEqual(m_object, m_propertyChangingSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangingSink.LastEventArgs.Property);
            Assert.AreEqual(3, m_propertyChangingSink.LastEventArgs.OldValue);
            Assert.AreEqual(4, m_propertyChangingSink.LastEventArgs.NewValue);

            Assert.AreEqual(7, m_object.Simple);
        }

        [Test]
        public void Test_When_the_value_changes_because_changing_the_base_value_the_PropertyChanged_event_is_triggered()
        {
            m_object.Simple = 3;
            AddEffect();

            Assert.EventCalledOnce(m_propertyChangedSink, () => m_object.Simple = 4);
            Assert.AreEqual(m_object, m_propertyChangedSink.LastEventArgs.Object);
            Assert.AreEqual(MyObject.SimpleProperty, m_propertyChangedSink.LastEventArgs.Property);
            Assert.AreEqual(7, m_propertyChangedSink.LastEventArgs.OldValue);
            Assert.AreEqual(9, m_propertyChangedSink.LastEventArgs.NewValue);
        }

        [Test]
        public void Test_When_the_value_doesnt_actually_changes_PropertyChanged_event_is_not_triggered()
        {
            m_object.Simple = 3;

            LocalEffectInstance nullEffect = null;

            Assert.EventNotCalled(m_propertyChangedSink, () => nullEffect = AddNullEffect());
            Assert.EventNotCalled(m_propertyChangedSink, nullEffect.Remove);
        }

        #endregion

        #endregion

        #region Transactions

        #region Base value change

        [Test]
        public void Test_Setting_a_value_on_a_property_is_undoable()
        {
            AddEffect();

            Assert.IsUndoRedoable(m_manager.Controller,
                                  () => Assert.AreEqual(1, m_object.Simple),
                                  () => m_object.Simple = 3,
                                  () => Assert.AreEqual(7, m_object.Simple));
        }

        [Test]
        public void Test_Setting_the_same_value_does_nothing()
        {
            AddEffect();
            Assert.AreEqual(1, m_object.Simple);

            Assert.Produces(m_manager.Controller, () => m_object.Simple = 0, 0);
        }

        [Test]
        public void Test_Adding_an_effect_is_undoable()
        {
            Assert.IsUndoRedoable(m_manager.Controller,
                                  () =>
                                  {
                                      Assert.AreEqual(0, m_object.Simple);
                                      Assert.Collections.IsEmpty(m_object.AppliedEffects);
                                  },
                                  () => AddEffect(),
                                  () => Assert.AreEqual(1, m_object.Simple));
        }

        [Test]
        public void Test_Removing_an_effect_is_undoable()
        {
            LocalEffectInstance effect = AddEffect();

            Assert.IsUndoRedoable(m_manager.Controller,
                                  () =>
                                  {
                                      Assert.AreEqual(1, m_object.Simple);
                                      Assert.Collections.Contains(effect, m_object.AppliedEffects);
                                  },
                                  effect.Remove,
                                  () => Assert.AreEqual(0, m_object.Simple));
        }

        #endregion

        #endregion

        #region Sorting

        [Test]
        public void Test_Effects_are_sorted_when_applied_1()
        {
            m_object.Simple = 1;

            AddSortableEffect(1, 1); // a. 1 * 1 + 1 = 2
            AddSortableEffect(2, 2); // b. 2 * 2 + 1 = 5

            Assert.AreEqual(5, m_object.Simple);
        }

        [Test]
        public void Test_Effects_are_sorted_when_applied_2()
        {
            m_object.Simple = 1;
            
            AddSortableEffect(2, 2); // b. 2 * 2 + 1 = 5
            AddSortableEffect(1, 1); // a. 1 * 1 + 1 = 2

            Assert.AreEqual(5, m_object.Simple);
        }

        [Test]
        public void Test_Effects_are_sorted_when_applied_3()
        {
            AddSortableEffect(1, 1); // a. 1 * 1 + 1 = 2
            AddSortableEffect(2, 2); // b. 2 * 2 + 1 = 5

            m_object.Simple = 1;

            Assert.AreEqual(5, m_object.Simple);
        }

        [Test]
        public void Test_Effects_are_sorted_when_applied_4()
        {
            AddSortableEffect(2, 1); // b. 3 * 1 + 1 = 4
            AddSortableEffect(1, 2); // a. 1 * 2 + 1 = 3

            m_object.Simple = 1;

            Assert.AreEqual(4, m_object.Simple);
        }

        [Test]
        public void Test_Effects_are_ultimately_sorted_by_creation_timestamp()
        {
            AddSortableEffect(1, 1); // a. 1 * 1 + 1 = 2
            AddSortableEffect(1, 2); // b. 2 * 2 + 1 = 5

            m_object.Simple = 1;

            Assert.AreEqual(5, m_object.Simple);
        }

        #endregion

        #endregion
    }
}

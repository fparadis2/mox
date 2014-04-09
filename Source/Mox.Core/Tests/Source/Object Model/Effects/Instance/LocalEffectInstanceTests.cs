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
using System.Linq;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class LocalEffectInstanceTests
    {
        #region Inner Types

        private class MyObject : Object
        {
            private int m_property;

            public static readonly Property<int> Property = Property<int>.RegisterProperty<MyObject>("Property", o => o.m_property, PropertyFlags.Modifiable);

            public int PropertyValue
            {
                get { return m_property; }
                set { SetValue(Property, value, ref m_property); }
            }
        }

        private class MyEffect : Effect<int>
        {
            public MyEffect()
                : base(MyObject.Property)
            {
            }

            public override int Modify(Object owner, int value)
            {
                return value * 2 + 1;
            }

            public override int CompareTo(EffectBase other)
            {
                if (other is MyOtherEffect)
                {
                    return 1;
                }

                return 0;
            }
        }

        private class MyOtherEffect : Effect<int>
        {
            public MyOtherEffect()
                : base(MyObject.Property)
            {
            }

            public override int Modify(Object owner, int value)
            {
                return value;
            }

            public override int CompareTo(EffectBase other)
            {
                if (other is MyEffect)
                {
                    return -1;
                }

                return 0;
            }
        }

        #endregion

        #region Variables

        private MockObjectManager m_objectManager;
        private MyObject m_myObject;
        private MyEffect m_effect;

        private LocalEffectInstance m_instance;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_objectManager = new MockObjectManager();
            m_myObject = m_objectManager.Create<MyObject>();
            m_effect = new MyEffect();

            m_instance = m_objectManager.CreateLocalEffect(m_myObject, m_effect);
        }

        #endregion

        #region Utilities

        private int CountOfLocalEffectInstances
        {
            get { return m_objectManager.Objects.Where(obj => obj is LocalEffectInstance).Count(); }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(() => m_objectManager.CreateLocalEffect(null, m_effect));
            Assert.Throws<ArgumentNullException>(() => m_objectManager.CreateLocalEffect(m_myObject, null));
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_effect, m_instance.Effect);
            Assert.AreEqual(m_myObject, m_instance.AffectedObject);
        }

        [Test]
        public void Test_Creation_automatically_adds_in_manager_objects()
        {
            Assert.Collections.Contains(m_instance, m_objectManager.Objects);
        }

        [Test]
        public void Test_Creation_is_undoable()
        {
            int initialCount = CountOfLocalEffectInstances;

            Assert.IsUndoRedoable(m_objectManager.Controller, 
                () => Assert.AreEqual(initialCount, CountOfLocalEffectInstances),
                () => m_objectManager.CreateLocalEffect(m_myObject, m_effect), 
                () => Assert.AreEqual(initialCount + 1, CountOfLocalEffectInstances));
        }

        [Test]
        public void Test_Effect_is_automatically_cached_in_affected_object()
        {
            LocalEffectInstance effectInstance = null;

            Assert.IsUndoRedoable(m_objectManager.Controller,
                () => Assert.IsFalse(m_myObject.AppliedEffects.Contains(effectInstance)),
                () => effectInstance = m_objectManager.CreateLocalEffect(m_myObject, m_effect),
                () => Assert.IsTrue(m_myObject.AppliedEffects.Contains(effectInstance)));
        }

        [Test]
        public void Test_EffectInstances_are_comparable_by_effect()
        {
            LocalEffectInstance minInstance = m_objectManager.CreateLocalEffect(m_myObject, new MyOtherEffect());
            LocalEffectInstance maxInstance = m_objectManager.CreateLocalEffect(m_myObject, new MyEffect());

            Assert.AreEqual(1, maxInstance.CompareTo(minInstance));
            Assert.AreEqual(-1, minInstance.CompareTo(maxInstance));
        }

        #endregion
    }
}

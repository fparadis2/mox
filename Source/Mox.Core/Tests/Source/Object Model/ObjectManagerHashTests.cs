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
    public class ObjectManagerHashTests
    {
        #region Inner Types

        private class MyObject : Object
        {
            public static readonly Property<int> IntProperty = Property<int>.RegisterProperty<MyObject>("Integer", o => o.m_intProperty);
            private int m_intProperty;

            public int Integer
            {
                get { return m_intProperty; }
                set { SetValue(IntProperty, value, ref m_intProperty); }
            }

            public static readonly Property<MyObject> OtherObjectProperty = Property<MyObject>.RegisterProperty<MyObject>("OtherObject", o => o.m_otherObjectProperty);
            private MyObject m_otherObjectProperty;

            public MyObject OtherObject
            {
                get { return m_otherObjectProperty; }
                set { SetValue(OtherObjectProperty, value, ref m_otherObjectProperty); }
            }
        }

        private class MyTrivialDerivedObject : MyObject
        {}

        #endregion

        #region Variables

        private MockObjectManager m_manager;
        private MyObject m_myObject;
        private MyObject m_myObject2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_manager = new MockObjectManager();

            m_myObject = m_manager.CreateAndAdd<MyObject>();
            m_myObject2 = m_manager.CreateAndAdd<MyObject>();
        }

        #endregion

        #region Utilities

        private Hash ComputeHash()
        {
            Hash hash = new Hash();
            m_manager.ComputeHash(hash);
            return hash;
        }

        private void Assert_Hash_changes(Action action)
        {
            Hash baseHash = ComputeHash();

            action();

            Hash newHash = ComputeHash();

            Assert.AreNotEqual(baseHash.Value, newHash.Value);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Hash_changes_when_adding_objects()
        {
            Assert_Hash_changes(() => m_manager.CreateAndAdd<MyObject>());
        }

        [Test]
        public void Test_Hash_changes_when_removing_objects()
        {
            Assert_Hash_changes(() => m_myObject.Remove());
        }

        [Test]
        public void Test_Hash_changes_when_changing_an_integer_property()
        {
            Assert_Hash_changes(() => m_myObject.Integer = 3);
        }

        [Test]
        public void Test_Hash_changes_when_changing_an_object_property()
        {
            Assert_Hash_changes(() => m_myObject.OtherObject = m_myObject);
            Assert_Hash_changes(() => m_myObject.OtherObject = m_myObject2);
            Assert_Hash_changes(() => m_myObject.OtherObject = null);
        }

        [Test]
        public void Test_Hash_depends_on_the_exact_type_of_the_objects()
        {
            m_manager = new MockObjectManager();
            m_manager.CreateAndAdd<MyObject>();
            var hash1 = ComputeHash().Value;

            m_manager = new MockObjectManager();
            m_manager.CreateAndAdd<MyTrivialDerivedObject>();
            var hash2 = ComputeHash().Value;

            Assert.AreNotEqual(hash1, hash2);
        }

        #endregion
    }
}

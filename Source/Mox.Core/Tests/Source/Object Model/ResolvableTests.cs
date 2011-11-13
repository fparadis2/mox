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

namespace Mox.Core.Tests
{
    [TestFixture]
    public class ResolvableTests
    {
        #region Inner Types

        private class MyObject : Object
        {
        }

        #endregion

        #region Variables

        private MockObjectManager m_objectManager;
        private MyObject m_object;

        private MyObject m_objectFromAnotherManager;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_objectManager = new MockObjectManager();
            m_object = m_objectManager.Create<MyObject>();

            MockObjectManager manager2 = new MockObjectManager();
            m_objectFromAnotherManager = manager2.Create<MyObject>();
            Assert.AreEqual(m_object.Identifier, m_objectFromAnotherManager.Identifier, "Sanity check");
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_be_implicitely_converted_from_T()
        {
            Resolvable<MyObject> resolvable = m_object;
            Assert.AreEqual(resolvable.Identifier, m_object.Identifier);
        }

        [Test]
        public void Test_Completely_implements_Equals()
        {
            Resolvable<MyObject> resolvable1 = m_object;
            Resolvable<MyObject> resolvable2 = m_object;
            Resolvable<MyObject> resolvable3 = m_objectManager.Create<MyObject>();

            Assert.AreCompletelyEqual(resolvable1, resolvable2, false);
            Assert.AreCompletelyNotEqual(resolvable1, resolvable3, false);
            Assert.AreCompletelyNotEqual(resolvable2, resolvable3, false);
        }

        [Test]
        public void Test_Resolvables_pointing_same_objects_from_different_managers_are_considered_equal()
        {
            Assert.AreCompletelyEqual((Resolvable<MyObject>)m_object, m_objectFromAnotherManager, false);
        }

        [Test]
        public void Test_Resolve_returns_the_object_in_the_given_manager()
        {
            Resolvable<MyObject> resolvable = m_object;
            Assert.AreEqual(m_object, resolvable.Resolve(m_objectManager));
            Assert.AreEqual(m_objectFromAnotherManager, resolvable.Resolve(m_objectFromAnotherManager.Manager));
        }

        [Test]
        public void Test_Cast_casts_the_resolvable()
        {
            Resolvable<Object> resolvable = m_object;
            Assert.AreEqual(m_object, resolvable.Cast<MyObject>().Resolve(m_objectManager));
        }

        [Test]
        public void Test_Can_resolve_from_an_identifier_and_a_manager()
        {
            MyObject resolved = Resolvable<MyObject>.Resolve(m_objectManager, m_object.Identifier);
            Assert.AreEqual(m_object, resolved);
        }

        [Test]
        public void Test_Can_resolve_from_an_object_and_a_manager()
        {
            Assert.AreEqual(m_object, Resolvable<MyObject>.Resolve(m_objectManager, m_object));
            Assert.AreEqual(m_objectFromAnotherManager, Resolvable<MyObject>.Resolve(m_objectFromAnotherManager.Manager, m_object));
        }

        [Test]
        public void Test_IsSerializable()
        {
            Resolvable<MyObject> resolvable = m_object;
            Resolvable<MyObject> result = Assert.IsSerializable(resolvable);

            // must resolve to get the real object.
            Assert.AreEqual(m_object, result.Resolve(m_objectManager));
        }

        [Test]
        public void Test_IsEmpty_returns_true_for_default_resolvables()
        {
            Resolvable<MyObject> resolvable = m_object;
            Assert.IsFalse(resolvable.IsEmpty);

            Resolvable<MyObject> empty = new Resolvable<MyObject>();
            Assert.IsTrue(empty.IsEmpty);
        }

        [Test]
        public void Test_Empty_returns_an_empty_resolvable()
        {
            Assert.That(Resolvable<MyObject>.Empty.IsEmpty);
        }

        [Test]
        public void Test_Represents_returns_true_if_has_same_identifier_than_object()
        {
            Resolvable<MyObject> resolvable = m_object;
            Assert.IsTrue(resolvable.Is(m_object));
            Assert.IsTrue(resolvable.Is(m_objectFromAnotherManager));
            Assert.IsFalse(resolvable.Is(m_objectManager.Create<MyObject>()));
        }

        #endregion
    }
}

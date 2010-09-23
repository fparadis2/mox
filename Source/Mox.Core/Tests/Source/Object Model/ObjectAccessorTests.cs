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
using System.Linq;

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox
{
    [TestFixture]
    public class ObjectAccessorTests
    {
        #region Inner Types

        private class MyObject : Object
        {
            #region Normal Properties

            public static readonly Property<int> SimpleProperty = Property<int>.RegisterProperty("SimpleAccessor", typeof(MyObject));

            public int Simple
            {
                get { return GetValue(SimpleProperty); }
                set { SetValue(SimpleProperty, value); }
            }

            #endregion
        }

        private class DerivedMyObject : MyObject
        {
            #region Properties

            public static readonly Property<int> DerivedSimpleProperty = Property<int>.RegisterProperty("DerivedAccessor", typeof(MyObject));

            public int DerivedSimple
            {
                get { return GetValue(DerivedSimpleProperty); }
                set { SetValue(DerivedSimpleProperty, value); }
            }

            #endregion
        }

        private class ThirdMyObject : MyObject
        {
            #region Properties

            public static readonly Property<int> OtherProperty = Property<int>.RegisterProperty("OtherProperty", typeof(MyObject));

            #endregion
        }

        #endregion

        #region Variables

        private MockObjectManager m_manager;
        private DerivedMyObject m_object;

        private Object.Accessor m_accessor;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_manager = new MockObjectManager();

            m_object = m_manager.Create<DerivedMyObject>();
            m_manager.Objects.Add(m_object);

            m_accessor = new Object.Accessor(m_object);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new Object.Accessor(null); });
        }

        [Test]
        public void Test_Get_properties_returns_all_the_properties_of_the_object_except_attached_properties()
        {
            List<PropertyBase> properties = new List<PropertyBase>(m_accessor.GetProperties());

            Assert.Collections.AreEquivalent(new PropertyBase[] { MyObject.SimpleProperty, DerivedMyObject.DerivedSimpleProperty, Object.ScopeTypeProperty }, properties);
        }

        [Test]
        public void Test_GetValue()
        {
            m_object.Simple = 10;
            Assert.AreEqual(10, m_accessor.GetValue(MyObject.SimpleProperty));
        }

        #endregion
    }
}

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
using System.Text;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class NewPropertyTests
    {
        #region Inner Types

        private class MyType : Object
        {
            public static Property<int> SimpleProperty = Property<int>.RegisterProperty<MyType>("Simple", t => t.m_simple);
            public int m_simple;

            public int m_another;

            public static Property<int> ModifiableProperty = Property<int>.RegisterProperty<MyType>("Modifiable", t => t.m_modifiable, PropertyFlags.Modifiable);
            private int m_modifiable;

            private void Foo()
            {
                m_simple = 0;
                m_another = 0;
                m_modifiable = 0;
            }
        }

        #endregion

        #region Setup

        [SetUp]
        public void SetUp()
        {
            // Make sure static instances are initialized
            MyType type = new MyType();
        }

        #endregion

        #region Tests

        [Test]
        public void Cannot_register_two_properties_on_the_same_object_with_the_same_key()
        {
            Assert.Throws<ArgumentException>(() => Property<int>.RegisterProperty<MyType>("Simple", t => t.m_another));
        }

        [Test]
        public void Cannot_register_a_property_with_an_invalid_key()
        {
            Assert.Throws<ArgumentNullException>(() => Property<int>.RegisterProperty<MyType>(null, t => t.m_another));
            Assert.Throws<ArgumentNullException>(() => Property<int>.RegisterProperty<MyType>(string.Empty, t => t.m_another));
        }

        [Test]
        public void Test_IsModifiable_checks_for_the_Modifiable_flag()
        {
            Assert.IsTrue(MyType.ModifiableProperty.IsModifiable);
            Assert.IsFalse(MyType.SimpleProperty.IsModifiable);
        }

        #endregion
    }
}

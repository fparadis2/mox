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
    public class PropertyTests
    {
        #region Inner Types

        private class OtherType : Object { }

        #endregion

        #region Tests

        [Test]
        public void Cannot_register_two_properties_on_the_same_object_with_the_same_key()
        {
            Property<bool>.RegisterProperty("Simple", typeof(OtherType));
            Assert.Throws<ArgumentException>(() => Property<int>.RegisterProperty("Simple", typeof(OtherType)));
        }

        [Test]
        public void Cannot_register_a_property_with_an_invalid_key()
        {
            Assert.Throws<ArgumentNullException>(() => Property<bool>.RegisterProperty(null, typeof(OtherType)));
            Assert.Throws<ArgumentNullException>(() => Property<bool>.RegisterProperty(string.Empty, typeof(OtherType)));
        }

        [Test]
        public void Test_Can_have_many_properties_with_the_same_name_accross_different_types()
        {
            Property<int> property1 = Property<int>.RegisterAttachedProperty("TheProperty", typeof(PropertyTests));
            Property<int> property2 = Property<int>.RegisterAttachedProperty("TheProperty", typeof(OtherType));

            MockObjectManager manager = new MockObjectManager();
            OtherType theObject = manager.CreateAndAdd<OtherType>();
            
            theObject.SetValue(property1, 3);
            theObject.SetValue(property2, 3);

            Assert.AreEqual(3, theObject.GetValue(property1));
            Assert.AreEqual(3, theObject.GetValue(property2));
        }

        [Test]
        public void Test_Properties_are_indexed_by_their_global_index()
        {
            Property<int> property1 = Property<int>.RegisterAttachedProperty("IndexedProperty", typeof(PropertyTests));
            Property<int> property2 = Property<int>.RegisterAttachedProperty("IndexedProperty", typeof(OtherType));

            Assert.IsNull(PropertyBase.AllProperties[-1]);
            Assert.AreEqual(property1, PropertyBase.AllProperties[property1.GlobalIndex]);
            Assert.AreEqual(property2, PropertyBase.AllProperties[property2.GlobalIndex]);
        }

        [Test]
        public void Test_IsModifiable_checks_for_the_Modifiable_flag()
        {
            Property<int> modifiable = Property<int>.RegisterAttachedProperty("Modifiable", typeof(OtherType), PropertyFlags.Modifiable);
            Property<int> nonModifiable = Property<int>.RegisterAttachedProperty("NonModifiable", typeof(OtherType), PropertyFlags.None);

            Assert.IsTrue(modifiable.IsModifiable);
            Assert.IsFalse(nonModifiable.IsModifiable);
        }

        #endregion
    }
}

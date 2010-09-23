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
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Mox.Transactions;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox
{
    [TestFixture]
    public class EffectTests
    {
        #region Inner Types

        private class MyObject : Object
        {
            public static readonly Property<int> MyProperty = Property<int>.RegisterProperty("MyProperty", typeof(MyObject), PropertyFlags.Modifiable);
            public static readonly Property<int> NonModifiableProperty = Property<int>.RegisterProperty("NonModifiableProperty", typeof(MyObject));
        }

        private class MyEffect : Effect<int>
        {
            public MyEffect(Property<int> property)
                : base(property)
            {
            }

            public override int Modify(Object owner, int value)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Variables

        private MockRepository m_mockery;

        private Effect<int> m_effect;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_effect = m_mockery.StrictMock<Effect<int>>(MyObject.MyProperty);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(MyObject.MyProperty, m_effect.Property);
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(() => new MyEffect(null));
            Assert.Throws<ArgumentException>(() => new MyEffect(MyObject.NonModifiableProperty));
        }

        [Test]
        public void Test_CompareTo_returns_0_by_default()
        {
            Assert.AreEqual(0, m_effect.CompareTo(new MyEffect(MyObject.MyProperty)));
        }

        #endregion
    }
}

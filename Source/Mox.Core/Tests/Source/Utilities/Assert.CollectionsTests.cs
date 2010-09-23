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
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class AssertCollectionTests
    {
        #region Inner Types

        private class TestObject
        {
            public int Value;

            public override bool Equals(object obj)
            {
                return obj is TestObject && ((TestObject)obj).Value == Value;
            }

            public override int GetHashCode()
            {
                return Value;
            }
        }

        #endregion

        #region Utilities

        private IEnumerable<TestObject> CreateTestObjects(params int[] values)
        {
            return values.Select(value => new TestObject { Value = value });
        }

        #endregion

        #region Tests

        #region Count

        [Test]
        public void Test_IsEmpty()
        {
            Assert.Throws<ArgumentNullException>(delegate { Assert.Collections.IsEmpty(null); });

            Assert.Collections.IsEmpty(new int[0]);
            Assert.Collections.IsEmpty(new List<int>());
            Assert.Collections.IsEmpty((IEnumerable)new List<int>());

            Assert.Throws<AssertionException>(delegate { Assert.Collections.IsEmpty(new int[] { 1 }); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.IsEmpty(new int[] { 1, 2 }); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.IsEmpty(new List<int>(new int[] { 1, 2 })); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.IsEmpty((IEnumerable)new List<int>(new int[] { 1, 2 })); });
        }

        [Test]
        public void Test_CountEquals()
        {
            Assert.Throws<ArgumentNullException>(delegate { Assert.Collections.CountEquals(0, null); });

            Assert.Collections.CountEquals(0, new int[0]);
            Assert.Collections.CountEquals(0, new List<int>());
            Assert.Collections.CountEquals(0, (IEnumerable)new List<int>());

            Assert.Collections.CountEquals(1, new int[] { 1 });
            Assert.Collections.CountEquals(3, new int[] { 1, 2, 10 });

            Assert.Throws<AssertionException>(delegate { Assert.Collections.CountEquals(0, new int[] { 1 }); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.CountEquals(1, new int[] { 1, 2 }); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.CountEquals(10, new List<int>(new int[] { 1, 2 })); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.CountEquals(-1, (IEnumerable)new List<int>(new int[] { 1, 2 })); });
        }

        #endregion

        #region Contains

        [Test]
        public void Test_Contains()
        {
            Assert.Throws<ArgumentNullException>(delegate { Assert.Collections.Contains(null, null); });

            Assert.Collections.Contains(1, new int[] { 1 });
            Assert.Collections.Contains(1, new List<int>(new int[] { 1 }));
            Assert.Collections.Contains(1, (IEnumerable)new List<int>(new int[] { 1 }));
            Assert.Collections.Contains(1, (IList)new List<int>(new int[] { 1 }));

            Assert.Collections.Contains(1, new int[] { 0, 1, 2 });

            Assert.Throws<AssertionException>(delegate { Assert.Collections.Contains(5, new int[] { 1 }); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.Contains(5, new List<int>(new int[] { 1 })); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.Contains(5, (IEnumerable)new List<int>(new int[] { 1 })); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.Contains(5, (IList)new List<int>(new int[] { 1 })); });

            Assert.Throws<AssertionException>(delegate { Assert.Collections.Contains(5, new int[] { 0, 1, 2 }); });
        }

        [Test]
        public void Test_Uses_Equals_to_test_for_equality()
        {
            TestObject a = new TestObject() { Value = 3 };
            TestObject b = new TestObject() { Value = 3 };
            TestObject c = new TestObject() { Value = 4 };

            Assert.Collections.Contains(a, new TestObject[] { a });
            Assert.Collections.Contains(a, new TestObject[] { b });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.Contains(a, new TestObject[] { c }); });
        }

        #endregion

        #region Equivalence / Equality

        [Test]
        public void Test_AreEquivalent()
        {
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEquivalent(null, new int[0]); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEquivalent(new int[0], null); });

            Assert.Collections.AreEquivalent(new int[0], new int[0]);
            Assert.Collections.AreEquivalent(new int[] { 1 }, new int[] { 1 });
            Assert.Collections.AreEquivalent(new int[] { 1, 2 }, new int[] { 1, 2 });
            Assert.Collections.AreEquivalent(new int[] { 1, 2, 3 }, new int[] { 3, 1, 2 });
            Assert.Collections.AreEquivalent(new int[] { 1, 2, 3 }, new int[] { 3, 2, 1 });

            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEquivalent(new int[0], new int[] { 1 }); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEquivalent(new int[] { 1 }, new int[1]); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEquivalent(new int[] { 1, 2 }, new int[] { 1, 3 }); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEquivalent(new int[] { 1, 2 }, new int[] { 2, 4, 5 }); });
        }

        [Test]
        public void Test_AreEquivalent_uses_Equals_for_equality()
        {
            Assert.Collections.AreEquivalent(CreateTestObjects(1, 2), CreateTestObjects(2, 1));
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEquivalent(CreateTestObjects(1, 2), CreateTestObjects(3, 1)); });
        }

        [Test]
        public void Test_AreEqual()
        {
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEqual(null, new int[0]); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEqual(new int[0], null); });

            Assert.Collections.AreEqual(new int[0], new int[0]);
            Assert.Collections.AreEqual(new int[] { 1 }, new int[] { 1 });
            Assert.Collections.AreEqual(new int[] { 1, 2 }, new int[] { 1, 2 });
            Assert.Collections.AreEqual(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 });

            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEqual(new int[0], new int[] { 1 }); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEqual(new int[] { 1 }, new int[1]); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEqual(new int[] { 1, 2 }, new int[] { 2, 1 }); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEqual(new int[] { 1, 2, 3 }, new int[] { 1, 4, 5 }); });
        }

        [Test]
        public void Test_AreEqual_uses_Equals_for_equality()
        {
            Assert.Collections.AreEqual(CreateTestObjects(1, 2), CreateTestObjects(1, 2));
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEqual(CreateTestObjects(1, 2), CreateTestObjects(2, 1)); });
            Assert.Throws<AssertionException>(delegate { Assert.Collections.AreEqual(CreateTestObjects(1, 2), CreateTestObjects(1, 3)); });
        }

        #endregion

        #endregion
    }
}

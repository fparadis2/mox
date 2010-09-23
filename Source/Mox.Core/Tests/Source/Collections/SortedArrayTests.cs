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

namespace Mox.Collections
{
    public abstract class SortedArrayTestsBase
    {
        #region Variables

        protected SortedArray<int> m_array;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_array = CreateArray();
        }

        protected abstract SortedArray<int> CreateArray();

        #endregion

        #region Utilities

        private void Assert_List_Contains(params int[] values)
        {
            List<int> sortedList = new List<int>(values.Length);
            sortedList.AddRange(values);
            sortedList.Sort(m_array.Comparer);

            Assert.AreEqual(values.Length, m_array.Count);
            Assert.Collections.AreEqual(sortedList, m_array);
        }

        private IEnumerable<int> Sort(IEnumerable<int> collection)
        {
            List<int> list = new List<int>(collection);
            list.Sort(m_array.Comparer);
            return list;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_List_is_empty_by_default()
        {
            Assert_List_Contains();
        }

        [Test]
        public void Test_Can_add_elements_to_the_list()
        {
            m_array.Add(1);
            m_array.Add(2);
            m_array.Add(3);

            Assert_List_Contains(1, 2, 3);
        }

        [Test]
        public void Test_Can_add_many_elements_to_the_list()
        {
            m_array.Add(1);
            m_array.Add(2);
            m_array.Add(3);
            m_array.Add(4);
            m_array.Add(5);
            m_array.Add(6);
            m_array.Add(7);
            m_array.Add(8);
            m_array.Add(9);
            m_array.Add(8);
            m_array.Add(7);

            Assert_List_Contains(1, 2, 3, 4, 5, 6, 7, 7, 8, 8, 9);
        }

        [Test]
        public void Test_Can_add_the_same_element_more_than_once()
        {
            m_array.Add(1);
            m_array.Add(1);
            m_array.Add(2);

            Assert_List_Contains(1, 1, 2);
        }

        [Test]
        public void Test_Items_are_added_in_order()
        {
            m_array.Add(3);
            m_array.Add(2);
            m_array.Add(1);

            Assert_List_Contains(1, 2, 3);
        }

        [Test]
        public void Test_Can_remove_elements()
        {
            m_array.Add(1);
            m_array.Add(2);
            Assert.That(m_array.Remove(1));

            Assert_List_Contains(2);
        }

        [Test]
        public void Test_removing_a_non_existing_element_does_nothing_but_returns_false()
        {
            m_array.Add(1);
            m_array.Add(2);
            Assert.IsFalse(m_array.Remove(3));

            Assert_List_Contains(1, 2);
        }

        [Test]
        public void Test_Can_clear_the_collection()
        {
            m_array.Add(1);
            m_array.Add(1);
            m_array.Add(2);
            m_array.Clear();

            Assert_List_Contains();
        }

        [Test]
        public void Test_Contains_returns_true_if_the_given_element_is_in_the_collection()
        {
            m_array.Add(1);
            m_array.Add(1);
            m_array.Add(2);

            Assert.IsTrue(m_array.Contains(1));
            Assert.IsTrue(m_array.Contains(2));
            Assert.IsFalse(m_array.Contains(3));
        }

        [Test]
        public void Test_CopyTo()
        {
            m_array.Add(1);
            m_array.Add(1);
            m_array.Add(2);

            IList<int> sorted = new[] { 1, 1, 2 };
            sorted = new List<int>(Sort(sorted));

            int[] array = new int[4];
            m_array.CopyTo(array, 1);

            Assert.AreEqual(0, array[0]);
            Assert.AreEqual(sorted[0], array[1]);
            Assert.AreEqual(sorted[1], array[2]);
            Assert.AreEqual(sorted[2], array[3]);
        }

        [Test]
        public void Test_Array_is_not_readonly()
        {
            Assert.IsFalse(m_array.IsReadOnly);
        }

        [Test]
        public void Test_Cannot_insert_at_a_given_index()
        {
            Assert.Throws<InvalidOperationException>(() => m_array.Insert(0, 1));
        }

        [Test]
        public void Test_Can_remove_at_the_given_index()
        {
            m_array.Add(1);
            m_array.Add(1);
            m_array.Add(2);
            m_array.RemoveAt(1);

            Assert_List_Contains(1, 2);

            Assert.Throws<IndexOutOfRangeException>(() => m_array[-1].ToString());
            Assert.Throws<IndexOutOfRangeException>(() => m_array[2].ToString());
        }

        [Test]
        public void Test_Can_get_the_item_at_a_given_index()
        {
            m_array.Add(1);
            m_array.Add(1);
            m_array.Add(2);

            IList<int> sorted = new[] { 1, 1, 2 };
            sorted = new List<int>(Sort(sorted));

            Assert.AreEqual(sorted[0], m_array[0]);
            Assert.AreEqual(sorted[1], m_array[1]);
            Assert.AreEqual(sorted[2], m_array[2]);

            Assert.Throws<IndexOutOfRangeException>(() => m_array[-1].ToString());
            Assert.Throws<IndexOutOfRangeException>(() => m_array[3].ToString());
        }

        [Test]
        public void Test_Cannot_set_the_item_at_a_given_index()
        {
            m_array.Add(1);

            Assert.Throws<InvalidOperationException>(() => m_array[0] = 1);
        }

        #endregion
    }

    [TestFixture]
    public class SortedArrayTests_WithDefaultComparer : SortedArrayTestsBase
    {
        protected override SortedArray<int> CreateArray()
        {
            return new SortedArray<int>();
        }

        #region Tests

        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(() => new SortedArray<int>(null));
        }

        [Test]
        public void Test_IndexOf_returns_the_lowest_index_of_the_given_item()
        {
            m_array.Add(1);
            m_array.Add(1);
            m_array.Add(3);

            Assert.AreEqual(~0, m_array.IndexOf(0));
            Assert.AreEqual(0, m_array.IndexOf(1));
            Assert.AreEqual(~2, m_array.IndexOf(2));
            Assert.AreEqual(2, m_array.IndexOf(3));
            Assert.AreEqual(~3, m_array.IndexOf(4));
        }

        #endregion
    }

    [TestFixture]
    public class SortedArrayTests_WithCustomComparer : SortedArrayTestsBase
    {
        protected override SortedArray<int> CreateArray()
        {
            return new SortedArray<int>(new CustomComparer());
        }

        #region Inner Types

        private class CustomComparer : IComparer<int>
        {
            #region Implementation of IComparer<int>

            public int Compare(int x, int y)
            {
                return y.CompareTo(x);
            }

            #endregion
        }

        #endregion

        #region Tests

        [Test]
        public void Test_IndexOf_returns_the_lowest_index_of_the_given_item()
        {
            m_array.Add(1);
            m_array.Add(1);
            m_array.Add(3);

            Assert.AreEqual(~3, m_array.IndexOf(0));
            Assert.AreEqual(1, m_array.IndexOf(1));
            Assert.AreEqual(~1, m_array.IndexOf(2));
            Assert.AreEqual(0, m_array.IndexOf(3));
            Assert.AreEqual(~0, m_array.IndexOf(4));
        }

        #endregion
    }

    [TestFixture]
    public class SortedArrayTests_WithClass
    {
        #region Inner Types

        private class MyClass : IComparable<MyClass>, IComparable
        {
            private readonly int m_order;

            public MyClass(int order)
            {
                m_order = order;
            }

            public int CompareTo(MyClass other)
            {
                return m_order.CompareTo(other.m_order);
            }

            public int CompareTo(object obj)
            {
                return CompareTo((MyClass)obj);
            }
        }

        #endregion

        #region Variables

        private SortedArray<MyClass> m_array;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_array = new SortedArray<MyClass>();
        }

        #endregion

        #region Utility

        private void Assert_IsSame(params MyClass[] elements)
        {
            Assert.Collections.CountEquals(elements.Length, m_array);

            for (int i = 0; i < elements.Length; i++)
            {
                Assert.AreSame(elements[i], m_array[i]);
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_SimpleAdd()
        {
            MyClass a = new MyClass(0);
            MyClass b = new MyClass(1);
            MyClass c = new MyClass(2);

            m_array.Add(a);
            m_array.Add(c);
            m_array.Add(b);

            Assert_IsSame(a, b, c);
        }

        [Test]
        public void Test_Equal_objects_are_added_at_the_end()
        {
            MyClass a = new MyClass(1);
            MyClass b = new MyClass(2);
            MyClass c = new MyClass(1);

            m_array.Add(c);
            m_array.Add(b);
            m_array.Add(a);

            Assert_IsSame(c, a, b);
        }

        [Test]
        public void Test_Removal_works_with_reference_equality_1()
        {
            MyClass a = new MyClass(1);
            MyClass b = new MyClass(1);

            m_array.Add(a);
            m_array.Add(b);

            m_array.Remove(a);

            Assert_IsSame(b);
        }

        [Test]
        public void Test_Removal_works_with_reference_equality_2()
        {
            MyClass a = new MyClass(1);
            MyClass b = new MyClass(1);

            m_array.Add(a);
            m_array.Add(b);

            m_array.Remove(b);

            Assert_IsSame(a);
        }

        [Test]
        public void Test_Contains_works_with_reference_equality()
        {
            MyClass a = new MyClass(1);
            MyClass b = new MyClass(1);
            MyClass c = new MyClass(1);

            m_array.Add(a);
            m_array.Add(b);

            Assert.IsTrue(m_array.Contains(a));
            Assert.IsTrue(m_array.Contains(b));
            Assert.IsFalse(m_array.Contains(c));
        }

        #endregion
    }
}

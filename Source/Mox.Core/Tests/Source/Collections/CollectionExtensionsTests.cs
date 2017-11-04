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
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Mox.Collections;
using System.Collections.Generic;

namespace Mox
{
    [TestFixture]
    public class CollectionExtensionsTests
    {
        #region Variables

        private MockRepository m_mockery;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
        }

        #endregion

        #region Utility

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

        private static IEnumerable<TestObject> CreateTestObjects(params int[] values)
        {
            return values.Select(value => new TestObject { Value = value });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Join()
        {
            Assert.AreEqual("1-2-3", new[] { 1, 2, 3 }.Join("-"));
            Assert.AreEqual(string.Empty, new int[0].Join("-"));
            Assert.AreEqual("3-_-4", new[] { 3, 4 }.Join("-_-"));
            Assert.AreEqual("34", new[] { 3, 4 }.Join(""));
            Assert.AreEqual("34", new[] { 3, 4 }.Join(null));
        }

        [Test]
        public void Test_ForEach()
        {
            int sum = 0;
            new[] { 1, 2, 3, 4 }.ForEach(i => sum += i);
            Assert.AreEqual(10, sum);
        }

        [Test]
        public void Test_Dispose()
        {
            IDisposable item1 = m_mockery.StrictMock<IDisposable>();
            IDisposable item2 = m_mockery.StrictMock<IDisposable>();

            using (m_mockery.Ordered())
            {
                item1.Dispose();
                item2.Dispose();
            }

            m_mockery.Test(() => new[] { item1, item2 }.Dispose());
        }

        [Test]
        public void Test_Synchronize_With_Add()
        {
            IObservableCollection<int> collection = new ObservableCollection<int> { 1, 2 };

            int lastAdd = -1;
            collection.CollectionChanged += ((sender, e) => e.Synchronize(i => lastAdd = i, null));

            collection.Add(1); Assert.AreEqual(1, lastAdd);
            collection.Add(99); Assert.AreEqual(99, lastAdd);
        }

        [Test]
        public void Test_Synchronize_With_Remove()
        {
            IObservableCollection<int> collection = new ObservableCollection<int> { 1, 2 };

            int lastRemove = -1;
            collection.CollectionChanged += ((sender, e) => e.Synchronize(null, i => lastRemove = i));

            Assert.IsFalse(collection.Remove(99)); Assert.AreEqual(-1, lastRemove);
            Assert.IsTrue(collection.Remove(1)); Assert.AreEqual(1, lastRemove);
        }

        [Test]
        public void Test_Synchronize_With_Clear()
        {
            IObservableCollection<int> collection = new ObservableCollection<int> { 1, 2 };

            int totalRemoved = 0;
            collection.CollectionChanged += ((sender, e) => e.Synchronize(null, i => totalRemoved += i));

            collection.Clear(); Assert.AreEqual(3, totalRemoved);
        }

        [Test]
        public void Test_Cannot_call_synchronize_with_a_null_arg_or_invalid_arg()
        {
            Assert.Throws<ArgumentNullException>(() => ((CollectionChangedEventArgs<int>)null).Synchronize(null, null));
            Assert.Throws<NotImplementedException>(() => new CollectionChangedEventArgs<int>((CollectionChangeAction)0xFFFF, new int[0]).Synchronize(null, null));
        }

        [Test]
        public void Test_GetSafeValue_returns_the_value_or_the_default_value_if_not_found_value_types()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int> { { "One", 1 }, { "Two", 2 } };

            Assert.AreEqual(1, dictionary.SafeGetValue("One"));
            Assert.AreEqual(2, dictionary.SafeGetValue("Two"));
            Assert.AreEqual(0, dictionary.SafeGetValue("Invalid"));
        }

        [Test]
        public void Test_GetSafeValue_returns_the_value_or_the_default_value_if_not_found_reference_types()
        {
            object obj = new object();

            Dictionary<string, object> dictionary = new Dictionary<string, object> { { "One", obj } };

            Assert.AreSame(obj, dictionary.SafeGetValue("One"));
            Assert.AreEqual(null, dictionary.SafeGetValue("Invalid"));
        }

        [Test]
        public void Test_Enumerate_just_enumerates_over_the_collection()
        {
            IEnumerable<object> enumerable = m_mockery.StrictMock<IEnumerable<object>>();
            IEnumerator<object> enumerator = m_mockery.StrictMock<IEnumerator<object>>();

            Expect.Call(enumerable.GetEnumerator()).Return(enumerator);

            Expect.Call(enumerator.MoveNext()).Return(true); Expect.Call(enumerator.Current).Return(new object());
            Expect.Call(enumerator.MoveNext()).Return(true); Expect.Call(enumerator.Current).Return(new object());
            Expect.Call(enumerator.MoveNext()).Return(false);
            enumerator.Dispose();

            m_mockery.Test(() => enumerable.Enumerate());
        }

        [Test]
        public void Test_Enumerate_is_nice_and_allows_null_argument()
        {
            const IEnumerable<object> nullEnumerable = null;
            nullEnumerable.Enumerate();
        }

        [Test]
        public void Test_ToArrayList_transforms_into_an_untyped_enumerable_into_an_ArrayList()
        {
            int[] array = new[] { 1, 2, 3 };

            ArrayList list = array.ToArrayList();
            Assert.Collections.AreEqual(array, list);
        }

        #region IsEquivalent

        [Test]
        public void Test_IsEquivalent_throws_on_null_collection()
        {
            List<object> list = new List<object>();
            Assert.Throws<ArgumentNullException>(() => list.IsEquivalent(null));
        }

        [Test]
        public void Test_IsEquivalent_returns_true_for_equivalent_collections()
        {
            int[] array = new[] { 1, 2, 3 };

            Assert.IsTrue(array.IsEquivalent(array));
            Assert.IsTrue(array.IsEquivalent(new[] { 1, 2, 3 }));
            Assert.IsTrue(array.IsEquivalent(new[] { 1, 3, 2 }));
            Assert.IsTrue(array.IsEquivalent(new[] { 3, 2, 1 }));
            Assert.IsTrue(new int[0].IsEquivalent(new int[0]));
        }

        [Test]
        public void Test_IsEquivalent_returns_false_otherwise()
        {
            int[] array = new[] { 1, 2, 3 };

            Assert.IsFalse(array.IsEquivalent(new[] { 1, 2 }));
            Assert.IsFalse(array.IsEquivalent(new[] { 1, 2, 3, 4 }));
            Assert.IsFalse(array.IsEquivalent(new[] { 1, 2, 3, 3 }));
            Assert.IsFalse(array.IsEquivalent(new[] { 1, 2, 4 }));
            Assert.IsFalse(array.IsEquivalent(new int[0]));
        }

        [Test]
        public void Test_IsEquivalent_uses_Equals()
        {
            var array = CreateTestObjects(1, 2, 3);

            Assert.IsTrue(array.IsEquivalent(CreateTestObjects(1, 2, 3)));
            Assert.IsTrue(array.IsEquivalent(CreateTestObjects(2, 1, 3)));
            Assert.IsFalse(array.IsEquivalent(CreateTestObjects(1)));
        }

        #endregion

        #region Permutations / Combinations

        [Test]
        public void Test_EnumerateCombinations_invalid_arguments()
        {
            int[] array = new[] { 1, 2, 3 };

            Assert.Throws<ArgumentOutOfRangeException>(() => array.EnumerateCombinations(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => array.EnumerateCombinations(4));
        }

        [Test]
        public void Test_EnumerateCombinations()
        {
            int[] array = new[] { 1, 2, 3, 4 };

            Assert_Combinations_Are(array, 0, new int[,] { { } });
            Assert_Combinations_Are(array, 1, new[,] { { 1 }, { 2 }, { 3 }, { 4 } });
            Assert_Combinations_Are(array, 2, new[,] { { 1, 2 }, { 1, 3 }, { 1, 4 }, { 2, 3 }, { 2, 4 }, { 3, 4 } });
            Assert_Combinations_Are(array, 3, new[,] { { 1, 2, 3 }, { 1, 2, 4 }, { 1, 3, 4 }, { 2, 3, 4 } });
            Assert_Combinations_Are(array, 4, new[,] { { 1, 2, 3, 4 } });
        }

        [Test]
        public void Test_EnumerateCombinationsWithRepetition_invalid_arguments()
        {
            int[] array = new[] { 1, 2, 3 };

            Assert.Throws<ArgumentOutOfRangeException>(() => array.EnumerateCombinationsWithRepetitions(-1));
        }

        [Test]
        public void Test_EnumerateCombinationsWithRepetition_with_more_requested_than_available()
        {
            int[] array = new[] { 0, 1 };

            Assert_CombinationsWithRepetitions_Are(array, 2, new[,] { { 0, 0 }, { 0, 1 }, { 1, 1 } });
            Assert_CombinationsWithRepetitions_Are(array, 3, new[,] { { 0, 0, 0 }, { 0, 0, 1 }, { 0, 1, 1 }, { 1, 1, 1 } });
        }

        [Test]
        public void Test_EnumerateCombinationsWithRepetition()
        {
            int[] array = new[] { 1, 2, 3, 4 };

            Assert_CombinationsWithRepetitions_Are(array, 0, new int[,] { { } });
            Assert_CombinationsWithRepetitions_Are(array, 1, new[,] { { 1 }, { 2 }, { 3 }, { 4 } });
            Assert_CombinationsWithRepetitions_Are(array, 2, new[,] { { 1, 1 }, { 1, 2 }, { 1, 3 }, { 1, 4 }, { 2, 2 }, { 2, 3 }, { 2, 4 }, { 3, 3 }, { 3, 4 }, { 4, 4 } });
            Assert_CombinationsWithRepetitions_Are(array, 3, new[,] { { 1, 1, 1 }, { 1, 1, 2 }, { 1, 1, 3 }, { 1, 1, 4 }, { 1, 2, 2 }, { 1, 2, 3 }, { 1, 2, 4 }, { 1, 3, 3 }, { 1, 3, 4 }, { 1, 4, 4 }, { 2, 2, 2 }, { 2, 2, 3 }, { 2, 2, 4 }, { 2, 3, 3 }, { 2, 3, 4 }, { 3, 3, 3 }, { 3, 3, 4 }, { 4, 4, 2 }, { 4, 4, 3 }, { 4, 4, 4 } });
        }

        [Test]
        public void Test_EnumeratePermutations_invalid_arguments()
        {
            int[] array = new[] { 1, 2, 3 };

            Assert.Throws<ArgumentOutOfRangeException>(() => array.EnumeratePermutations(-1));
        }

        [Test]
        public void Test_EnumeratePermutations_with_more_requested_than_available()
        {
            int[] array = new[] { 0, 1 };

            Assert_Permutations_Are(array, 2, new[,] { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 1, 1 } });
            Assert_Permutations_Are(array, 3, new[,] { { 0, 0, 0 }, { 0, 0, 1 }, { 0, 0, 1 }, { 0, 1, 1 }, { 1, 0, 0 }, { 1, 0, 1 }, { 1, 1, 0 }, { 1, 1, 1 } });
        }

        [Test]
        public void Test_EnumeratePermutations()
        {
            int[] array = new[] { 1, 2, 3, 4 };

            Assert_Permutations_Are(array, 0, new int[,] { { } });
            Assert_Permutations_Are(array, 1, new[,] { { 1 }, { 2 }, { 3 }, { 4 } });
            Assert_Permutations_Are(array, 2, new[,] { { 1, 1 }, { 1, 2 }, { 1, 3 }, { 1, 4 }, { 2, 1 }, { 2, 2 }, { 2, 3 }, { 2, 4 }, { 3, 1 }, { 3, 2 }, { 3, 3 }, { 3, 4 }, { 4, 1 }, { 4, 2 }, { 4, 3 }, { 4, 4 } });
        }

        [Test]
        public void Test_EnumerateGroupCombinations_invalid_arguments()
        {
            int[] array = new[] { 1, 2, 3 };

            Assert.Throws<ArgumentOutOfRangeException>(() => array.EnumerateGroupCombinations(-1));
        }

        [Test]
        public void Test_EnumerateGroupCombinations()
        {
            int[] array = new[] { 1, 2, 3, 1 };

            Assert_Group_Combinations_Are(array, 0, new int[,] { { } });
            Assert_Group_Combinations_Are(array, 1, new[,] { { 0 }, { 1 }, { 2 }, { 3 } });
            Assert_Group_Combinations_Are(array, 2, new[,] { { 0, 1 }, { 0, 2 }, { 0, 3 }, { 1, 1 }, { 1, 2 }, { 1, 3 }, { 2, 2 }, { 2, 3 } });
            Assert_Group_Combinations_Are(array, 3, new[,] { { 0, 1, 1 }, { 0, 1, 2 }, { 0, 1, 3 }, { 0, 2, 2 }, { 0, 2, 3 }, { 1, 1, 2 }, { 1, 1, 3 }, { 1, 2, 2 }, { 1, 2, 3 }, { 2, 2, 2 }, { 2, 2, 3 } });
            Assert_Group_Combinations_Are(array, 4, new[,] { { 0, 1, 1, 2 }, { 0, 1, 1, 3 }, { 0, 1, 2, 2 }, { 0, 1, 2, 3 }, { 0, 2, 2, 2 }, { 0, 2, 2, 3 }, { 1, 1, 2, 2 }, { 1, 1, 2, 3 }, { 1, 2, 2, 2 }, { 1, 2, 2, 3 }, { 2, 2, 2, 3 } });
        }

        private static void Assert_Combinations_Are<T>(IList<T> array, int size, T[,] combinations)
        {
            Assert_Combinations_AreImpl(array, size, combinations, CollectionExtensions.EnumerateCombinations);
        }

        private static void Assert_CombinationsWithRepetitions_Are<T>(IList<T> array, int size, T[,] combinations)
        {
            Assert_Combinations_AreImpl(array, size, combinations, CollectionExtensions.EnumerateCombinationsWithRepetitions);
        }

        private static void Assert_Permutations_Are<T>(IList<T> array, int size, T[,] combinations)
        {
            Assert_Combinations_AreImpl(array, size, combinations, CollectionExtensions.EnumeratePermutations);
        }

        private static void Assert_Group_Combinations_Are(IList<int> array, int size, int[,] combinations)
        {
            Assert_Combinations_AreImpl(array, size, combinations, CollectionExtensions.EnumerateGroupCombinations);
        }

        private static void Assert_Combinations_AreImpl<T>(IList<T> array, int size, T[,] combinations, Func<IList<T>, int, IEnumerable<IList<T>>> action)
        {
            List<IList<T>> newCombinations = new List<IList<T>>();

            Assert.AreEqual(size, combinations.GetLength(1));

            for (int i = 0; i < combinations.GetLength(0); i++)
            {
                List<T> list = new List<T>();
                for (int j = 0; j < combinations.GetLength(1); j++)
                {
                    list.Add(combinations[i, j]);
                }
                newCombinations.Add(list);
            }

            Assert_Combinations_AreImpl(array, size, newCombinations, action);
        }

        private static void Assert_Combinations_AreImpl<T>(IList<T> array, int size, IEnumerable<IList<T>> combinations, Func<IList<T>, int, IEnumerable<IList<T>>> action)
        {
            var actualCombinations = action(array, size).Select(list => new Equivalent<T>(list));
            var expectedCombinations = combinations.Select(list => new Equivalent<T>(list));
            Assert.Collections.AreEquivalent(expectedCombinations, actualCombinations);
        }

        private class Equivalent<T>
        {
            private readonly List<T> m_elements = new List<T>();

            public Equivalent(IEnumerable<T> elements)
            {
                if (elements != null)
                {
                    m_elements.AddRange(elements);
                }
            }

            public override bool Equals(object obj)
            {
                return ((Equivalent<T>)obj).m_elements.IsEquivalent(m_elements);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return "{" + m_elements.Join(", ") + "}";
            }
        }

        #endregion

        #region Sort

        [Test]
        public void Test_SortAndRemoveDuplicates_doesnt_do_anything_on_an_empty_list()
        {
            List<int> empty = new List<int>();
            empty.SortAndRemoveDuplicates();
            Assert.Collections.IsEmpty(empty);
        }

        [Test]
        public void Test_SortAndRemoveDuplicates_with_an_already_sorted_and_distinct_list_does_nothing()
        {
            List<int> sorted = new List<int> { 1, 2, 3, 4 };
            sorted.SortAndRemoveDuplicates();
            Assert.Collections.AreEqual(new [] { 1, 2, 3, 4 }, sorted);
        }

        [Test]
        public void Test_SortAndRemoveDuplicates_with_an_unsorted_distinct_list_will_sort_the_list()
        {
            List<int> sorted = new List<int> { 4, 1, 3, 2 };
            sorted.SortAndRemoveDuplicates();
            Assert.Collections.AreEqual(new[] { 1, 2, 3, 4 }, sorted);
        }

        [Test]
        public void Test_SortAndRemoveDuplicates_with_an_already_sorted_list_removes_the_duplicates()
        {
            List<int> sorted = new List<int> { 1, 1, 1, 2, 2, 3, 4, 4 };
            sorted.SortAndRemoveDuplicates();
            Assert.Collections.AreEqual(new[] { 1, 2, 3, 4 }, sorted);
        }

        [Test]
        public void Test_SortAndRemoveDuplicates_with_an_unsorted_list_with_duplicates()
        {
            List<int> sorted = new List<int> { 1, 4, 1, 2, 3, 4, 4, 1, 2 };
            sorted.SortAndRemoveDuplicates();
            Assert.Collections.AreEqual(new[] { 1, 2, 3, 4 }, sorted);
        }

        #endregion

        #region Binary Search

        [Test]
        public void Test_BinarySearch_searches_in_a_sorted_array_and_returns_twos_complement_when_not_found()
        {
            int[] array = { 1, 2, 3, 3, 10, 12, 12, 12, 15 };

            Assert.AreEqual(~0, array.BinarySearch(0));
            Assert.AreEqual(0, array.BinarySearch(1));
            Assert.AreEqual(1, array.BinarySearch(2));
            Assert.IsInBetween(2, 3, array.BinarySearch(3));
            Assert.AreEqual(~4, array.BinarySearch(4));
            Assert.IsInBetween(5, 7, array.BinarySearch(12));
            Assert.AreEqual(~9, array.BinarySearch(50));
        }

        #endregion

        #region RemoveAtFast

        [Test]
        public void Test_RemoveAtFast()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            list.RemoveAtFast(1);

            Assert.Collections.AreEqual(new[] { 1, 5, 3, 4 }, list);
        }

        #endregion

        #endregion
    }
}

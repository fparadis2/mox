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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Mox.Collections;

namespace Mox
{
    /// <summary>
    /// Provides extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        #region Methods

        /// <summary>
        /// Disposes every element of the collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        public static void Dispose<T>(this IEnumerable<T> collection)
            where T : IDisposable
        {
            ForEach(collection, item => item.Dispose());
        }

        /// <summary>
        /// Applies the given <paramref name="action"/> to every element in the <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T item in collection)
            {
                action(item);
            }
        }

        /// <summary>
        /// Iterates over the enumerable, without doing anything to its contents.
        /// </summary>
        /// <remarks>
        /// Can be useful to evalute entirely enumerables made from methods with the yield keyword.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        public static void Enumerate<T>(this IEnumerable<T> collection)
        {
            if (collection != null)
            {
                ForEach(collection, NullAction);
            }
        }

        private static void NullAction<T>(T obj)
        {
        }

        /// <summary>
        /// Joins the <paramref name="collection"/> into a string.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string Join(this IEnumerable collection, string separator)
        {
            StringBuilder result = new StringBuilder();

            foreach (object o in collection)
            {
                result.Append(o);
                result.Append(separator);
            }

            if (result.Length > 0 && !string.IsNullOrEmpty(separator))
            {
                result.Remove(result.Length - separator.Length, separator.Length);
            }

            return result.ToString();
        }

        /// <summary>
        /// Synchronizes this event by calling either the given <paramref name="addAction"/> or <paramref name="removeAction"/>.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="addAction"></param>
        /// <param name="removeAction"></param>
        public static void Synchronize<T>(this CollectionChangedEventArgs<T> e, Action<T> addAction, Action<T> removeAction)
        {
            Throw.IfNull(e, "e");

            switch (e.Action)
            {
                case CollectionChangeAction.Add:
                    if (addAction != null)
                    {
                        addAction(e.Items[0]);
                    }
                    break;

                case CollectionChangeAction.Remove:
                    if (removeAction != null)
                    {
                        removeAction(e.Items[0]);
                    }
                    break;

                case CollectionChangeAction.Clear:
                    if (removeAction != null)
                    {
                        e.Items.ForEach(removeAction);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the value corresponding to the given <paramref name="key"/> in the given <paramref name="dictionary"/>,
        /// or the default value of the <typeparamref name="TValue"/> if not found.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue SafeGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static bool TryGetValue<TKey, TValue>(this KeyedCollection<TKey, TValue> collection, TKey key, out TValue value)
        {
            if (collection.Contains(key))
            {
                value = collection[key];
                return true;
            }

            value = default(TValue);
            return false;
        }

        public static ArrayList ToArrayList(this IEnumerable collection)
        {
            ArrayList list = new ArrayList();

            foreach (object o in collection)
            {
                list.Add(o);
            }

            return list;
        }

        /// <summary>
        /// Returns whether this collection is equivalent (same content, not necessarily in the same order) to the given <paramref name="collection"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool IsEquivalent(this IEnumerable source, IEnumerable collection)
        {
            Throw.IfNull(collection, "collection");

            ArrayList expectedButNotThere = source.ToArrayList();

            foreach (object actualItem in collection)
            {
                if (expectedButNotThere.Contains(actualItem))
                {
                    expectedButNotThere.Remove(actualItem);
                }
                else
                {
                    return false;
                }
            }

            return expectedButNotThere.Count == 0;
        }

        #region Permutations / Combinations
        
        /// <summary>
        /// Enumerates the combinations of the given <paramref name="size"/> from this collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<IList<T>> EnumerateCombinations<T>(this IList<T> list, int size)
        {
            Throw.ArgumentOutOfRangeIf(size < 0 || size > list.Count, "Invalid size. Expected size > 0 and size < " + list.Count, "size");
            return EnumerateCombinations(list, new Stack<T>(), CombinationType.Combinations, 0, size);
        }

        /// <summary>
        /// Enumerates the combinations of the given <paramref name="size"/> from this collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<IList<T>> EnumerateCombinationsWithRepetitions<T>(this IList<T> list, int size)
        {
            Throw.ArgumentOutOfRangeIf(size < 0, "Invalid size. Expected size > 0", "size");
            return EnumerateCombinations(list, new Stack<T>(), CombinationType.CombinationsWithRepetition, 0, size);
        }

        /// <summary>
        /// Enumerates the permutations of the given <paramref name="size"/> from this collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static IEnumerable<IList<T>> EnumeratePermutations<T>(this IList<T> list, int size)
        {
            Throw.ArgumentOutOfRangeIf(size < 0, "Invalid size. Expected size > 0", "size");
            return EnumerateCombinations(list, new Stack<T>(), CombinationType.Permutations, 0, size);
        }

        private enum CombinationType
        {
            Combinations,
            CombinationsWithRepetition,
            Permutations
        }

        private static IEnumerable<IList<T>> EnumerateCombinations<T>(IList<T> fullList, Stack<T> workingList, CombinationType type, int startingIndex, int remaining)
        {
            if (remaining == 0)
            {
                yield return workingList.ToArray();
                yield break;
            }

            for (int i = (type == CombinationType.Permutations ? 0 : startingIndex); i < fullList.Count; i++)
            {
                workingList.Push(fullList[i]);

                foreach (var completeList in EnumerateCombinations(fullList, workingList, type, type == CombinationType.Combinations ? i + 1 : i, remaining - 1))
                {
                    yield return completeList;
                }

                workingList.Pop();
            }
        }

        public static IEnumerable<IList<int>> EnumerateGroupCombinations(this IList<int> source, int size)
        {
            Throw.ArgumentOutOfRangeIf(size < 0, "Invalid size. Expected size > 0", "size");

            Stack<int> result = new Stack<int>();
            return EnumerateGroupCombinations(source, result, size, 0);
        }

        private static IEnumerable<IList<int>> EnumerateGroupCombinations(IList<int> source, Stack<int> result, int size, int startingIndex)
        {
            Debug.Assert(startingIndex >= 0 && startingIndex < source.Count);

            if (size == 0)
            {
                yield return result.ToArray();
            }
            else
            {
                for (int currentIndex = startingIndex; currentIndex < source.Count; currentIndex++)
                {
                    if (source[currentIndex] > 0)
                    {
                        source[currentIndex]--;
                        result.Push(currentIndex);

                        foreach (var completeResult in EnumerateGroupCombinations(source, result, size - 1, currentIndex))
                        {
                            yield return completeResult;
                        }

                        result.Pop();
                        source[currentIndex]++;
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}

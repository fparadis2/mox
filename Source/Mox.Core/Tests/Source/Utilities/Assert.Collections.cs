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

namespace Mox
{
    partial class Assert
    {
        /// <summary>
        /// Contains assert methods for collections.
        /// </summary>
        public static class Collections
        {
            #region Methods

            #region Count

            /// <summary>
            /// Asserts that the given <paramref name="collection"/> is empty.
            /// </summary>
            /// <param name="collection"></param>
            public static void IsEmpty(IEnumerable collection)
            {
                Throw.IfNull(collection, "collection");

                foreach (object o in collection)
                {
                    Assert.Fail("Collection is not empty. First element: {0}", o);
                }
            }

            /// <summary>
            /// Asserts that the given <paramref name="collection"/> contains exactly <paramref name="expectedCount"/> elements.
            /// </summary>
            /// <param name="expectedCount"></param>
            /// <param name="collection"></param>
            public static void CountEquals(int expectedCount, IEnumerable collection)
            {
                Assert.AreEqual(expectedCount, collection.Cast<object>().Count(), "Expected {0} elements but got {1}.", expectedCount, collection.Cast<object>().Count());
            }

            #endregion

            #region Contains

            /// <summary>
            /// Asserts that the given <paramref name="collection"/> contains the given <paramref name="expected"/> object.
            /// </summary>
            public static void Contains<T>(T expected, ICollection<T> collection)
            {
                Throw.IfNull(collection, "collection");
                Assert.IsTrue(collection.Contains(expected), "Expected collection to contain {0}", expected);
            }

            /// <summary>
            /// Asserts that the given <paramref name="collection"/> contains the given <paramref name="expected"/> object.
            /// </summary>
            public static void Contains(object expected, IList collection)
            {
                Throw.IfNull(collection, "collection");
                Assert.IsTrue(collection.Contains(expected), "Expected collection to contain {0}", expected);
            }

            /// <summary>
            /// Asserts that the given <paramref name="collection"/> contains the given <paramref name="expected"/> object.
            /// </summary>
            /// <remarks>
            /// This is the possibly-less-efficient-but-more-generic version.
            /// </remarks>
            public static void Contains(object expected, IEnumerable collection)
            {
                Throw.IfNull(collection, "collection");

                foreach (object o in collection)
                {
                    if (Equals(o, expected))
                    {
                        return;
                    }
                }

                Assert.Fail("Expected collection to contain {0}", expected);
            }

            #endregion

            #region AreEquivalent / AreEqual

            /// <summary>
            /// Asserts that the <paramref name="actual"/> collection is equivalent to the <paramref name="expected"/> collection, 
            /// i.e. that it contains the same elements, but not necessarily in the same order.
            /// </summary>
            /// <param name="expected">The collection to test against.</param>
            /// <param name="actual">The collection to test.</param>
            public static void AreEquivalent(IEnumerable expected, IEnumerable actual)
            {
                Assert.IsNotNull(expected, "Error in the test, expected should not be null");
                Assert.IsNotNull(actual, "Collections are differents: expected {0} but was {1}. ", GetDisplayString(expected), GetDisplayString(actual));

                ArrayList actualList = actual.ToArrayList();

                ArrayList expectedButNotThere = expected.ToArrayList();
                ArrayList notExpected = new ArrayList();

                foreach (object actualItem in actualList)
                {
                    if (expectedButNotThere.Contains(actualItem))
                    {
                        expectedButNotThere.Remove(actualItem);
                    }
                    else
                    {
                        notExpected.Add(actualItem);
                    }
                }

                if (notExpected.Count > 0 || expectedButNotThere.Count > 0)
                {
                    string common = string.Format("{0} elements in common.", actualList.Count - notExpected.Count);
                    string expectedString = string.Format("{0} elements that were not in actual ({1}).", expectedButNotThere.Count, GetDisplayString(expectedButNotThere));
                    string resultString = string.Format("{0} elements that were not expected ({1}).", notExpected.Count, GetDisplayString(notExpected));

                    Assert.Fail("The collections differ.\n Found {0}.\n Expected {1}.\n Received {2}.\n", common, expectedString, resultString);
                }
            }

            /// <summary>
            /// Asserts that the <paramref name="actual"/> collection is equal to the <paramref name="expected"/> collection, 
            /// i.e. that it contains the same elements, in the same order.
            /// </summary>
            /// <param name="expected">The collection to test against.</param>
            /// <param name="actual">The collection to test.</param>
            public static void AreEqual(IEnumerable expected, IEnumerable actual)
            {
                Assert.IsNotNull(expected, "Error in the test, expected should not be null");
                Assert.IsNotNull(actual, "Collections are differents: expected {0} but was {1}. ", GetDisplayString(expected), GetDisplayString(actual));

                ArrayList actualList = actual.ToArrayList();
                ArrayList expectedList = expected.ToArrayList();

                Assert.AreEqual(expectedList.Count, actualList.Count, "Expected {0} elements but got {1} elements.", expectedList.Count, actualList.Count);

                for (int i = 0; i < expectedList.Count; i++)
                {
                    Assert.AreEqual(expectedList[i], actualList[i], "Element {0} differs: expected {1} but got {2}.", i, expectedList[i], actualList[i]);
                }
            }

            #endregion

            #endregion
        }
    }
}

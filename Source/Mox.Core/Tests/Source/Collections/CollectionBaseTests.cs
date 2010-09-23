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
using Rhino.Mocks;
using System.Collections.Generic;
using System.Collections;

namespace Mox.Collections
{
    [TestFixture]
    public class CollectionBaseTests
    {
        #region Variables

        private MockRepository m_mockery;
        private CollectionBase<int> m_collection;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_collection = m_mockery.PartialMock<CollectionBase<int>>();
        }

        #endregion

        #region Utilities

        IEnumerable<int> GetValues(params int[] values)
        {
            return new List<int>(values);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_CopyTo_copies_the_elements_of_the_collection_to_the_array()
        {
            Expect.Call(m_collection.GetEnumerator()).Return(GetValues(1, 2, 3).GetEnumerator());

            m_mockery.Test(delegate
            {
                int[] array = new int[5];
                m_collection.CopyTo(array, 1);
                Assert.Collections.AreEqual(new int[] { 0, 1, 2, 3, 0 }, array);
            });
        }

        [Test]
        public void Test_Is_not_read_only_by_default()
        {
            m_mockery.Test(delegate
            {
                Assert.IsFalse(m_collection.IsReadOnly);
            });
        }

        [Test]
        public void Test_Non_typed_enumerator_uses_typed_enumerator()
        {
            IEnumerator<int> enumerator = GetValues(1, 2, 3).GetEnumerator();
            Expect.Call(m_collection.GetEnumerator()).Return(enumerator);

            m_mockery.Test(delegate
            {
                Assert.AreEqual(enumerator, ((IEnumerable)m_collection).GetEnumerator());
            });
        }

        #endregion
    }
}

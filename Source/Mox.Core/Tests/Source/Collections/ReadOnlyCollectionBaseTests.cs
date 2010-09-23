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
    public class ReadOnlyCollectionBaseTests
    {
        #region Variables

        private MockRepository m_mockery;
        private ReadOnlyCollectionBase<int> m_collection;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_collection = m_mockery.PartialMock<ReadOnlyCollectionBase<int>>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Is_read_only()
        {
            m_mockery.Test(delegate
            {
                Assert.IsTrue(m_collection.IsReadOnly);
            });
        }

        [Test]
        public void Test_Trying_to_modify_the_collection_throws()
        {
            m_mockery.Test(delegate
            {
                Assert.Throws<NotSupportedException>(delegate { m_collection.Add(3); });
                Assert.Throws<NotSupportedException>(delegate { m_collection.Remove(3); });
                Assert.Throws<NotSupportedException>(delegate { m_collection.Clear(); });
            });
        }

        #endregion
    }
}

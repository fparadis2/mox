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

namespace Mox.Collections
{
    [TestFixture]
    public class CollectionChangedEventArgsTests
    {
        #region Variables

        private CollectionChangedEventArgs<string> m_args;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_args = new CollectionChangedEventArgs<string>(CollectionChangeAction.Remove, new[] { "Item" });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Constructor_values()
        {
            Assert.AreEqual(CollectionChangeAction.Remove, m_args.Action);
            Assert.AreEqual(new[] { "Item" }, m_args.Items);
        }
        
        #endregion
    }
}

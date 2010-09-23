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

namespace Mox.Database
{
    [TestFixture]
    public class SetInfoTests
    {
        #region Variables

        private DateTime m_releaseDate;

        private CardDatabase m_database;
        private SetInfo m_set;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_releaseDate = DateTime.Now;

            m_database = new CardDatabase();
            m_set = new SetInfo(m_database, "Identifier", "My Name", "MyBlock", m_releaseDate);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual("Identifier", m_set.Identifier);
            Assert.AreEqual("My Name", m_set.Name);
            Assert.AreEqual("MyBlock", m_set.Block);
            Assert.AreEqual(m_releaseDate, m_set.ReleaseDate);
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new SetInfo(null, "MySet", "Some name", "MyBlock", m_releaseDate); });
            Assert.Throws<ArgumentNullException>(delegate { new SetInfo(m_database, null, "Some name", "MyBlock", m_releaseDate); });
            Assert.Throws<ArgumentNullException>(delegate { new SetInfo(m_database, string.Empty, "Some name", "MyBlock", m_releaseDate); });
        }

        #endregion
    }
}

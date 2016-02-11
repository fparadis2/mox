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
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Library
{
    [TestFixture]
    public class CardLibraryViewModelTests
    {
        #region Variables

        private CardLibraryViewModel m_collection;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            var database = new CardDatabase();

            database.AddCard("MyCard", "R", Color.Red, SuperType.None, Type.Creature, new SubType[0], "0", "1", null);
            database.AddCard("SuperCard", "R", Color.Red, SuperType.None, Type.Creature, new SubType[0], "0", "1", null);
            database.AddCard("AnotherCard", "R", Color.Red, SuperType.None, Type.Creature, new SubType[0], "0", "1", null);

            m_collection = new CardLibraryViewModel(database.Cards);
        }

        #endregion

        #region Utilities

        private IEnumerable<CardViewModel> View
        {
            get { return m_collection.Cards.Cast<CardViewModel>(); }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.CountEquals(3, View);
            Assert.IsNull(m_collection.FilterText);
        }

        [Test]
        public void Test_Can_apply_simple_text_filter()
        {
            m_collection.FilterText = "Super";
            Assert.AreEqual("Super", m_collection.FilterText);

            Assert.Collections.CountEquals(1, View);
            Assert.AreEqual("SuperCard", View.First().Name);
        }

        #endregion
    }
}

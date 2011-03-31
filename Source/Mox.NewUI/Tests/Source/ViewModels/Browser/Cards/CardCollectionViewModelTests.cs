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

namespace Mox.UI.Browser
{
    [TestFixture]
    public class CardCollectionViewModelTests
    {
        #region Variables

        private CardCollectionViewModel m_collection;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            var database = new CardDatabase();

            database.AddCard("MyCard", "R", SuperType.None, Type.Creature, new SubType[0], "0", "1", new string[0]);
            database.AddCard("SuperCard", "R", SuperType.None, Type.Creature, new SubType[0], "0", "1", new string[0]);
            database.AddCard("AnotherCard", "R", SuperType.None, Type.Creature, new SubType[0], "0", "1", new string[0]);

            m_collection = new CardCollectionViewModel(database.Cards, null);
        }

        #endregion

        #region Utilities

        private IEnumerable<CardViewModel> View
        {
            get { return m_collection.CardsViewSource.View.Cast<CardViewModel>(); }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.CountEquals(3, View);
            Assert.IsNull(m_collection.Filter);
        }

        [Test]
        public void Test_Can_apply_simple_text_filter()
        {
            m_collection.Filter = "Super";
            Assert.AreEqual("Super", m_collection.Filter);

            Assert.Collections.CountEquals(1, View);
            Assert.AreEqual("SuperCard", View.First().Name);
        }

        [Test]
        public void Test_Property_Change_notifications()
        {
            Assert.ThatAllPropertiesOn(m_collection).RaiseChangeNotification();
        }

        #endregion
    }
}

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

            m_collection = new CardCollectionViewModel(database.Cards);
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

        #endregion
    }
}

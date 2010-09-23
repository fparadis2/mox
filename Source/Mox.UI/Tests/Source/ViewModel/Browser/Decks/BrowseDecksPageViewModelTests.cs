using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class BrowseDecksPageViewModelTests
    {
        #region Variables

        private BrowseDecksPageViewModel m_pageModel;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            CardDatabase database = new CardDatabase();
            DeckLibrary library = new DeckLibrary();

            m_pageModel = new BrowseDecksPageViewModel(library, database);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNotNull(m_pageModel.Library);
            Assert.AreEqual("Deck library", m_pageModel.Title);
            Assert.IsNotNull(m_pageModel.Editor);
            Assert.IsNotNull(m_pageModel.Editor.Database);
        }
        
        #endregion
    }
}

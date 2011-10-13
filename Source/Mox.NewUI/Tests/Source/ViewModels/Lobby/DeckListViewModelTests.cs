using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class DeckListViewModelTests
    {
        #region Variables

        private DeckViewModel m_deck;
        private DeckListViewModel m_viewModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_deck = new DeckViewModel(new Deck { Name = "My Deck" });
            m_viewModel = new DeckListViewModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.Collections.IsEmpty(m_viewModel.Decks);
        }

        [Test]
        public void Test_Can_add_decks()
        {
            m_viewModel.Decks.Add(m_deck);
            Assert.Collections.AreEqual(m_viewModel.Decks, new[] { m_deck });
        }

        #endregion
    }
}

using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class DeckViewModelTests
    {
        #region Variables

        private Deck m_deck;
        private DeckViewModel m_viewModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_deck = new Deck { Name = "My Deck" };
            m_viewModel = new DeckViewModel(m_deck);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_deck, m_viewModel.Deck);
            Assert.AreEqual("My Deck", m_viewModel.Name);
        }

        #endregion
    }
}

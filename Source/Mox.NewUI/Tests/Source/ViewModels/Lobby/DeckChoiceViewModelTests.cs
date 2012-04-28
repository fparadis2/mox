using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class DeckChoiceViewModelTests
    {
        #region Variables

        private Deck m_deck;
        private DeckChoiceViewModel m_viewModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_deck = new Deck { Name = "My Deck" };
            m_viewModel = new DeckChoiceViewModel(m_deck);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_deck.Name, m_viewModel.Name);
            Assert.AreEqual(m_deck.Guid, m_viewModel.Id);
        }

        [Test]
        public void Test_Equality()
        {
            Assert.AreEqual(new DeckChoiceViewModel(m_deck), m_viewModel);
            Assert.AreNotEqual(new DeckChoiceViewModel(new Deck()), m_viewModel);
            Assert.AreNotEqual(DeckChoiceViewModel.Random, m_viewModel);
        }

        #endregion
    }
}

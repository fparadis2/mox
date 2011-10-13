using System;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class DeckChoiceViewModelTests
    {
        #region Variables

        private DeckListViewModel m_deckList;
        private DeckChoiceViewModel m_viewModel;
        private DeckViewModel m_deck;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_deckList = new DeckListViewModel();
            m_viewModel = new DeckChoiceViewModel(m_deckList);
            m_deck = new DeckViewModel(new Deck { Name = "My Deck" });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsFalse(m_viewModel.UseRandomDeck);
            Assert.IsNull(m_viewModel.SelectedDeck);
            Assert.AreEqual(m_deckList, m_viewModel.DeckList);
        }

        [Test]
        public void Test_Can_set_SelectedDeck()
        {
            m_viewModel.SelectedDeck = m_deck;
            Assert.AreEqual(m_deck, m_viewModel.SelectedDeck);
        }

        [Test]
        public void Test_Can_set_UseRandomDeck()
        {
            m_viewModel.UseRandomDeck = true;
            Assert.AreEqual(true, m_viewModel.UseRandomDeck);
        }

        [Test]
        public void Test_Change_notification()
        {
            Assert.ThatAllPropertiesOn(m_viewModel)
                .SetValue(choice => choice.SelectedDeck, m_deck)
                .RaiseChangeNotification();
        }

        [Test]
        public void Test_Text_returns_Random_if_random_deck_is_used()
        {
            Assert.ThatProperty(m_viewModel, choice => choice.Text).RaisesChangeNotificationWhen(() => m_viewModel.UseRandomDeck = true);
            Assert.AreEqual("Random Deck", m_viewModel.Text);
        }

        [Test]
        public void Test_Text_returns_the_name_of_the_selected_deck()
        {
            Assert.ThatProperty(m_viewModel, choice => choice.Text).RaisesChangeNotificationWhen(() => m_viewModel.SelectedDeck = m_deck);
            Assert.AreEqual(m_deck.Name, m_viewModel.Text);
        }

        #endregion
    }
}

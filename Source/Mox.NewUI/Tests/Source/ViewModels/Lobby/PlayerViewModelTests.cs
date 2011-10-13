using System;
using Mox.Database;
using Mox.Lobby;

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class PlayerViewModelTests
    {
        #region Variables

        private Mox.Lobby.Player m_player;
        private DeckListViewModel m_deckList;
        private UserViewModel m_user;
        private DeckViewModel m_deck;

        private MockRepository m_mockery;
        private ILobby m_lobby;

        private PlayerViewModel m_unboundModel;
        private PlayerViewModel m_boundModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            User user = new User("John");
            m_deckList = new DeckListViewModel();
            m_player = new Mox.Lobby.Player(user);
            m_user = new UserViewModel(user);
            m_deck = new DeckViewModel(new Deck { Name = "My Deck" });

            m_mockery = new MockRepository();
            m_lobby = m_mockery.StrictMock<ILobby>();

            m_unboundModel = new PlayerViewModel(m_deckList, m_player, m_user);
            m_boundModel = new PlayerViewModel(m_deckList, m_player, m_user, m_lobby);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_player.Id, m_unboundModel.Id);
            Assert.AreEqual(m_user, m_unboundModel.User);
            Assert.IsFalse(m_unboundModel.UseRandomDeck);
            Assert.IsNull(m_unboundModel.SelectedDeck);
            Assert.AreEqual(m_deckList, m_unboundModel.DeckList);
        }

        [Test]
        public void Test_Can_set_User()
        {
            var newUser = new UserViewModel(new User("Jack"));
            Assert.ThatProperty(m_unboundModel, p => p.User).SetValue(newUser).RaisesChangeNotification();
            Assert.AreEqual(newUser, m_unboundModel.User);
        }

        [Test]
        public void Test_Can_set_SelectedDeck_on_unbound_model()
        {
            m_unboundModel.SelectedDeck = m_deck;
            Assert.AreEqual(m_deck, m_unboundModel.SelectedDeck);
        }

        [Test]
        public void Test_Can_set_UseRandomDeck_on_unbound_model()
        {
            m_unboundModel.UseRandomDeck = true;
            Assert.AreEqual(true, m_unboundModel.UseRandomDeck);
        }

        [Test]
        public void Test_Can_set_SelectedDeck_on_bound_model()
        {
            Expect.Call(m_lobby.SetPlayerData(m_player.Id, new PlayerData())).Return(SetPlayerDataResult.Success).Callback<Guid, PlayerData>((g, data) =>
            {
                Assert.AreEqual(data.Deck, m_deck.Deck);
                return true;
            });

            using (m_mockery.Test())
            {
                m_boundModel.SelectedDeck = m_deck;
            }
        }

        [Test]
        public void Test_Can_set_UseRandomDeck_on_bound_model()
        {
            Expect.Call(m_lobby.SetPlayerData(m_player.Id, new PlayerData())).Return(SetPlayerDataResult.Success).Callback<Guid, PlayerData>((g, data) =>
            {
                Assert.That(data.UseRandomDeck);
                return true;
            });

            using (m_mockery.Test())
            {
                m_boundModel.UseRandomDeck = true;
            }
        }

        [Test]
        public void Test_SelectedDeckName_returns_Random_if_random_deck_is_used()
        {
            m_unboundModel.UseRandomDeck = true;
            Assert.AreEqual("Random Deck", m_unboundModel.SelectedDeckName);
        }

        [Test]
        public void Test_SelectedDeckName_returns_the_name_of_the_selected_deck()
        {
            m_unboundModel.SelectedDeck = m_deck;
            Assert.AreEqual(m_deck.Name, m_unboundModel.SelectedDeckName);
        }

        #endregion
    }
}

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
            m_player = new Mox.Lobby.Player(user);
            m_user = new UserViewModel(user);
            m_deck = new DeckViewModel(new Deck { Name = "My Deck" });

            m_mockery = new MockRepository();
            m_lobby = m_mockery.StrictMock<ILobby>();

            m_unboundModel = new PlayerViewModel(m_player, m_user);
            m_boundModel = new PlayerViewModel(m_player, m_user, m_lobby);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_player.Id, m_unboundModel.Id);
            Assert.AreEqual(m_user, m_unboundModel.User);
            Assert.AreEqual(DeckChoiceViewModel.Random, m_unboundModel.SelectedDeck);
            Assert.IsNotNull(m_unboundModel.DeckList);
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
            m_unboundModel.SelectedDeck = new DeckChoiceViewModel(m_deck.Deck);
            Assert.AreEqual(new DeckChoiceViewModel(m_deck.Deck), m_unboundModel.SelectedDeck);
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
                m_boundModel.SelectedDeck = new DeckChoiceViewModel(m_deck.Deck);
            }
        }

        #endregion
    }
}

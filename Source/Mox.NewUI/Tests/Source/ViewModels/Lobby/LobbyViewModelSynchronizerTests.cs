using System;
using Mox.Lobby;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class LobbyViewModelSynchronizerTests
    {
        #region Variables

        private readonly FreeDispatcher m_freeDispatcher = new FreeDispatcher();

        private LocalServer m_server;
        private ILobby m_lobby;
        private LobbyViewModel m_viewModel;
        private LobbyViewModelSynchronizer m_synchronizer;
        private Client m_john;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_server = Server.CreateLocal(LogContext.Empty);
            m_john = Client.CreateLocal(m_server);
            m_john.Connect();
            m_john.CreateLobby("John");
            m_lobby = m_john.Lobby;

            m_viewModel = new LobbyViewModel();
            m_synchronizer = new LobbyViewModelSynchronizer(m_viewModel, m_lobby, m_freeDispatcher);
        }

        [TearDown]
        public void TearDown()
        {
            m_synchronizer.Dispose();
        }

        #endregion

        #region Utilities

        private Client AddPlayer(string name)
        {
            var client = Client.CreateLocal(m_server);
            client.Connect();
            client.EnterLobby(m_lobby.Id, name);
            return client;
        }

        #endregion

        #region Tests

        #region Users

        [Test]
        public void Test_Lobby_contains_only_the_user_when_creating_the_lobby()
        {
            Assert.Collections.CountEquals(1, m_viewModel.Users);
            Assert.AreEqual("John", m_viewModel.Users[0].Name);
        }

        [Test]
        public void Test_Lobby_users_are_synchronized()
        {
            var client = AddPlayer("Jack");

            Assert.Collections.CountEquals(2, m_viewModel.Users);
            Assert.AreEqual("Jack", m_viewModel.Users[1].Name);

            client.Disconnect();

            Assert.Collections.CountEquals(1, m_viewModel.Users);
        }

        [Test]
        public void Test_Users_are_already_there_when_connecting_to_an_existing_lobby()
        {
            var client = AddPlayer("Jack");
            var lobbyViewModel = new LobbyViewModel();

            using (new LobbyViewModelSynchronizer(lobbyViewModel, client.Lobby, m_freeDispatcher))
            {
                Assert.Collections.CountEquals(2, lobbyViewModel.Users);
            }
        }

        #endregion

        #region Players

        [Test]
        public void Test_Lobby_contains_players_when_creating_the_lobby()
        {
            Assert.Collections.CountEquals(2, m_viewModel.Players);
            Assert.AreEqual(m_lobby.User.Id, m_viewModel.Players[0].User.Id);
        }

        [Test]
        public void Test_Players_are_synchronized()
        {
            var client = AddPlayer("Jack");
            var clientId = client.Lobby.User.Id;

            Assert.Collections.CountEquals(2, m_viewModel.Players);
            Assert.AreEqual(clientId, m_viewModel.Players[1].User.Id);

            client.Disconnect();

            Assert.Collections.CountEquals(2, m_viewModel.Players);
            Assert.AreNotEqual(clientId, m_viewModel.Players[1].User.Id);
        }

        [Test]
        public void Test_Players_are_already_there_when_connecting_to_an_existing_lobby()
        {
            var client = AddPlayer("Jack");
            var lobbyViewModel = new LobbyViewModel();

            using (new LobbyViewModelSynchronizer(lobbyViewModel, client.Lobby, m_freeDispatcher))
            {
                Assert.Collections.CountEquals(2, lobbyViewModel.Players);
                Assert.AreEqual(m_lobby.User.Id, lobbyViewModel.Players[0].User.Id);
                Assert.AreEqual(client.Lobby.User.Id, lobbyViewModel.Players[1].User.Id);
            }
        }

        [Test]
        public void Test_Players_data_is_synchronized()
        {
            var player = m_viewModel.Players[0];
            var data = player.Data;
            data.Deck = new Database.Deck();
            data.UseRandomDeck = true;

            Assert.AreEqual(m_john.Lobby.User.Id, player.User.Id, "Sanity check");
            Assert.AreEqual(SetPlayerDataResult.Success, m_john.Lobby.SetPlayerData(player.Id, data));
            Assert.AreEqual(data.Deck, player.DeckChoice.SelectedDeck.Deck);
            Assert.That(player.DeckChoice.UseRandomDeck);
        }

        #endregion

        #region Chat

        [Test]
        public void Test_Chat_is_set_correctly()
        {
            Assert.IsNotNull(m_viewModel.Chat.ChatService);
        }

        [Test]
        public void Test_Chat_messages_are_accumulated_in_ChatViewModel()
        {
            m_lobby.Chat.Say("It's awfully lonely here.");

            var client = AddPlayer("Henry");

            var clientViewModel = new LobbyViewModel();

            using (new LobbyViewModelSynchronizer(clientViewModel, client.Lobby, m_freeDispatcher))
            {
                client.Lobby.Chat.Say("Hello World!");
                m_lobby.Chat.Say("Hello Henry!");
            }

            Assert.AreEqual("John: It's awfully lonely here." + Environment.NewLine + "Henry: Hello World!" + Environment.NewLine + "John: Hello Henry!", m_viewModel.Chat.Text);
            Assert.AreEqual("Henry: Hello World!" + Environment.NewLine + "John: Hello Henry!", clientViewModel.Chat.Text);
        }

        #endregion

        #endregion
    }
}

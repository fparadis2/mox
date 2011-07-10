using System;
using Mox.Lobby;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class LobbyViewModelTests
    {
        #region Variables

        private readonly FreeDispatcher m_freeDispatcher = new FreeDispatcher();

        private LocalServer m_server;
        private Guid m_lobbyId;
        private LobbyViewModel m_viewModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_server = Server.CreateLocal(new LogContext());
            var client = Client.CreateLocal(m_server);
            client.Connect();
            client.CreateLobby("John");
            m_lobbyId = client.Lobby.Id;
            m_viewModel = new LobbyViewModel(client.Lobby, m_freeDispatcher);
        }

        private Client AddPlayer(string name)
        {
            var client = Client.CreateLocal(m_server);
            client.Connect();
            client.EnterLobby(m_lobbyId, name);
            return client;
        }

        #endregion

        #region Tests

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
            var lobbyViewModel = new LobbyViewModel(client.Lobby, m_freeDispatcher);

            Assert.Collections.CountEquals(2, lobbyViewModel.Users);
        }

        #endregion
    }
}

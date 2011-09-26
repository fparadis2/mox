using System;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class LobbyViewModelSynchronizerTests : LobbyViewModelTestsBase
    {
        #region Variables

        private LobbyViewModel m_viewModel;
        private LobbyViewModelSynchronizer m_synchronizer;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_viewModel = new LobbyViewModel();
            m_synchronizer = new LobbyViewModelSynchronizer(m_viewModel, m_lobby, m_freeDispatcher);
        }

        [TearDown]
        public void TearDown()
        {
            m_synchronizer.Dispose();
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
            var lobbyViewModel = new LobbyViewModel();

            using (new LobbyViewModelSynchronizer(lobbyViewModel, client.Lobby, m_freeDispatcher))
            {
                Assert.Collections.CountEquals(2, lobbyViewModel.Users);
            }
        }

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

        #endregion
    }
}

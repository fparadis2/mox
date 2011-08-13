using System;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class LobbyViewModelTests : LobbyViewModelTestsBase
    {
        #region Variables

        private LobbyViewModel m_viewModel;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_viewModel = new LobbyViewModel(m_lobby, m_freeDispatcher);
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

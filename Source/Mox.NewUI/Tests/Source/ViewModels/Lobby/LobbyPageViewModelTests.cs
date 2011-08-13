using System;
using Mox.Lobby;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class LobbyPageViewModelTests
    {
        #region Variables

        private LobbyPageViewModel m_page;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_page = new LobbyPageViewModel(CreateLocalLobby());
        }

        private static ILobby CreateLocalLobby()
        {
            // Code duplication...

            LocalServer server = Server.CreateLocal(new LogContext());
            var client = Client.CreateLocal(server);
            client.Connect();
            client.CreateLobby("John");
            return client.Lobby;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Fill()
        {
            MoxWorkspace workspace = new MoxWorkspace
            {
                LeftView = new object()
            };

            m_page.Fill(workspace);

            Assert.IsInstanceOf<PlayerListPartViewModel>(workspace.CenterView);
            Assert.IsInstanceOf<GameInfoPartViewModel>(workspace.RightView);
            Assert.IsInstanceOf<LobbyChatPartViewModel>(workspace.BottomView);
            Assert.IsInstanceOf<LobbyCommandPartViewModel>(workspace.CommandView);
            Assert.IsNull(workspace.LeftView);
        }

        #endregion
    }
}

using System;
using Mox.Lobby;
using Mox.UI.Game;
using Mox.UI.Shell;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class LobbyCommandPartViewModelTests
    {
        #region Variables

        private MockViewModelServices m_viewModelServices;
        private MockRepository m_mockery;

        private IShellViewModel m_shell;
        private ILobby m_lobby;
        private LobbyCommandPartViewModel m_command;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_viewModelServices = MockViewModelServices.Use(m_mockery);

            m_shell = m_mockery.StrictMock<IShellViewModel>();
            m_lobby = m_mockery.StrictMock<ILobby>();

            m_command = new LobbyCommandPartViewModel(m_lobby);
        }

        [TearDown]
        public void TearDown()
        {
            DisposableHelper.SafeDispose(m_viewModelServices);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_LeaveGame_pops_the_nearest_conductor()
        {
            m_viewModelServices.Expect_PopParent(m_command);

            using (m_mockery.Test())
            {
                m_command.LeaveGame();
            }
        }

        [Test]
        public void Test_StartGame_starts_the_game()
        {
            IGameService gameService = m_mockery.StrictMock<IGameService>();
            SetupResult.For(m_lobby.GameService).Return(gameService);

            gameService.StartGame();

            using (m_mockery.Test())
            {
                m_command.StartGame();
            }
        }

        #endregion
    }
}

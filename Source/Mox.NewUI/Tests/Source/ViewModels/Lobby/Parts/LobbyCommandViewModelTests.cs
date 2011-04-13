using System;
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
        private LobbyCommandPartViewModel m_command;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_viewModelServices = MockViewModelServices.Use(m_mockery);

            m_shell = m_mockery.StrictMock<IShellViewModel>();

            m_command = new LobbyCommandPartViewModel();
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
        public void Test_StartGame_pushes_the_game_page_on_the_shell()
        {
            m_viewModelServices.Expect_Push<object>(m_command, Assert.IsInstanceOf<GamePageViewModel>);

            using (m_mockery.Test())
            {
                m_command.StartGame();
            }
        }

        #endregion
    }
}

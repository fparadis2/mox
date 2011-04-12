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
        private INavigationConductor m_conductor;
        private LobbyCommandPartViewModel m_command;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_viewModelServices = MockViewModelServices.Use();

            m_mockery = new MockRepository();
            m_shell = m_mockery.StrictMock<IShellViewModel>();
            m_conductor = m_mockery.StrictMock<INavigationConductor>();

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
            m_viewModelServices.Expect_FindParent(m_command, m_conductor);

            m_conductor.Pop();

            using (m_mockery.Test())
            {
                m_command.LeaveGame();
            }
        }

        [Test]
        public void Test_StartGame_pushes_the_game_page_on_the_shell()
        {
            m_viewModelServices.Expect_FindParent(m_command, m_shell);

            Expect.Call(m_shell.Push(null)).Return(new MockPageHandle()).IgnoreArguments().Constraints(Rhino.Mocks.Constraints.Is.TypeOf<GamePageViewModel>());

            using (m_mockery.Test())
            {
                m_command.StartGame();
            }
        }

        #endregion
    }
}

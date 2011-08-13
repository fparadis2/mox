using System;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    [TestFixture]
    public class LobbyChatViewModelTests : LobbyViewModelTestsBase
    {
        #region Variables

        private LobbyChatViewModel m_viewModel;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            m_viewModel = new LobbyChatViewModel(m_lobby, m_freeDispatcher);
        }

        #endregion
    }
}

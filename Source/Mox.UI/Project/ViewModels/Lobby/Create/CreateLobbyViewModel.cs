using System;

namespace Mox.UI.Lobby
{
    public class CreateLobbyViewModel : PageViewModel
    {
        private readonly LobbyServerParametersViewModel m_serverParameters = new LobbyServerParametersViewModel();
        public LobbyServerParametersViewModel ServerParameters
        {
            get { return m_serverParameters; }
        }

        private readonly LobbyGameParametersViewModel m_gameParameters = new LobbyGameParametersViewModel();
        public LobbyGameParametersViewModel GameParameters
        {
            get { return m_gameParameters; }
        }
    }
}

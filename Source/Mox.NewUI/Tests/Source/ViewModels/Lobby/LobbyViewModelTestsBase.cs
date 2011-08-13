using Mox.Lobby;
using NUnit.Framework;

namespace Mox.UI.Lobby
{
    public class LobbyViewModelTestsBase
    {
        #region Variables

        protected readonly FreeDispatcher m_freeDispatcher = new FreeDispatcher();

        protected LocalServer m_server;
        protected ILobby m_lobby;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public virtual void Setup()
        {
            m_server = Server.CreateLocal(LogContext.Empty);
            var client = Client.CreateLocal(m_server);
            client.Connect();
            client.CreateLobby("John");
            m_lobby = client.Lobby;
        }

        protected Client AddPlayer(string name)
        {
            var client = Client.CreateLocal(m_server);
            client.Connect();
            client.EnterLobby(m_lobby.Id, name);
            return client;
        }

        #endregion
    }
}
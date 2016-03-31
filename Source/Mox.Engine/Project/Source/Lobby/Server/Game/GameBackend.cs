using System;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Server
{
    public class GameBackend : IDisposable
    {
        #region Variables

        private readonly object m_lock = new object();
        private readonly ILog m_log;
        private GameInstance m_game;

        #endregion

        #region Constructor

        public GameBackend(ILog log)
        {
            m_log = log;
        }

        public void Dispose()
        {
            lock (m_lock)
            {
                var game = m_game;
                if (game != null)
                    game.Dispose();
            }
        }

        #endregion

        #region Methods

        public void StartGame(LobbyBackend lobby)
        {
            lock (m_lock)
            {
                if (m_game != null)
                {
                    return;
                }

                m_log.Log(LogImportance.Low, "Starting game in lobby {0}", lobby.Id);
                m_game = new GameInstance(lobby.Id);

                m_game.Prepare(lobby);

                lobby.Broadcast(new PrepareGameMessage { Players = m_game.GetPlayerMapping() });

                m_game.SetupReplication(lobby);

                lobby.Broadcast(new GameStartedMessage());

                m_game.Run();
            }
        }

        #endregion
    }
}

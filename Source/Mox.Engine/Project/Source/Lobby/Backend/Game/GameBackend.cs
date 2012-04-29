using System;

namespace Mox.Lobby.Backend
{
    public class GameBackend
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

                lobby.Broadcast(new StartGameMessage());

                m_game.Run();
            }
        }

        #endregion
    }
}

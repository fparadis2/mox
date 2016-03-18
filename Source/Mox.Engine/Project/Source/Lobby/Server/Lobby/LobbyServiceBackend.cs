using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Mox.Lobby.Server
{
    public class LobbyServiceBackend
    {
        #region Variables

        private readonly ReadWriteLock m_lock = ReadWriteLock.CreateNoRecursion();
        private readonly LobbyCollection m_lobbies = new LobbyCollection();
        private readonly ILog m_log;

        #endregion

        #region Constructor

        public LobbyServiceBackend(ILog log)
        {
            m_log = log;
        }

        #endregion

        #region Properties

        public IEnumerable<LobbyBackend> Lobbies
        {
            get
            {
                using (m_lock.Read)
                {
                    return m_lobbies.ToArray();
                }
            }
        }

        internal ILog Log
        {
            get 
            {
                return m_log;
            }
        }

        #endregion

        #region Methods

        public LobbyBackend GetLobby(Guid lobbyId)
        {
            LobbyBackend lobby;
            using (m_lock.Read)
            {
                m_lobbies.TryGetValue(lobbyId, out lobby);
            }
            return lobby;
        }

        public LobbyBackend CreateLobby(User user, LobbyParameters lobbyParameters)
        {
            LobbyBackend newLobby = new LobbyBackend(this, lobbyParameters);
            bool success = newLobby.Login(user);
            Debug.Assert(success, "Always supposed to be able to log into a new lobby");

            using (m_lock.Write)
            {
                m_lobbies.Add(newLobby);
            }
            
            return newLobby;
        }

        public LobbyBackend JoinLobby(Guid lobbyId, User user)
        {
            LobbyBackend lobby = GetLobby(lobbyId);

            if (lobby == null)
                return null;

            if (!lobby.Login(user))
            {
                return null;
            }

            return lobby;
        }

        public void Logout(User user, LobbyBackend lobby, string reason)
        {
            if (lobby.Logout(user, reason))
            {
                DestroyLobby(lobby);
            }
        }

        private void DestroyLobby(LobbyBackend lobby)
        {
            using (m_lock.Write)
            {
                m_lobbies.Remove(lobby.Id);
            }
        }

        #endregion

        #region Inner Types

        private class LobbyCollection : KeyedCollection<Guid, LobbyBackend>
        {
            protected override Guid GetKeyForItem(LobbyBackend item)
            {
                return item.Id;
            }
        }

        #endregion
    }
}

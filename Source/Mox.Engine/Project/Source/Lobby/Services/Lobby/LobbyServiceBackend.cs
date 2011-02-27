using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Mox.Lobby.Backend
{
    public class LobbyServiceBackend
    {
        #region Variables

        private readonly ReadWriteLock m_lock = ReadWriteLock.CreateNoRecursion();
        private readonly LobbyCollection m_lobbies = new LobbyCollection();

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

        #endregion

        #region Methods

        public LobbyBackend CreateLobby(IClient client)
        {
            LobbyBackend newLobby = new LobbyBackend(this);
            bool success = newLobby.Login(client);
            Debug.Assert(success, "Always supposed to be able to log into a new lobby");

            using (m_lock.Write)
            {
                m_lobbies.Add(newLobby);
            }

            return newLobby;
        }

        public LobbyBackend JoinLobby(Guid lobbyId, IClient client)
        {
            LobbyBackend lobby;

            using (m_lock.Read)
            {
                if (!m_lobbies.TryGetValue(lobbyId, out lobby))
                {
                    return null;
                }
            }

            if (!lobby.Login(client))
            {
                return null;
            }

            return lobby;
        }

        internal void DestroyLobby(LobbyBackend lobby)
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

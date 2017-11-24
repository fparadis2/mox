using System;
using System.Collections;
using System.Collections.Generic;

using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Client
{
    internal class ClientUserCollection : ILobbyUserCollection
    {
        #region Variables

        private readonly Dictionary<Guid, LobbyUser> m_users = new Dictionary<Guid, LobbyUser>();

        #endregion

        #region Properties

        public int Count
        {
            get { return m_users.Count; }
        }

        public ILobbyUser this[Guid id]
        {
            get
            {
                m_users.TryGetValue(id, out LobbyUser user);
                return user;
            }
        }

        #endregion

        #region Methods

        internal void HandleMessage(UserJoinedMessage message)
        {
            var user = new LobbyUser(message.UserId);
            user.Data = message.Data;
            m_users.Add(message.UserId, user);

            UserJoined.Raise(this, new ItemEventArgs<ILobbyUser>(user));
        }

        internal void HandleMessage(UserLeftMessage message)
        {
            if (m_users.TryGetValue(message.UserId, out LobbyUser user))
            {
                m_users.Remove(message.UserId);
                UserLeft.Raise(this, new ItemEventArgs<ILobbyUser>(user));
            }
        }
        
        public IEnumerator<ILobbyUser> GetEnumerator()
        {
            return m_users.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Events

        public event EventHandler<ItemEventArgs<ILobbyUser>> UserJoined;
        public event EventHandler<ItemEventArgs<ILobbyUser>> UserLeft;

        #endregion

        #region Nested Types

        private class LobbyUser : ILobbyUser
        {
            public LobbyUser(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; }

            public UserData Data
            {
                get;
                set;
            }
        }

        #endregion
    }
}

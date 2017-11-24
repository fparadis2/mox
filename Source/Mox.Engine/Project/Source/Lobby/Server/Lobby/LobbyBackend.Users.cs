using Mox.Lobby.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Server
{
    partial class LobbyBackend
    {
        private class UserCollection
        {
            #region Variables

            private readonly Dictionary<Guid, UserInfo> m_users = new Dictionary<Guid, UserInfo>();

            #endregion

            #region Properties

            public User[] AllUsers
            {
                get { return m_users.Values.Select(u => u.User).ToArray(); }
            }

            public int Count
            {
                get { return m_users.Count; }
            }

            public User FirstConnectedPlayer
            {
                get
                {
                    foreach (var user in m_users.Values)
                    {
                        if (user.User.Channel != null)
                            return user.User;
                    }

                    return User.Invalid;
                }
            }

            #endregion

            #region Methods

            public bool Contains(User user)
            {
                return m_users.ContainsKey(user.Id);
            }

            public UserData Add(User user, IUserIdentity identity)
            {
                Debug.Assert(user.IsValid);

                var userInfo = new UserInfo(user, identity);
                m_users.Add(user.Id, userInfo);
                return userInfo.Data;
            }

            public bool Remove(User user)
            {
                return m_users.Remove(user.Id);
            }

            public bool TryGetUser(Guid id, out User user, out IUserIdentity identity)
            {
                UserInfo info;
                if (m_users.TryGetValue(id, out info))
                {
                    user = info.User;
                    identity = info.Identity;
                    return true;
                }

                user = User.Invalid;
                identity = null;
                return false;
            }

            internal UserJoinedMessage[] CreateUserJoinedMessageForAllUsers()
            {
                List<UserJoinedMessage> messages = new List<UserJoinedMessage>();

                foreach (var userInfo in m_users.Values)
                {
                    messages.Add(new UserJoinedMessage { UserId = userInfo.User.Id, Data = userInfo.Data });
                }

                return messages.ToArray();
            }

            public void Broadcast(Message message)
            {
                foreach (var client in m_users.Values)
                {
                    var channel = client.User.Channel;

                    if (channel != null)
                        channel.Send(message);
                }
            }

            public void BroadcastExceptTo(User user, Message message)
            {
                foreach (var client in m_users.Values)
                {
                    if (client.User != user)
                    {
                        var channel = client.User.Channel;

                        if (channel != null)
                            channel.Send(message);
                    }
                }
            }

            #endregion

            #region Nested Types

            private class UserInfo
            {
                public readonly User User;
                public readonly IUserIdentity Identity;

                public UserInfo(User user, IUserIdentity identity)
                {
                    User = user;
                    Identity = identity;
                }

                public UserData Data
                {
                    get { return new UserData { Name = Identity.Name }; }
                }
            }

            #endregion
        }
    }
}

using Mox.Lobby.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Server
{
    partial class LobbyBackend
    {
        private class UserCollection
        {
            #region Variables

            private readonly Dictionary<Guid, LobbyUser> m_users = new Dictionary<Guid, LobbyUser>();

            #endregion

            #region Properties

            public User[] AllUsers
            {
                get { return m_users.Values.Select(u => u.User).ToArray(); }
            }

            public KeyValuePair<User, UserData>[] AllUserDatas
            {
                get
                {
                    var datas = new List<KeyValuePair<User, UserData>>(m_users.Count);

                    foreach (var user in m_users.Values)
                    {
                        datas.Add(new KeyValuePair<User, UserData>(user.User, user.Data));
                    }

                    return datas.ToArray();
                }
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
                        if (!user.IsBot)
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

            public void Add(LobbyUser user)
            {
                Debug.Assert(user.User.IsValid);
                m_users.Add(user.User.Id, user);
            }

            public bool Remove(User user)
            {
                return m_users.Remove(user.Id);
            }

            public bool TryGetUser(Guid id, out LobbyUser user)
            {
                return m_users.TryGetValue(id, out user);
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
        }

        internal class LobbyUser
        {
            public readonly User User;
            public readonly IUserIdentity Identity;

            private UserData m_data = new UserData();

            public LobbyUser(User user, IUserIdentity identity, bool isBot)
            {
                User = user;
                Identity = identity;

                m_data.IsBot = isBot;
            }

            public Guid Id
            {
                get { return User.Id; }
            }

            public UserData Data
            {
                get
                {
                    var data = m_data;
                    data.Name = Identity.Name;
                    return data;
                }
            }

            public bool IsBot
            {
                get { return m_data.IsBot; }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mox.Lobby.Backend
{
    /// <summary>
    /// Backend implementation of the chat service.
    /// </summary>
    public class ChatServiceBackend
    {
        #region Inner Types

        private class ChatClient
        {
            public readonly User User;
            public readonly IChatClient Client;
            public readonly ChatLevel Level;

            public ChatClient(User user, IChatClient client, ChatLevel level)
            {
                User = user;
                Client = client;
                Level = level;
            }
        }

        private class ChatClientCollection : KeyedCollection<User, ChatClient>
        {
            protected override User GetKeyForItem(ChatClient item)
            {
                return item.User;
            }
        }

        #endregion

        #region Variables

        private readonly ILog m_log;
        private readonly ChatClientCollection m_clients = new ChatClientCollection();
        private readonly ReadWriteLock m_lock = ReadWriteLock.CreateNoRecursion();

        #endregion

        #region Constructor

        public ChatServiceBackend(ILog log)
        {
            m_log = log;
        }

        #endregion

        #region Methods

        public void Register(User user, IChatClient client, ChatLevel level)
        {
            Throw.IfNull(user, "user");
            Throw.IfNull(client, "client");

            ChatClient holder = new ChatClient(user, client, level);
            
            using (m_lock.Write)
            {
                m_clients.Add(holder);
            }
        }

        public void Unregister(User user)
        {
            using (m_lock.Write)
            {
                m_clients.Remove(user);
            }
        }

        private static bool CanSendTo(ChatLevel sender, ChatLevel receiver)
        {
            return sender >= receiver;
        }

        public void Say(User speaker, string message)
        {
            ChatClient speakerClient;
            IEnumerable<ChatClient> clients;
            using (m_lock.Read)
            {
                if (!m_clients.TryGetValue(speaker, out speakerClient))
                {
                    return;
                }

                clients = m_clients.ToArray();
            }

            m_log.Log(LogImportance.Normal, "{0}: {1}", speaker.Name, message);

            foreach (ChatClient listener in clients)
            {
                if (speaker != listener.User && CanSendTo(speakerClient.Level, listener.Level))
                {
                    listener.Client.OnMessageReceived(speakerClient.User, message);
                }
            }
        }

        #endregion
    }
}

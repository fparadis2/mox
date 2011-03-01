using System;
using System.Collections.ObjectModel;

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

        private readonly ChatClientCollection m_clients = new ChatClientCollection();

        #endregion

        #region Constructor

        #endregion

        #region Methods

        public void Register(User user, IChatClient client, ChatLevel level)
        {
            Throw.IfNull(user, "user");
            Throw.IfNull(client, "client");

            ChatClient holder = new ChatClient(user, client, level);
            m_clients.Add(holder);
        }

        public void Unregister(User user)
        {
            m_clients.Remove(user);
        }

        private static bool CanSendTo(ChatLevel sender, ChatLevel receiver)
        {
            return sender >= receiver;
        }

        public void Say(User speaker, string message)
        {
            if (!m_clients.Contains(speaker))
            {
                return;
            }

            ChatClient speakerClient = m_clients[speaker];
            ChatLevel speakerLevel = speakerClient.Level;

            foreach (ChatClient listener in m_clients)
            {
                if (speaker != listener.User && CanSendTo(speakerLevel, listener.Level))
                {
                    listener.Client.OnMessageReceived(speakerClient.User, message);
                }
            }
        }

        #endregion
    }
}

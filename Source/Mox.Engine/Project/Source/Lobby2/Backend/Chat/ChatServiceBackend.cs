using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mox.Lobby2.Backend
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
            public readonly IChannel Channel;
            public readonly ChatLevel Level;

            public ChatClient(User user, IChannel channel, ChatLevel level)
            {
                User = user;
                Channel = channel;
                Level = level;
            }
        }

        private class ChatClientCollection : KeyedCollection<IChannel, ChatClient>
        {
            protected override IChannel GetKeyForItem(ChatClient item)
            {
                return item.Channel;
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

        public void Register(User user, IChannel channel, ChatLevel level)
        {
            Throw.IfNull(user, "user");
            Throw.IfNull(channel, "channel");

            ChatClient holder = new ChatClient(user, channel, level);
            
            using (m_lock.Write)
            {
                m_clients.Add(holder);
            }
        }

        public void Unregister(IChannel channel)
        {
            using (m_lock.Write)
            {
                m_clients.Remove(channel);
            }
        }

        private static bool CanSendTo(ChatLevel sender, ChatLevel receiver)
        {
            return sender >= receiver;
        }

        public void Say(IChannel channel, string message)
        {
            ChatClient speakerClient;
            IEnumerable<ChatClient> clients;
            using (m_lock.Read)
            {
                if (!m_clients.TryGetValue(channel, out speakerClient))
                {
                    return;
                }

                clients = m_clients.ToArray();
            }

            m_log.Log(LogImportance.Normal, "{0}: {1}", speakerClient.User.Name, message);

            foreach (ChatClient listener in clients)
            {
                if (speakerClient != listener && CanSendTo(speakerClient.Level, listener.Level))
                {
                    listener.Channel.Send(new ChatMessage { User = speakerClient.User, Message = message });
                }
            }
        }

        #endregion
    }
}

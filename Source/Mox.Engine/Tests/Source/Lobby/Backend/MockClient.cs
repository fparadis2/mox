using System;
using System.Collections.Generic;

namespace Mox.Lobby.Backend
{
    public class MockClient
    {
        #region Variables

        private readonly MockChannel m_channel;
        private readonly User m_user;

        #endregion

        #region Constructor

        public MockClient(string name)
        {
            m_channel = new MockChannel();
            m_user = new User(name);
        }

        #endregion

        #region Properties

        public User User
        {
            get { return m_user; }
        }

        public MockChannel Channel
        {
            get { return m_channel; }
        }

        #endregion
    }

    public class MockChannel : IChannel
    {
        #region Variables

        private readonly List<Message> m_sentMessages = new List<Message>();

        #endregion

        #region Properties

        public IList<Message> SentMessages
        {
            get { return m_sentMessages; }
        }

        #endregion

        #region Implementation of IChannel

        public IAsyncResult<TResponse> BeginRequest<TResponse>(Message message) where TResponse : Message
        {
            throw new NotImplementedException();
        }

        public TResponse Request<TResponse>(Message message) where TResponse : Message
        {
            throw new NotImplementedException();
        }

        public void Send(Message message)
        {
            m_sentMessages.Add(message);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived
        {
            add { }
            remove { }
        }

        public event EventHandler Disconnected
        {
            add { }
            remove { }
        }

        #endregion
    }
}

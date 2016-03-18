using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mox.Lobby.Network;

namespace Mox.Lobby.Server
{
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

        public string EndPointIdentifier
        {
            get { return "mock"; }
        }

        #endregion

        #region Implementation of IChannel

        public Task<TResponse> Request<TRequest, TResponse>(TRequest message) where TRequest : Request<TResponse> where TResponse : Response
        {
            throw new NotImplementedException();
        }

        public void Respond<TRequest, TResponse>(TRequest request, TResponse response) where TRequest : Request<TResponse> where TResponse : Response
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

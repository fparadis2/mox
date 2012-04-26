using System;
using System.Collections.Generic;

namespace Mox.Lobby2
{
    internal class LocalClient : Client
    {
        #region Variables

        private readonly LocalServer m_serverBackend;

        #endregion

        #region Constructor

        public LocalClient(LocalServer server)
        {
            Throw.IfNull(server, "server");
            m_serverBackend = server;
        }

        #endregion

        #region Methods

        internal override IChannel CreateChannel()
        {
            var channel = new LocalChannel();
            m_serverBackend.CreateConnection(channel.RemoteChannel);
            return channel;
        }

        internal override void DeleteServerImpl(IChannel channel)
        {
            LocalChannel localChannel = (LocalChannel)channel;

            localChannel.RemoteChannel.Disconnect();
            localChannel.Disconnect();
        }

        #endregion

        #region Inner Types

        internal class LocalChannel : IChannel
        {
            #region Variables

            private readonly LocalChannel m_remoteChannel;
            private readonly List<PendingRequest> m_pendingRequests = new List<PendingRequest>();

            #endregion

            #region Constructor

            public LocalChannel()
            {
                m_remoteChannel = new LocalChannel(this);
            }

            private LocalChannel(LocalChannel remoteChannel)
            {
                m_remoteChannel = remoteChannel;
            }

            #endregion

            #region Properties

            public LocalChannel RemoteChannel
            {
                get { return m_remoteChannel; }
            }
		 
	        #endregion

            #region Implementation of IChannel

            public TResponse Request<TResponse>(Message message) 
                where TResponse : Message
            {
                PendingRequest<TResponse> request = new PendingRequest<TResponse>();

                try
                {
                    m_pendingRequests.Add(request);

                    Send(message);

                    return request.Consume();
                }
                finally
                {
                    m_pendingRequests.Remove(request);
                }
            }

            public void Send(Message message)
            {
                m_remoteChannel.OnMessageReceived(message);
            }

            public event EventHandler<MessageReceivedEventArgs> MessageReceived;

            private void OnMessageReceived(Message message)
            {
                foreach (var request in m_pendingRequests)
                {
                    if (request.Consider(message))
                    {
                        return;
                    }
                }

                MessageReceived.Raise(this, new MessageReceivedEventArgs(message));
            }

            public event EventHandler Disconnected;

            public void Disconnect()
            {
                Disconnected.Raise(this, EventArgs.Empty);
            }

            #endregion
        }

        #endregion
    }
}

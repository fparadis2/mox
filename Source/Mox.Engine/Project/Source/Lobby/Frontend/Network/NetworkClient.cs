using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using Mox.Threading;

namespace Mox.Lobby
{
    public class NetworkClient : Client
    {
        #region Variables

        private string m_host = "localhost";
        private int m_port = NetworkServer.DefaultPort;

        #endregion

        #region Properties

        /// <summary>
        /// Host to connect to.
        /// </summary>
        /// <remarks>
        /// Can be a hostname or an ip address.
        /// </remarks>
        public string Host
        {
            get { return m_host; }
            set
            {
                AssertStateIs(ClientState.New);
                m_host = value;
            }
        }

        /// <summary>
        /// The port to connect to.
        /// </summary>
        public int Port
        {
            get { return m_port; }
            set
            {
                AssertStateIs(ClientState.New);
                m_port = value;
            }
        }

        #endregion

        #region Methods

        internal override IChannel CreateChannel()
        {
            // TODO: Handle when cannot resolve address
            IPAddress address = Dns.GetHostAddresses(Host).Where(a => a.AddressFamily == AddressFamily.InterNetwork).First();

            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.Connect(address, Port);

            return new ClientTcpChannel(client, new MessageSerializer());
        }

        internal override void DeleteServerImpl(IChannel channel)
        {
            ClientTcpChannel clientChannel = (ClientTcpChannel)channel;
            clientChannel.Close();
        }

        #endregion

        #region Inner Types

        private class ClientTcpChannel : TcpChannel
        {
            #region Variables

            private readonly Dispatcher m_dispatcher = Dispatcher.CurrentDispatcher;
            private readonly MessageQueue m_receiveQueue = new MessageQueue(WakeUpJob.FromDispatcher(Dispatcher.CurrentDispatcher));

            #endregion

            #region Constructor

            public ClientTcpChannel(TcpClient client, IMessageSerializer serializer)
                : base(client, serializer, new MessageQueue(WakeUpJob.FromThreadPool()))
            {
            }

            #endregion

            #region Overrides of TcpChannel

            protected override void OnReadMessage(Message message, Action<Message> readMessage)
            {
                m_receiveQueue.Enqueue(message, readMessage);
            }

            protected override void OnDisconnected()
            {
                m_dispatcher.Invoke(new System.Action(base.OnDisconnected), null);
            }

            #endregion
        }

        #endregion
    }
}
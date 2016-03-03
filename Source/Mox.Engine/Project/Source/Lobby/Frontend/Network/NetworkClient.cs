using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            try
            {
                IPAddress address = Dns.GetHostAddresses(Host).Where(a => a.AddressFamily == AddressFamily.InterNetwork).First();

                TcpClient client = new TcpClient(AddressFamily.InterNetwork);
                client.Connect(address, Port);

                return new ClientTcpChannel(client, Dispatcher);
            }
            catch
            {
                return null;
            }
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
            #region Constructor

            public ClientTcpChannel(TcpClient client, IDispatcher dispatcher)
                : base(client, new MessageQueue(WakeUpJob.FromThreadPool()))
            {
                ReceptionDispatcher = new ClientReceptionDispatcher(dispatcher);
            }

            #endregion
        }

        #endregion
    }
}
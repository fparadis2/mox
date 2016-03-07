using System;
using Mox.Lobby.Network;
using Mox.Lobby.Server;

namespace Mox.Lobby.Client
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

        #region Properties

        public override string ServerName
        {
            get { return "Local"; }
        }

        #endregion

        #region Methods

        internal override IChannel CreateChannel()
        {
            var channel = new LocalChannel { ReceptionDispatcher = new ClientReceptionDispatcher(Dispatcher) };

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
    }
}

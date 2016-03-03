using System;

namespace Mox.Lobby
{
    internal class LocalChannel : ChannelBase
    {
        #region Variables

        private readonly LocalChannel m_remoteChannel;

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

        #region Methods

        public void Disconnect()
        {
            OnDisconnected();
        }

        protected override void SendMessage(Message message)
        {
            m_remoteChannel.OnMessageReceived(message);
        }

        #endregion
    }
}

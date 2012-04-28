using System;

using Mox.Replication;

namespace Mox.Lobby
{
    internal class ClientGame : IGameService
    {
        #region Variables

        private static readonly MessageRouter<ClientGame> ms_router = new MessageRouter<ClientGame>();

        private readonly IChannel m_channel;

        private Instance m_instance = new Instance();

        #endregion

        #region Constructor

        static ClientGame()
        {
            ms_router.Register<StartGameMessage>(g => g.StartGame);
        }

        public ClientGame(IChannel channel)
        {
            m_channel = channel;
        }

        #endregion

        #region Properties

        public Game Game
        {
            get { return m_instance.Game; }
        }

        #endregion

        #region Methods

        public void ReceiveMessage(Message message)
        {
            ms_router.Route(this, m_channel, message);
        }

        private void StartGame(StartGameMessage message)
        {
            m_instance = new Instance();
            GameStarted.Raise(this, EventArgs.Empty);
        }

        #endregion

        #region Events

        public event EventHandler GameStarted;

        #endregion

        #region Inner Types

        private class Instance
        {
            private readonly ReplicationClient<Game> m_replicationClient = new ReplicationClient<Game>();

            public Game Game
            {
                get { return m_replicationClient.Host; }
            }
        }

        #endregion
    }
}

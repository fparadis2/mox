using System;

using Mox.Replication;

namespace Mox.Lobby
{
    internal class ClientGame : IGameService
    {
        #region Variables

        private static readonly MessageRouter<ClientGame> ms_router = new MessageRouter<ClientGame>();

        private readonly IChannel m_channel;

        private Instance m_instance = new Instance(Resolvable<Mox.Player>.Empty);

        #endregion

        #region Constructor

        static ClientGame()
        {
            ms_router.Register<StartGameMessage>(g => g.StartGame);
            ms_router.Register<GameReplicationMessage>(g => g.OnReplicationMessage);
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

        public Mox.Player Player
        {
            get { return m_instance.Player; }
        }

        public User User
        {
            get; 
            internal set;
        }

        #endregion

        #region Methods

        void IGameService.StartGame()
        {
            m_channel.Send(new StartGameRequest());
        }

        public void ReceiveMessage(Message message)
        {
            ms_router.Route(this, m_channel, message);
        }

        private void StartGame(StartGameMessage message)
        {
            var player = Resolvable<Mox.Player>.Empty;
            if (User != null && message.Players != null)
            {
                message.Players.TryGetValue(User, out player);
            }

            m_instance = new Instance(player);
            GameStarted.Raise(this, EventArgs.Empty);
        }

        private void OnReplicationMessage(GameReplicationMessage message)
        {
            m_instance.ReplicationClient.Replicate(message.Command);
        }

        #endregion

        #region Events

        public event EventHandler GameStarted;

        #endregion

        #region Inner Types

        private class Instance
        {
            private readonly ReplicationClient<Game> m_replicationClient = new ReplicationClient<Game>();
            private readonly Mox.Player m_player;

            public Instance(Resolvable<Mox.Player> player)
            {
                if (!player.IsEmpty)
                {
                    m_player = player.Resolve(Game);
                }
            }

            public Mox.Player Player
            {
                get { return m_player; }
            }

            public Game Game
            {
                get { return m_replicationClient.Host; }
            }

            public IReplicationClient ReplicationClient
            {
                get { return m_replicationClient; }
            }
        }

        #endregion
    }
}

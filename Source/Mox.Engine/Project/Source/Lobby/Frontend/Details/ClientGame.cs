using System;
using System.Diagnostics;
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
            ms_router.Register<PrepareGameMessage>(g => g.PrepareGame);
            ms_router.Register<StartGameMessage>(g => g.StartGame);
            ms_router.Register<GameReplicationMessage>(g => g.OnReplicationMessage);
            ms_router.Register<ChoiceDecisionRequest>(g => g.OnChoiceDecision);
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

        private void PrepareGame(PrepareGameMessage message)
        {
            var player = Resolvable<Mox.Player>.Empty;
            if (User != null && message.Players != null)
            {
                message.Players.TryGetValue(User, out player);
            }

            m_instance = new Instance(player);
        }

        private void StartGame(StartGameMessage message)
        {
            GameStarted.Raise(this, EventArgs.Empty);
        }

        private void OnReplicationMessage(GameReplicationMessage message)
        {
            m_instance.ReplicationClient.Replicate(message.Command);
        }

        private void OnChoiceDecision(ChoiceDecisionRequest request)
        {
            Debug.Assert(InteractionRequested != null);

            InteractionRequestedEventArgs e = new InteractionRequestedEventArgs(m_channel, request);
            InteractionRequested.Raise(this, e);
            // Response is asynchronous
        }

        #endregion

        #region Events

        public event EventHandler GameStarted;
        public event EventHandler<InteractionRequestedEventArgs> InteractionRequested;

        #endregion

        #region Inner Types

        private class Instance
        {
            private readonly ReplicationClient<Game> m_replicationClient = new ReplicationClient<Game>();
            private readonly Resolvable<Mox.Player> m_player;

            public Instance(Resolvable<Mox.Player> player)
            {
                m_player = player;
            }

            public Mox.Player Player
            {
                get
                {
                    return m_player.IsEmpty ? null : m_player.Resolve(Game);
                }
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

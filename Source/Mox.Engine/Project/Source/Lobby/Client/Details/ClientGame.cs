using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;
using Mox.Replication;

namespace Mox.Lobby.Client
{
    internal class ClientGame : IGameService
    {
        #region Variables

        private static readonly MessageRouter<ClientGame, IChannel> ms_router = new MessageRouter<ClientGame, IChannel>();

        private readonly IChannel m_channel;

        private Instance m_instance = new Instance(Resolvable<Mox.Player>.Empty);

        #endregion

        #region Constructor

        static ClientGame()
        {
            ms_router.Register<PrepareGameMessage>(g => g.PrepareGame);
            ms_router.Register<GameStartedMessage>(g => g.OnGameStarted);
            ms_router.Register<GameReplicationMessage>(g => g.OnReplicationMessage);
            ms_router.Register<ChoiceDecisionRequest>(g => g.OnChoiceDecision);
        }

        public ClientGame(IChannel channel)
        {
            m_channel = channel;
        }

        #endregion

        #region Properties

        public bool IsStarted
        {
            get { return m_instance != null; }
        }

        public Game Game
        {
            get { return m_instance.Game; }
        }

        public Player Player
        {
            get { return m_instance.Player; }
        }

        public Guid LocalUserId
        {
            get; 
            internal set;
        }

        #endregion

        #region Methods

        Task<bool> IGameService.StartGame()
        {
            return StartGameImpl();
        }

        private async Task<bool> StartGameImpl()
        {
            var response = await m_channel.Request<StartGameRequest, StartGameResponse>(new StartGameRequest());
            return response.Result;
        }

        public void ReceiveMessage(Message message)
        {
            ms_router.Route(this, m_channel, message);
        }

        private void PrepareGame(PrepareGameMessage message)
        {
            var player = Resolvable<Player>.Empty;
            if (message.Players != null)
            {
                message.Players.TryGetValue(LocalUserId, out player);
            }

            m_instance = new Instance(player);
        }

        private void OnGameStarted(GameStartedMessage message)
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

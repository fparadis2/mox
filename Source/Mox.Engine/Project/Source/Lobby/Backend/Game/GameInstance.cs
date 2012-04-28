using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mox.Database;
using Mox.Replication;
using Mox.Transactions;

namespace Mox.Lobby.Backend
{
    public class GameInstance
    {
        #region Variables

        private readonly GameEngine m_gameEngine;
        private readonly ReplicationSource<Mox.Player> m_replicationSource;
        private readonly Thread m_gameThread;

        private readonly Dictionary<User, Mox.Player> m_playerMapping = new Dictionary<User, Mox.Player>();

        #endregion

        #region Constructor

        public GameInstance()
        {
            // Create game
            Game game = new Game();

            m_gameEngine = new GameEngine(game);
            m_replicationSource = new ReplicationSource<Mox.Player>(game, new OpenAccessControlStrategy<Mox.Player>());
            m_gameThread = new Thread(RunImpl);
        }

        #endregion

        #region Properties

        private Game Game
        {
            get { return m_gameEngine.Game; }
        }

        #endregion

        #region Methods

        public void Prepare(LobbyBackend lobby)
        {
            GameInitializer initializer = new GameInitializer(MasterCardFactory.Instance);

            PreparePlayers(initializer, lobby);
            PrepareAI();
            initializer.Initialize(Game);

            PrepareReplication(lobby);
        }

        private void PrepareAI()
        {
            m_gameEngine.AISupervisor.Parameters.GlobalAITimeout = TimeSpan.FromSeconds(10);
        }

        private void PreparePlayers(GameInitializer initializer, LobbyBackend lobby)
        {
            foreach (var player in lobby.Players)
            {
                Mox.Player gamePlayer = Game.CreatePlayer();
                gamePlayer.Name = player.User.Name;

                if (!player.User.IsAI)
                {
                    // Give a "slight" advantage to human players for debugging purposes
                    foreach (Color color in Enum.GetValues(typeof(Color)))
                    {
                        gamePlayer.ManaPool[color] = 10;
                    }
                }
                else
                {
                    m_playerMapping.Add(player.User, gamePlayer);
                }

                initializer.AssignDeck(gamePlayer, ResolveDeck(player.Data));
            }
        }

        private void PrepareReplication(LobbyBackend lobby)
        {
            foreach (var user in lobby.Users)
            {
                IChannel channel;
                if (lobby.TryGetChannel(user, out channel))
                {
                    Mox.Player player;
                    m_playerMapping.TryGetValue(user, out player);

                    ReplicationClient client = new ReplicationClient(channel);
                    m_replicationSource.Register(player, client);
                }
            }
        }

        private static Deck ResolveDeck(PlayerData data)
        {
            if (data.Deck != null)
            {
                return data.Deck;
            }

            return Random.New().Choose(MasterDeckLibrary.Instance.Decks.ToList());
        }

        public void Run()
        {
            m_gameThread.Start();
        }

        private void RunImpl()
        {
            m_gameEngine.Run(m_gameEngine.Game.Players[0]);
        }

        #endregion

        #region Inner Types

        private class ReplicationClient : IReplicationClient
        {
            #region Variables

            private readonly IChannel m_channel;

            #endregion

            #region Constructor

            public ReplicationClient(IChannel channel)
            {
                m_channel = channel;
            }

            #endregion

            #region Implementation of IReplicationClient

            public void Replicate(ICommand command)
            {
                m_channel.Send(new GameReplicationMessage { Command = command });
            }

            #endregion
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Mox.Database;
using Mox.Flow;
using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;
using Mox.Replication;
using Mox.Transactions;

namespace Mox.Lobby.Server
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

        public GameInstance(Guid lobbyId)
        {
            // Create game
            Game game = new Game();

            m_gameEngine = new GameEngine(game);
            m_replicationSource = new ReplicationSource<Mox.Player>(game, new OpenAccessControlStrategy<Mox.Player>());
            m_gameThread = new Thread(RunImpl) { IsBackground = true, Name = "Game Instance " + lobbyId };
        }

        #endregion

        #region Properties

        private Game Game
        {
            get { return m_gameEngine.Game; }
        }

        public Dictionary<Guid, Resolvable<Player>> GetPlayerMapping()
        {
            Dictionary<Guid, Resolvable<Player>> mapping = new Dictionary<Guid, Resolvable<Player>>();

            foreach (var m in m_playerMapping)
            {
                mapping.Add(m.Key.Id, m.Value);
            }

            return mapping;
        }

        #endregion

        #region Methods

        public void Prepare(LobbyBackend lobby)
        {
            GameInitializer initializer = new GameInitializer(MasterCardFactory.Instance, MasterCardDatabase.Instance);

            PreparePlayers(initializer, lobby);
            PrepareAI();
            initializer.Initialize(Game);

            PrepareLogger(lobby);
        }

        public void SetupReplication(LobbyBackend lobby)
        {
            PrepareReplication(lobby);
            PrepareControllers();
        }

        private void PrepareAI()
        {
            m_gameEngine.AISupervisor.Parameters.GlobalAITimeout = TimeSpan.FromSeconds(10);
        }

        private void PreparePlayers(GameInitializer initializer, LobbyBackend lobby)
        {
            foreach (var slot in lobby.PlayerSlots)
            {
                Player gamePlayer = Game.CreatePlayer();

                User user;
                PlayerData data;
                if (lobby.TryGetPlayer(slot.PlayerId, out user, out data))
                {
                    gamePlayer.Name = user.Name;

                    m_playerMapping.Add(user, gamePlayer);

                    // Give a "slight" advantage to human players for "debugging purposes"
                    foreach (Color color in Enum.GetValues(typeof(Color)))
                    {
                        gamePlayer.ManaPool[color] = 10;
                    }
                }
                else
                {
                    gamePlayer.Name = "HAL";
                }

                initializer.AssignDeck(gamePlayer, slot.CreateDeck());
            }
        }

        private void PrepareReplication(LobbyBackend lobby)
        {
            foreach (var user in lobby.Users)
            {
                Player player;
                m_playerMapping.TryGetValue(user, out player);

                ReplicationClient client = new ReplicationClient(user.Channel);
                m_replicationSource.Register(player, client);
            }
        }

        private void PrepareControllers()
        {
            foreach (var kvp in m_playerMapping)
            {
                m_gameEngine.Input.AssignClientInput(kvp.Value, new ChoiceDecisionMaker(kvp.Key.Channel));
            }
        }

        private void PrepareLogger(LobbyBackend lobby)
        {
            m_gameEngine.Game.Log = new GameLog(lobby);
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

        private class ChoiceDecisionMaker : IChoiceDecisionMaker
        {
            #region Variables

            private readonly IChannel m_channel;

            #endregion

            #region Constructor

            public ChoiceDecisionMaker(IChannel channel)
            {
                m_channel = channel;
            }

            #endregion

            #region Implementation of IChoiceDecisionMaker

            public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
            {
                var result = m_channel.Request<ChoiceDecisionRequest, ChoiceDecisionResponse>(new ChoiceDecisionRequest { Choice = choice }).Result;

                if (result == null)
                {
                    return choice.DefaultValue;
                }

                return result.Result;
            }

            #endregion
        }

        private class GameLog : ILog
        {
            #region Variables

            private readonly LobbyBackend m_lobby;

            #endregion

            #region Constructor

            public GameLog(LobbyBackend lobby)
            {
                m_lobby = lobby;
            }

            #endregion

            #region Implementation of ILog

            public void Log(LogMessage message)
            {
                m_lobby.Log.Log(message);

                m_lobby.Broadcast(new ServerMessage { Message = message.Text });
            }

            #endregion
        }

        #endregion
    }
}

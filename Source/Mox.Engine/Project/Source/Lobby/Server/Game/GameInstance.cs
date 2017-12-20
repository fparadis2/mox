using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Mox.AI;
using Mox.Database;
using Mox.Flow;
using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;
using Mox.Replication;
using Mox.Transactions;

namespace Mox.Lobby.Server
{
    public class GameInstance : IDisposable, ICancellable
    {
        #region Variables

        private readonly GameEngine m_gameEngine;
        private readonly ReplicationSource<Player> m_replicationSource;
        private readonly Thread m_gameThread;

        private readonly Dictionary<User, Player> m_playerMapping = new Dictionary<User, Player>();
        private readonly Dictionary<Player, User> m_userMapping = new Dictionary<Player, User>();

        private bool m_isCancelled;

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

        public void Dispose()
        {
            m_isCancelled = true;
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

        bool ICancellable.Cancel
        {
            get { return m_isCancelled; }
        }

        #endregion

        #region Methods

        public void Prepare(LobbyBackend lobby)
        {
            GameInitializer initializer = new GameInitializer(MasterCardFactory.Instance, MasterCardDatabase.Instance);

            m_gameEngine.Input.Fallback = PrepareAI(lobby);

            PreparePlayers(initializer, lobby);
            initializer.Initialize(Game);

            PrepareLogger(lobby);
        }

        public void SetupReplication(LobbyBackend lobby)
        {
            PrepareReplication(lobby);
            PrepareControllers();
        }

        private IChoiceDecisionMaker PrepareAI(LobbyBackend lobby)
        {
            var parameters = lobby.GameParameters.BotParameters;

            switch (parameters.AIType)
            {
                case LobbyGameAIType.Dead:
                    return new DeadGameInput();

                case LobbyGameAIType.Random:
                    return new RandomGameInput();

                case LobbyGameAIType.MinMax:
                    return new AISupervisor(Game)
                    {
                        Parameters =
                        {
                            GlobalAITimeout = TimeSpan.FromSeconds(parameters.TimeOut)
                        }
                    };

                default:
                    throw new NotImplementedException();
            }
        }

        private void PreparePlayers(GameInitializer initializer, LobbyBackend lobby)
        {
            var slots = lobby.PlayerSlots;

            for (int index = 0; index < slots.Count; index++)
            {
                var slot = slots[index];

                Player gamePlayer = Game.CreatePlayer();
                Debug.Assert(gamePlayer.Index == index);

                if (slot.Player != null)
                {
                    gamePlayer.Name = slot.Player.Identity.Name;

                    m_playerMapping.Add(slot.Player.User, gamePlayer);
                    m_userMapping.Add(gamePlayer, slot.Player.User);

                    if (!slot.Player.IsBot)
                    {
                        // Give a "slight" advantage to human players for "debugging purposes"
                        /*foreach (Color color in Enum.GetValues(typeof(Color)))
                        {
                            gamePlayer.ManaPool[color] = 10;
                        }*/
                    }
                }

                initializer.AssignDeck(gamePlayer, slot.Deck.CreateDeck());
            }
        }

        private void PrepareReplication(LobbyBackend lobby)
        {
            foreach (var user in lobby.Users)
            {
                if (user.Channel != null)
                {
                    Player player;
                    m_playerMapping.TryGetValue(user, out player);

                    ReplicationClient client = new ReplicationClient(user.Channel);
                    m_replicationSource.Register(player, client);
                }
            }
        }

        private void PrepareControllers()
        {
            foreach (var kvp in m_playerMapping)
            {
                if (kvp.Key.Channel != null)
                {
                    m_gameEngine.Input.AssignClientInput(kvp.Value, new ChoiceDecisionMaker(kvp.Key.Channel));
                }
            }
        }

        private void PrepareLogger(LobbyBackend lobby)
        {
            m_gameEngine.Game.Log = new GameLog(lobby, this);
        }

        public void Run()
        {
            m_gameThread.Start();
        }

        private void RunImpl()
        {
            m_gameEngine.Run(m_gameEngine.Game.Players[0], this);
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
                var request = new ChoiceDecisionRequest { Choice = choice };

                ChoiceDecisionResponse result = null;

                try
                {
                    result = m_channel.Request<ChoiceDecisionRequest, ChoiceDecisionResponse>(request).Result;
                }
                catch
                {
                    // todo, replace with ai?
                }

                if (result == null)
                {
                    return choice.DefaultValue;
                }

                return result.Result;
            }

            #endregion
        }

        private class GameLog : IGameLog
        {
            #region Variables

            private readonly LobbyBackend m_lobby;
            private readonly GameInstance m_gameInstance;

            #endregion

            #region Constructor

            public GameLog(LobbyBackend lobby, GameInstance gameInstance)
            {
                m_lobby = lobby;
                m_gameInstance = gameInstance;
            }

            #endregion

            #region Implementation of ILog

            public void Log(Player source, FormattableString message)
            {
                LogMessage logMessage = new LogMessage
                {
                    Importance = LogImportance.Normal,
                    Text = "> " + message.ToString(Mox.GameLog.Formatter)
                };

                m_lobby.Log.Log(logMessage);

                if (m_gameInstance.m_userMapping.TryGetValue(source, out User user))
                {
                    m_lobby.Broadcast(new Network.Protocol.GameMessage
                    {
                        User = user.Id,
                        Message = message.ToString(Mox.GameLog.Formatter)
                    });
                }
            }

            #endregion
        }

        #endregion
    }
}

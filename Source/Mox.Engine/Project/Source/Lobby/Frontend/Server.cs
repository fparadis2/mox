using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mox.Lobby.Backend;

namespace Mox.Lobby
{
    public abstract class Server
    {
        #region Variables

        private static readonly MessageRouter<Server> ms_router = new MessageRouter<Server>();

        private readonly LobbyServiceBackend m_lobbyServiceBackend;
        private readonly object m_connectionLock = new object();
        private readonly ConnectionList m_connections;
        private readonly ILog m_log;

        #endregion

        #region Constructor

        static Server()
        {
            ms_router.Register<EnumerateLobbiesRequest, EnumerateLobbiesResponse>(s => s.EnumerateLobbies);
            ms_router.Register<EnterLobbyRequest, JoinLobbyResponse>(s => s.JoinLobby);
            ms_router.Register<CreateLobbyRequest, JoinLobbyResponse>(s => s.CreateLobby);
        }

        protected Server(ILog log)
        {
            Throw.IfNull(log, "log");
            m_log = log;
            m_connections = new ConnectionList(this);
            m_lobbyServiceBackend = new LobbyServiceBackend(m_log);
        }

        #endregion

        #region Properties

        protected ILog Log
        {
            get { return m_log; }
        }

        /// <summary>
        /// Returns true if the server is started.
        /// </summary>
        protected bool IsStarted
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        #region Static Creation

        public static LocalServer CreateLocal(ILog log)
        {
            return new LocalServer(log);
        }

        public static NetworkServer CreateNetwork(ILog log)
        {
            return new NetworkServer(log);
        }

        #endregion

        #region Host

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <remarks>
        /// Returns true if the host was opened correctly.
        /// </remarks>
        public bool Start()
        {
            Throw.InvalidOperationIf(IsStarted, "Server is already started");

            if (!StartImpl())
            {
                return false;
            }

            Log.Log(LogImportance.Low, "Server is running...");

            IsStarted = true;
            return true;
        }

        protected virtual bool StartImpl()
        {
            return true;
        }

        public void Stop()
        {
            if (IsStarted)
            {
                StopImpl();
                IsStarted = false;
            }
        }

        protected virtual void StopImpl()
        {
            lock (m_connectionLock)
            {
                foreach (var connection in m_connections.ToList())
                {
                    Logout(connection, "server stopping");
                }

                m_connections.Clear();
            }
        }

        #endregion

        #region Connection

        protected void OnClientConnected(IChannel client)
        {
            lock (m_connectionLock)
            {
                m_connections.Add(client);
            }
        }

        private void WhenMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ms_router.Route(this, (IChannel)sender, e.Message);
        }

        private void WhenDisconnected(object sender, EventArgs e)
        {
            lock (m_connectionLock)
            {
                Logout((IChannel)sender, "disconnected");
            }
        }

        #endregion

        #region Lobbies

        private EnumerateLobbiesResponse EnumerateLobbies(EnumerateLobbiesRequest request)
        {
            return new EnumerateLobbiesResponse { Lobbies = m_lobbyServiceBackend.Lobbies.Select(l => l.Id) };
        }

        private JoinLobbyResponse JoinLobby(IChannel channel, EnterLobbyRequest request)
        {
            User user = new User(request.Username);

            lock (m_connectionLock)
            {
                UserInfo userInfo;
                if (m_connections.TryGetUserInfo(channel, out userInfo))
                {
                    return AlreadyLoggedIn(userInfo.User, userInfo.Lobby);
                }

                var lobby = m_lobbyServiceBackend.JoinLobby(request.LobbyId, channel, user);

                if (lobby != null)
                {
                    m_connections.JoinLobby(channel, user, lobby);
                }

                return lobby == null ?
                    new JoinLobbyResponse { Result = LoginResult.InvalidLobby, User = user, LobbyId = request.LobbyId } :
                    new JoinLobbyResponse { Result = LoginResult.Success, User = user, LobbyId = lobby.Id };
            }
        }

        private JoinLobbyResponse CreateLobby(IChannel channel, CreateLobbyRequest request)
        {
            User user = new User(request.Username);
            
            lock (m_connectionLock)
            {
                UserInfo userInfo;
                if (m_connections.TryGetUserInfo(channel, out userInfo))
                {
                    return AlreadyLoggedIn(userInfo.User, userInfo.Lobby);
                }

                var newLobby = m_lobbyServiceBackend.CreateLobby(channel, user);
                Debug.Assert(newLobby != null);
                m_connections.JoinLobby(channel, user, newLobby);

                return new JoinLobbyResponse { Result = LoginResult.Success, User = user, LobbyId = newLobby.Id };
            }
        }

        private JoinLobbyResponse AlreadyLoggedIn(User user, LobbyBackend lobby)
        {
            // Log ip?
            Log.Log(LogImportance.Debug, "{0} is already logged in", user);
            return new JoinLobbyResponse { Result = LoginResult.AlreadyLoggedIn, User = user, LobbyId = lobby.Id };
        }

        private void Logout(IChannel client, string reason)
        {
            UserInfo userInfo;
            lock (m_connectionLock)
            {
                userInfo = m_connections.Remove(client);
            }

            if (userInfo != null)
            {
                Log.Log(LogImportance.Normal, "{0} is leaving ({1})", userInfo.User, reason);
                userInfo.Lobby.Logout(client);
            }
        }

        // For tests
        public LobbyBackend GetLobby(Guid lobbyId)
        {
            return m_lobbyServiceBackend.GetLobby(lobbyId);
        }

        #endregion

        #endregion

        #region Inner Types

        private class ConnectionList : IEnumerable<IChannel>
        {
            #region Variables

            private readonly Server m_server;

            private readonly List<IChannel> m_allConnections = new List<IChannel>();
            private readonly Dictionary<IChannel, UserInfo> m_connectionsByLobby = new Dictionary<IChannel, UserInfo>();

            #endregion

            #region Constructor

            public ConnectionList(Server server)
            {
                m_server = server;
            }

            #endregion

            #region Methods

            public bool TryGetUserInfo(IChannel channel, out UserInfo userInfo)
            {
                return m_connectionsByLobby.TryGetValue(channel, out userInfo);
            }

            public void Add(IChannel connection)
            {
                m_allConnections.Add(connection);

                connection.MessageReceived += m_server.WhenMessageReceived;
                connection.Disconnected += m_server.WhenDisconnected;
            }

            public UserInfo Remove(IChannel connection)
            {
                connection.MessageReceived -= m_server.WhenMessageReceived;
                connection.Disconnected -= m_server.WhenDisconnected;

                m_allConnections.Remove(connection);

                UserInfo userInfo;
                if (m_connectionsByLobby.TryGetValue(connection, out userInfo))
                {
                    m_connectionsByLobby.Remove(connection);
                    return userInfo;
                }

                return null;
            }

            public void Clear()
            {
                foreach (var connection in m_allConnections)
                {
                    connection.MessageReceived -= m_server.WhenMessageReceived;
                    connection.Disconnected -= m_server.WhenDisconnected;
                }

                m_allConnections.Clear();
                m_connectionsByLobby.Clear();
            }

            public void JoinLobby(IChannel channel, User user, LobbyBackend lobby)
            {
                m_connectionsByLobby.Add(channel, new UserInfo(user, lobby));
            }

            #endregion

            #region Implementation of IEnumerable

            public IEnumerator<IChannel> GetEnumerator()
            {
                return m_allConnections.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        private class UserInfo
        {
            public readonly User User;
            public readonly LobbyBackend Lobby;

            public UserInfo(User user, LobbyBackend lobby)
            {
                User = user;
                Lobby = lobby;
            }
        }

        #endregion
    }
}

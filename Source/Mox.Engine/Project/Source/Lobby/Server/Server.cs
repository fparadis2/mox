using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Server
{
    public abstract class Server
    {
        #region Variables

        private static readonly MessageRouter<Server, IChannel> ms_router = new MessageRouter<Server, IChannel>();

        private readonly LobbyServiceBackend m_lobbyServiceBackend;
        private readonly object m_connectionLock = new object();
        private readonly ConnectionList m_connections;
        private readonly ILog m_log;

        #endregion

        #region Constructor

        static Server()
        {
            ms_router.Register<LogoutMessage>(s => s.Logout);
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
            IChannel channel = (IChannel) sender;
            e.Response = Route(channel, e.Message);
        }

        private void WhenDisconnected(object sender, EventArgs e)
        {
            lock (m_connectionLock)
            {
                Logout((IChannel)sender, "disconnected");
            }
        }

        private bool TryGetUserInfo(IChannel channel, out UserInfo userInfo)
        {
            lock (m_connectionLock)
            {
                return m_connections.TryGetUserInfo(channel, out userInfo);
            }
        }

        private Response Route(IChannel channel, Message message)
        {
            UserInfo userInfo;
            if (TryGetUserInfo(channel, out userInfo))
            {
                var response = userInfo.Lobby.Route(userInfo.User, message);
                if (response != null)
                    return response;
            }

            return ms_router.Route(this, channel, message);
        }

        #endregion

        #region Lobbies

        private EnumerateLobbiesResponse EnumerateLobbies(EnumerateLobbiesRequest request)
        {
            return new EnumerateLobbiesResponse { Lobbies = m_lobbyServiceBackend.Lobbies.Select(l => l.Id).ToArray() };
        }

        private JoinLobbyResponse CreateLobby(IChannel channel, CreateLobbyRequest request)
        {
            string error;
            if (!ValidateIdentity(request.Identity, out error))
                return InvalidIdentity(error);

            User user = new User(channel, request.Identity.Name);

            var parameters = request.Parameters.ToParameters(out error);
            if (!string.IsNullOrEmpty(error))
            {
                return InvalidLobbyParameters(user, error);
            }

            lock (m_connectionLock)
            {
                UserInfo userInfo;
                if (m_connections.TryGetUserInfo(channel, out userInfo))
                {
                    return AlreadyLoggedIn(user, userInfo.Lobby);
                }

                var newLobby = m_lobbyServiceBackend.CreateLobby(user, request.Identity, parameters);
                Debug.Assert(newLobby != null);
                m_connections.JoinLobby(channel, user, newLobby);

                Log.Log(LogImportance.Normal, "{0} created lobby {1}", user, newLobby.Id);
                return CreateSuccessfulLobbyJoinResponse(user, newLobby);
            }
        }

        private JoinLobbyResponse JoinLobby(IChannel channel, EnterLobbyRequest request)
        {
            string error;
            if (!ValidateIdentity(request.Identity, out error))
                return InvalidIdentity(error);

            User user = new User(channel, request.Identity.Name);

            lock (m_connectionLock)
            {
                UserInfo userInfo;
                if (m_connections.TryGetUserInfo(channel, out userInfo))
                {
                    return AlreadyLoggedIn(user, userInfo.Lobby);
                }

                var lobby = m_lobbyServiceBackend.JoinLobby(request.LobbyId, user, request.Identity);

                if (lobby != null)
                {
                    m_connections.JoinLobby(channel, user, lobby);
                    Log.Log(LogImportance.Normal, "{0} entered lobby {1}", user, lobby.Id);
                    return CreateSuccessfulLobbyJoinResponse(user, lobby);
                }

                return FailedToLogin(user, request.LobbyId);
            }
        }

        private JoinLobbyResponse CreateSuccessfulLobbyJoinResponse(User user, LobbyBackend lobby)
        {
            return new JoinLobbyResponse
            {
                Result = LoginResult.Success, 
                UserId = user.Id, 
                LobbyId = lobby.Id,
                NumSlots = lobby.PlayerSlots.Count,
                LobbyParameters = LobbyParametersNetworkData.FromParameters(lobby.Parameters)
            };
        }
        
        private JoinLobbyResponse AlreadyLoggedIn(User user, LobbyBackend lobby)
        {
            Log.Log(LogImportance.Normal, "{0} is already logged in", user);
            return new JoinLobbyResponse { Result = LoginResult.AlreadyLoggedIn, LobbyId = lobby.Id };
        }

        private JoinLobbyResponse FailedToLogin(User user, Guid lobby)
        {
            Log.Log(LogImportance.Normal, "{0} tried to enter invalid lobby {1}", user, lobby);
            return new JoinLobbyResponse { Result = LoginResult.InvalidLobby, LobbyId = lobby };
        }

        private JoinLobbyResponse InvalidLobbyParameters(User user, string error)
        {
            Log.Log(LogImportance.Debug, "CreateLobby attempt from {0} with invalid lobby parameters: {1}", user, error);
            return new JoinLobbyResponse { Result = LoginResult.InvalidLobbyParameters, Error = error };
        }

        private static bool ValidateIdentity(IUserIdentity identity, out string error)
        {
            if (identity == null)
            {
                error = "Null Identity";
                return false;
            }

            if (string.IsNullOrEmpty(identity.Name))
            {
                error = "Invalid Name";
                return false;
            }

            error = null;
            return true;
        }

        private JoinLobbyResponse InvalidIdentity(string error)
        {
            return new JoinLobbyResponse { Result = LoginResult.InvalidIdentity, Error = error };
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
                m_lobbyServiceBackend.Logout(userInfo.User, userInfo.Lobby, reason);
            }
        }

        private void Logout(IChannel channel, LogoutMessage message)
        {
            Logout(channel, "logged out");
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

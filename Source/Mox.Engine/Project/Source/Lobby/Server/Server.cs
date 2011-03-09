using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Mox.Lobby.Backend;
using Mox.Lobby.Network;

namespace Mox.Lobby
{
    public abstract class Server : IServerContract
    {
        #region Variables

        private readonly LobbyServiceBackend m_lobbyServiceBackend;

        private readonly Dictionary<string, ClientInfo> m_clients = new Dictionary<string, ClientInfo>();
        private readonly ReadWriteLock m_clientLock = ReadWriteLock.CreateNoRecursion();
        private readonly ILog m_log;

        #endregion

        #region Constructor

        protected Server(ILog log)
        {
            Throw.IfNull(log, "log");
            m_log = log;

            m_lobbyServiceBackend = new LobbyServiceBackend(m_log);
        }

        #endregion

        #region Properties

        protected ILog Log
        {
            get { return m_log; }
        }

        private ClientInfo CurrentClient
        {
            get
            {
                ClientInfo currentClient;
                using (m_clientLock.Read)
                {
                    m_clients.TryGetValue(CurrentSessionId, out currentClient);
                }
                return currentClient;
            }
        }

        protected IEnumerable<ClientInfo> AllClients
        {
            get
            {
                using (m_clientLock.Read)
                {
                    return m_clients.Values.ToArray();
                }
            }
        }

        protected abstract string CurrentSessionId
        {
            get;
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

        #region IServerContract

        public IEnumerable<Guid> GetLobbies()
        {
            return m_lobbyServiceBackend.Lobbies.Select(l => l.Id);
        }

        public LoginDetails CreateLobby(string userName)
        {
            var client = CurrentClient;
            if (client != null)
            {
                return AlreadyLoggedIn(client);
            }

            client = CreateClient(userName);

            LoginDetails details;
            using (m_clientLock.Write)
            {
                var lobbyBackend = m_lobbyServiceBackend.CreateLobby(client);
                details = LoginImpl(client, lobbyBackend);
            }

            Log.Log(LogImportance.Normal, "{0} created lobby {1}", client.User, details.LobbyId);
            return details;
        }

        private LoginDetails AlreadyLoggedIn(ClientInfo client)
        {
            Log.Log(LogImportance.Debug, "{0} is already logged in", client.User);
            return new LoginDetails(LoginResult.AlreadyLoggedIn, client.User, client.LobbyId);
        }

        public LoginDetails EnterLobby(Guid lobby, string userName)
        {
            var client = CurrentClient;
            if (client != null)
            {
                return AlreadyLoggedIn(client);
            }

            client = CreateClient(userName);

            LoginDetails details;
            using (m_clientLock.Write)
            {
                var lobbyBackend = m_lobbyServiceBackend.JoinLobby(lobby, client);

                details = lobbyBackend == null ? 
                    new LoginDetails(LoginResult.InvalidLobby, client.User, lobby) : 
                    LoginImpl(client, lobbyBackend);
            }

            switch (details.Result)
            {
                case LoginResult.InvalidLobby:
                    Log.Log(LogImportance.Debug, "{0} tried to enter invalid lobby {1}", client.User, details.LobbyId);
                    break;
                case LoginResult.Success:
                    Log.Log(LogImportance.Normal, "{0} entered lobby {1}", client.User, details.LobbyId);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return details;
        }

        private LoginDetails LoginImpl(ClientInfo client, LobbyBackend lobby)
        {
            Debug.Assert(lobby != null);
            client.Initialize(lobby);
            m_clients.Add(client.SessionId, client);
            return new LoginDetails(LoginResult.Success, client.User, client.LobbyId);
        }

        public void Logout()
        {
            var client = CurrentClient;
            if (client != null)
            {
                Logout(client, "user quit");
            }
        }

        private bool Logout(ClientInfo client, string reason)
        {
            bool loggingOut;
            using (m_clientLock.Write)
            {
                loggingOut = m_clients.Remove(client.SessionId);
            }

            if (loggingOut)
            {
                Log.Log(LogImportance.Normal, "{0} is leaving ({1})", client.User, reason);
                client.Lobby.Logout(client);
            }

            return loggingOut;
        }

        public User[] GetUsers()
        {
            throw new NotImplementedException();
        }

        public void Say(string message)
        {
            var client = CurrentClient;
            if (client != null)
            {
                client.Lobby.ChatService.Say(client.User, message);
            }
        }

        #endregion

        #region Methods

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
        }

        #endregion

        #region Connection

        protected abstract IClientContract GetCurrentCallback();

        protected virtual void Disconnect(IClientContract callback)
        {}

        #endregion

        public LobbyBackend GetLobby(Guid lobbyId)
        {
            return m_lobbyServiceBackend.GetLobby(lobbyId);
        }

        private ClientInfo CreateClient(string userName)
        {
            User newUser = new User(userName);
            return new ClientInfo(this, newUser, CurrentSessionId, GetCurrentCallback());
        }

        protected void TryDo(ClientInfo client, Action<IClientContract> action)
        {
            try
            {
                action(client.Callback);
            }
            catch (Exception e)
            {
                if (Logout(client, e is TimeoutException ? "timed out" : "client-side failure"))
                {
                    Disconnect(client.Callback);
                }
            }
        }

        #endregion

        #region Inner Types

        protected class ClientInfo : IClient, IChatClient
        {
            #region Variables

            private readonly Server m_owner;
            private readonly User m_user;
            private readonly string m_sessionId;
            private readonly IClientContract m_clientCallback;
            private LobbyBackend m_lobby;

            #endregion

            #region Constructor

            public ClientInfo(Server owner, User user, string sessionId, IClientContract clientCallback)
            {
                m_owner = owner;
                m_user = user;
                m_sessionId = sessionId;
                m_clientCallback = clientCallback;
            }

            #endregion

            #region Properties

            public string SessionId
            {
                get { return m_sessionId; }
            }

            public LobbyBackend Lobby
            {
                get { return m_lobby; }
            }

            public Guid LobbyId
            {
                get { return m_lobby.Id; }
            }

            #endregion

            #region Methods

            public void Initialize(LobbyBackend lobby)
            {
                Throw.IfNull(lobby, "lobby");
                m_lobby = lobby;
            }

            private void TryDo(Action<IClientContract> action)
            {
                m_owner.TryDo(this, action);
            }

            #endregion

            #region IClient

            public User User
            {
                get { return m_user; }
            }

            public IChatClient ChatClient
            {
                get { return this; }
            }

            public IClientContract Callback
            {
                get { return m_clientCallback; }
            }

            public void OnUserChanged(UserChange change, User user)
            {
                TryDo(c => c.OnUserChanged(change, user));
            }

            #endregion

            #region Chat

            public void OnMessageReceived(User user, string message)
            {
                TryDo(c => c.OnMessageReceived(user, message));
            }

            #endregion
        }

        #endregion

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
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using Mox.Lobby.Backend;

namespace Mox.Lobby.Network
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class ServerBackend : IServerContract
    {
        #region Variables

        private readonly IServerAdapter m_adapter;
        private readonly LobbyServiceBackend m_lobbyServiceBackend = new LobbyServiceBackend();

        private readonly Dictionary<string, ClientInfo> m_clients = new Dictionary<string, ClientInfo>();
        private readonly ReadWriteLock m_clientLock = ReadWriteLock.CreateNoRecursion();

        #endregion

        #region Constructor

        private ServerBackend(IServerAdapter adapter)
        {
            Throw.IfNull(adapter, "adapter");
            m_adapter = adapter;
        }

        #endregion

        #region Properties

        private ClientInfo CurrentClient
        {
            get
            {
                ClientInfo currentClient;
                using (m_clientLock.Read)
                {
                    m_clients.TryGetValue(m_adapter.SessionId, out currentClient);
                }
                return currentClient;
            }
        }

        #endregion

        #region IServerContract

        public LoginDetails CreateLobby(string userName)
        {
            var client = CurrentClient;
            if (client != null)
            {
                // Already logged in
                return new LoginDetails(LoginResult.AlreadyLoggedIn, client.User, client.LobbyId);
            }

            client = CreateClient(userName);

            using (m_clientLock.Write)
            {
                var lobbyBackend = m_lobbyServiceBackend.CreateLobby(client);
                return LoginImpl(client, lobbyBackend);
            }
        }

        public LoginDetails EnterLobby(Guid lobby, string userName)
        {
            var client = CurrentClient;
            if (client != null)
            {
                // Already logged in
                return new LoginDetails(LoginResult.AlreadyLoggedIn, client.User, client.LobbyId);
            }

            client = CreateClient(userName);

            using (m_clientLock.Write)
            {
                var lobbyBackend = m_lobbyServiceBackend.JoinLobby(lobby, client);

                if (lobbyBackend == null)
                {
                    return new LoginDetails(LoginResult.InvalidLobby, client.User, lobby);
                }

                return LoginImpl(client, lobbyBackend);
            }
        }

        private LoginDetails LoginImpl(ClientInfo client, LobbyBackend lobby)
        {
            Debug.Assert(lobby != null);
            client.Initialize(lobby);
            m_clients.Add(m_adapter.SessionId, client);
            return new LoginDetails(LoginResult.Success, client.User, client.LobbyId);
        }

        public void Logout()
        {
            throw new NotImplementedException();
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

        public LobbyBackend GetLobby(Guid lobbyId)
        {
            return m_lobbyServiceBackend.GetLobby(lobbyId);
        }

        private ClientInfo CreateClient(string userName)
        {
            User newUser = new User(userName);
            return new ClientInfo(newUser, m_adapter.GetCallback<IClientContract>());
        }

        #endregion

        #region Inner Types

        private class ClientInfo : IClient, IChatClient
        {
            #region Variables

            private readonly User m_user;
            private readonly IClientContract m_clientCallback;
            private LobbyBackend m_lobby;

            #endregion

            #region Constructor

            public ClientInfo(User user, IClientContract clientCallback)
            {
                m_user = user;
                m_clientCallback = clientCallback;
            }

            #endregion

            #region Properties

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

            #endregion

            #region Chat

            public void OnMessageReceived(User user, string message)
            {
#warning TODO: Try/Catch + Drop client
                m_clientCallback.OnMessageReceived(user, message);
            }

            #endregion
        }

        #endregion

        #region Static Creation

        public static ServerBackend CreateLocal()
        {
            return new ServerBackend(new LocalServerAdapter());
        }

        #endregion
    }
}

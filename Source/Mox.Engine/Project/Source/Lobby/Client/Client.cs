using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Mox.Lobby.Network;
using Mox.Lobby.Network.Protocol;
using Mox.Lobby.Server;
using Mox.Threading;

namespace Mox.Lobby.Client
{
    public abstract class Client
    {
        #region Variables
        
        private ClientState m_state = ClientState.New;
        private IChannel m_channel;
        private ClientLobby m_lobby;
        private IDispatcher m_dispatcher = new FreeDispatcher();

        #endregion

        #region Properties

        /// <summary>
        /// Whether the client is currently connected.
        /// </summary>
        public bool IsConnected
        {
            get { return m_state == ClientState.Connected; }
        }

        public abstract string ServerName { get; }

        public ILobby Lobby
        {
            get
            {
                ThrowIfNotLoggedIn();
                return m_lobby;
            }
        }

        public IDispatcher Dispatcher
        {
            get { return m_dispatcher; }
            set { m_dispatcher = value ?? new FreeDispatcher(); }
        }

        #endregion

        #region Methods

        #region Connection

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <remarks>
        /// True if connection could be established.
        /// </remarks>
        public bool Connect()
        {
            if (m_state == ClientState.New)
            {
                Debug.Assert(m_channel == null);
                m_channel = CreateChannel();
                if (m_channel != null)
                {
                    m_lobby = new ClientLobby(m_channel);
                    m_state = ClientState.Connected;
                }
            }

            return m_state == ClientState.Connected;
        }

        internal abstract IChannel CreateChannel();

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            if (m_state == ClientState.Connected)
            {
                m_channel.Send(new LogoutMessage());
                DeleteServer();
            }
        }

        protected void DeleteServer()
        {
            if (m_state == ClientState.Connected)
            {
                m_state = ClientState.Disconnected;
                DisposableHelper.SafeDispose(ref m_lobby);
                DeleteServerImpl(m_channel);
                m_channel = null;
                OnDisconnected(EventArgs.Empty);
            }
        }

        internal abstract void DeleteServerImpl(IChannel channel);

        #endregion

        #region Login

        public async Task<IEnumerable<Guid>> GetLobbies()
        {
            var result = await m_channel.Request<EnumerateLobbiesRequest, EnumerateLobbiesResponse>(new EnumerateLobbiesRequest());
            return result.Lobbies;
        }

        public void CreateLobby(IPlayerIdentity identity, LobbyParameters lobbyParameters)
        {
            ThrowIfLoggedIn();

            var request = new CreateLobbyRequest
            {
                Identity = identity, 
                Parameters = LobbyParametersNetworkData.FromParameters(lobbyParameters)
            };

            var response = m_channel.Request<CreateLobbyRequest, JoinLobbyResponse>(request).Result;

            CheckLogin(Guid.Empty, response);
            m_lobby.Initialize(response, identity);
        }

        public void EnterLobby(Guid lobbyId, IPlayerIdentity identity)
        {
            ThrowIfLoggedIn();

            var request = new EnterLobbyRequest
            {
                LobbyId = lobbyId,
                Identity = identity
            };

            var response = m_channel.Request<EnterLobbyRequest, JoinLobbyResponse>(request).Result;

            CheckLogin(lobbyId, response);
            m_lobby.Initialize(response, identity);
        }

        private static void CheckLogin(Guid lobbyId, JoinLobbyResponse response)
        {
            switch (response.Result)
            {
                case LoginResult.Success:
                    break;

                case LoginResult.AlreadyLoggedIn:
                    Throw.InvalidArgumentIf(lobbyId != response.LobbyId, "Already connected to another lobby", "lobbyId");
                    break;

                case LoginResult.InvalidLobby:
                    throw new ArgumentException("Unknown lobby id");

                case LoginResult.UnknownFailure:
                    throw new ApplicationException("Unknown failure while loggin in");

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region Utils

        protected void ThrowIfLoggedIn()
        {
            Throw.InvalidOperationIf(m_lobby.IsLoggedIn, "The client is already logged into another lobby.");
        }

        protected void ThrowIfNotLoggedIn()
        {
            Throw.InvalidOperationIf(!m_lobby.IsLoggedIn, "Cannot access this property/method when not logged in.");
        }

        protected void AssertStateIs(ClientState state)
        {
            Throw.InvalidOperationIf(m_state == state, string.Format("Cannot do this operation while the client is in the {0} state.", m_state));
        }

        #endregion

        #region Creation Methods

        public static Client CreateLocal(LocalServer server)
        {
            return new LocalClient(server);
        }

        public static NetworkClient CreateNetwork()
        {
            return new NetworkClient();
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler Disconnected;

        protected virtual void OnDisconnected(EventArgs e)
        {
            Disconnected.Raise(this, EventArgs.Empty);
        }

        #endregion

        #region Inner Types
        
        protected enum ClientState
        {
            /// <summary>
            /// New client, ready to connect
            /// </summary>
            New,
            /// <summary>
            /// Connected
            /// </summary>
            Connected,
            /// <summary>
            /// Disconnected,
            /// </summary>
            Disconnected,
        }

        #endregion
    }
}

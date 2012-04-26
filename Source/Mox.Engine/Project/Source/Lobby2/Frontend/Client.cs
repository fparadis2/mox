﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mox.Lobby2
{
    public abstract class Client
    {
        #region Variables
        
        private ClientState m_state = ClientState.New;
        private IChannel m_networkChannel;
        private ClientLobby m_lobby;

        #endregion

        #region Constructor

        #endregion

        #region Properties

        /// <summary>
        /// Whether the client is currently connected.
        /// </summary>
        public bool IsConnected
        {
            get { return m_state == ClientState.Connected; }
        }

        public ILobby Lobby
        {
            get
            {
                ThrowIfNotLoggedIn();
                return m_lobby;
            }
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
                Debug.Assert(m_networkChannel == null);
                m_networkChannel = CreateChannel();
                if (m_networkChannel != null)
                {
                    m_lobby = new ClientLobby(m_networkChannel);
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
                Send(new LogoutMessage());
                DeleteServer();
            }
        }

        protected void DeleteServer()
        {
            if (m_state == ClientState.Connected)
            {
                m_state = ClientState.Disconnected;
                DisposableHelper.SafeDispose(ref m_lobby);
                DeleteServerImpl(m_networkChannel);
                m_networkChannel = null;
                OnDisconnected(EventArgs.Empty);
            }
        }

        internal abstract void DeleteServerImpl(IChannel channel);

        #endregion

        #region Communication

        public TResponse Request<TResponse>(Message message)
            where TResponse : Message
        {
            return m_networkChannel.Request<TResponse>(message);
        }

        public void Send(Message message)
        {
            m_networkChannel.Send(message);
        }

        #endregion

        #region Login

        public IEnumerable<Guid> GetLobbies()
        {
            return Request<EnumerateLobbiesResponse>(new EnumerateLobbiesRequest()).Lobbies;
        }

        public void CreateLobby(string username)
        {
            ThrowIfLoggedIn();

            var response = Request<JoinLobbyResponse>(new CreateLobbyRequest { Username = username });

            CheckLogin(Guid.Empty, response);
            m_lobby.Initialize(response.User, response.LobbyId);
        }

        public void EnterLobby(Guid lobbyId, string username)
        {
            ThrowIfLoggedIn();

            var response = Request<JoinLobbyResponse>(new EnterLobbyRequest { LobbyId = lobbyId, Username = username });

            CheckLogin(lobbyId, response);
            m_lobby.Initialize(response.User, response.LobbyId);
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

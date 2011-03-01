using System;
using System.ServiceModel;

using Mox.Lobby;

namespace Mox.Network
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ILobbyClient))]
    public interface ILobbyService
    {
        #region Lobby management

        [OperationContract(IsInitiating = true)]
        Guid CreateLobby();

        [OperationContract(IsInitiating = true)]
        void EnterLobby(Guid lobby);

        [OperationContract(IsTerminating = true)]
        void Logout();

        #endregion

        #region User info

        [OperationContract]
        User[] GetUsers();

        #endregion

        #region Chat

        /// <summary>
        /// Called by the client to speak on behalf of the user
        /// </summary>
        /// <param name="message">The user message</param>
        [OperationContract(IsOneWay = true)]
        void Say(string message);

        #endregion
    }

    public interface ILobbyClient
    {
        #region User info

        /// <summary>
        /// Called by the service when a user connects or disconnects.
        /// </summary>
        /// <param name="user">The user that connected.</param>
        /// <param name="change">The change that occured.</param>
        [OperationContract(IsOneWay = true)]
        void OnUserChanged(User user, UserChange change);

        #endregion

        #region Chat

        /// <summary>
        /// Called by the service when a client says something.
        /// </summary>
        /// <param name="user">The player which talked</param>
        /// <param name="message">The player message</param>
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(User user, string message);

        #endregion
    }

    public enum UserChange
    {
        /// <summary>
        /// New user
        /// </summary>
        Connected,
        /// <summary>
        /// User quit
        /// </summary>
        Disconnected,
        /// <summary>
        /// User details changed (name, etc).
        /// </summary>
        DetailsChanged
    }

    //namespace Elucubrations
    //{
    //    public interface IChatClient
    //    {
    //        void Say(string msg);
    //        event EventHandler MessageReceived;
    //    }

    //    // Bound directly to a ChatServer
    //    public class LocalChatClient : IChatClient, ChatServer.IClient
    //    {
    //        public void Say(string msg)
    //        {
    //            ChatServer server;
    //            server.Say(localUser, msg);
    //        }

    //        public event EventHandler MessageReceived;

    //        public void OnMessageReceived()
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public class NetworkLobbyClient : INetworkLobbyClient
    //    {
    //        public void OnMessageReceived()
    //        {
    //            NetworkChatClient chatClient;
    //            chatClient.OnMessageReceived();
    //        }
    //    }

    //    public class NetworkChatClient : IChatClient
    //    {
    //        public void Say(string msg)
    //        {
    //            INetworkLobbyServer server;
    //            server.Say(msg);
    //        }

    //        public event EventHandler MessageReceived;

    //        public void OnMessageReceived()
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public class NetworkLobbyServer : INetworkLobbyServer
    //    {
    //        private readonly ChatServer m_server;

    //        public void Login()
    //        {
    //            INetworkLobbyClient client;
    //            m_server.Register(Wrap(client));
    //        }

    //        private ChatServer.IClient Wrap(INetworkLobbyClient client)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public void Say(string msg)
    //        {
    //            m_server.Say(user, msg);
    //        }
    //    }

    //    public interface INetworkLobbyClient
    //    {
    //        void OnMessageReceived();
    //    }

    //    public interface INetworkLobbyServer
    //    {
    //        void Login();
    //        void Say(string msg);
    //    }

    //    public class ChatServer
    //    {
    //        public void Register(IClient client)
    //        {
    //        }

    //        public void Say(User user, string msg)
    //        {
    //        }

    //        public interface IClient
    //        {
    //            void OnMessageReceived();
    //        }
    //    }
    //}
}

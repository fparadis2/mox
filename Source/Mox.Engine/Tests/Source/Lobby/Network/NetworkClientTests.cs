using System;
using Mox.Lobby.Backend;
using NUnit.Framework;

namespace Mox.Lobby.Network
{
    public abstract class NetworkClientTestsBase
    {
        #region Variables

        private ServerBackend m_server;
        private NetworkClient m_client1;
        private NetworkClient m_client2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_server = CreateServer();
            Assert.IsNotNull(m_server);

            m_client1 = CreateClient(m_server);
            m_client2 = CreateClient(m_server);

            Assert.IsNotNull(m_client1);
            Assert.IsNotNull(m_client2);

            Assert.That(m_client1.Connect());
            Assert.That(m_client2.Connect());

            m_client1.CreateLobby("Georges");
            m_client2.EnterLobby(m_client1.Lobby.Id, "John");
        }

        protected abstract ServerBackend CreateServer();
        protected abstract NetworkClient CreateClient(ServerBackend server);

        #endregion

        #region Tests

        [Test]
        public void Test_IsConnected_is_true_after_connection()
        {
            Assert.That(m_client1.IsConnected);
        }

        [Test]
        public void Test_Cannot_access_lobby_when_not_logged_in()
        {
            var client = CreateClient(m_server);
            client.Connect();

            Assert.Throws<InvalidOperationException>(() => client.Lobby.ToString());
        }

        [Test]
        public void Test_CreateLobby_creates_a_lobby_and_logs_the_user_in()
        {
            var lobby = m_client1.Lobby;
            Assert.IsNotNull(lobby);
            Assert.AreNotEqual(Guid.Empty, lobby.Id);
            Assert.AreEqual("Georges", lobby.User.Name);

            LobbyBackend serverLobby = m_server.GetLobby(lobby.Id);
            Assert.IsNotNull(serverLobby);
            Assert.Collections.Contains(lobby.User, serverLobby.Users);
        }

        [Test]
        public void Test_EnterLobby_logs_into_an_existing_lobby()
        {
            var lobby = m_client2.Lobby;
            Assert.IsNotNull(lobby);
            Assert.AreEqual(m_client1.Lobby.Id, lobby.Id);
            Assert.AreEqual("John", lobby.User.Name);

            LobbyBackend serverLobby = m_server.GetLobby(lobby.Id);
            Assert.IsNotNull(serverLobby);
            Assert.Collections.Contains(lobby.User, serverLobby.Users);
        }

        [Test]
        public void Test_EnterLobby_throws_when_lobby_doesnt_exist()
        {
            var client = CreateClient(m_server);
            client.Connect();
            Assert.Throws<ArgumentException>(() => client.EnterLobby(Guid.Empty, "Name"));
        }

        [Test]
        public void Test_CreateLobby_and_EnterLobby_throws_when_client_is_already_logged_in()
        {
            Assert.Throws<InvalidOperationException>(() => m_client2.CreateLobby("Kip"));
            Assert.Throws<InvalidOperationException>(() => m_client1.EnterLobby(m_client2.Lobby.Id, "Kip"));
        }

        [Test]
        public void Test_ChatService_works()
        {
            EventSink<MessageReceivedEventArgs> sink = new EventSink<MessageReceivedEventArgs>();
            m_client2.Lobby.Chat.MessageReceived += sink;

            Assert.EventCalledOnce(sink, () => m_client1.Lobby.Chat.Say("Hello!"));
            Assert.AreEqual(m_client1.Lobby.User, sink.LastEventArgs.User);
            Assert.AreEqual("Hello!", sink.LastEventArgs.Message);
        }

        #endregion
    }

    [TestFixture]
    public class LocalClientTests : NetworkClientTestsBase
    {
        protected override ServerBackend CreateServer()
        {
            return ServerBackend.CreateLocal();
        }

        protected override NetworkClient CreateClient(ServerBackend server)
        {
            return NetworkClient.CreateLocal(server);
        }
    }
}

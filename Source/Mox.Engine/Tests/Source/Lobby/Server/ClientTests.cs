using System;
using System.Linq;
using Mox.Lobby.Backend;
using NUnit.Framework;

namespace Mox.Lobby
{
    public abstract class ClientTestsBase
    {
        #region Variables

        private LogContext m_log;

        private Server m_server;
        private Client m_client1;
        private Client m_client2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_log = new LogContext();

            m_server = CreateServer(m_log);
            Assert.IsNotNull(m_server);

            Assert.That(m_server.Start());

            m_client1 = CreateClient(m_server);
            m_client2 = CreateClient(m_server);

            Assert.IsNotNull(m_client1);
            Assert.IsNotNull(m_client2);

            Assert.That(m_client1.Connect());
            Assert.That(m_client2.Connect());

            m_client1.CreateLobby("Georges");
            m_client2.EnterLobby(m_client1.Lobby.Id, "John");
        }

        [TearDown]
        public void Teardown()
        {
            m_server.Stop();
        }

        protected abstract Server CreateServer(ILog log);
        protected abstract Client CreateClient(Server server);

        #endregion

        #region Tests

        #region Misc

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
        public void Test_GetLobbies_returns_the_active_lobbies()
        {
            Assert.Collections.AreEqual(new[] { m_client1.Lobby.Id }, m_client1.GetLobbies());
        }

        #endregion

        #region Chat

        [Test]
        public void Test_ChatService_works()
        {
            EventSink<ChatMessageReceivedEventArgs> sink = new EventSink<ChatMessageReceivedEventArgs>();
            m_client2.Lobby.Chat.MessageReceived += sink;

            Assert.EventCalledOnce(sink, () => m_client1.Lobby.Chat.Say("Hello!"));
            Assert.AreEqual(m_client1.Lobby.User, sink.LastEventArgs.User);
            Assert.AreEqual("Hello!", sink.LastEventArgs.Message);
        }

        [Test]
        public void Test_ChatService_local_messages_get_echoed()
        {
            EventSink<ChatMessageReceivedEventArgs> sink = new EventSink<ChatMessageReceivedEventArgs>();
            m_client1.Lobby.Chat.MessageReceived += sink;

            Assert.EventCalledOnce(sink, () => m_client1.Lobby.Chat.Say("Hello!"));
            Assert.AreEqual(m_client1.Lobby.User, sink.LastEventArgs.User);
            Assert.AreEqual("Hello!", sink.LastEventArgs.Message);
        }

        #endregion

        #region Users & Players

        [Test]
        public void Test_Users_contain_the_users_of_the_lobby()
        {
            Assert.Collections.AreEquivalent(new [] { m_client1.Lobby.User, m_client2.Lobby.User }, m_client1.Lobby.Users);

            var client = CreateClient(m_server);
            client.Connect();

            client.EnterLobby(m_client1.Lobby.Id, "Third");

            Assert.Collections.AreEquivalent(new[] { m_client1.Lobby.User, m_client2.Lobby.User, client.Lobby.User }, m_client1.Lobby.Users);
        }

        [Test]
        public void Test_Users_are_updated_when_a_user_leaves()
        {
            m_client2.Disconnect();

            Assert.Collections.AreEquivalent(new[] { m_client1.Lobby.User }, m_client1.Lobby.Users);
        }

        [Test]
        public void Test_Players_contain_the_players_of_the_lobby()
        {
            User user2 = m_client2.Lobby.User;
            Assert.Collections.AreEquivalent(new[] { m_client1.Lobby.User, user2 }, m_client1.Lobby.Players.Select(p => p.User));

            m_client2.Disconnect();

            Assert.IsTrue(m_client1.Lobby.Players.Any(p => p.User == m_client1.Lobby.User));
            Assert.IsFalse(m_client1.Lobby.Players.Any(p => p.User == user2));
            
            var client = CreateClient(m_server);
            client.Connect();

            client.EnterLobby(m_client1.Lobby.Id, "Third");

            Assert.IsTrue(m_client1.Lobby.Players.Any(p => p.User == m_client1.Lobby.User));
            Assert.IsTrue(m_client1.Lobby.Players.Any(p => p.User == client.Lobby.User));
        }

        #endregion

        #endregion
    }
}

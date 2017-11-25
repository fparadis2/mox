using System;
using System.Linq;
using Mox.Lobby.Server;
using NUnit.Framework;

namespace Mox.Lobby.Client
{
    public abstract class ClientTestsBase
    {
        #region Variables

        private LogContext m_log;

        private LobbyParameters m_lobbyParameters;

        private Server.Server m_server;
        private Client m_client1;
        private Client m_client2;

        private UserIdentity m_identity1;
        private UserIdentity m_identity2;

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

            m_lobbyParameters = new LobbyParameters
            {
                GameFormat = new DuelFormat(),
                DeckFormat = new StandardDeckFormat(),
                AssignNewPlayersToFreeSlots = true
            };

            m_identity1 = new UserIdentity { Name = "Georges" };
            m_identity2 = new UserIdentity { Name = "John" };

            m_client1.CreateLobby(m_identity1, m_lobbyParameters);
            m_client2.EnterLobby(m_client1.Lobby.Id, m_identity2);
        }

        [TearDown]
        public void Teardown()
        {
            m_server.Stop();
        }

        protected abstract Server.Server CreateServer(ILog log);
        protected abstract Client CreateClient(Server.Server server);

        #endregion

        #region Tests

        #region Login

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
            Assert.AreNotEqual(Guid.Empty, lobby.LocalUserId);

            LobbyBackend serverLobby = m_server.GetLobby(lobby.Id);
            Assert.IsNotNull(serverLobby);
            var serverUser = serverLobby.Users.Single(u => u.Id == lobby.LocalUserId);
            Assert.AreEqual("Georges", serverUser.Name);
        }

        [Test]
        public void Test_EnterLobby_logs_into_an_existing_lobby()
        {
            var lobby = m_client2.Lobby;
            Assert.IsNotNull(lobby);
            Assert.AreEqual(m_client1.Lobby.Id, lobby.Id);
            Assert.AreNotEqual(Guid.Empty, lobby.LocalUserId);

            LobbyBackend serverLobby = m_server.GetLobby(lobby.Id);
            Assert.IsNotNull(serverLobby);
            var serverUser = serverLobby.Users.Single(u => u.Id == lobby.LocalUserId);
            Assert.AreEqual("John", serverUser.Name);
        }

        [Test]
        public void Test_EnterLobby_throws_when_lobby_doesnt_exist()
        {
            var client = CreateClient(m_server);
            client.Connect();
            Assert.Throws<ArgumentException>(() => client.EnterLobby(Guid.Empty, m_identity1));
        }

        [Test]
        public void Test_CreateLobby_and_EnterLobby_throws_when_client_is_already_logged_in()
        {
            Assert.Throws<InvalidOperationException>(() => m_client2.CreateLobby(m_identity1, m_lobbyParameters));
            Assert.Throws<InvalidOperationException>(() => m_client1.EnterLobby(m_client2.Lobby.Id, m_identity2));
        }

        [Test]
        public void Test_GetLobbies_returns_the_active_lobbies()
        {
            Assert.Collections.AreEqual(new[] { m_client1.Lobby.Id }, m_client1.GetLobbies().Result);
        }

        #endregion

        #region IMessageService

        [Test]
        public void Test_MessageService_SendMessage_works()
        {
            EventSink<ChatMessage> sink = new EventSink<ChatMessage>();
            m_client2.Lobby.Messages.ChatMessageReceived += sink;

            Assert.EventCalledOnce(sink, () => m_client1.Lobby.Messages.SendMessage("Hello!"));
            Assert.AreEqual(m_client1.Lobby.LocalUserId, sink.LastEventArgs.SpeakerUserId);
            Assert.AreEqual("Hello!", sink.LastEventArgs.Text);
        }

        [Test]
        public void Test_ChatService_local_messages_get_echoed()
        {
            EventSink<ChatMessage> sink = new EventSink<ChatMessage>();
            m_client1.Lobby.Messages.ChatMessageReceived += sink;

            Assert.EventCalledOnce(sink, () => m_client1.Lobby.Messages.SendMessage("Hello!"));
            Assert.AreEqual(m_client1.Lobby.LocalUserId, sink.LastEventArgs.SpeakerUserId);
            Assert.AreEqual("Hello!", sink.LastEventArgs.Text);
        }

        #endregion

        #region Users & Slots

        [Test]
        public void Test_Users_contain_the_users_of_the_lobby()
        {
            Assert.Collections.AreEquivalent(new[] { m_client1.Lobby.LocalUserId, m_client2.Lobby.LocalUserId }, m_client1.Lobby.Users.Select(p => p.Id));

            var client = CreateClient(m_server);
            client.Connect();

            client.EnterLobby(m_client1.Lobby.Id, new UserIdentity { Name = "Third" });

            Assert.Collections.AreEquivalent(new[] { m_client1.Lobby.LocalUserId, m_client2.Lobby.LocalUserId, client.Lobby.LocalUserId }, m_client1.Lobby.Users.Select(p => p.Id));
        }

        [Test]
        public void Test_Players_are_updated_when_a_player_leaves()
        {
            m_client2.Disconnect();

            Assert.Collections.AreEquivalent(new[] { m_client1.Lobby.LocalUserId }, m_client1.Lobby.Users.Select(p => p.Id));
        }

        [Test]
        public void Test_Slots_are_synchronized_when_players_join()
        {
            var lobby = m_client1.Lobby;
            Assert.AreEqual(2, lobby.Slots.Count);

            Assert.AreEqual(lobby.LocalUserId, lobby.Slots[0].PlayerId);
            Assert.AreEqual(m_client2.Lobby.LocalUserId, lobby.Slots[1].PlayerId);

            m_client2.Disconnect();

            Assert.AreEqual(lobby.LocalUserId, lobby.Slots[0].PlayerId);
            Assert.That(!lobby.Slots[1].IsAssigned);
            
            var client = CreateClient(m_server);
            client.Connect();

            client.EnterLobby(m_client1.Lobby.Id, new UserIdentity { Name = "Third" });

            Assert.AreEqual(lobby.LocalUserId, lobby.Slots[0].PlayerId);
            Assert.AreEqual(client.Lobby.LocalUserId, lobby.Slots[1].PlayerId);
        }

        [Test]
        public void Test_Can_change_the_PlayerSlotData_of_the_player()
        {
            var slotData = m_client1.Lobby.Slots[0];
            slotData.Deck.Name = "My Deck";

            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_client1.Lobby.SetPlayerSlotData(0, slotData).Result);
            Assert.AreEqual("My Deck", m_client1.Lobby.Slots[0].Deck.Name);
            Assert.AreEqual("My Deck", m_client2.Lobby.Slots[0].Deck.Name);
        }

        #endregion

        #endregion
    }
}

﻿using System;
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
        public void Test_User_gets_disconnected_if_exception_is_thrown_when_a_message_is_received()
        {
            var lobby = m_server.GetLobby(m_client1.Lobby.Id);

            m_client2.Lobby.Chat.MessageReceived += (o, e) => { throw new Exception(); };

            Assert.Collections.CountEquals(2, lobby.Users);
            m_client1.Lobby.Chat.Say("Hello!");

            Assert.Collections.CountEquals(1, lobby.Users);
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
            EventSink<MessageReceivedEventArgs> sink = new EventSink<MessageReceivedEventArgs>();
            m_client2.Lobby.Chat.MessageReceived += sink;

            Assert.EventCalledOnce(sink, () => m_client1.Lobby.Chat.Say("Hello!"));
            Assert.AreEqual(m_client1.Lobby.User, sink.LastEventArgs.User);
            Assert.AreEqual("Hello!", sink.LastEventArgs.Message);
        }

        [Test]
        public void Test_ChatService_local_messages_get_echoed()
        {
            EventSink<MessageReceivedEventArgs> sink = new EventSink<MessageReceivedEventArgs>();
            m_client1.Lobby.Chat.MessageReceived += sink;

            Assert.EventCalledOnce(sink, () => m_client1.Lobby.Chat.Say("Hello!"));
            Assert.AreEqual(m_client1.Lobby.User, sink.LastEventArgs.User);
            Assert.AreEqual("Hello!", sink.LastEventArgs.Message);
        }

        #endregion

        #region Users & Players

        [Test]
        public void Test_UserChanged_is_triggered_for_all_registered_users_when_subscribing()
        {
            EventSink<UserChangedEventArgs> sink1 = new EventSink<UserChangedEventArgs>();
            sink1.Callback += (o, e) => Assert.AreEqual(UserChange.Joined, e.Change);
            Assert.EventCalled(sink1, () => m_client1.Lobby.UserChanged += sink1, 2);

            EventSink<UserChangedEventArgs> sink2 = new EventSink<UserChangedEventArgs>();
            sink2.Callback += (o, e) => Assert.AreEqual(UserChange.Joined, e.Change);
            Assert.EventCalled(sink2, () => m_client2.Lobby.UserChanged += sink2, 2);
        }

        [Test]
        public void Test_UserChanged_is_triggered_when_a_user_joins_the_lobby()
        {
            EventSink<UserChangedEventArgs> sink = new EventSink<UserChangedEventArgs>();
            m_client1.Lobby.UserChanged += sink;

            var client = CreateClient(m_server);
            client.Connect();

            Assert.EventCalledOnce(sink, () => client.EnterLobby(m_client1.Lobby.Id, "Third"));

            Assert.AreEqual(UserChange.Joined, sink.LastEventArgs.Change);
            Assert.AreEqual(client.Lobby.User, sink.LastEventArgs.User);
        }

        [Test]
        public void Test_UserChanged_is_triggered_when_a_user_leaves_the_lobby()
        {
            EventSink<UserChangedEventArgs> sink = new EventSink<UserChangedEventArgs>();
            m_client1.Lobby.UserChanged += sink;

            User user2 = m_client2.Lobby.User;

            Assert.EventCalledOnce(sink, () => m_client2.Disconnect());

            Assert.AreEqual(UserChange.Left, sink.LastEventArgs.Change);
            Assert.AreEqual(user2, sink.LastEventArgs.User);
        }

        [Test]
        public void Test_PlayerChanged_is_triggered_for_all_players_when_subscribing()
        {
            EventSink<PlayerChangedEventArgs> sink1 = new EventSink<PlayerChangedEventArgs>();
            sink1.Callback += (o, e) => Assert.AreEqual(PlayerChange.Joined, e.Change);
            Assert.EventCalled(sink1, () => m_client1.Lobby.PlayerChanged += sink1, 2);

            EventSink<PlayerChangedEventArgs> sink2 = new EventSink<PlayerChangedEventArgs>();
            sink2.Callback += (o, e) => Assert.AreEqual(PlayerChange.Joined, e.Change);
            Assert.EventCalled(sink2, () => m_client2.Lobby.PlayerChanged += sink2, 2);
        }

        [Test]
        public void Test_PlayerChanged_is_triggered_when_a_player_joins()
        {
            m_client2.Disconnect();

            EventSink<PlayerChangedEventArgs> sink = new EventSink<PlayerChangedEventArgs>();
            m_client1.Lobby.PlayerChanged += sink;

            var client = CreateClient(m_server);
            client.Connect();

            Assert.EventCalledOnce(sink, () => client.EnterLobby(m_client1.Lobby.Id, "Third"));

            Assert.AreEqual(PlayerChange.Changed, sink.LastEventArgs.Change);
            Assert.AreEqual(client.Lobby.User, sink.LastEventArgs.Player.User);
        }

        [Test]
        public void Test_PlayerChanged_is_triggered_when_a_player_leaves()
        {
            EventSink<PlayerChangedEventArgs> sink = new EventSink<PlayerChangedEventArgs>();
            m_client1.Lobby.PlayerChanged += sink;

            Assert.EventCalledOnce(sink, () => m_client2.Disconnect());

            Assert.AreEqual(PlayerChange.Changed, sink.LastEventArgs.Change);
            Assert.That(sink.LastEventArgs.Player.User.IsAI);
        }

        #endregion

        #endregion
    }
}

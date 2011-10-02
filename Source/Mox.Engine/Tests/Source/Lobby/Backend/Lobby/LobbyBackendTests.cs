using System;
using System.Collections.Generic;

using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Lobby.Backend
{
    [TestFixture]
    public class LobbyBackendTests
    {
        #region Variables

        private MockRepository m_mockery;

        private LogContext m_logContext;
        private LobbyServiceBackend m_lobbyService;
        private LobbyBackend m_lobby;

        private MockClient m_client1;
        private MockClient m_client2;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();

            m_logContext = new LogContext();
            m_lobbyService = new LobbyServiceBackend(m_logContext);
            m_lobby = new LobbyBackend(m_lobbyService);

            m_client1 = CreateClient(new User("John"));
            m_client2 = CreateClient(new User("Jack"));
        }

        #endregion

        #region Utilities

        private MockClient CreateClient(User user)
        {
            return new MockClient(m_mockery, user);
        }

        private static IDisposable Expect_OnUserChanged(MockClient client, UserChange change, params User[] users)
        {
            EventSink<UserChangedEventArgs> sink = new EventSink<UserChangedEventArgs>();
            client.UserChanged += sink;

            List<User> actualUsers = new List<User>();

            sink.Callback += (o, e) =>
            {
                Assert.AreEqual(change, e.Change);
                actualUsers.Add(e.User);
            };

            return new DisposableHelper(() =>
            {
                Assert.Collections.AreEquivalent(users, actualUsers);
                client.UserChanged -= sink;
            });
        }

        private static IDisposable Expect_OnPlayerChanged(MockClient client, PlayerChange change, int numAis, params User[] users)
        {
            EventSink<PlayerChangedEventArgs> sink = new EventSink<PlayerChangedEventArgs>();
            client.PlayerChanged += sink;

            List<User> actualUsers = new List<User>();

            sink.Callback += (o, e) =>
            {
                Assert.AreEqual(change, e.Change);
                actualUsers.Add(e.Player.User);
            };

            return new DisposableHelper(() =>
            {
                int actualNumAis = actualUsers.RemoveAll(u => u.IsAI);
                Assert.AreEqual(numAis, actualNumAis, "Expected {0} AI users but got {1}", numAis, actualNumAis);
                Assert.Collections.AreEquivalent(users, actualUsers);
                client.PlayerChanged -= sink;
            });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Id_is_always_unique()
        {
            Assert.AreNotEqual(new LobbyBackend(m_lobbyService).Id, new LobbyBackend(m_lobbyService).Id);
        }

        [Test]
        public void Test_ChatService_works()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            m_client2.Expect_Chat_Message(m_client1.User, "Hello");

            using (m_mockery.Test())
            {
                m_lobby.ChatService.Say(m_client1.User, "Hello");
            }
        }

        [Test]
        public void Test_GameInfo_returns_the_info_for_the_game()
        {
            Assert.AreEqual(2, m_lobby.GameInfo.NumberOfPlayers);

            // Returns a copy
            m_lobby.GameInfo.NumberOfPlayers = 3;
            Assert.AreEqual(2, m_lobby.GameInfo.NumberOfPlayers);
        }

        #region Users

        [Test]
        public void Test_No_users_by_default()
        {
            Assert.Collections.IsEmpty(m_lobby.Users);
        }

        [Test]
        public void Test_Users_returns_the_list_of_logged_users()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            Assert.Collections.AreEquivalent(new[] { m_client1.User, m_client2.User }, m_lobby.Users);

            m_lobby.Logout(m_client1);

            Assert.Collections.AreEquivalent(new[] { m_client2.User }, m_lobby.Users);
        }

        [Test]
        public void Test_Login_does_nothing_if_already_logged_in()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client1);

            Assert.Collections.AreEquivalent(new[] { m_client1.User }, m_lobby.Users);
        }

        [Test]
        public void Test_Logout_does_nothing_if_user_is_not_logged_in()
        {
            m_lobby.Logout(m_client1);

            Assert.Collections.IsEmpty(m_lobby.Users);
        }

        [Test]
        public void Test_Login_immediatly_sends_the_client_the_list_of_users_except_the_new_user()
        {
            using (Expect_OnUserChanged(m_client1, UserChange.Joined))
            {
                m_lobby.Login(m_client1);
            }

            using (Expect_OnUserChanged(m_client1, UserChange.Joined, m_client2.User))
            using (Expect_OnUserChanged(m_client2, UserChange.Joined, m_client1.User))
            {
                m_lobby.Login(m_client2);
            }

            var client3 = CreateClient(new User("Albert"));

            using (Expect_OnUserChanged(m_client1, UserChange.Joined, client3.User))
            using (Expect_OnUserChanged(m_client2, UserChange.Joined, client3.User))
            using (Expect_OnUserChanged(client3, UserChange.Joined, m_client1.User, m_client2.User))
            {
                m_lobby.Login(client3);
            }
        }

        [Test]
        public void Test_Logout_sends_the_change_to_all_other_users()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            using (Expect_OnUserChanged(m_client1, UserChange.Left, m_client2.User))
            {
                m_lobby.Logout(m_client2);
            }
        }

        #endregion

        #region Players

        [Test]
        public void Test_Players_are_filled_with_AI_automatically()
        {
            var players = m_lobby.Players;

            Assert.AreEqual(2, players.Count);

            Assert.That(players[0].User.IsAI);
            Assert.That(players[1].User.IsAI);
        }

        [Test]
        public void Test_Users_replace_AI_players_when_joining()
        {
            var players = m_lobby.Players;
            var player1ID = players[0].Id;

            m_lobby.Login(m_client1);

            Assert.AreEqual(2, players.Count);

            Assert.AreEqual(m_client1.User, players[0].User);
            Assert.AreEqual(player1ID, players[0].Id);
            Assert.That(players[1].User.IsAI);
        }

        [Test]
        public void Test_AI_players_replace_users_when_they_leave()
        {
            var players = m_lobby.Players;
            var player1ID = players[0].Id;

            m_lobby.Login(m_client1);
            m_lobby.Logout(m_client1);

            Assert.AreEqual(2, players.Count);

            Assert.That(players[0].User.IsAI);
            Assert.AreEqual(player1ID, players[0].Id);
            Assert.That(players[1].User.IsAI);
        }

        [Test]
        public void Test_Login_immediatly_sends_the_client_the_list_of_all_players_including_the_new_player()
        {
            using (Expect_OnPlayerChanged(m_client1, PlayerChange.Joined, 1, m_client1.User))
            {
                m_lobby.Login(m_client1);
            }
        }

        [Test]
        public void Test_Login_immediatly_sends_the_new_player_to_other_clients()
        {
            m_lobby.Login(m_client1);

            using (Expect_OnPlayerChanged(m_client1, PlayerChange.Changed, 0, m_client2.User))
            using (Expect_OnPlayerChanged(m_client2, PlayerChange.Joined, 0, m_client1.User, m_client2.User))
            {
                m_lobby.Login(m_client2);
            }
        }

        [Test]
        public void Test_Logout_immediately_sends_the_other_clients_the_removed_players()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            using (Expect_OnPlayerChanged(m_client1, PlayerChange.Changed, 1))
            {
                m_lobby.Logout(m_client2);
            }
        }

        [Test]
        public void Test_SetPlayerData_changes_the_player_data()
        {
            m_lobby.Login(m_client1);

            var playerData = new PlayerData { Deck = new Database.Deck() };

            Assert.AreEqual(SetPlayerDataResult.Success, m_lobby.SetPlayerData(m_client1, m_lobby.Players[0].Id, playerData));
            Assert.AreEqual(playerData, m_lobby.Players[0].Data);
        }

        [Test]
        public void Test_Cannot_SetPlayerData_on_an_invalid_player()
        {
            m_lobby.Login(m_client1);

            var playerData = new PlayerData { Deck = new Database.Deck() };

            Assert.AreEqual(SetPlayerDataResult.InvalidPlayer, m_lobby.SetPlayerData(m_client1, Guid.NewGuid(), playerData));
        }

        [Test]
        public void Test_Cannot_SetPlayerData_for_other_users_players()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            var playerData = new PlayerData { Deck = new Database.Deck() };

            Assert.AreEqual(SetPlayerDataResult.UnauthorizedAccess, m_lobby.SetPlayerData(m_client2, m_lobby.Players[0].Id, playerData));
        }

        [Test]
        public void Test_SetPlayerData_triggers_PlayerChanged_event()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            var playerData = new PlayerData { Deck = new Database.Deck() };

            m_lobby.Login(m_client1);

            using (Expect_OnPlayerChanged(m_client1, PlayerChange.Changed, 0, m_client1.User))
            using (Expect_OnPlayerChanged(m_client2, PlayerChange.Changed, 0, m_client1.User))
            {
                Assert.AreEqual(SetPlayerDataResult.Success, m_lobby.SetPlayerData(m_client1, m_lobby.Players[0].Id, playerData));
            }
        }

        #endregion

        #endregion
    }
}

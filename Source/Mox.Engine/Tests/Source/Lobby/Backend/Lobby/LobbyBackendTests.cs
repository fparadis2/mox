using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Lobby2.Backend
{
    [TestFixture]
    public class LobbyBackendTests
    {
        #region Variables

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
            m_logContext = new LogContext();
            m_lobbyService = new LobbyServiceBackend(m_logContext);
            m_lobby = new LobbyBackend(m_lobbyService);

            m_client1 = new MockClient("John");
            m_client2 = new MockClient("Jack");
        }

        #endregion

        #region Utilities

        private static IDisposable Expect_OnUserChanged(MockClient client, UserChange change, params User[] users)
        {
            client.Channel.SentMessages.Clear();

            return new DisposableHelper(() =>
            {
                var response = client.Channel.SentMessages.OfType<UserChangedResponse>().Single();

                Assert.AreEqual(change, response.Change);
                Assert.Collections.AreEquivalent(users, response.Users);
            });
        }

        private static IDisposable Expect_OnPlayerChanged(MockClient client, PlayerChange change, int numAis, params User[] users)
        {
            client.Channel.SentMessages.Clear();

            return new DisposableHelper(() =>
            {
                var response = client.Channel.SentMessages.OfType<PlayerChangedResponse>().Single();
                var actualUsers = response.Players.Select(p => p.User).ToList();

                int actualNumAis = actualUsers.RemoveAll(u => u.IsAI);

                Assert.AreEqual(change, response.Change);
                Assert.Collections.AreEquivalent(users, actualUsers);
                Assert.AreEqual(numAis, actualNumAis, "Expected {0} AI users but got {1}", numAis, actualNumAis);
            });
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Id_is_always_unique()
        {
            Assert.AreNotEqual(new LobbyBackend(m_lobbyService).Id, new LobbyBackend(m_lobbyService).Id);
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
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            Assert.Collections.AreEquivalent(new[] { m_client1.User, m_client2.User }, m_lobby.Users);

            m_lobby.Logout(m_client1.Channel);

            Assert.Collections.AreEquivalent(new[] { m_client2.User }, m_lobby.Users);
        }

        [Test]
        public void Test_Login_does_nothing_if_already_logged_in()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client1.Channel, m_client1.User);

            Assert.Collections.AreEquivalent(new[] { m_client1.User }, m_lobby.Users);
        }

        [Test]
        public void Test_Logout_does_nothing_if_user_is_not_logged_in()
        {
            m_lobby.Logout(m_client1.Channel);

            Assert.Collections.IsEmpty(m_lobby.Users);
        }

        [Test]
        public void Test_Login_sends_the_client_the_list_of_users_including_itself()
        {
            using (Expect_OnUserChanged(m_client1, UserChange.Joined, m_client1.User))
            {
                m_lobby.Login(m_client1.Channel, m_client1.User);
            }

            using (Expect_OnUserChanged(m_client1, UserChange.Joined, m_client2.User))
            using (Expect_OnUserChanged(m_client2, UserChange.Joined, m_client1.User, m_client2.User))
            {
                m_lobby.Login(m_client2.Channel, m_client2.User);
            }

            var client3 = new MockClient("Albert");

            using (Expect_OnUserChanged(m_client1, UserChange.Joined, client3.User))
            using (Expect_OnUserChanged(m_client2, UserChange.Joined, client3.User))
            using (Expect_OnUserChanged(client3, UserChange.Joined, m_client1.User, m_client2.User, client3.User))
            {
                m_lobby.Login(client3.Channel, client3.User);
            }
        }

        [Test]
        public void Test_Logout_sends_the_change_to_all_other_users()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            using (Expect_OnUserChanged(m_client1, UserChange.Left, m_client2.User))
            {
                m_lobby.Logout(m_client2.Channel);
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

            m_lobby.Login(m_client1.Channel, m_client1.User);

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

            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Logout(m_client1.Channel);

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
                m_lobby.Login(m_client1.Channel, m_client1.User);
            }
        }

        [Test]
        public void Test_Login_immediatly_sends_the_new_player_to_other_clients()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);

            using (Expect_OnPlayerChanged(m_client1, PlayerChange.Changed, 0, m_client2.User))
            using (Expect_OnPlayerChanged(m_client2, PlayerChange.Joined, 0, m_client1.User, m_client2.User))
            {
                m_lobby.Login(m_client2.Channel, m_client2.User);
            }
        }

        [Test]
        public void Test_Logout_immediately_sends_the_other_clients_the_removed_players()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            using (Expect_OnPlayerChanged(m_client1, PlayerChange.Changed, 1))
            {
                m_lobby.Logout(m_client2.Channel);
            }
        }

        [Test]
        public void Test_SetPlayerData_changes_the_player_data()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);

            var playerData = new PlayerData { Deck = "3 Plains" };

            Assert.AreEqual(SetPlayerDataResult.Success, m_lobby.SetPlayerData(m_client1.Channel, m_lobby.Players[0].Id, playerData));
            Assert.AreEqual(playerData, m_lobby.Players[0].Data);
        }

        [Test]
        public void Test_Cannot_SetPlayerData_on_an_invalid_player()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);

            var playerData = new PlayerData { Deck = "3 Plains" };

            Assert.AreEqual(SetPlayerDataResult.InvalidPlayer, m_lobby.SetPlayerData(m_client1.Channel, Guid.NewGuid(), playerData));
        }

        [Test]
        public void Test_Cannot_SetPlayerData_for_other_users_players()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            var playerData = new PlayerData { Deck = "3 Plains" };

            Assert.AreEqual(SetPlayerDataResult.UnauthorizedAccess, m_lobby.SetPlayerData(m_client2.Channel, m_lobby.Players[0].Id, playerData));
        }

        [Test]
        public void Test_SetPlayerData_triggers_PlayerChanged_event()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            var playerData = new PlayerData { Deck = "3 Plains" };

            using (Expect_OnPlayerChanged(m_client1, PlayerChange.Changed, 0, m_client1.User))
            using (Expect_OnPlayerChanged(m_client2, PlayerChange.Changed, 0, m_client1.User))
            {
                Assert.AreEqual(SetPlayerDataResult.Success, m_lobby.SetPlayerData(m_client1.Channel, m_lobby.Players[0].Id, playerData));
            }
        }

        #endregion

        #endregion
    }
}

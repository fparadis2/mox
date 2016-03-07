using System;
using System.Linq;
using Mox.Database;
using Mox.Lobby.Network.Protocol;
using NUnit.Framework;

namespace Mox.Lobby.Server
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

        private LobbyParameters m_lobbyParameters;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_lobbyParameters = new LobbyParameters
            {
                GameFormat = new DuelFormat(),
                DeckFormat = new StandardDeckFormat()
            };

            m_logContext = new LogContext();
            m_lobbyService = new LobbyServiceBackend(m_logContext);
            m_lobby = new LobbyBackend(m_lobbyService, m_lobbyParameters);

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

        private void Check_PlayerSlotChanged(MockClient client, int index, PlayerSlotNetworkDataChange change)
        {
            var changes = client.Channel.SentMessages.OfType<PlayerSlotChangedMessage>().SelectMany(m => m.Changes);
            var lastChange = changes.Last(c => c.Index == index && c.Type.HasFlag(change));

            var expectedSlot = m_lobby.PlayerSlots[index];
            var actualSlot = lastChange.SlotData;

            if (change.HasFlag(PlayerSlotNetworkDataChange.User))
            {
                Assert.AreEqual(expectedSlot.User.Id, actualSlot.User);
            }

            if (change.HasFlag(PlayerSlotNetworkDataChange.Deck))
            {
                var deckName = expectedSlot.Data.Deck != null ? expectedSlot.Data.Deck.Name : null;

                Assert.AreEqual(deckName, actualSlot.Deck.Name);
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Id_is_always_unique()
        {
            Assert.AreNotEqual(new LobbyBackend(m_lobbyService, m_lobbyParameters).Id, new LobbyBackend(m_lobbyService, m_lobbyParameters).Id);
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

            m_lobby.Logout(m_client1.Channel, "gone");

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
            m_lobby.Logout(m_client1.Channel, "gone");

            Assert.Collections.IsEmpty(m_lobby.Users);
        }

        [Test]
        public void Test_Logout_sends_the_change_to_all_other_users()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            using (Expect_OnUserChanged(m_client1, UserChange.Left, m_client2.User))
            {
                m_lobby.Logout(m_client2.Channel, "gone");
            }
        }

        #endregion

        #region Slots

        [Test]
        public void Test_Slots_are_not_assigned_by_default()
        {
            var slots = m_lobby.PlayerSlots;

            Assert.AreEqual(2, slots.Count);
            Assert.That(!slots[0].IsAssigned);
            Assert.That(!slots[1].IsAssigned);
        }

        #region Login/Logout

        [Test]
        public void Test_Users_fill_slots_when_joining()
        {
            var slots = m_lobby.PlayerSlots;
            Assert.AreEqual(2, slots.Count);

            m_lobby.Login(m_client1.Channel, m_client1.User);

            Assert.AreEqual(2, slots.Count);
            Assert.AreEqual(m_client1.User, slots[0].User);
            Assert.That(!slots[1].IsAssigned);
        }

        [Test]
        public void Test_Users_leave_their_slot_when_leaving()
        {
            var slots = m_lobby.PlayerSlots;

            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Logout(m_client1.Channel, "gone");

            Assert.AreEqual(2, slots.Count);
            Assert.That(!slots[0].IsAssigned);
            Assert.That(!slots[1].IsAssigned);
        }

        [Test]
        public void Test_Login_immediatly_sends_the_new_player_slot_to_other_clients()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            Check_PlayerSlotChanged(m_client1, 1, PlayerSlotNetworkDataChange.User);
        }

        [Test]
        public void Test_Logout_immediately_sends_the_other_clients_the_removed_players()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            m_client1.Channel.SentMessages.Clear();
            m_lobby.Logout(m_client2.Channel, "gone");
            Check_PlayerSlotChanged(m_client1, 1, PlayerSlotNetworkDataChange.All);
        }

        #endregion

        #region SetPlayerSlotData

        [Test]
        public void Test_SetPlayerData_changes_the_player_data()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);

            var slotData = new PlayerSlotData { Deck = new Deck("Deck") };

            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1.Channel, 0, slotData));
            Assert.AreEqual(slotData, m_lobby.PlayerSlots[0].Data);
        }

        [Test]
        public void Test_Cannot_SetPlayerData_on_an_invalid_player()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);

            var slotData = new PlayerSlotData { Deck = new Deck("Deck") };

            Assert.AreEqual(SetPlayerSlotDataResult.InvalidPlayerSlot, m_lobby.SetPlayerSlotData(m_client1.Channel, 24, slotData));
        }

        [Test]
        public void Test_Cannot_SetPlayerData_for_other_users_players()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            var slotData = new PlayerSlotData { Deck = new Deck("Deck") };
            Assert.AreEqual(SetPlayerSlotDataResult.UnauthorizedAccess, m_lobby.SetPlayerSlotData(m_client2.Channel, 0, slotData));
        }

        [Test]
        public void Test_SetPlayerSlotData_sends_PlayerSlotChangedMessage_to_all_clients()
        {
            m_lobby.Login(m_client1.Channel, m_client1.User);
            m_lobby.Login(m_client2.Channel, m_client2.User);

            var slotData = new PlayerSlotData { Deck = new Deck("Deck") };
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1.Channel, 0, slotData));

            Check_PlayerSlotChanged(m_client1, 0, PlayerSlotNetworkDataChange.Deck);
            Check_PlayerSlotChanged(m_client2, 0, PlayerSlotNetworkDataChange.Deck);
        }

        #endregion

        #endregion

        #endregion
    }
}

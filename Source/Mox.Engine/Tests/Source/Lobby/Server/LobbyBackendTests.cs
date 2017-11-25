using System;
using System.Collections.Generic;
using System.Linq;
using Mox.Lobby.Network.Protocol;
using NUnit.Framework;

namespace Mox.Lobby.Server
{
    [TestFixture]
    public class LobbyBackendTests
    {
        #region Variables

        private readonly PlayerSlotDeck m_deck = new PlayerSlotDeck { Name = "Deck" };

        private LogContext m_logContext;
        private LobbyServiceBackend m_lobbyService;
        private LobbyBackend m_lobby;

        private User m_client1;
        private User m_client2;

        private Dictionary<User, UserIdentity> m_identities;

        private LobbyParameters m_lobbyParameters;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_lobbyParameters = new LobbyParameters
            {
                GameFormat = new DuelFormat(),
                DeckFormat = new AnyDeckFormat(),
                AssignNewPlayersToFreeSlots = true,
            };

            m_logContext = new LogContext();
            m_lobbyService = new LobbyServiceBackend(m_logContext);
            
            m_client1 = new User(new MockChannel(), "John");
            m_client2 = new User(new MockChannel(), "Jack");

            m_identities = new Dictionary<User, UserIdentity>();

            CreateLobby();
        }

        #endregion

        #region Utilities

        private void CreateLobby()
        {
            m_lobby = new LobbyBackend(m_lobbyService, m_lobbyParameters);
        }

        private UserIdentity GetIdentity(User user)
        {
            UserIdentity identity;
            if (!m_identities.TryGetValue(user, out identity))
            {
                identity = new UserIdentity { Name = user.Name };
                m_identities.Add(user, identity);
            }
            return identity;
        }

        private bool Login(User user)
        {
            return m_lobby.Login(user, GetIdentity(user));
        }

        private TMessage GetLastMessage<TMessage>(User user)
        {
            MockChannel channel = (MockChannel)user.Channel;
            return channel.SentMessages.OfType<TMessage>().Last();
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
            Assert.IsTrue(Login(m_client1));
            Assert.IsTrue(Login(m_client2));

            Assert.Collections.AreEquivalent(new[] { m_client1, m_client2 }, m_lobby.Users);

            m_lobby.Logout(m_client1, "gone");

            Assert.Collections.AreEquivalent(new[] { m_client2 }, m_lobby.Users);
        }

        [Test]
        public void Test_Login_sends_the_new_player_to_existing_users()
        {
            Assert.IsTrue(Login(m_client1));
            Assert.IsTrue(Login(m_client2));

            var msg = GetLastMessage<UserJoinedMessage>(m_client1);
            Assert.AreEqual(m_client2.Id, msg.UserId);
            Assert.AreEqual(m_client2.Name, msg.Data.Name);
        }

        [Test]
        public void Test_Login_does_nothing_if_already_logged_in()
        {
            Assert.IsTrue(Login(m_client1));
            Assert.IsFalse(Login(m_client1));

            Assert.Collections.AreEquivalent(new[] { m_client1 }, m_lobby.Users);
        }

        [Test]
        public void Test_Logout_does_nothing_if_user_is_not_logged_in()
        {
            m_lobby.Logout(m_client1, "gone");

            Assert.Collections.IsEmpty(m_lobby.Users);
        }

        [Test]
        public void Test_Logout_returns_true_when_the_last_player_leaves()
        {
            Login(m_client1);
            Login(m_client2);

            Assert.IsFalse(m_lobby.Logout(m_client1, "gone"));
            Assert.IsTrue(m_lobby.Logout(m_client2, "gone"));
        }

        [Test]
        public void Test_Logout_sends_the_change_to_all_other_users()
        {
            Login(m_client1);
            Login(m_client2);

            m_lobby.Logout(m_client2, "gone");

            var msg = GetLastMessage<UserLeftMessage>(m_client1);
            Assert.AreEqual(m_client2.Id, msg.UserId);
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
        public void Test_Players_dont_fill_slots_when_joining_by_default()
        {
            var slots = m_lobby.PlayerSlots;
            Assert.AreEqual(2, slots.Count);

            Login(m_client1);

            Assert.AreEqual(2, slots.Count);
            Assert.That(!slots[0].IsAssigned);
            Assert.That(!slots[1].IsAssigned);
        }

        [Test]
        public void Test_Players_can_join_slots()
        {
            Login(m_client1);

            var slots = m_lobby.PlayerSlots;
            Assert.AreEqual(2, slots.Count);
            Assert.AreEqual(m_client1.Id, slots[0].PlayerId);
            Assert.AreNotEqual(m_client1.Id, slots[1].PlayerId);

            m_lobby.SetPlayerSlotData(m_client1, 1, new PlayerSlotData { PlayerId = m_client1.Id });

            slots = m_lobby.PlayerSlots;
            Assert.AreEqual(2, slots.Count);
            Assert.AreNotEqual(m_client1.Id, slots[0].PlayerId);
            Assert.AreEqual(m_client1.Id, slots[1].PlayerId);
        }

        [Test]
        public void Test_Players_leave_their_slot_when_leaving()
        {
            var slots = m_lobby.PlayerSlots;

            Login(m_client1);
            m_lobby.SetPlayerSlotData(m_client1, 0, new PlayerSlotData { PlayerId = m_client1.Id });

            m_lobby.Logout(m_client1, "gone");

            Assert.AreEqual(2, slots.Count);
            Assert.That(!slots[0].IsAssigned);
            Assert.That(!slots[1].IsAssigned);
        }

        #endregion

        #region SetPlayerSlotData

        [Test]
        public void Test_SetPlayerData_changes_the_player_data()
        {
            Login(m_client1);

            var slotData = new PlayerSlotData { Deck = m_deck };

            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 0, slotData));
            Assert.AreEqual(m_deck, m_lobby.PlayerSlotDatas[0].Deck);
        }

        [Test]
        public void Test_Cannot_SetPlayerData_on_an_invalid_slot_index()
        {
            Login(m_client1);

            var slotData = new PlayerSlotData { Deck = m_deck };
            Assert.AreEqual(SetPlayerSlotDataResult.InvalidPlayerSlot, m_lobby.SetPlayerSlotData(m_client1, 24, slotData));
        }

        [Test]
        public void Test_Cannot_SetPlayerData_for_slots_assigned_to_other_players()
        {
            Login(m_client1);
            Login(m_client2);

            var slotData = new PlayerSlotData { Deck = m_deck };
            Assert.AreEqual(SetPlayerSlotDataResult.UnauthorizedAccess, m_lobby.SetPlayerSlotData(m_client2, 0, slotData));
        }

        [Test]
        public void Test_SetPlayerSlotData_sends_PlayerSlotsChangedMessage_to_all_clients()
        {
            Login(m_client1);
            Login(m_client2);

            var slotData = new PlayerSlotData { Deck = m_deck };
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 0, slotData));

            var msg1 = GetLastMessage<PlayerSlotsChangedMessage>(m_client1);
            Assert.AreEqual(1, msg1.Changes.Count);
            Assert.AreEqual(0, msg1.Changes[0].Index);
            Assert.AreEqual(m_deck, msg1.Changes[0].Data.Deck);

            var msg2 = GetLastMessage<PlayerSlotsChangedMessage>(m_client2);
            Assert.AreEqual(1, msg2.Changes.Count);
            Assert.AreEqual(0, msg2.Changes[0].Index);
            Assert.AreEqual(m_deck, msg2.Changes[0].Data.Deck);
        }

        [Test]
        public void Test_SetPlayerData_can_be_used_to_join_another_slot()
        {
            Login(m_client1);

            var slotData = new PlayerSlotData { PlayerId = m_client1.Id };

            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 1, slotData));

            Assert.That(!m_lobby.PlayerSlots[0].IsAssigned);
            Assert.AreEqual(m_client1.Id, m_lobby.PlayerSlots[1].PlayerId);
        }

        #endregion

        #region Slot Validity

        [Test]
        public void Test_Slots_are_invalid_by_default()
        {
            var slots = m_lobby.PlayerSlots;

            Assert.AreEqual(2, slots.Count);
            Assert.That(!slots[0].IsValid);
            Assert.That(!slots[1].IsValid);
        }

        [Test]
        public void Test_Slots_are_valid_when_their_deck_is_valid()
        {
            Login(m_client1);

            var slot0 = m_lobby.PlayerSlotDatas[0];
            slot0.Deck.Name = "My Deck";
            slot0.Deck.Contents = "1 Anything";
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 0, slot0));
            Assert.That(m_lobby.PlayerSlotDatas[0].IsValid);

            // Invalid name
            slot0.Deck.Name = null;
            slot0.Deck.Contents = "1 Anything";
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 0, slot0));
            Assert.That(!m_lobby.PlayerSlotDatas[0].IsValid);

            // Invalid contents
            slot0.Deck.Name = "My Deck";
            slot0.Deck.Contents = null;
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 0, slot0));
            Assert.That(!m_lobby.PlayerSlotDatas[0].IsValid);
        }

        #endregion

        #region Slot Readiness

        [Test]
        public void Test_Can_only_set_IsReady_on_a_valid_slot()
        {
            Login(m_client1);

            var slot0 = m_lobby.PlayerSlotDatas[0];
            slot0.IsReady = true;
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 0, slot0));
            Assert.That(!m_lobby.PlayerSlotDatas[0].IsReady);

            slot0.Deck.Name = "My Deck";
            slot0.Deck.Contents = "1 Anything";
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 0, slot0));
            Assert.That(m_lobby.PlayerSlotDatas[0].IsReady);
        }

        [Test]
        public void Test_Unassigned_slots_are_never_ready()
        {
            Login(m_client1);

            Assert.That(!m_lobby.PlayerSlotDatas[1].IsReady);

            var slot1 = m_lobby.PlayerSlotDatas[1];
            slot1.Deck.Name = "My Deck";
            slot1.Deck.Contents = "1 Anything";
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 1, slot1));
            Assert.That(!m_lobby.PlayerSlotDatas[1].IsReady);
        }

        [Test]
        public void Test_Can_set_IsReady_on_an_assigned_slot()
        {
            var slots = m_lobby.PlayerSlotDatas;
            Assert.AreEqual(2, slots.Count);

            Login(m_client1);

            var slot1 = slots[1];
            slot1.IsReady = true;
            slot1.Deck.Name = "My Deck";
            slot1.Deck.Contents = "1 Anything";
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 1, slot1));
            Assert.That(!slots[0].IsReady);
        }
    
        #endregion

        #endregion

        #region Leader

        [Test]
        public void Test_Leader_is_invalid_while_there_is_no_players()
        {
            Assert.AreEqual(User.Invalid, m_lobby.Leader);

            Login(m_client1);
            m_lobby.Logout(m_client1, "gone");

            Assert.AreEqual(User.Invalid, m_lobby.Leader);
        }

        [Test]
        public void Test_First_user_to_login_becomes_the_leader()
        {
            Login(m_client1);
            Assert.AreEqual(m_client1, m_lobby.Leader);
        }

        [Test]
        public void Test_A_new_leader_is_chosen_when_the_leader_leaves()
        {
            Login(m_client1);
            Login(m_client2);

            Assert.AreEqual(m_client1, m_lobby.Leader);
            
            m_lobby.Logout(m_client1, "gone");
            Assert.AreEqual(m_client2, m_lobby.Leader);

            m_lobby.Logout(m_client2, "gone");
            Assert.AreEqual(User.Invalid, m_lobby.Leader);
        }

        [Test]
        public void Test_When_a_new_leader_is_chosen_a_LeaderChangedMessage_is_sent_to_remaining_players()
        {
            Login(m_client1);
            Login(m_client2);

            m_lobby.Logout(m_client1, "gone");

            var msg = GetLastMessage<LeaderChangedMessage>(m_client2);
            Assert.AreEqual(m_client2.Id, msg.LeaderId);
        }

        #endregion

        #region AutoFillWithBots

        [Test]
        public void Test_When_AutoFillWithBots_is_true_the_lobby_already_has_bot_users()
        {
            m_lobbyParameters.AutoFillWithBots = true;
            CreateLobby();

            var users = m_lobby.UserDatas;
            Assert.AreEqual(2, users.Count);
            Assert.That(users[0].Value.IsBot);
            Assert.That(users[1].Value.IsBot);
        }

        [Test]
        public void Test_When_AutoFillWithBots_is_true_the_slots_are_already_assigned_to_bots()
        {
            m_lobbyParameters.AutoFillWithBots = true;
            CreateLobby();

            var slots = m_lobby.PlayerSlots;
            Assert.AreEqual(2, slots.Count);
            Assert.That(slots[0].IsAssigned);
            Assert.That(slots[1].IsAssigned);
        }

        [Test]
        public void Test_Joining_a_lobby_takes_a_slot_from_a_bot()
        {
            m_lobbyParameters.AutoFillWithBots = true;
            CreateLobby();

            Login(m_client1);

            var slots = m_lobby.PlayerSlots;
            Assert.AreEqual(2, slots.Count);
            Assert.AreEqual(m_client1.Id, slots[0].PlayerId);
        }

        [Test]
        public void Test_Leaving_a_lobby_fills_the_slot_with_a_bot()
        {
            m_lobbyParameters.AutoFillWithBots = true;
            CreateLobby();

            Login(m_client1);
            m_lobby.Logout(m_client1, "gone");

            var slots = m_lobby.PlayerSlots;

            Assert.AreEqual(2, slots.Count);
            Assert.That(slots[0].IsAssigned);
            Assert.That(slots[0].PlayerId != m_client1.Id);
        }

        [Test]
        public void Test_Login_immediatly_sends_the_new_player_slot_to_other_clients()
        {
            m_lobbyParameters.AutoFillWithBots = true;
            CreateLobby();

            Login(m_client1);
            Login(m_client2);

            var msg = GetLastMessage<PlayerSlotsChangedMessage>(m_client1);
            Assert.AreEqual(1, msg.Changes.Count);
            Assert.AreEqual(1, msg.Changes[0].Index);
            Assert.AreEqual(m_client2.Id, msg.Changes[0].Data.PlayerId);
        }

        [Test]
        public void Test_Logout_immediately_sends_the_other_clients_the_removed_players()
        {
            m_lobbyParameters.AutoFillWithBots = true;
            CreateLobby();

            Login(m_client1);
            Login(m_client2);
            m_lobby.Logout(m_client2, "gone");

            var msg = GetLastMessage<PlayerSlotsChangedMessage>(m_client1);
            Assert.AreEqual(1, msg.Changes.Count);
            Assert.AreEqual(1, msg.Changes[0].Index);
            Assert.AreNotEqual(m_client2.Id, msg.Changes[0].Data.PlayerId);
        }

        #endregion

        #endregion
    }
}

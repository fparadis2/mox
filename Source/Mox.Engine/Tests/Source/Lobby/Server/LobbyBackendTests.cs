﻿using System;
using System.Linq;
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

        private User m_client1;
        private User m_client2;

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

            m_client1 = new User(new MockChannel(), "John");
            m_client2 = new User(new MockChannel(), "Jack");
        }

        #endregion

        #region Utilities

        private PlayersChangedMessage GetLastPlayersChangedMessage(User user, Func<PlayersChangedMessage, bool> predicate = null)
        {
            predicate = predicate ?? (m => true);

            MockChannel channel = (MockChannel)user.Channel;
            return channel.SentMessages.OfType<PlayersChangedMessage>().Where(predicate).Last();
        }

        private PlayerSlotsChangedMessage GetLastPlayerSlotsChangedMessage(User user, Func<PlayerSlotsChangedMessage, bool> predicate = null)
        {
            predicate = predicate ?? (m => true);

            MockChannel channel = (MockChannel)user.Channel;
            return channel.SentMessages.OfType<PlayerSlotsChangedMessage>().Where(predicate).Last();
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
            Assert.IsTrue(m_lobby.Login(m_client1));
            Assert.IsTrue(m_lobby.Login(m_client2));

            Assert.Collections.AreEquivalent(new[] { m_client1, m_client2 }, m_lobby.Users);

            m_lobby.Logout(m_client1, "gone");

            Assert.Collections.AreEquivalent(new[] { m_client2 }, m_lobby.Users);
        }

        [Test]
        public void Test_Login_sends_the_new_player_to_existing_users()
        {
            Assert.IsTrue(m_lobby.Login(m_client1));
            Assert.IsTrue(m_lobby.Login(m_client2));

            var msg = GetLastPlayersChangedMessage(m_client1);
            Assert.AreEqual(PlayersChangedMessage.ChangeType.Joined, msg.Change);
            Assert.AreEqual(1, msg.Players.Count);
            Assert.AreEqual(m_client2.Id, msg.Players[0].Id);
        }

        [Test]
        public void Test_Login_does_nothing_if_already_logged_in()
        {
            Assert.IsTrue(m_lobby.Login(m_client1));
            Assert.IsFalse(m_lobby.Login(m_client1));

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
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            Assert.IsFalse(m_lobby.Logout(m_client1, "gone"));
            Assert.IsTrue(m_lobby.Logout(m_client2, "gone"));
        }

        [Test]
        public void Test_Logout_sends_the_change_to_all_other_users()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            m_lobby.Logout(m_client2, "gone");

            var msg = GetLastPlayersChangedMessage(m_client1);
            Assert.AreEqual(PlayersChangedMessage.ChangeType.Left, msg.Change);
            Assert.AreEqual(1, msg.Players.Count);
            Assert.AreEqual(m_client2.Id, msg.Players[0].Id);
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
        public void Test_Players_fill_slots_when_joining()
        {
            var slots = m_lobby.PlayerSlots;
            Assert.AreEqual(2, slots.Count);

            m_lobby.Login(m_client1);

            Assert.AreEqual(2, slots.Count);
            Assert.AreEqual(m_client1.Id, slots[0].PlayerId);
            Assert.That(!slots[1].IsAssigned);
        }

        [Test]
        public void Test_Players_leave_their_slot_when_leaving()
        {
            var slots = m_lobby.PlayerSlots;

            m_lobby.Login(m_client1);
            m_lobby.Logout(m_client1, "gone");

            Assert.AreEqual(2, slots.Count);
            Assert.That(!slots[0].IsAssigned);
            Assert.That(!slots[1].IsAssigned);
        }

        [Test]
        public void Test_Login_immediatly_sends_the_new_player_slot_to_other_clients()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            var msg = GetLastPlayerSlotsChangedMessage(m_client1);
            Assert.AreEqual(1, msg.Changes.Count);
            Assert.AreEqual(1, msg.Changes[0].Index);
            Assert.AreEqual(m_client2.Id, msg.Changes[0].Data.PlayerId);
        }

        [Test]
        public void Test_Logout_immediately_sends_the_other_clients_the_removed_players()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);
            m_lobby.Logout(m_client2, "gone");

            var msg = GetLastPlayerSlotsChangedMessage(m_client1);
            Assert.AreEqual(1, msg.Changes.Count);
            Assert.AreEqual(1, msg.Changes[0].Index);
            Assert.IsFalse(msg.Changes[0].Data.IsAssigned);
        }

        #endregion

        #region SetPlayerSlotData

        [Test]
        public void Test_SetPlayerData_changes_the_player_data()
        {
            m_lobby.Login(m_client1);

            var slotData = new PlayerSlotData { DeckName = "Deck" };

            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 0, slotData));
            Assert.AreEqual(slotData, m_lobby.PlayerSlots[0]);
        }

        [Test]
        public void Test_Cannot_SetPlayerData_on_an_invalid_slot_index()
        {
            m_lobby.Login(m_client1);

            var slotData = new PlayerSlotData { DeckName = "Deck" };
            Assert.AreEqual(SetPlayerSlotDataResult.InvalidPlayerSlot, m_lobby.SetPlayerSlotData(m_client1, 24, slotData));
        }

        [Test]
        public void Test_Cannot_SetPlayerData_for_slots_assigned_to_other_players()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            var slotData = new PlayerSlotData { DeckName = "Deck" };
            Assert.AreEqual(SetPlayerSlotDataResult.UnauthorizedAccess, m_lobby.SetPlayerSlotData(m_client2, 0, slotData));
        }

        [Test]
        public void Test_SetPlayerSlotData_sends_PlayerSlotsChangedMessage_to_all_clients()
        {
            m_lobby.Login(m_client1);
            m_lobby.Login(m_client2);

            var slotData = new PlayerSlotData { DeckName = "Deck" };
            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 0, slotData));

            var msg1 = GetLastPlayerSlotsChangedMessage(m_client1);
            Assert.AreEqual(1, msg1.Changes.Count);
            Assert.AreEqual(0, msg1.Changes[0].Index);
            Assert.AreEqual(slotData, msg1.Changes[0].Data);

            var msg2 = GetLastPlayerSlotsChangedMessage(m_client2);
            Assert.AreEqual(1, msg2.Changes.Count);
            Assert.AreEqual(0, msg2.Changes[0].Index);
            Assert.AreEqual(slotData, msg2.Changes[0].Data);
        }

        [Test]
        public void Test_SetPlayerData_can_be_used_to_join_another_slot()
        {
            m_lobby.Login(m_client1);

            var slotData = new PlayerSlotData { PlayerId = m_client1.Id };

            Assert.AreEqual(SetPlayerSlotDataResult.Success, m_lobby.SetPlayerSlotData(m_client1, 1, slotData));

            Assert.That(!m_lobby.PlayerSlots[0].IsAssigned);
            Assert.AreEqual(slotData, m_lobby.PlayerSlots[1]);
        }

        #endregion

        #endregion

        #endregion
    }
}

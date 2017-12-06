using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Lobby.Server
{
    public partial class LobbyBackend
    {
        private class PlayerSlotCollection
        {
            private readonly LobbyBackend m_owner;
            private readonly PlayerSlot[] m_slots;

            public PlayerSlotCollection(LobbyBackend owner)
            {
                m_owner = owner;

                int numPlayers = m_owner.m_lobbyParameters.GameFormat.NumPlayers;
                m_slots = new PlayerSlot[numPlayers];

                if (owner.m_lobbyParameters.AutoFillWithBots)
                {
                    for (int i = 0; i < m_slots.Length; i++)
                    {
                        var slotData = m_slots[i];
                        slotData.Player = CreateBot();
                        Set(i, ref slotData);
                    }
                }
            }

            public PlayerSlot this[int i]
            {
                get { return m_slots[i]; }
            }

            public PlayerSlot[] Slots
            {
                get { return (PlayerSlot[])m_slots.Clone(); }
            }

            public PlayerSlotData[] ClientSlots
            {
                get
                {
                    PlayerSlotData[] result = new PlayerSlotData[m_slots.Length];

                    for (int i = 0; i < m_slots.Length; i++)
                    {
                        result[i] = m_slots[i].ToClientData();
                    }

                    return result;
                }
            }

            public void Set(int index, ref PlayerSlot slot)
            {
                slot.Update(m_owner);
                m_slots[index] = slot;
                m_owner.SendPlayerSlotChangedMessages(index, slot.ToClientData());
            }

            public void AssignToFreeSlot(LobbyUser user)
            {
                int freeSlot = FindFreeSlot();
                if (freeSlot < 0)
                    return; // No free slot

                var slot = m_slots[freeSlot];
                slot.Player = user;
                slot.IsReady = false;
                Set(freeSlot, ref slot);
            }

            public void Unassign(LobbyUser user)
            {
                for (int i = 0; i < m_slots.Length; i++)
                {
                    if (m_slots[i].Player == user)
                    {
                        var slot = m_slots[i];

                        if (m_owner.m_lobbyParameters.AutoFillWithBots)
                        {
                            slot.Player = GetOrCreateFreeBot();
                        }
                        else
                        {
                            slot.Player = null;
                        }

                        Set(i, ref slot);
                    }
                }
            }

            private int FindFreeSlot()
            {
                for (int i = 0; i < m_slots.Length; i++)
                {
                    var slot = m_slots[i];
                    if (slot.IsFree)
                        return i;
                }

                return -1;
            }

            private LobbyUser GetOrCreateFreeBot()
            {
                foreach (var bot in m_owner.m_bots)
                {
                    if (!IsAssigned(bot))
                        return bot;
                }

                return CreateBot();
            }

            private LobbyUser CreateBot()
            {
                return m_owner.m_bots.CreateBot(m_owner);
            }

            private bool IsAssigned(LobbyUser user)
            {
                for (int i = 0; i < m_slots.Length; i++)
                {
                    if (m_slots[i].Player == user)
                        return true;
                }

                return false;
            }

            public bool CanStartGame()
            {
                foreach (var slot in m_slots)
                {
                    if (!slot.IsReady)
                        return false;
                }

                return true;
            }
        }

        internal struct PlayerSlot
        {
            private bool m_IsReady;
            private bool m_IsValid;

            public LobbyUser Player;
            public PlayerSlotDeck Deck;

            public bool IsFree
            {
                get { return Player == null || Player.IsBot; }
            }

            public bool IsAssigned
            {
                get { return Player != null; }
            }

            public bool IsReady
            {
                get { return m_IsReady; }
                set { m_IsReady = value; }
            }

            public bool IsValid
            {
                get { return m_IsValid; }
            }

            public Guid PlayerId
            {
                get { return Player != null ? Player.User.Id : Guid.Empty; }
            }

            public PlayerSlotData ToClientData()
            {
                return new PlayerSlotData
                {
                    PlayerId = PlayerId,
                    IsReady = m_IsReady,
                    IsValid = m_IsValid,
                    Deck = Deck
                };
            }

            public void FromClientData(PlayerSlotData data)
            {
                // Player is copied separately

                Deck = data.Deck;
                IsReady = data.IsReady;
            }

            public void Update(LobbyBackend lobby)
            {
                m_IsValid = Validate(lobby);

                if (Player == null)
                    m_IsReady = false; // A slot must be assigned to be ready
                else if (Player.IsBot)
                    m_IsReady = true; // Bots are always ready
                else if (Player.User == lobby.Leader)
                    m_IsReady = true; // Leader is always ready

                m_IsReady &= m_IsValid; // A slot can only be ready when valid
            }

            private bool Validate(LobbyBackend lobby)
            {
                if (string.IsNullOrEmpty(Deck.Name) ||
                    string.IsNullOrEmpty(Deck.Contents))
                    return false;

                var deck = Deck.CreateDeck();
                if (!lobby.m_lobbyParameters.DeckFormat.Validate(deck))
                    return false;

                return true;
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Mox.Lobby.Server
{
    public partial class LobbyBackend
    {
        public class PlayerSlotCollection : List<PlayerSlotData>
        {
            private readonly LobbyBackend m_owner;

            public PlayerSlotCollection(LobbyBackend owner)
            {
                m_owner = owner;

                int numPlayers = m_owner.m_lobbyParameters.GameFormat.NumPlayers;

                for (int i = 0; i < numPlayers; i++)
                {
                    Add(new PlayerSlotData());
                }
            }

            public void Set(int index, ref PlayerSlotData slot)
            {
                slot.IsValid = Validate(ref slot);
                slot.IsReady &= slot.IsValid && slot.IsAssigned; // A slot can only be ready when valid and assigned

                this[index] = slot;
                m_owner.SendPlayerSlotChangedMessages(index, slot);
            }

            public void AssignPlayerToFreeSlot(User user)
            {
                for (int i = 0; i < Count; i++)
                {
                    var slot = this[i];

                    if (!slot.IsAssigned)
                    {
                        slot.PlayerId = user.Id;
                        slot.State &= ~PlayerSlotState.IsReady;
                        Set(i, ref slot);
                        return;
                    }
                }
            }

            public void UnassignPlayerFromSlot(User user)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].PlayerId == user.Id)
                    {
                        var slot = new PlayerSlotData();
                        Set(i, ref slot);
                    }
                }
            }

            private bool Validate(ref PlayerSlotData slot)
            {
                if (string.IsNullOrEmpty(slot.DeckName) ||
                    string.IsNullOrEmpty(slot.DeckContents))
                    return false;

                var deck = slot.CreateDeck();
                if (!m_owner.m_lobbyParameters.DeckFormat.Validate(deck))
                    return false;

                return true;
            }
        }
    }
}

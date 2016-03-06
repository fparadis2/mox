using System;
using System.Collections.Generic;
using Mox.Database;

namespace Mox.Lobby
{
    [Flags]
    public enum PlayerSlotNetworkDataChange
    {
        None = 0,
        User = 1,
        Deck = 2,
        Data = Deck,
        All = User | Data
    }

    [Serializable]
    public struct DeckNetworkData
    {
        public string Name;
        public string Contents;

        public DeckNetworkData(IDeck deck)
        {
            if (deck != null)
            {
                Name = deck.Name;
                Contents = deck.Contents;
            }
            else
            {
                Name = null;
                Contents = null;
            }
        }

        public bool TryGetDeck(out IDeck deck)
        {
            if (string.IsNullOrEmpty(Name))
            {
                deck = null;
                return true;
            }

            deck = Deck.Read(Name, Contents);
            return string.IsNullOrEmpty(deck.Error);
        }
    }

    [Serializable]
    public struct PlayerSlotNetworkData
    {
        public Guid User;
        public DeckNetworkData Deck;

        public PlayerSlotNetworkData(PlayerSlotData data)
        {
            User = Guid.Empty;
            Deck = new DeckNetworkData(data.Deck);
        }

        public bool TryGetPlayerSlotData(out PlayerSlotData data)
        {
            if (!Deck.TryGetDeck(out data.Deck))
            {
                return false;
            }

            return true;
        }
    }

    [Serializable]
    public class PlayerSlotChangedMessage : Message
    {
        #region Inner Types

        public struct Change
        {
            public int Index;
            public PlayerSlotNetworkDataChange Type;
            public PlayerSlotNetworkData SlotData;
        }

        #endregion

        #region Variables

        public readonly List<Change> Changes = new List<Change>();

        #endregion

        #region Constructor

        public PlayerSlotChangedMessage(int index, PlayerSlotNetworkDataChange change, PlayerSlot slot)
        {
            AddChange(index, change, slot);
        }

        public PlayerSlotChangedMessage(IReadOnlyList<PlayerSlot> slots)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                AddChange(i, PlayerSlotNetworkDataChange.All, slots[i]);
            }
        }

        public void AddChange(int index, PlayerSlotNetworkDataChange change, PlayerSlot slot)
        {
            Changes.Add(new Change
            {
                Index = index,
                Type = change,
                SlotData = ExtractDataFromSlot(change, slot)
            });
        }

        private static PlayerSlotNetworkData ExtractDataFromSlot(PlayerSlotNetworkDataChange change, PlayerSlot slot)
        {
            PlayerSlotNetworkData data = new PlayerSlotNetworkData();

            if (change.HasFlag(PlayerSlotNetworkDataChange.User))
            {
                data.User = slot.User.Id;
            }

            if (change.HasFlag(PlayerSlotNetworkDataChange.Deck))
            {
                data.Deck = new DeckNetworkData(slot.Data.Deck);
            }

            return data;
        }

        #endregion
    }
}

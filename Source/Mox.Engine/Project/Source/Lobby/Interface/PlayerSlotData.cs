using System;
using Mox.Database;

namespace Mox.Lobby
{
    [Flags]
    public enum PlayerSlotState
    {
        None = 0,
        /// <summary>
        /// The slot is valid (assigned, valid deck)
        /// </summary>
        IsValid = 1 << 1,
        /// <summary>
        /// The player in the slot is ready to start
        /// </summary>
        IsReady = 1 << 2
    }

    public struct PlayerSlotData
    {
        public Guid PlayerId;
        public PlayerSlotState State;

        public string DeckName;
        public string DeckContents;

        public bool IsAssigned { get { return PlayerId != Guid.Empty; } }

        public bool IsValid
        {
            get { return (State & PlayerSlotState.IsValid) == PlayerSlotState.IsValid; }
            set
            {
                if (value)
                    State |= PlayerSlotState.IsValid;
                else
                    State &= ~PlayerSlotState.IsValid;
            }
        }

        public bool IsReady
        {
            get { return (State & PlayerSlotState.IsReady) == PlayerSlotState.IsReady; }
            set
            {
                if (value)
                    State |= PlayerSlotState.IsReady;
                else
                    State &= ~PlayerSlotState.IsReady;
            }
        }

        #region Utilities

        public IDeck CreateDeck()
        {
            if (string.IsNullOrEmpty(DeckContents))
                return null;

            return Deck.Read(DeckName, DeckContents);
        }

        public void FromDeck(IDeck deck)
        {
            if (deck == null)
            {
                DeckName = "Invalid Deck";
                DeckContents = null;
            }
            else
            {
                DeckName = deck.Name;
                DeckContents = deck.Contents;
            }
        }

        #endregion
    }

    public enum SetPlayerSlotDataResult
    {
        Success,
        InvalidPlayerSlot,
        InvalidData,
        UnauthorizedAccess
    }
}

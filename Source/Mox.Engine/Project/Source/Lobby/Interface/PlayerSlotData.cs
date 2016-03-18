using System;
using Mox.Database;

namespace Mox.Lobby
{
    public struct PlayerSlotData
    {
        public Guid PlayerId;
        public string DeckName;
        public string DeckContents;

        public bool IsAssigned { get { return PlayerId != Guid.Empty; } }

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

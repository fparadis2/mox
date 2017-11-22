using System;
using Mox.Database;

namespace Mox.Lobby
{
    public struct PlayerSlotDeck : IEquatable<PlayerSlotDeck>
    {
        public string Name;
        public string Contents;

        #region Utilities

        public IDeck CreateDeck()
        {
            if (string.IsNullOrEmpty(Contents))
                return null;

            return Deck.Read(Name, Contents);
        }

        public void FromDeck(IDeck deck)
        {
            if (deck == null)
            {
                Name = "Invalid Deck";
                Contents = null;
            }
            else
            {
                Name = deck.Name;
                Contents = deck.Contents;
            }
        }

        #endregion

        #region Equality

        public override bool Equals(object obj)
        {
            return Equals((PlayerSlotDeck)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Contents.GetHashCode();
        }

        public bool Equals(PlayerSlotDeck other)
        {
            return Name == other.Name && Contents == other.Contents;
        }

        public static bool operator==(PlayerSlotDeck a, PlayerSlotDeck b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(PlayerSlotDeck a, PlayerSlotDeck b)
        {
            return !a.Equals(b);
        }

        #endregion
    }

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

    [Flags]
    public enum PlayerSlotDataMask
    {
        None = 0,
        PlayerId = 1 << 1,
        Deck = 1 << 2,
        Ready = 1 << 24
    }

    public struct PlayerSlotData
    {
        public Guid PlayerId;
        public PlayerSlotState State;

        public PlayerSlotDeck Deck;

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
    }

    public enum SetPlayerSlotDataResult
    {
        Success,
        GameAlreadyStarted,
        InvalidPlayerSlot,
        InvalidData,
        UnauthorizedAccess
    }
}

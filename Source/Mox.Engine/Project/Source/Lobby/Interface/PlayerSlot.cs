using System;
using Mox.Database;

namespace Mox.Lobby
{
    public struct PlayerSlotData
    {
        public IDeck Deck;
    }

    public struct PlayerSlot
    {
        #region Variables

        public User User;
        public PlayerSlotData Data;

        #endregion

        #region Properties

        public bool IsAssigned
        {
            get { return User.Id != Guid.Empty; }
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

    public enum AssignPlayerSlotResult
    {
        Success,
        InvalidPlayerSlot,
        UnauthorizedAccess
    }
}

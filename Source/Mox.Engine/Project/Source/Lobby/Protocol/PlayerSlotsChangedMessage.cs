using System;
using System.Collections.Generic;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class PlayerSlotsChangedMessage : Message
    {
        #region Inner Types

        public struct Change
        {
            public int Index;
            public PlayerSlotData Data;
        }

        #endregion

        #region Constructor

        public PlayerSlotsChangedMessage(IReadOnlyList<PlayerSlotData> slots)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                Changes.Add(new Change
                {
                    Index = i,
                    Data = slots[i]
                });
            }
        }

        public PlayerSlotsChangedMessage(int index, PlayerSlotData slot)
        {
            Changes.Add(new Change
            {
                Index = index,
                Data = slot
            });
        }

        #endregion

        #region Properties

        public readonly List<Change> Changes = new List<Change>();

        #endregion
    }
}

using System;
using System.Collections.Generic;
using Mox.Lobby.Network.Protocol;

namespace Mox.Lobby.Client
{
    internal class ClientPlayerSlotCollection : List<PlayerSlotData>, IPlayerSlotCollection
    {
        #region Methods

        internal void HandleChangedMessage(PlayerSlotsChangedMessage message)
        {
            List<int> changedIndices = new List<int>(message.Changes.Count);

            foreach (var change in message.Changes)
            {
                this[change.Index] = change.Data;
                changedIndices.Add(change.Index);
            }

            Changed.Raise(this, new ItemEventArgs<int[]>(changedIndices.ToArray()));
        }

        #endregion

        #region Events

        public event EventHandler<ItemEventArgs<int[]>> Changed;

        #endregion
    }
}

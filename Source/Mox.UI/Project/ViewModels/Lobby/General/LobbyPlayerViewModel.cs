using System;
using System.Diagnostics;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyPlayerViewModel : LobbyUserViewModel
    {
        #region Variables

        #endregion

        #region Constructor

        public LobbyPlayerViewModel(PlayerData player)
            : base(player.Id)
        {
            SyncFromPlayer(player);
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        internal void SyncFromPlayer(PlayerData player)
        {
            Debug.Assert(player.Id == Id);
            Name = player.Name;
        }

        #endregion
    }
}

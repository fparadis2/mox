using System;

namespace Mox.Lobby.Backend
{
    public interface IClient
    {
        #region Properties

        User User { get; }
        IChatClient ChatClient { get; }

        #endregion

        #region Methods

        void OnUserChanged(UserChange change, User user);
        void OnPlayerChanged(PlayerChange change, Player player);

        #endregion
    }
}

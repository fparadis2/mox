using System;

namespace Mox.Lobby
{
    public interface ILobbyService
    {
        #region Methods

        ILobby CreateLobby();

        ILobby JoinLobby(Guid lobbyId);

        #endregion
    }
}

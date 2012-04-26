using System;

namespace Mox.Lobby2.Backend
{
    public interface IClient
    {
        #region Properties

        User User { get; }
        LobbyBackend Lobby { get; }

        #endregion

        #region Methods

        void SendMessage(Message message);

        #endregion
    }
}

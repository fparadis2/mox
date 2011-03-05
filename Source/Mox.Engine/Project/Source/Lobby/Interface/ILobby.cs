using System;

namespace Mox.Lobby
{
    public interface ILobby
    {
        #region Properties

        /// <summary>
        /// Id of the lobby.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The user connected to the lobby.
        /// </summary>
        User User { get; }

        /// <summary>
        /// The chat service for this lobby.
        /// </summary>
        IChatService Chat { get; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a user joins or leave. Also, triggered for all current users when subscribing.
        /// </summary>
        event EventHandler<UserChangedEventArgs> UserChanged;

        #endregion
    }
}
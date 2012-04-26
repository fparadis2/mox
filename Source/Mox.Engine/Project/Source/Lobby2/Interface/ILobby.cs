using System;

namespace Mox.Lobby2
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
        /// The users connected to the lobby.
        /// </summary>
        IObservableCollection<User> Users { get; }

        /// <summary>
        /// The users connected to the lobby.
        /// </summary>
        IObservableCollection<Player> Players { get; }

        /// <summary>
        /// Chat service
        /// </summary>
        IChatService Chat { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the data for a player.
        /// </summary>
        SetPlayerDataResult SetPlayerData(Guid playerId, PlayerData player);

        #endregion
    }
}
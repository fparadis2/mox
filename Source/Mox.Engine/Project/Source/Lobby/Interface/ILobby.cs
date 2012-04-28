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
        /// The users connected to the lobby.
        /// </summary>
        ILobbyItemCollection<User> Users { get; }

        /// <summary>
        /// The users connected to the lobby.
        /// </summary>
        ILobbyItemCollection<Player> Players { get; }

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

    public interface ILobbyItemCollection<T> : IObservableCollection<T>
    {
        event EventHandler<ItemEventArgs<T>> ItemChanged;
    }
}
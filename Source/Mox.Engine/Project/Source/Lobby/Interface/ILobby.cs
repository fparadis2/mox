using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// The slots available for the game.
        /// </summary>
        IPlayerSlotCollection Slots { get; }

        /// <summary>
        /// Chat service
        /// </summary>
        IChatService Chat { get; }

        /// <summary>
        /// Server Messages
        /// </summary>
        IServerMessages ServerMessages { get; }

        /// <summary>
        /// The game
        /// </summary>
        IGameService GameService { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the data for a player slot.
        /// </summary>
        Task<SetPlayerSlotDataResult> SetPlayerSlotData(int slotIndex, PlayerSlotData data);

        /// <summary>
        /// Assigns a user to a player slot.
        /// </summary>
        Task<AssignPlayerSlotResult> AssignPlayerSlot(int slotIndex, User user);

        #endregion
    }

    public interface ILobbyItemCollection<T> : IObservableCollection<T>
    {
        event EventHandler<ItemEventArgs<T>> ItemChanged;
    }

    public interface IPlayerSlotCollection : IReadOnlyList<PlayerSlot>
    {
        event EventHandler<ItemEventArgs<int>> ItemChanged;
    }
}
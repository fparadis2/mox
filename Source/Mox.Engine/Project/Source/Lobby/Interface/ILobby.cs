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
        Guid LocalUserId { get; }

        /// <summary>
        /// The leader of the lobby.
        /// </summary>
        Guid LeaderId { get; }

        /// <summary>
        /// The players connected to the lobby.
        /// </summary>
        IPlayerCollection Players { get; }

        /// <summary>
        /// The slots available for the game.
        /// </summary>
        IPlayerSlotCollection Slots { get; }

        /// <summary>
        /// The parameters that were used to create the lobby.
        /// </summary>
        LobbyParameters Parameters { get; }

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
        /// Gets the player identity info for a given player id.
        /// </summary>
        Task<IPlayerIdentity> GetPlayerIdentity(Guid playerId);

        /// <summary>
        /// Sets the data for a player.
        /// </summary>
        Task<SetPlayerDataResult> SetPlayerData(PlayerData data);

        /// <summary>
        /// Sets the data for a player slot.
        /// </summary>
        Task<SetPlayerSlotDataResult> SetPlayerSlotData(int slotIndex, PlayerSlotData data);

        #endregion

        #region Events

        /// <summary>
        /// Fired when the leader changes.
        /// </summary>
        event EventHandler LeaderChanged;

        #endregion
    }

    public interface IPlayerCollection : IReadOnlyCollection<PlayerData>
    {
        bool TryGet(Guid id, out PlayerData player);

        event EventHandler<PlayersChangedEventArgs> Changed;
    }

    public interface IPlayerSlotCollection : IReadOnlyList<PlayerSlotData>
    {
        event EventHandler<ItemEventArgs<int[]>> Changed;
    }
}
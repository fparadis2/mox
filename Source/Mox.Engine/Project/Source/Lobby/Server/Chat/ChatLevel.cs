namespace Mox.Lobby.Server
{
    public enum ChatLevel
    {
        /// <summary>
        /// Spectators, messages are sent only to other spectators
        /// </summary>
        Spectator,
        /// <summary>
        /// Normal user, messages are sent to everyone.
        /// </summary>
        Normal,
    }
}
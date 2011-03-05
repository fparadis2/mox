using System.ServiceModel;

namespace Mox.Lobby.Network
{
    public interface IClientContract
    {
        #region User info

        /// <summary>
        /// Called by the service when a user joins/leaves the lobby.
        /// Also called when joining to enumerate the current users.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnUserChanged(UserChange change, User user);

        #endregion

        #region Chat

        /// <summary>
        /// Called by the service when a client says something.
        /// </summary>
        /// <param name="user">The player which talked</param>
        /// <param name="message">The player message</param>
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(User user, string message);

        #endregion
    }
}
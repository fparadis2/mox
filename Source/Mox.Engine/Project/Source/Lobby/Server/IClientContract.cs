using System.ServiceModel;

namespace Mox.Lobby.Network
{
    public interface IClientContract
    {
        #region User info

#warning Todo: user change feedback
        ///// <summary>
        ///// Called by the service when a user connects or disconnects.
        ///// </summary>
        ///// <param name="user">The user that connected.</param>
        ///// <param name="change">The change that occured.</param>
        //[OperationContract(IsOneWay = true)]
        //void OnUserChanged(User user, UserChange change);

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

#warning Todo: user change feedback
    //public enum UserChange
    //{
    //    /// <summary>
    //    /// New user
    //    /// </summary>
    //    Connected,
    //    /// <summary>
    //    /// User quit
    //    /// </summary>
    //    Disconnected,
    //    /// <summary>
    //    /// User details changed (name, etc).
    //    /// </summary>
    //    DetailsChanged
    //}
}
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Mox.Lobby.Network
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IClientContract))]
    public interface IServerContract
    {
        #region Lobby management

        [OperationContract]
        IEnumerable<Guid> GetLobbies();

        [OperationContract(IsInitiating = true)]
        LoginDetails CreateLobby(string userName);

        [OperationContract(IsInitiating = true)]
        LoginDetails EnterLobby(Guid lobby, string userName);

        [OperationContract(IsTerminating = true)]
        void Logout();

        #endregion

        #region User info

        [OperationContract]
        User[] GetUsers();

        #endregion

        #region Chat

        /// <summary>
        /// Called by the client to speak on behalf of the user
        /// </summary>
        /// <param name="message">The user message</param>
        [OperationContract(IsOneWay = true)]
        void Say(string message);

        #endregion
    }
}

using System;

namespace Mox.Lobby
{
    [Serializable]
    public class GetLobbyDetailsRequest : Request<GetLobbyDetailsResponse>
    {
    }

    [Serializable]
    public class GetLobbyDetailsResponse : Response
    {
        public UserChangedResponse Users;
        public PlayerSlotChangedMessage Slots;
    }
}

using System;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class GetLobbyDetailsRequest : Request<GetLobbyDetailsResponse>
    {
    }

    [Serializable]
    public class GetLobbyDetailsResponse : Response
    {
        public PlayersChangedMessage Players;
        public PlayerSlotsChangedMessage Slots;
        public LeaderChangedMessage Leader;
        public LobbyGameParametersChangedMessage GameParameters;
    }
}

using System;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class LobbyGameParametersChangedMessage : Message
    {
        public LobbyGameParameters Parameters;
    }
}

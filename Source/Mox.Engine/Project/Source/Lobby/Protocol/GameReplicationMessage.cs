using System;
using Mox.Transactions;

namespace Mox.Lobby.Network.Protocol
{
    [Serializable]
    public class GameReplicationMessage : Message
    {
        public ICommand Command
        {
            get;
            set;
        }
    }
}
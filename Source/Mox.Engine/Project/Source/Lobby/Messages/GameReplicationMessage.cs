using System;
using Mox.Transactions;

namespace Mox.Lobby
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
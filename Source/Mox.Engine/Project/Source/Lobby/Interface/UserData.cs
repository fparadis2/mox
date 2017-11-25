using System;

namespace Mox.Lobby
{
    // Per-user data that is always sent completely to all clients
    [Serializable]
    public struct UserData
    {
        public bool IsBot;
        public string Name;
    }
}

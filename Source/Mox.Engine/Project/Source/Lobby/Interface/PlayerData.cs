using System;

namespace Mox.Lobby
{
    [Serializable]
    public struct PlayerData
    {
        public Guid Id;
        public string Name;

        public PlayerData(Guid user, string username)
        {
            Id = user;
            Name = username;
        }
    }

    public enum SetPlayerDataResult
    {
        Success,
        InvalidPlayer
    }
}

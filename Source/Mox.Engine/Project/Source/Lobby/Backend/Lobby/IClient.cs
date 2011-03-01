using System;

namespace Mox.Lobby.Backend
{
    public interface IClient
    {
        User User { get; }
        IChatClient ChatClient { get; }
    }
}

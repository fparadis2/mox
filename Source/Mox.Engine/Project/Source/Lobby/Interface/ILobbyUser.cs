using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Lobby
{
    public interface ILobbyUser
    {
        Guid Id { get; }
        UserData Data { get; }
    }

    public interface ILobbyUserCollection : IReadOnlyCollection<ILobbyUser>
    {
        ILobbyUser this[Guid id] { get; }

        event EventHandler<ItemEventArgs<ILobbyUser>> UserJoined;
        event EventHandler<ItemEventArgs<ILobbyUser>> UserLeft;
    }
}

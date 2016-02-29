using System;

namespace Mox
{
    public interface IGameFormat
    {
        string Name { get; }
        int NumPlayers { get; }
    }
}

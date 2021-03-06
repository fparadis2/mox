﻿using System;

namespace Mox
{
    public interface IGameFormat
    {
        string Name { get; }
        string Description { get; }
        int NumPlayers { get; }
    }
}

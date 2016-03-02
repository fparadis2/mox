using System;

namespace Mox
{
    public interface IDeckFormat
    {
        string Name { get; }
        string Description { get; }
        int MinimumCardCount { get; }
    }
}

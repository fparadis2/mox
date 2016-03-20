using System;
using Mox.Database;

namespace Mox
{
    public interface IDeckFormat
    {
        string Name { get; }
        string Description { get; }
        int MinimumCardCount { get; }

        bool Validate(IDeck deck);
    }

    public class AnyDeckFormat : IDeckFormat
    {
        public string Name { get { return "Any deck"; } }
        public string Description { get { return "A format that allows any deck whatsoever"; } }
        public int MinimumCardCount { get { return 0; } }
        public bool Validate(IDeck deck)
        {
            return true;
        }
    }
}

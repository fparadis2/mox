using System;

namespace Mox.Replication
{
    [Flags]
    public enum UserAccess
    {
        None,
        Read = 1,
        Write = 2,
        All = Read | Write
    }
}
using System;
using System.IO;

namespace Mox.Database
{
    public interface IDeckStorageStrategy
    {
        void LoadAll(Action<Stream, Guid> loadingAction);
        Stream OpenWrite(Guid guid);
        void Delete(Guid guid);
    }
}
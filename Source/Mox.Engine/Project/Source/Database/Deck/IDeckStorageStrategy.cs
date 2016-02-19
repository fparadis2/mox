using System;
using System.Collections.Generic;

namespace Mox.Database
{
    public interface IDeckStorageStrategy
    {
        IEnumerable<IDeck> LoadAll();
        string GetDeckContents(IDeck deck);
        DateTime GetLastModificationTime(IDeck deck);

        IDeck Save(string name, string contents);
        void Delete(IDeck deck);

        bool ValidateDeckName(ref string name, out string error);
    }
}
using System;
using System.Collections.Generic;

namespace Mox.Database
{
    public interface IDeckStorageStrategy
    {
        IEnumerable<IDeck> LoadAll();
        string GetDeckContents(IDeck deck);
        DateTime GetLastModificationTime(IDeck deck);

        IDeck Save(IDeck deck, string newContents);
        IDeck Rename(IDeck deck, string newName);

        void Delete(IDeck deck);
    }
}
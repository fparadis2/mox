using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Mox.Database
{
    public class DiskDeckStorageStrategy : IDeckStorageStrategy
    {
        private const string DeckExtension = ".txt";

        private static readonly char[] ms_invalidChars = Path.GetInvalidFileNameChars();

        private readonly string m_baseDirectory;

        public DiskDeckStorageStrategy(string directory)
        {
            m_baseDirectory = directory;
            Directory.CreateDirectory(directory);
        }

        public IEnumerable<IDeck> LoadAll()
        {
            foreach (string file in Directory.GetFiles(m_baseDirectory, "*" + DeckExtension))
            {
                string deckName = Path.GetFileNameWithoutExtension(file);
                string contents = File.ReadAllText(file);

                yield return Deck.Read(deckName, contents);
            }
        }

        public string GetDeckContents(IDeck deck)
        {
            string filename = GetFilename(deck);
            return File.ReadAllText(filename);
        }

        public DateTime GetLastModificationTime(IDeck deck)
        {
            string filename = GetFilename(deck);
            return File.GetLastWriteTimeUtc(filename);
        }

        public IDeck Save(string name, string contents)
        {
            string filename = GetFilename(name);
            File.WriteAllText(filename, contents);
            return Deck.Read(name, contents);
        }

        public void Delete(IDeck deck)
        {
            string filename = GetFilename(deck);
            File.Delete(filename);
        }

        public bool ValidateDeckName(ref string name, out string error)
        {
            if (name.IndexOfAny(ms_invalidChars) >= 0)
            {
                error = string.Format("{0} is an invalid deck name.", name);

                foreach (char c in ms_invalidChars)
                {
                    name = name.Replace(c.ToString(CultureInfo.InvariantCulture), string.Empty);
                }

                return false;
            }

            error = null;
            return true;
        }

        public bool IsValidName(string name)
        {
            return name.IndexOfAny(ms_invalidChars) < 0;
        }

        private string GetFilename(IDeck deck)
        {
            return GetFilename(deck.Name);
        }

        private string GetFilename(string deckName)
        {
            return Path.Combine(m_baseDirectory, deckName + DeckExtension);
        }
    }
}
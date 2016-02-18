using System;
using System.Collections.Generic;
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

        public IDeck Save(IDeck deck, string newContents)
        {
            string filename = GetFilename(deck);
            File.WriteAllText(filename, newContents);
            return Deck.Read(deck.Name, newContents);
        }

        public IDeck Rename(IDeck deck, string newName)
        {
            if (!IsValidName(newName))
                return null;

            string newFilename = GetFilename(newName);

            File.Move(GetFilename(deck), newFilename);

            string contents = File.ReadAllText(newFilename);
            return Deck.Read(newName, contents);
        }

        public void Delete(IDeck deck)
        {
            string filename = GetFilename(deck);
            File.Delete(filename);
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
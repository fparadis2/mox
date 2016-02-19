using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Database
{
    public class MemoryDeckStorageStrategy : IDeckStorageStrategy
    {
        private readonly Dictionary<string, string> m_persistedDecks = new Dictionary<string, string>(DeckLibrary.DeckNameComparer);

        public bool IsPersisted(IDeck deck)
        {
            return m_persistedDecks.ContainsKey(deck.Name);
        }

        public int PersistedDecksCount
        {
            get { return m_persistedDecks.Count; }
        }

        public IEnumerable<IDeck> LoadAll()
        {
            return m_persistedDecks.Select(pair => Deck.Read(pair.Key, pair.Value));
        }

        public string GetDeckContents(IDeck deck)
        {
            string contents;
            m_persistedDecks.TryGetValue(deck.Name, out contents);
            return contents;
        }

        public DateTime GetLastModificationTime(IDeck deck)
        {
            return DateTime.UtcNow;
        }

        public IDeck Save(string name, string newContents)
        {
            m_persistedDecks[name] = newContents;
            return Deck.Read(name, newContents);
        }

        public void Delete(IDeck deck)
        {
            m_persistedDecks.Remove(deck.Name);
        }

        public virtual bool ValidateDeckName(ref string name, out string error)
        {
            error = null;
            return true;
        }
    }
}
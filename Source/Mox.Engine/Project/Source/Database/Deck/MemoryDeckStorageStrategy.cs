using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Database
{
    public class MemoryDeckStorageStrategy : IDeckStorageStrategy
    {
        private readonly Dictionary<string, string> m_persistedDecks = new Dictionary<string, string>();

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

        public IDeck Save(IDeck deck, string newContents)
        {
            m_persistedDecks[deck.Name] = newContents;
            return Deck.Read(deck.Name, newContents);
        }

        public IDeck Rename(IDeck deck, string newName)
        {
            string contents;
            if (!m_persistedDecks.TryGetValue(deck.Name, out contents))
                return null;

            if (m_persistedDecks.ContainsKey(newName))
                return null;

            m_persistedDecks.Remove(deck.Name);

            Deck renamedDeck = Deck.Read(newName, contents);
            m_persistedDecks.Add(renamedDeck.Name, contents);
            return renamedDeck;
        }

        public void Delete(IDeck deck)
        {
            m_persistedDecks.Remove(deck.Name);
        }
    }
}
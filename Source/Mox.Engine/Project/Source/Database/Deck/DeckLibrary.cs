using System;
using System.Collections.Generic;

namespace Mox.Database
{
    public class DeckLibrary
    {
        #region Variables

        private static readonly IEqualityComparer<string> ms_comparer = StringComparer.OrdinalIgnoreCase;

        private readonly IDeckStorageStrategy m_storageStrategy;
        private readonly Dictionary<string, IDeck> m_decks = new Dictionary<string, IDeck>(ms_comparer);

        #endregion

        #region Constructor

        public DeckLibrary()
            : this(new MemoryDeckStorageStrategy())
        {
        }

        public DeckLibrary(IDeckStorageStrategy storageStrategy)
        {
            Throw.IfNull(storageStrategy, "storageStrategy");
            m_storageStrategy = storageStrategy;
        }

        #endregion

        #region Properties

        public IEnumerable<IDeck> Decks
        {
            get
            {
                return m_decks.Values;
            }
        }

        #endregion

        #region Methods

        public void Load()
        {
            m_decks.Clear();
            foreach (var deck in m_storageStrategy.LoadAll())
            {
                Add(deck);
            }
        }

        public string GetDeckContents(IDeck deck)
        {
            return m_storageStrategy.GetDeckContents(deck);
        }

        public DateTime GetLastModificationTime(IDeck deck)
        {
            return m_storageStrategy.GetLastModificationTime(deck);
        }

        public IDeck Save(IDeck deck, string newContents)
        {
            IDeck newDeck = m_storageStrategy.Save(deck, newContents);
            m_decks[deck.Name] = newDeck;
            return newDeck;
        }

        public void Delete(IDeck deck)
        {
            Remove(deck);
            m_storageStrategy.Delete(deck);
        }

        public IDeck Rename(IDeck deck, string newName)
        {
            Throw.IfEmpty(newName, "newName");

            IDeck existingDeck;
            if (m_decks.TryGetValue(newName, out existingDeck) && existingDeck != deck)
            {
                // Already a deck with that name
                return null;
            }

            IDeck renamedDeck = m_storageStrategy.Rename(deck, newName);
            if (renamedDeck == null)
                return null;

            Remove(deck);
            Add(renamedDeck);

            return renamedDeck;
        }

        private void Add(IDeck deck)
        {
            m_decks.Add(deck.Name, deck);
        }

        private void Remove(IDeck deck)
        {
            m_decks.Remove(deck.Name);
        }

        public IDeck GetDeck(string name)
        {
            IDeck deck;
            m_decks.TryGetValue(name, out deck);
            return deck;
        }

        public bool IsValidName(string name)
        {
            return m_storageStrategy.IsValidName(name);
        }

        #endregion
    }
}

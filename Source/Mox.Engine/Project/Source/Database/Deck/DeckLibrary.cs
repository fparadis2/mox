using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mox.Database
{
    public class DeckLibrary
    {
        #region Variables

        private readonly IDeckStorageStrategy m_storageStrategy;
        private readonly Dictionary<string, IDeck> m_decks = new Dictionary<string, IDeck>();

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

            if (string.Equals(deck.Name, newName))
                return deck; // Already has that name

            if (m_decks.ContainsKey(newName))
                return null; // Already a deck with that name

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

        #endregion
    }
}

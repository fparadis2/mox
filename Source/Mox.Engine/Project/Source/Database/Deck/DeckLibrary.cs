using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Mox.Collections;

namespace Mox.Database
{
    public class DeckLibrary
    {
        #region Inner Types

        private class DeckCollection : KeyedCollection<Guid, Deck>
        {
            protected override Guid GetKeyForItem(Deck item)
            {
                return item.Guid;
            }
        }

        #endregion

        #region Variables

        private readonly IDeckStorageStrategy m_storageStrategy;
        private readonly DeckCollection m_decks = new DeckCollection();

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

        public ICollection<Deck> Decks
        {
            get
            {
                return new ReadOnlyKeyedCollection<Guid, Deck>(m_decks);
            }
        }

        #endregion

        #region Methods

        public void Load()
        {
            m_decks.Clear();

            m_storageStrategy.LoadAll(Load);
        }

        private void Load(Stream stream, Guid guid)
        {
            Deck deck = Deck.Load(stream, guid);
            m_decks.Add(deck);
        }

        public Deck GetDeck(Guid deckId)
        {
            if (m_decks.Contains(deckId))
            {
                return m_decks[deckId];
            }
            return null;
        }

        public void Save(Deck deck)
        {
            using (Stream stream = m_storageStrategy.OpenWrite(deck.Guid))
            {
                deck.Save(stream);
            }

            if (m_decks.Contains(deck.Guid))
            {
                m_decks.Remove(deck.Guid);
            }

            m_decks.Add(deck);
        }

        public void Delete(Deck deck)
        {
            m_decks.Remove(deck);
            m_storageStrategy.Delete(deck.Guid);
        }

        #endregion
    }
}

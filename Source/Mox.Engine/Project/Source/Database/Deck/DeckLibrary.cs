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
            Deck deck = new Deck();

            XPathDocument document = new XPathDocument(stream);
            deck.Load(document.CreateNavigator(), guid);
            m_decks.Add(deck);
        }

        public bool Save(Deck deck)
        {
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (Stream stream = m_storageStrategy.OpenWrite(deck.Guid))
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                deck.Save(writer);
            }

            if (!m_decks.Contains(deck.Guid))
            {
                m_decks.Add(deck);
                return true;
            }
            Debug.Assert(m_decks[deck.Guid] == deck);
            return false;
        }

        public void Delete(Deck deck)
        {
            m_decks.Remove(deck);
            m_storageStrategy.Delete(deck.Guid);
        }

        #endregion
    }
}

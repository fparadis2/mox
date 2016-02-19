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

        internal static IEqualityComparer<string> DeckNameComparer
        {
            get { return ms_comparer; }
        }

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

        public bool ValidateDeckName(IDeck oldDeck, ref string name, out string error)
        {
            if (string.IsNullOrEmpty(name))
            {
                error = "A deck cannot have an empty name.";
                return false;
            }

            if (oldDeck != null && ms_comparer.Equals(oldDeck.Name, name))
            {
                error = null;
                return true;
            }

            IDeck existingDeck;
            if (m_decks.TryGetValue(name, out existingDeck))
            {
                // Already a deck with that name
                error = string.Format("There is already another deck named {0}.", name);
                name = GenerateUnusedName(name);
                return false;
            }

            return m_storageStrategy.ValidateDeckName(ref name, out error);
        }

        public IDeck Create(string name, string contents)
        {
            return Save(null, name, contents);
        }

        public IDeck Save(IDeck oldDeck, string name, string contents)
        {
            string error;
            if (!ValidateDeckName(oldDeck, ref name, out error))
            {
                return null;
            }

            IDeck newDeck = m_storageStrategy.Save(name, contents);
            m_decks[name] = newDeck;

            if (oldDeck != null && !ms_comparer.Equals(oldDeck.Name, name))
            {
                Delete(oldDeck);
            }

            return newDeck;
        }

        public void Delete(IDeck deck)
        {
            m_decks.Remove(deck.Name);
            m_storageStrategy.Delete(deck);
        }

        private void Add(IDeck deck)
        {
            m_decks.Add(deck.Name, deck);
        }

        public IDeck GetDeck(string name)
        {
            IDeck deck;
            m_decks.TryGetValue(name, out deck);
            return deck;
        }

        private static void RemoveNumberSuffix(ref string name)
        {
            if (name.Length < 3)
                return;

            if (name[name.Length - 1] != ')')
                return;

            int lastParenIndex = name.LastIndexOf('(');
            if (lastParenIndex < 0)
                return;

            int number;
            if (!int.TryParse(name.Substring(lastParenIndex + 1, name.Length - lastParenIndex - 2), out number))
                return;

            name = name.Substring(0, lastParenIndex).Trim();
        }

        private string GenerateUnusedName(string name)
        {
            RemoveNumberSuffix(ref name);

            if (!m_decks.ContainsKey(name))
                return name;

            int counter = 0;
            while (true)
            {
                string suffixedName = string.Format("{0} ({1})", name, ++counter);
                if (!m_decks.ContainsKey(suffixedName))
                    return suffixedName;
            }
        }

        #endregion
    }
}

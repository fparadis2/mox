using System;
using Mox.Database;

namespace Mox.UI.Lobby
{
    public class DeckChoiceViewModel
    {
        #region Variables

        public static readonly DeckChoiceViewModel Random = new DeckChoiceViewModel(null, Guid.Empty, "(Random Deck)");

        private readonly Deck m_deck;
        private readonly Guid m_id;
        private readonly string m_name;

        #endregion

        #region Constructor

        public DeckChoiceViewModel(Deck deck)
            : this(deck, deck.Guid, deck.Name)
        {
        }

        private DeckChoiceViewModel(Deck deck, Guid guid, string name)
        {
            m_deck = deck;
            m_id = guid;
            m_name = name;
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return m_name; }
        }

        public Guid Id
        {
            get { return m_id; }
        }

        internal Deck Deck
        {
            get { return m_deck; }
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            DeckChoiceViewModel other = obj as DeckChoiceViewModel;
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return other.m_id == m_id;
        }

        public override int GetHashCode()
        {
            return m_id.GetHashCode();
        }

        #endregion
    }
}

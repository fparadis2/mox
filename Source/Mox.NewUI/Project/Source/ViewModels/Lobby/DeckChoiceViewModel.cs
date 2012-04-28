using System;
using Mox.Database;

namespace Mox.UI.Lobby
{
    public class DeckChoiceViewModel
    {
        #region Variables

        public static readonly DeckChoiceViewModel Random = new DeckChoiceViewModel(Guid.Empty, "(Random Deck)");

        private readonly Guid m_id;
        private readonly string m_name;

        #endregion

        #region Constructor

        public DeckChoiceViewModel(Deck deck)
            : this(deck.Guid, deck.Name)
        {
        }

        private DeckChoiceViewModel(Guid guid, string name)
        {
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

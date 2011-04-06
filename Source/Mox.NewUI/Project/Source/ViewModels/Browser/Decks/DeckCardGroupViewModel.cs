using System;
using Caliburn.Micro;
using Mox.Database;

namespace Mox.UI.Browser
{
    public enum DeckCardGroup
    {
        Creatures,
        Spells,
        Lands,
        Misc
    }

    public class DeckCardGroupViewModel : PropertyChangedBase, IComparable<DeckCardGroupViewModel>, IComparable
    {
        #region Variables

        private readonly DeckCardGroup m_group;
        private int m_quantity;

        #endregion

        #region Constructor

        public DeckCardGroupViewModel(CardInfo card)
            : this(GetGroup(card))
        {
        }

        public DeckCardGroupViewModel(DeckCardGroup group)
        {
            m_group = group;
        }

        #endregion

        #region Properties

        public DeckCardGroup Group
        {
            get { return m_group; }
        }

        public string DisplayName
        {
            get { return string.Format("{0} ({1})", Group, Quantity); }
        }

        public int Quantity
        {
            get { return m_quantity; }
            set
            {
                if (m_quantity != value)
                {
                    m_quantity = value;
                    NotifyOfPropertyChange(() => Quantity);
                    NotifyOfPropertyChange(() => DisplayName);
                }
            }
        }

        #endregion

        #region Methods

        public static DeckCardGroup GetGroup(CardInfo card)
        {
            return GetGroup(card.Type.GetDominantType());
        }

        private static DeckCardGroup GetGroup(Type dominantType)
        {
            switch (dominantType)
            {
                case Type.Creature:
                case Type.Planeswalker:
                    return DeckCardGroup.Creatures;

                case Type.Artifact:
                case Type.Enchantment:
                case Type.Instant:
                case Type.Sorcery:
                    return DeckCardGroup.Spells;

                case Type.Land:
                    return DeckCardGroup.Lands;

                default:
                    return DeckCardGroup.Misc;
            }
        }

        public override bool Equals(object obj)
        {
            DeckCardGroupViewModel other = obj as DeckCardGroupViewModel;

            if (other == null)
            {
                return false;
            }

            return m_group == other.m_group;
        }

        public override int GetHashCode()
        {
            return m_group.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return CompareTo((DeckCardGroupViewModel)obj);
        }

        public int CompareTo(DeckCardGroupViewModel other)
        {
            return Group.CompareTo(other.Group);
        }

        public override string ToString()
        {
            return Group.ToString();
        }

        #endregion
    }
}
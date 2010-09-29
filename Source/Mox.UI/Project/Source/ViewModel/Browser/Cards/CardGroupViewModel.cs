using System;
using Mox.Database;

namespace Mox.UI.Browser
{
    public enum CardGroup
    {
        Creatures,
        Spells,
        Lands,
        Misc
    }

    public class CardGroupViewModel : IComparable<CardGroupViewModel>, IComparable
    {
        #region Variables

        private readonly CardGroup m_group;

        #endregion

        #region Constructor

        public CardGroupViewModel(CardInfo card)
        {
            m_group = GetGroup(card.Type.GetDominantType());
        }

        #endregion

        #region Properties

        public CardGroup Group
        {
            get { return m_group; }
        }

        #endregion

        #region Methods

        private static CardGroup GetGroup(Type dominantType)
        {
            switch (dominantType)
            {
                case Type.Creature:
                case Type.Planeswalker:
                    return CardGroup.Creatures;

                case Type.Artifact:
                case Type.Enchantment:
                case Type.Instant:
                case Type.Sorcery:
                    return CardGroup.Spells;

                case Type.Land:
                    return CardGroup.Lands;

                default:
                    return CardGroup.Misc;
            }
        }

        public override bool Equals(object obj)
        {
            CardGroupViewModel other = obj as CardGroupViewModel;

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
            return CompareTo((CardGroupViewModel)obj);
        }

        public int CompareTo(CardGroupViewModel other)
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
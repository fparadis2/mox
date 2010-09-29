using System;
using System.Diagnostics;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class CardGroupViewModel : IComparable<CardGroupViewModel>
    {
        #region Variables

        private readonly Type m_type;

        #endregion

        #region Constructor

        public CardGroupViewModel(CardInfo card)
        {
            m_type = card.Type.GetDominantType();
        }

        #endregion

        #region Properties

        public string DisplayName
        {
            get { return m_type + "s"; }
        }

        private int SortOrder
        {
            get
            {
                switch (m_type)
                {
                    case Type.Creature:
                        return 1;
                    case Type.Artifact:
                        return 2;
                    case Type.Enchantment:
                        return 3;
                    case Type.Instant:
                        return 4;
                    case Type.Sorcery:
                        return 5;
                    case Type.Planeswalker:
                        return 6;
                    case Type.Scheme:
                        return 7;
                    case Type.Tribal:
                        return 8;
                    case Type.Land:
                        return 9;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            CardGroupViewModel other = obj as CardGroupViewModel;

            if (other == null)
            {
                return false;
            }

            return m_type == other.m_type;
        }

        public override int GetHashCode()
        {
            return m_type.GetHashCode();
        }

        public int CompareTo(CardGroupViewModel other)
        {
            return SortOrder.CompareTo(other.SortOrder);
        }

        public override string ToString()
        {
            return DisplayName;
        }

        #endregion
    }
}
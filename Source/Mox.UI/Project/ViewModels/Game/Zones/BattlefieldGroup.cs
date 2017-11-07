using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mox.UI.Game
{
    public class BattlefieldGroup
    {
        private readonly GroupKey m_key;
        private readonly List<CardViewModel> m_cards = new List<CardViewModel>();

        public BattlefieldGroup(GroupKey key)
        {
            m_key = key;
        }

        public GroupKey Key
        {
            get { return m_key; }
        }

        public PermanentType Type
        {
            get { return m_key.Type; }
        }

        public IReadOnlyList<CardViewModel> Cards
        {
            get { return m_cards; }
        }

        public bool IsEmpty
        {
            get { return m_cards.Count == 0; }
        }

        #region Methods

        public void Add(CardViewModel card)
        {
            m_cards.Add(card);
        }

        public bool Remove(CardViewModel card)
        {
            return m_cards.Remove(card);
        }

        #endregion

        #region Nested Types

        // Order is important
        public enum PermanentType
        {
            Creature,
            Artifact,
            Land,
            Planeswalker,
            Enchantment
        }

        public struct GroupKey : IComparable<GroupKey>, IComparable<BattlefieldGroup>
        {
            public PermanentType Type;
            public string Name;
            public bool IsTapped;

            public GroupKey(CardViewModel card)
            {
                if (card.Source.AttachedTo != null)
                    throw new NotImplementedException("TODO");

                var source = card.Source;
                Type = GetPermanentType(source.Type);
                Name = source.Name;
                IsTapped = source.Tapped;
            }

            public int CompareTo(GroupKey other)
            {
                int result = Type.CompareTo(other.Type);
                if (result != 0)
                    return result;

                result = string.Compare(Name, other.Name, StringComparison.Ordinal);
                if (result != 0)
                    return result;

                return -IsTapped.CompareTo(other.IsTapped);
            }

            public int CompareTo(BattlefieldGroup other)
            {
                return CompareTo(other.Key);
            }

            private static PermanentType GetPermanentType(Type type)
            {
                if (type.HasFlag(Mox.Type.Creature))
                {
                    return PermanentType.Creature;
                }

                if (type.HasFlag(Mox.Type.Artifact))
                {
                    return PermanentType.Artifact;
                }

                if (type.HasFlag(Mox.Type.Land))
                {
                    return PermanentType.Land;
                }

                if (type.HasFlag(Mox.Type.Planeswalker))
                {
                    return PermanentType.Planeswalker;
                }

                if (type.HasFlag(Mox.Type.Enchantment))
                {
                    return PermanentType.Enchantment;
                }

                // Fallback for tests
                return PermanentType.Enchantment;
            }
        }

        #endregion
    }
}
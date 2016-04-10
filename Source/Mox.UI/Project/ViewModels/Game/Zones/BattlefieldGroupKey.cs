using System;

namespace Mox.UI.Game
{
    internal struct BattlefieldGroupKey : IComparable<BattlefieldGroupKey>
    {
        public BattlefieldViewModel.PermanentType Type;
        public string Name;

        public BattlefieldGroupKey(CardViewModel card)
        {
            var source = card.Source;
            Type = GetPermanentType(source.Type);
            Name = source.Name;
        }

        public int CompareTo(BattlefieldGroupKey other)
        {
            int result = Type.CompareTo(other.Type);
            if (result != 0)
                return result;

            return string.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        private static BattlefieldViewModel.PermanentType GetPermanentType(Type type)
        {
            if (type.HasFlag(Mox.Type.Creature))
            {
                return BattlefieldViewModel.PermanentType.Creature;
            }

            if (type.HasFlag(Mox.Type.Artifact))
            {
                return BattlefieldViewModel.PermanentType.Artifact;
            }

            if (type.HasFlag(Mox.Type.Land))
            {
                return BattlefieldViewModel.PermanentType.Land;
            }

            if (type.HasFlag(Mox.Type.Planeswalker))
            {
                return BattlefieldViewModel.PermanentType.Planeswalker;
            }

            if (type.HasFlag(Mox.Type.Enchantment))
            {
                return BattlefieldViewModel.PermanentType.Enchantment;
            }

            // Fallback for tests
            return BattlefieldViewModel.PermanentType.Enchantment;
        }
    }
}
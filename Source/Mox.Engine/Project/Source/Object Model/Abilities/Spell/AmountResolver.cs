using System;

namespace Mox.Abilities
{
    [Serializable]
    public abstract class AmountResolver
    {
        public abstract int Resolve(ISpellContext spell);

        public static implicit operator AmountResolver(int amount)
        {
            return new ConstantAmountResolver(amount);
        }

        public abstract bool Invalidate(PropertyBase property);
    }

    [Serializable]
    public class ConstantAmountResolver : AmountResolver
    {
        public ConstantAmountResolver(int amount)
        {
            Amount = amount;
        }

        public int Amount { get; }

        public override int Resolve(ISpellContext spell)
        {
            return Amount;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return false;
        }
    }
}

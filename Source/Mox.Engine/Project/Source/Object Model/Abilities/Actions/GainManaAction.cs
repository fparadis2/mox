using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mox.Flow;

namespace Mox.Abilities
{
    public class GainManaAction : Action
    {
        private readonly ManaAmount[] m_amounts;

        public GainManaAction(params ManaAmount[] amounts)
        {
            Throw.InvalidArgumentIf(amounts == null || amounts.Length == 0, "Invalid mana amounts", "amounts");
            m_amounts = amounts;
        }

        public IReadOnlyList<ManaAmount> Amounts
        {
            get { return m_amounts; }
        }

        public override void FillManaOutcome(IManaAbilityOutcome outcome)
        {
            foreach (var amount in m_amounts)
            {
                outcome.Add(amount);
            }
        }

        public override Part ResolvePart(Spell2 spell)
        {
            if (m_amounts.Length > 1)
            {
                return new GainManaChoicePart(spell.Controller, m_amounts);
            }
            else
            {
                return new GainManaPart(spell.Controller, m_amounts[0]);
            }
        }

        #region Inner Types

        private class GainManaChoicePart : ChoicePart<int>
        {
            private readonly ManaAmount[] m_amounts;

            public GainManaChoicePart(Resolvable<Player> player, ManaAmount[] amounts)
                : base(player)
            {
                Debug.Assert(amounts.Length > 1);
                m_amounts = amounts;
            }

            public override Choice GetChoice(Sequencer sequencer)
            {
                return new GainManaChoice(ResolvablePlayer, m_amounts);
            }

            public override Part Execute(Context context, int index)
            {
                if (index >= m_amounts.Length)
                    return this;

                return new GainManaPart(ResolvablePlayer, m_amounts[index]);
            }
        }

        private class GainManaPart : PlayerPart
        {
            private readonly ManaAmount m_amount;

            public GainManaPart(Resolvable<Player> player, ManaAmount amount)
                : base(player)
            {
                m_amount = amount;
            }

            public override Part Execute(Context context)
            {
                GetPlayer(context).ManaPool.GainMana(m_amount);
                return null;
            }
        }

        #endregion
    }
}

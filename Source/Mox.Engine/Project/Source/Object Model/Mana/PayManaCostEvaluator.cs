using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    internal class ManaAbilityCache
    {
        public ManaAbilityCache(Player player)
        {
            ExecutionEvaluationContext context = new ExecutionEvaluationContext { Type = EvaluationContextType.ManaPayment };

            foreach (Card card in player.Manager.Cards)
            {
                foreach (var ability in card.Abilities)
                {
                    if (!ability.IsManaAbility)
                        continue;

                    if (!ability.CanPlay(player, context))
                        continue;

                    Abilities.Add(ability);
                }
            }
        }

        public List<Ability> Abilities { get; } = new List<Ability>();
    }

    public class PayManaCostEvaluator
    {
        private readonly ManaCost m_cost;

        private readonly List<Ability> m_abilities;

        private readonly List<ManaSymbol> m_symbols = new List<ManaSymbol>();
        private readonly List<int> m_amounts = new List<int>();
        private int m_minAmount; // todo: remove

        public static PayManaCostEvaluator Create(Player player, ManaCost cost)
        {
            var abilityCache = new ManaAbilityCache(player);
            return new PayManaCostEvaluator(abilityCache.Abilities, cost);
        }

        public PayManaCostEvaluator(IEnumerable<Ability> abilities, ManaCost cost)
        {
            m_abilities = new List<Ability>(abilities);
            m_cost = cost;

            ComputeCompressedCost();
        }

        public bool CanPotentiallyPay()
        {
            return true;
        }

        private void ComputeCompressedCost()
        {
            var symbols = m_cost.Symbols;

            for (int i = symbols.Count; i-- > 0; )
            {
                // Ignore generic
                switch (symbols[i])
                {
                    case ManaSymbol.X:
                    case ManaSymbol.Y:
                    case ManaSymbol.Z:
                        continue;
                }

                // From specific to generic
                if (m_symbols.Count == 0 || m_symbols[m_symbols.Count - 1] != symbols[i])
                {
                    m_symbols.Add(symbols[i]);
                    m_amounts.Add(0);
                }

                m_amounts[m_amounts.Count - 1] += 1;
                m_minAmount++;
            }

            if (m_cost.Colorless > 0)
            {
                m_symbols.Add(ManaSymbol.X); // Hack: using X symbol for generic mana for this computation
                m_amounts.Add(m_cost.Colorless);

                m_minAmount += m_cost.Colorless;
            }
        }
    }
}

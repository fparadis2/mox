using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    #warning Todo Test!
    internal class ManaAbilityCache
    {
        private readonly List<ManaAmount> m_possibleAmounts = new List<ManaAmount>();
        private readonly bool m_anythingCouldHappen = false;

        public ManaAbilityCache(Player player)
        {
            ExecutionEvaluationContext context = new ExecutionEvaluationContext { Type = EvaluationContextType.ManaPayment };
            ManaAbilityEvaluator manaAbilityEvaluator = new ManaAbilityEvaluator(player.ManaPool);

            List<Ability> cardAbilities = new List<Ability>();

            foreach (Card card in player.Manager.Cards)
            {
                cardAbilities.Clear();

                foreach (var ability in card.Abilities)
                {
                    if (!ability.IsManaAbility)
                        continue;

                    if (!ability.CanPlay(player, context))
                        continue;

                    cardAbilities.Add(ability);
                }

                if (!manaAbilityEvaluator.Consider(cardAbilities))
                {
                    m_anythingCouldHappen = true;
                    break;
                }
            }

            m_possibleAmounts.AddRange(manaAbilityEvaluator.Amounts);
        }

        public bool CanPay(ManaCost cost)
        {
            if (m_anythingCouldHappen)
                return true;

            ManaPaymentEvaluator evaluator = new ManaPaymentEvaluator(cost);

            foreach (var possibleAmount in m_possibleAmounts)
            {
                if (evaluator.CanPay(possibleAmount))
                    return true;
            }

            return false;
        }
    }
}

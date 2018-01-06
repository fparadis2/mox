using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mox.Abilities;

namespace Mox
{
    /// <summary>
    /// Takes a list of mana abilities and returns the possible (upperbound) mana amount combinations resulting from these abilities
    /// </summary>
    public class ManaAbilityEvaluator
    {
        private readonly Outcome m_outcome;
        private readonly List<ManaAmount> m_amounts = new List<ManaAmount>();
        private readonly List<ManaAmount> m_newAmounts = new List<ManaAmount>();

        public IEnumerable<ManaAmount> Amounts
        {
            get { return m_amounts; }
        }

        public ManaAbilityEvaluator(ManaAmount baseAmount)
        {
            m_outcome = new Outcome(this);
            m_amounts.Add(baseAmount);
        }

        public bool Consider(IEnumerable<Ability> abilities)
        {
            m_newAmounts.Clear();

            foreach (var ability in abilities)
            {
                ability.FillManaOutcome(m_outcome);

                if (m_outcome.AnythingCouldHappen)
                    return false;
            }

            if (m_newAmounts.Count == 0)
            {
                return true;
            }
            else if (m_newAmounts.Count == 1)
            {
                ManaAmount newAmount = m_newAmounts[0];

                for (int i = 0; i < m_amounts.Count; i++)
                {
                    m_amounts[i] += newAmount;
                }
            }
            else
            {
                HashSet<ManaAmount> combinedAmounts = new HashSet<ManaAmount>();

                for (int i = 0; i < m_amounts.Count; i++)
                {
                    for (int j = 0; j < m_newAmounts.Count; j++)
                    {
                        combinedAmounts.Add(m_amounts[i] + m_newAmounts[j]);
                    }
                }

                m_amounts.Clear();
                m_amounts.AddRange(combinedAmounts);
            }

            return true;
        }

        private class Outcome : IManaAbilityOutcome
        {
            private readonly ManaAbilityEvaluator m_evaluator;

            public Outcome(ManaAbilityEvaluator evaluator)
            {
                m_evaluator = evaluator;
            }

            public bool AnythingCouldHappen { get; private set; }

            public void Add(ManaAmount amount)
            {
                Debug.Assert(amount.TotalAmount > 0, "Cannot add an empty ManaAmount");
                m_evaluator.m_newAmounts.Add(amount);
            }

            public void AddAny()
            {
                AnythingCouldHappen = true;
            }
        }
    }
}

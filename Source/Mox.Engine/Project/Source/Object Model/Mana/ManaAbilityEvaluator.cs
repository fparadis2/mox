using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    /// <summary>
    /// Takes a list of mana abilities and returns the possible (upperbound) mana amount combinations resulting from these abilities
    /// </summary>
    public class ManaAbilityEvaluator
    {
        private readonly List<ManaAmount> m_amounts = new List<ManaAmount>();
        private readonly List<ManaAmount> m_newAmounts = new List<ManaAmount>();

        public IEnumerable<ManaAmount> Amounts
        {
            get { return m_amounts; }
        }

        public ManaAbilityEvaluator(ManaAmount baseAmount)
        {
            m_amounts.Add(baseAmount);
        }

        public bool Consider(IEnumerable<Ability> abilities)
        {
            m_newAmounts.Clear();

            foreach (var ability in abilities)
            {
                var outcome = ability.ManaOutcome;
                Debug.Assert(outcome != null);

                var amounts = outcome.GetPossibleAmounts();
                if (amounts == null)
                    return false; // Null => Any

                int oldSize = m_newAmounts.Count;
                m_newAmounts.AddRange(amounts);
                if (oldSize == m_newAmounts.Count)
                    return false; // No possible amounts => Any
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
    }
}

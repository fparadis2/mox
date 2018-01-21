using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mox.Abilities
{
    public class ActionManaOutcome
    {
        private readonly List<ManaAmount> m_amounts = new List<ManaAmount>();
        public IReadOnlyList<ManaAmount> Amounts => m_amounts;
        public bool AnythingCanHappen = false;

        public bool IsEmpty
        {
            get { return m_amounts.Count == 0 || AnythingCanHappen; }
        }

        public bool Consider(Action action)
        {
            var outcome = new Outcome();
            action.FillManaOutcome(outcome);

            if (outcome.AnythingCouldHappen)
            {
                AnythingCanHappen = true;
                return false;
            }

            if (outcome.Amounts.Count == 0)
                return true;

            if (m_amounts.Count == 0)
                m_amounts.Add(new ManaAmount());

            if (outcome.Amounts.Count == 1)
            {
                ManaAmount newAmount = outcome.Amounts[0];

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
                    for (int j = 0; j < outcome.Amounts.Count; j++)
                    {
                        combinedAmounts.Add(m_amounts[i] + outcome.Amounts[j]);
                    }
                }

                m_amounts.Clear();
                m_amounts.AddRange(combinedAmounts);
            }

            return true;
        }

        #region Nested Types

        private class Outcome : IManaAbilityOutcome
        {
            private readonly List<ManaAmount> m_amounts = new List<ManaAmount>();

            public bool AnythingCouldHappen { get; private set; }
            public IReadOnlyList<ManaAmount> Amounts => m_amounts;

            public void Add(ManaAmount amount)
            {
                Debug.Assert(amount.TotalAmount > 0, "Cannot add an empty ManaAmount");
                m_amounts.Add(amount);
            }

            public void AddAny()
            {
                AnythingCouldHappen = true;
            }
        }

        #endregion
    }
}

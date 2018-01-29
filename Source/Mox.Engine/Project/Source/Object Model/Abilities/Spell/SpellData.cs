using System;
using System.Collections.Generic;

namespace Mox.Abilities
{
    [Serializable]
    internal struct CostResult
    {
        public Cost Cost;
        public object Result;
    }

    [Serializable]
    internal class SpellData
    {
        private readonly List<CostResult> m_results = new List<CostResult>();

        public object GetCostResult(Cost cost)
        {
            foreach (var result in m_results)
            {
                if (result.Cost == cost) // Todo: this comparison will fail through serialization...
                {
                    return result.Result;
                }
            }

            throw new InvalidOperationException("Cannot get the result for this cost. It either has not been played yet or is now invalid.");
        }

        public void SetCostResult(Cost cost, object result)
        {
            for (int i = 0; i < m_results.Count; i++)
            {
                if (m_results[i].Cost == cost) // Todo: this comparison will fail through serialization...
                {
                    var costResult = m_results[i];
                    costResult.Result = result;
                    m_results[i] = costResult;
                    return;
                }
            }

            m_results.Add(new CostResult { Cost = cost, Result = result });
        }
    }
}

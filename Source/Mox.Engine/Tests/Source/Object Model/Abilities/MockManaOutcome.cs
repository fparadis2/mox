using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class MockManaOutcome : IManaAbilityOutcome
    {
        public readonly List<ManaAmount> Amounts = new List<ManaAmount>();
        public bool AnythingCanHappen = false;

        public void Add(ManaAmount amount)
        {
            Amounts.Add(amount);
        }

        public void AddAny()
        {
            AnythingCanHappen = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class CounterAction : Action
    {
        public CounterAction(ObjectResolver spells)
        {
            Debug.Assert(spells != null);
            Spells = spells;
        }

        public ObjectResolver Spells { get; private set; }

        protected override void Resolve(Spell2 spell)
        {
            foreach (var spellToCounter in Spells.Resolve<Spell2>(spell))
            {
                spellToCounter.Counter();
            }
        }
    }
}

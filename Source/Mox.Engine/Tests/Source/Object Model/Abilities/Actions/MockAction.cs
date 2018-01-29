using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Flow;

namespace Mox.Abilities
{
    public class MockAction : Action
    {
        public Part Part
        {
            get;
            set;
        }

        public int ResolveCount;

        public System.Action Effect
        {
            get;
            set;
        }

        public override Part ResolvePart(Spell2 spell)
        {
            return Part ?? base.ResolvePart(spell);
        }

        protected override void Resolve(Spell2 spell)
        {
            base.Resolve(spell);

            ResolveCount++;
            Effect?.Invoke();
        }
    }
}

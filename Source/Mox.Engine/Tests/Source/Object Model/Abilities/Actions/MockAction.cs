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

        public override Part ResolvePart(SpellResolutionContext2 context)
        {
            return Part ?? base.ResolvePart(context);
        }

        protected override void Resolve(Game game, SpellResolutionContext2 context)
        {
            base.Resolve(game, context);

            ResolveCount++;
            Effect?.Invoke();
        }
    }
}

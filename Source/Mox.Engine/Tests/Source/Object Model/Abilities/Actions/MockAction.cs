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

        public override Part ResolvePart(Game game, SpellContext context)
        {
            return Part ?? base.ResolvePart(game, context);
        }

        protected override void Resolve(Game game, SpellContext context)
        {
            base.Resolve(game, context);

            ResolveCount++;
            Effect?.Invoke();
        }
    }
}

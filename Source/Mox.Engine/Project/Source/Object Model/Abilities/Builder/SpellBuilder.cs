using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Abilities
{
    [Serializable]
    public abstract class ObjectResolver
    {
        public abstract IEnumerable<GameObject> Resolve(Game game, SpellResolutionContext2 context);

        public IEnumerable<T> Resolve<T>(Game game, SpellResolutionContext2 context)
        {
            return Resolve(game, context).OfType<T>();
        }
    }

    public class TargetObjectResolver : ObjectResolver
    {
        public TargetObjectResolver(TargetCost targetCost)
        {
            TargetCost = targetCost;
        }

        public TargetCost TargetCost
        {
            get;
            private set;
        }

        public override IEnumerable<GameObject> Resolve(Game game, SpellResolutionContext2 context)
        {
            yield return TargetCost.Resolve(game);
        }
    }
}

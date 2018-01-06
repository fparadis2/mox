using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Flow;

namespace Mox.Abilities
{
    [Serializable]
    public abstract class ObjectResolver
    {
        public abstract IEnumerable<GameObject> Resolve(SpellResolutionContext context);

        public IEnumerable<T> Resolve<T>(SpellResolutionContext context)
        {
            return Resolve(context).OfType<T>();
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

        public override IEnumerable<GameObject> Resolve(SpellResolutionContext context)
        {
            yield return TargetCost.Resolve(context.Game);
        }
    }
}

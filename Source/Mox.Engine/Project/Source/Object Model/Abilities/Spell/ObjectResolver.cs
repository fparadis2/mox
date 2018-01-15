using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Abilities
{
#warning todo spell_v2 split this file

    [Serializable]
    public abstract class ObjectResolver
    {
        public abstract IEnumerable<GameObject> Resolve(Game game, SpellContext context);

        public IEnumerable<T> Resolve<T>(Game game, SpellContext context)
        {
            return Resolve(game, context).OfType<T>();
        }

        public static implicit operator ObjectResolver(GameObject o)
        {
            return new SingleObjectResolver(o);
        }

        public static readonly ObjectResolver SpellSource = new SpellSourceObjectResolver();
    }

    public class SingleObjectResolver : ObjectResolver
    {
        private readonly Resolvable<GameObject> m_object;

        public SingleObjectResolver(GameObject o)
        {
            m_object = o;
        }

        public override IEnumerable<GameObject> Resolve(Game game, SpellContext context)
        {
            yield return m_object.Resolve(game);
        }
    }

    public class SpellSourceObjectResolver : ObjectResolver
    {
        public override IEnumerable<GameObject> Resolve(Game game, SpellContext context)
        {
            yield return context.Ability.Resolve(game).Source;
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

        public override IEnumerable<GameObject> Resolve(Game game, SpellContext context)
        {
            yield return TargetCost.Resolve(game);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Abilities
{
#warning todo spell_v2 split this file

    public abstract class AmountResolver
    {
        public abstract int Resolve(Game game, SpellContext context);
    }

    public class ConstantAmountResolver : AmountResolver
    {
        public ConstantAmountResolver(int amount)
        {
            Amount = amount;
        }

        public int Amount { get; }

        public override int Resolve(Game game, SpellContext context)
        {
            return Amount;
        }
    }

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
        public static readonly ObjectResolver SpellController = new SpellControllerObjectResolver();
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

    public class MultipleObjectResolver : ObjectResolver
    {
        private readonly List<Resolvable<GameObject>> m_objects = new List<Resolvable<GameObject>>();

        public MultipleObjectResolver(IEnumerable<Resolvable<GameObject>> objects)
        {
            m_objects.AddRange(objects);
        }

        public MultipleObjectResolver(params Resolvable<GameObject>[] objects)
        {
            m_objects.AddRange(objects);
        }

        public override IEnumerable<GameObject> Resolve(Game game, SpellContext context)
        {
            foreach (var o in m_objects)
                yield return o.Resolve(game);
        }
    }

    public class SpellSourceObjectResolver : ObjectResolver
    {
        public override IEnumerable<GameObject> Resolve(Game game, SpellContext context)
        {
            yield return context.Ability.Resolve(game).Source;
        }
    }

    public class SpellControllerObjectResolver : ObjectResolver
    {
        public override IEnumerable<GameObject> Resolve(Game game, SpellContext context)
        {
            yield return context.Controller.Resolve(game);
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

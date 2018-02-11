using System;
using System.Collections.Generic;
using System.Linq;

namespace Mox.Abilities
{
#warning todo spell_v2 split this file

    public abstract class AmountResolver
    {
        public abstract int Resolve(Spell2 spell);

        public static implicit operator AmountResolver(int amount)
        {
            return new ConstantAmountResolver(amount);
        }
    }

    public class ConstantAmountResolver : AmountResolver
    {
        public ConstantAmountResolver(int amount)
        {
            Amount = amount;
        }

        public int Amount { get; }

        public override int Resolve(Spell2 spell)
        {
            return Amount;
        }
    }

    public abstract class ObjectResolver
    {
        public abstract IEnumerable<GameObject> Resolve(Spell2 spell);

        public IEnumerable<T> Resolve<T>(Spell2 spell)
        {
            return Resolve(spell).OfType<T>();
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

        public override IEnumerable<GameObject> Resolve(Spell2 spell)
        {
            yield return m_object.Resolve(spell.Manager);
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

        public override IEnumerable<GameObject> Resolve(Spell2 spell)
        {
            foreach (var o in m_objects)
                yield return o.Resolve(spell.Manager);
        }
    }

    public class SpellSourceObjectResolver : ObjectResolver
    {
        public override IEnumerable<GameObject> Resolve(Spell2 spell)
        {
            yield return spell.Ability.Source;
        }
    }

    public class SpellControllerObjectResolver : ObjectResolver
    {
        public override IEnumerable<GameObject> Resolve(Spell2 spell)
        {
            yield return spell.Controller;
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

        public override IEnumerable<GameObject> Resolve(Spell2 spell)
        {
            yield return TargetCost.Resolve(spell);
        }
    }

    public class FilterObjectResolver : ObjectResolver
    {
        public FilterObjectResolver(Filter filter)
        {
            Throw.IfNull(filter, "filter");
            Filter = filter;
        }

        public Filter Filter { get; }

        public override IEnumerable<GameObject> Resolve(Spell2 spell)
        {
            List<GameObject> objects = new List<GameObject>();
            Filter.EnumerateObjects(spell.Manager, spell.Controller, objects);
            return objects;
        }
    }
}

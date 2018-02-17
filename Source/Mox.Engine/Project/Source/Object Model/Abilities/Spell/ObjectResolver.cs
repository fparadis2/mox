using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mox.Abilities
{
    public abstract class ObjectResolver
    {
        public abstract IEnumerable<GameObject> Resolve(ISpellContext spell);
        public abstract bool Invalidate(PropertyBase property);

        public IEnumerable<T> Resolve<T>(ISpellContext spell)
        {
            return Resolve(spell).OfType<T>();
        }

        public static implicit operator ObjectResolver(GameObject o)
        {
            return new SingleObjectResolver(o);
        }

        public static implicit operator ObjectResolver(Filter f)
        {
            return new FilterObjectResolver(f);
        }

        public static readonly ObjectResolver SpellSource = new SpellSourceObjectResolver();
        public static readonly ObjectResolver SpellController = new SpellControllerObjectResolver();
        public static readonly ObjectResolver AttachedTo = new AttachedToObjectResolver();
    }

    public class SingleObjectResolver : ObjectResolver
    {
        private readonly Resolvable<GameObject> m_object;

        public SingleObjectResolver(GameObject o)
        {
            m_object = o;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return false;
        }

        public override IEnumerable<GameObject> Resolve(ISpellContext spell)
        {
            yield return m_object.Resolve(spell.Ability.Manager);
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

        public override IEnumerable<GameObject> Resolve(ISpellContext spell)
        {
            foreach (var o in m_objects)
                yield return o.Resolve(spell.Ability.Manager);
        }

        public override bool Invalidate(PropertyBase property)
        {
            return false;
        }
    }

    public class SpellSourceObjectResolver : ObjectResolver
    {
        public override IEnumerable<GameObject> Resolve(ISpellContext spell)
        {
            yield return spell.Ability.Source;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return false;
        }
    }

    public class SpellControllerObjectResolver : ObjectResolver
    {
        public override IEnumerable<GameObject> Resolve(ISpellContext spell)
        {
            yield return spell.Controller;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return property == Card.ControllerProperty;
        }
    }

    public class AttachedToObjectResolver : ObjectResolver
    {
        public override IEnumerable<GameObject> Resolve(ISpellContext spell)
        {
            var attachedTo = spell.Ability.Source.AttachedTo;

            if (attachedTo != null)
                yield return attachedTo;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return property == Card.AttachedToProperty;
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

        public override IEnumerable<GameObject> Resolve(ISpellContext spell)
        {
            Debug.Assert(spell.Spell != null, "Cannot resolve cost");
            yield return TargetCost.Resolve(spell.Spell);
        }

        public override bool Invalidate(PropertyBase property)
        {
            throw new InvalidOperationException("Not supported");
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

        public override IEnumerable<GameObject> Resolve(ISpellContext spell)
        {
            List<GameObject> objects = new List<GameObject>();
            Filter.EnumerateObjects(spell.Ability.Manager, spell.Controller, objects);
            return objects;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return Filter.Invalidate(property);
        }
    }
}

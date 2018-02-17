using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class TypeFilter : CardFilter
    {
        public TypeFilter(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
        
        public override bool Accept(GameObject o, Player controller)
        {
            Card card = (Card)o;
            return card.Is(Type);
        }

        public override bool Invalidate(PropertyBase property)
        {
            return property == Card.TypeProperty;
        }
    }

    public class NotTypeFilter : TypeFilter
    {
        public NotTypeFilter(Type type)
            : base(type)
        {
        }

        public override bool Accept(GameObject o, Player controller)
        {
            Card card = (Card)o;
            return !card.Is(Type);
        }
    }

    public class SubTypeFilter : CardFilter
    {
        public SubTypeFilter(SubType subtype)
        {
            SubType = subtype;
        }

        public SubType SubType { get; }

        public override bool Accept(GameObject o, Player controller)
        {
            Card card = (Card)o;
            return card.Is(SubType);
        }

        public override bool Invalidate(PropertyBase property)
        {
            return property == Card.SubTypesProperty;
        }
    }

    public class NotSubTypeFilter : SubTypeFilter
    {
        public NotSubTypeFilter(SubType subtype)
            : base(subtype)
        {
        }

        public override bool Accept(GameObject o, Player controller)
        {
            Card card = (Card)o;
            return !card.Is(SubType);
        }
    }

    public class SuperTypeFilter : CardFilter
    {
        public SuperTypeFilter(SuperType supertype)
        {
            SuperType = supertype;
        }

        public SuperType SuperType { get; }

        public override bool Accept(GameObject o, Player controller)
        {
            Card card = (Card)o;
            return card.Is(SuperType);
        }

        public override bool Invalidate(PropertyBase property)
        {
            return property == Card.SuperTypeProperty;
        }
    }

    public class NotSuperTypeFilter : SuperTypeFilter
    {
        public NotSuperTypeFilter(SuperType supertype)
            : base(supertype)
        {
        }

        public override bool Accept(GameObject o, Player controller)
        {
            Card card = (Card)o;
            return !card.Is(SuperType);
        }
    }
}

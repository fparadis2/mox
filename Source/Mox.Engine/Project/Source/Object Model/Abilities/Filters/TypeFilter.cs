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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public abstract class HandFilter : Filter
    {
        public override sealed FilterType FilterType => FilterType.Hand;

        public static readonly HandFilter Any = new AnyHandFilter();
    }

    public class AnyHandFilter : HandFilter
    {
        public override bool Accept(GameObject o, Player controller)
        {
            return true;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return property == Card.ZoneIdProperty || property == Card.ControllerProperty;
        }
    }
}

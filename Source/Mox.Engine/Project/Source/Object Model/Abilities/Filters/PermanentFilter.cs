using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public abstract class PermanentFilter : Filter
    {
        public override sealed FilterType FilterType => FilterType.Permanent;

        public static readonly PermanentFilter Any = new AnyPermanentFilter();
        public static readonly PermanentFilter AnyCreature = new AnyCreatureFilter();
        public static readonly Filter CreatureOrPlayer = new CreatureOrPlayerFilter();

        public static readonly ControlledByYouFilter ControlledByYou = new ControlledByYouFilter();
        public static readonly ControlledByOpponentsFilter ControlledByOpponents = new ControlledByOpponentsFilter();
    }

    public class AnyPermanentFilter : PermanentFilter
    {
        public override bool Accept(GameObject o, Player controller)
        {
            return true;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return property == Card.ZoneIdProperty;
        }
    }

    public class AnyCreatureFilter : PermanentFilter
    {
        public override bool Accept(GameObject o, Player controller)
        {
            Card card = (Card)o;
            return card.Is(Type.Creature);
        }

        public override bool Invalidate(PropertyBase property)
        {
            return 
                property == Card.ZoneIdProperty ||
                property == Card.TypeProperty;
        }
    }
}

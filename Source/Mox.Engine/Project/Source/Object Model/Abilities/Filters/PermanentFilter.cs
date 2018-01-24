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

        public override void EnumerateObjects(Game game, List<GameObject> result)
        {
            var type = FilterType;

            if (type.HasFlag(FilterType.Permanent))
            {
                foreach (Card card in game.Zones.Battlefield.AllCards)
                {
                    Consider(card, result);
                }
            }
        }

        public static readonly PermanentFilter Any = new AnyPermanentFilter();
        public static readonly PermanentFilter AnyCreature = new AnyCreatureFilter();
        public static readonly Filter CreatureOrPlayer = new CreatureOrPlayerFilter();
    }

    public class AnyPermanentFilter : PermanentFilter
    {
        public override bool Accept(GameObject o)
        {
            return true;
        }
    }

    public class AnyCreatureFilter : PermanentFilter
    {
        public override bool Accept(GameObject o)
        {
            Card card = (Card)o;
            return card.Is(Type.Creature);
        }
    }
}

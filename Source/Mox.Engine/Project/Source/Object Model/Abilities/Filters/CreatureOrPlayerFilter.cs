using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class CreatureOrPlayerFilter : Filter
    {
        public override FilterType FilterType => FilterType.Player | FilterType.Permanent;

        public override bool Accept(GameObject o, Player controller)
        {
            if (o is Card c)
            {
                return c.Is(Type.Creature);
            }

            return o is Player;
        }
    }
}

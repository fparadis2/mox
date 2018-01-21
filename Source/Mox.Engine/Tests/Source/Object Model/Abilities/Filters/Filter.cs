using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public enum FilterType
    {
        Player = 1,
        Permanent = 2,
    }

    public abstract class Filter
    {
        public abstract FilterType FilterType { get; }

        public abstract bool Accept(GameObject o);

        public virtual void EnumerateObjects(Game game, List<GameObject> result)
        {
            var type = FilterType;

            if (type.HasFlag(FilterType.Player))
            {
                foreach (Player player in game.Players)
                {
                    Consider(player, result);
                }
            }

            if (type.HasFlag(FilterType.Permanent))
            {
                foreach (Card card in game.Zones.Battlefield.AllCards)
                {
                    Consider(card, result);
                }
            }
        }

        protected void Consider(GameObject o, List<GameObject> result)
        {
            if (Accept(o))
                result.Add(o);
        }
    }

    public abstract class PlayerFilter : Filter
    {
        public override FilterType FilterType => FilterType.Player;

        public override void EnumerateObjects(Game game, List<GameObject> result)
        {
            var type = FilterType;

            if (type.HasFlag(FilterType.Player))
            {
                foreach (Player player in game.Players)
                {
                    Consider(player, result);
                }
            }
        }
    }
}

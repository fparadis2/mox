using System;
using System.Collections.Generic;

namespace Mox.Abilities
{
    public enum FilterType
    {
        None = 0,
        Player = 1,
        Permanent = 2,
        All = ~0
    }

    public abstract class Filter
    {
        public abstract FilterType FilterType { get; }

        public abstract bool Accept(GameObject o, Player controller);
        public abstract bool Invalidate(PropertyBase property);

        public void EnumerateObjects(Game game, Player controller, List<GameObject> result)
        {
            var type = FilterType;

            if (type.HasFlag(FilterType.Player))
            {
                foreach (Player player in game.Players)
                {
                    Consider(player, controller, result);
                }
            }

            if (type.HasFlag(FilterType.Permanent))
            {
                foreach (Card card in game.Zones.Battlefield.AllCards)
                {
                    Consider(card, controller, result);
                }
            }
        }

        protected void Consider(GameObject o, Player controller, List<GameObject> result)
        {
            if (Accept(o, controller))
                result.Add(o);
        }

        #region Operators

        public static Filter operator &(Filter a, Filter b)
        {
            return new AndFilter { a, b };
        }

        #endregion
    }
}

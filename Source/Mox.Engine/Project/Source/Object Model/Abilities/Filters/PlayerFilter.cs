using System.Collections.Generic;

namespace Mox.Abilities
{
    public abstract class PlayerFilter : Filter
    {
        public override sealed FilterType FilterType => FilterType.Player;

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

        public static readonly PlayerFilter Any = new AnyPlayerFilter();
    }

    public class AnyPlayerFilter : PlayerFilter
    {
        public override bool Accept(GameObject o)
        {
            return true;
        }
    }
}

using System.Collections.Generic;

namespace Mox.Abilities
{
    public abstract class PlayerFilter : Filter
    {
        public override sealed FilterType FilterType => FilterType.Player;

        public static readonly PlayerFilter Any = new AnyPlayerFilter();
    }

    public class AnyPlayerFilter : PlayerFilter
    {
        public override bool Accept(GameObject o, Player controller)
        {
            return true;
        }

        public override bool Invalidate(PropertyBase property)
        {
            return false;
        }
    }
}

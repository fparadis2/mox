namespace Mox.Abilities
{
    public abstract class CardFilter : Filter
    {
        public override FilterType FilterType => FilterType.Permanent | FilterType.Hand;

        public static TypeFilter OfType(Type type) { return new TypeFilter(type); }
        public static ColorFilter OfColor(Color color) { return new ColorFilter(color); }
    }
}

namespace Mox.Abilities
{
    public abstract class CardFilter : Filter
    {
        public override FilterType FilterType => FilterType.Permanent | FilterType.Hand;

        public static TypeFilter WithType(Type type) { return new TypeFilter(type); }
        public static ColorFilter WithColor(Color color) { return new ColorFilter(color); }
        public static SubTypeFilter WithSubType(SubType subtype) { return new SubTypeFilter(subtype); }
        public static SuperTypeFilter WithSuperType(SuperType supertype) { return new SuperTypeFilter(supertype); }

        public static NotTypeFilter NotWithType(Type type) { return new NotTypeFilter(type); }
        public static NotColorFilter NotWithColor(Color color) { return new NotColorFilter(color); }
        public static NotSubTypeFilter NotWithSubType(SubType subtype) { return new NotSubTypeFilter(subtype); }
        public static NotSuperTypeFilter NotWithSuperType(SuperType supertype) { return new NotSuperTypeFilter(supertype); }
    }
}

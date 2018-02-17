namespace Mox.Abilities
{
    public class ColorFilter : CardFilter
    {
        public ColorFilter(Color color)
        {
            Color = color;
        }

        public Color Color { get; }

        public override bool Accept(GameObject o, Player controller)
        {
            Card card = (Card)o;
            return card.Color.HasFlag(Color);
        }

        public override bool Invalidate(PropertyBase property)
        {
            return property == Card.ColorProperty;
        }
    }

    public class NotColorFilter : ColorFilter
    {
        public NotColorFilter(Color color) 
            : base(color)
        {
        }

        public override bool Accept(GameObject o, Player controller)
        {
            Card card = (Card)o;
            return !card.Color.HasFlag(Color);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Flow;

namespace Mox.Abilities
{
    public class GainManaAction : Action
    {
        private readonly Color m_color;

        public GainManaAction(Color color)
        {
            m_color = color;
        }

        public Color Color
        {
            get { return m_color; }
        }

        public override Part ResolvePart(Game game, SpellContext context)
        {
            if (m_color.HasMoreThanOneColor())
            {
                return new GainManaChoicePart(context.Controller, m_color);
            }
            else
            {
                return new GainManaPart(context.Controller, m_color);
            }
        }

        #region Inner Types

        private class GainManaChoicePart : ChoicePart<Color>
        {
            private static readonly Color[] ms_colors = new[] { Color.White, Color.Blue, Color.Black, Color.Red, Color.Green };

            private readonly Color m_color;

            public GainManaChoicePart(Resolvable<Player> player, Color color)
                : base(player)
            {
                m_color = color;
            }

            public override Choice GetChoice(Sequencer sequencer)
            {
                return new GainManaChoice(ResolvablePlayer, EnumerateColors());
            }

            public override Part Execute(Context context, Color color)
            {
                if (!ValidateResult(color))
                    return this;

                return new GainManaPart(ResolvablePlayer, color);
            }

            private bool ValidateResult(Color color)
            {
                if (!m_color.HasFlag(color))
                    return false;

                return !color.HasMoreThanOneColor();
            }

            private IEnumerable<Color> EnumerateColors()
            {
                foreach (var singleColor in ms_colors)
                {
                    if (m_color.HasFlag(singleColor))
                    {
                        yield return singleColor;
                    }
                }
            }
        }

        private class GainManaPart : PlayerPart
        {
            private readonly Color m_color;

            public GainManaPart(Resolvable<Player> player, Color color)
                : base(player)
            {
                m_color = color;
            }

            public override Part Execute(Context context)
            {
                GetPlayer(context).ManaPool[m_color] += 1;
                return null;
            }
        }

        #endregion
    }
}

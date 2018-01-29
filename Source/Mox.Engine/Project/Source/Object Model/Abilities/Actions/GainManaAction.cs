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
        private static readonly Color[] ms_colors = new[] { Color.White, Color.Blue, Color.Black, Color.Red, Color.Green };

        private readonly Color m_color;

        public GainManaAction(Color color)
        {
            m_color = color;
        }

        public Color Color
        {
            get { return m_color; }
        }

        public override void FillManaOutcome(IManaAbilityOutcome outcome)
        {
            if (m_color.HasMoreThanOneColor())
            {
                foreach (var color in EnumerateColors(m_color))
                {
                    ManaAmount amount = new ManaAmount();
                    amount.Add(color, 1);
                    outcome.Add(amount);
                }
            }
            else
            {
                ManaAmount amount = new ManaAmount();
                amount.Add(m_color, 1);
                outcome.Add(amount);
            }
        }

        public override Part ResolvePart(Spell2 spell)
        {
            if (m_color.HasMoreThanOneColor())
            {
                return new GainManaChoicePart(spell.Controller, m_color);
            }
            else
            {
                return new GainManaPart(spell.Controller, m_color);
            }
        }

        private static IEnumerable<Color> EnumerateColors(Color color)
        {
            foreach (var singleColor in ms_colors)
            {
                if (color.HasFlag(singleColor))
                {
                    yield return singleColor;
                }
            }
        }

        #region Inner Types

        private class GainManaChoicePart : ChoicePart<Color>
        {
            private readonly Color m_color;

            public GainManaChoicePart(Resolvable<Player> player, Color color)
                : base(player)
            {
                m_color = color;
            }

            public override Choice GetChoice(Sequencer sequencer)
            {
                return new GainManaChoice(ResolvablePlayer, EnumerateColors(m_color));
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

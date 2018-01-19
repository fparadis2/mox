using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mox.Flow
{
    [Serializable, AI.ChoiceEnumerator(typeof(AI.ChoiceEnumerators.GainManaChoiceEnumerator))]
    public class GainManaChoice : Choice
    {
        #region Variables

        private readonly List<Color> m_colors = new List<Color>();

        #endregion

        #region Constructor

        public GainManaChoice(Resolvable<Player> player, IEnumerable<Color> colors)
            : base(player)
        {
            m_colors.AddRange(colors);
            Debug.Assert(m_colors.Count > 1);
        }

        #endregion

        #region Properties

        public IReadOnlyList<Color> Colors
        {
            get { return m_colors; }
        }

        public override object DefaultValue
        {
            get
            {
                return m_colors[0];
            }
        }

        #endregion
    }
}
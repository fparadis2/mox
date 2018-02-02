using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mox.Flow
{
    [Serializable, AI.ChoiceEnumerator(typeof(AI.ChoiceEnumerators.GainManaChoiceEnumerator))]
    public class GainManaChoice : Choice
    {
        #region Variables

        private readonly List<ManaAmount> m_amounts = new List<ManaAmount>();

        #endregion

        #region Constructor

        public GainManaChoice(Resolvable<Player> player, IEnumerable<ManaAmount> amounts)
            : base(player)
        {
            m_amounts.AddRange(amounts);
            Debug.Assert(m_amounts.Count > 1);
        }

        #endregion

        #region Properties

        public IReadOnlyList<ManaAmount> Amounts
        {
            get { return m_amounts; }
        }

        public override object DefaultValue
        {
            get
            {
                return m_amounts[0];
            }
        }

        #endregion
    }
}
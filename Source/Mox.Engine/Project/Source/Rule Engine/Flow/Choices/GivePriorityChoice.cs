using System;

namespace Mox.Flow
{
    [Serializable, AI.ChoiceEnumerator(typeof(AI.ChoiceEnumerators.GivePriorityChoiceEnumerator))]
    public class GivePriorityChoice : Choice
    {
        #region Constructor

        public GivePriorityChoice(Resolvable<Player> player)
            : base(player)
        {
        }

        #endregion

        #region Overrides of Choice

        public override object DefaultValue
        {
            get { return null; }
        }

        #endregion
    }
}
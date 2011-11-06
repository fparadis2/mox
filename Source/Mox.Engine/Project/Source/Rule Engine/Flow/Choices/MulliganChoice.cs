using System;

namespace Mox.Flow
{
    public class MulliganChoice : Choice
    {
        #region Constructor

        public MulliganChoice(Resolvable<Player> player)
            : base(player)
        {
        }

        #endregion

        #region Overrides of Choice

        public override object DefaultValue
        {
            get { return false; }
        }

        #endregion
    }
}
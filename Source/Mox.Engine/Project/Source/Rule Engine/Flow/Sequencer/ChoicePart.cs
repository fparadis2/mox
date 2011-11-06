using System;

namespace Mox.Flow
{
    internal interface IChoicePart
    {
        Choice GetChoice(Game game);
    }

    public abstract class ChoicePart<TResult> : PlayerPart, IChoicePart
    {
        #region Constructor

        protected ChoicePart(Player player)
            : base(player)
        {
        }

        #endregion

        #region Methods

        public abstract Choice GetChoice(Game game);

        public override sealed NewPart Execute(Context context)
        {
            return Execute(context, (TResult)context.ChoiceResult);
        }

        public abstract NewPart Execute(Context context, TResult choice);

        #endregion
    }
}

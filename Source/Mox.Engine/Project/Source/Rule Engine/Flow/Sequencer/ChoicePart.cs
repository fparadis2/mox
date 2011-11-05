using System;

namespace Mox.Flow
{
    internal interface IChoicePart
    {}

    public abstract class ChoicePart<TChoice> : NewPart, IChoicePart
    {
        #region Methods

        public override sealed NewPart Execute(Context context)
        {
            return Execute(context, (TChoice)context.ChoiceResult);
        }

        public abstract NewPart Execute(Context context, TChoice choice);

        #endregion
    }
}

using System;

namespace Mox.Flow
{
    public interface IChoicePart
    {
        Choice GetChoice(Sequencer sequencer);
    }

    public abstract class ChoicePart<TResult> : PlayerPart, IChoicePart
    {
        #region Constructor

        protected ChoicePart(Resolvable<Player> player)
            : base(player)
        {
        }

        #endregion

        #region Methods

        public abstract Choice GetChoice(Sequencer sequencer);

        public override sealed Part Execute(Context context)
        {
            TResult result = this.PopChoiceResult<TResult>(context);
            return Execute(context, result);
        }

        public abstract Part Execute(Context context, TResult choice);

        #endregion
    }

    public static class ChoicePartExtensions
    {
        private const string ChoiceResultToken = "ChoiceResult";

        public static TResult PopChoiceResult<TResult>(this IChoicePart part, Part.Context context)
        {
            return context.PopArgument<TResult>(ChoiceResultToken);
        }

        public static void PushChoiceResult<TResult>(this IChoicePart part, Part.Context context, TResult result)
        {
            context.PushArgument(result, ChoiceResultToken);
        }
    }
}

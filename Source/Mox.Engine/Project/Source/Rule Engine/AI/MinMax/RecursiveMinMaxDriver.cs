using System.Collections.Generic;
using Mox.Flow;

namespace Mox.AI
{
    public class RecursiveMinMaxDriver : MinMaxDriver
    {
        #region Constructor

        public RecursiveMinMaxDriver(AIEvaluationContext context, ICancellable cancellable)
            : base(context, cancellable)
        {
        }

        #endregion

        #region Methods

        public override bool RunWithChoice(Sequencer sequencer, Choice theChoice, object choiceResult)
        {
            return RunWithChoiceImpl(sequencer, theChoice, choiceResult, true);
        }

        private bool RunWithChoiceImpl(Sequencer sequencer, Choice theChoice, object choiceResult, bool isMaximizingPlayer)
        {
            using (var choiceScope = BeginChoice(sequencer.Game, isMaximizingPlayer, choiceResult, theChoice.GetType().Name))
            {
                if (RunOnce(sequencer, new AIDecisionMaker(choiceResult)))
                {
                    Run(sequencer);
                }

                if (!choiceScope.End())
                {
                    return false;
                }
            }

            return true;
        }

        public override void Run(Sequencer sequencer)
        {
            RunImpl(sequencer);
        }

        protected override void TryChoices(Sequencer sequencer, Choice theChoice, bool isMaximizingPlayer, IEnumerable<object> choices)
        {
            foreach (var choiceResult in choices)
            {
                Sequencer clonedSequencer = sequencer.Clone();

                if (!RunWithChoiceImpl(clonedSequencer, theChoice, choiceResult, isMaximizingPlayer) || IsCancelled)
                {
                    break;
                }
            }
        }

        #endregion
    }
}
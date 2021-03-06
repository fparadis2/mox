﻿using System.Collections.Generic;
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

        public override void RunWithChoice(Sequencer sequencer, Choice theChoice, object choiceResult)
        {
            RunWithChoiceImpl(sequencer, theChoice, choiceResult);
        }

        private bool RunWithChoiceImpl(Sequencer sequencer, Choice theChoice, object choiceResult)
        {
            using (var choiceScope = BeginChoice(sequencer.Game, choiceResult, theChoice.GetType().Name))
            {
                if (RunOnce(sequencer, new AIDecisionMaker(choiceResult)))
                {
                    Run(sequencer);
                }

                return choiceScope.End();
            }
        }

        public override void Run(Sequencer sequencer)
        {
            RunImpl(sequencer);
        }

        protected override void TryChoices(Sequencer sequencer, Choice theChoice, IEnumerable<object> choices)
        {
            foreach (var choiceResult in choices)
            {
                Sequencer clonedSequencer = sequencer.Clone();

                if (!RunWithChoiceImpl(clonedSequencer, theChoice, choiceResult) || IsCancelled)
                {
                    break;
                }
            }
        }

        #endregion
    }
}
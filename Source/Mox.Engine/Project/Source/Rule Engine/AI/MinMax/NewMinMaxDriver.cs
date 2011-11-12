using System;
using System.Linq;
using Mox.Flow;

namespace Mox.AI
{
    public class NewMinMaxDriver
    {
        #region Variables

        private static readonly NullDecisionMaker ms_nullDecisionMaker = new NullDecisionMaker();

        private readonly AIEvaluationContext m_context;
        private readonly ICancellable m_cancellable;

        #endregion

        #region Constructor

        public NewMinMaxDriver(AIEvaluationContext context, ICancellable cancellable)
        {
            m_context = context;
            m_cancellable = cancellable;
        }

        #endregion

        #region Methods

        public bool RunWithChoice(NewSequencer sequencer, Choice theChoice, object choiceResult)
        {
            return RunWithChoiceImpl(sequencer, theChoice, choiceResult, true);
        }

        private bool RunWithChoiceImpl(NewSequencer sequencer, Choice theChoice, object choiceResult, bool isMaximizingPlayer)
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

        public void Run(NewSequencer sequencer)
        {
            if (sequencer.IsEmpty || m_context.Algorithm.IsTerminal(m_context.Tree, sequencer.Game))
            {
                Evaluate(sequencer.Game);
                return;
            }

            while (!m_cancellable.Cancel)
            {
                if (sequencer.IsEmpty)
                {
                    Evaluate(sequencer.Game);
                    return;
                }

                var nextPart = sequencer.NextPart;
                var choicePart = nextPart as IChoicePart;

                if (choicePart != null)
                {
                    Choice theChoice = choicePart.GetChoice(sequencer);
                    var choiceEnumerator = m_context.ChoiceEnumeratorProvider.GetEnumerator(theChoice);
                    
                    var choices = choiceEnumerator.EnumerateChoices(sequencer.Game, theChoice).ToList();

					if (choices.Count == 0)
					{
					    Discard();
                        return;
					}

                    Player player = theChoice.Player.Resolve(sequencer.Game);
                    bool isMaximizingPlayer = m_context.Algorithm.IsMaximizingPlayer(player);

                    foreach (var choiceResult in choices)
                    {
                        NewSequencer clonedSequencer = sequencer.Clone();

                        if (!RunWithChoiceImpl(clonedSequencer, theChoice, choiceResult, isMaximizingPlayer) || m_cancellable.Cancel)
                        {
                            break;
                        }
                    }

                    return;
                }

                if (!RunOnce(sequencer, ms_nullDecisionMaker))
                {
                    return;
                }
            }
        }

        private bool RunOnce(NewSequencer sequencer, IChoiceDecisionMaker god)
        {
            var result = sequencer.RunOnce(god);

            switch (result)
            {
                case SequencerResult.Continue:
                case SequencerResult.Stop:
                    return true;

                case SequencerResult.Retry:
                    Discard();
                    return false;

                default:
                    throw new NotImplementedException();
            }
        }

        private void Discard()
        {
            m_context.Tree.Discard();
        }

        private void Evaluate(Game game)
        {
            m_context.Tree.Evaluate(m_context.Algorithm.ComputeHeuristic(game, true));
        }

        private ChoiceScope BeginChoice(Game game, bool isMaximizingPlayer, object choice, string debugInfo)
        {
            return new ChoiceScope(game, m_context.Tree, isMaximizingPlayer, choice, debugInfo);
        }

        #endregion

        #region Inner Types

        private class ChoiceScope : IDisposable
        {
            private const string TransactionToken = "MinMaxChoice";

            private readonly IMinimaxTree m_tree;
            private readonly Game m_game;

            public ChoiceScope(Game game, IMinimaxTree tree, bool isMaximizingPlayer, object choice, string debugInfo)
            {
                m_game = game;
                m_tree = tree;

                m_tree.BeginNode(isMaximizingPlayer, choice, debugInfo);
                m_game.Controller.BeginTransaction(TransactionToken);
            }

            public void Dispose()
            {
                m_game.Controller.EndTransaction(true, TransactionToken);
            }

            public bool End()
            {
                return m_tree.EndNode();
            }
        }

        private class NullDecisionMaker : IChoiceDecisionMaker
        {
            #region Implementation of IChoiceDecisionMaker

            public object MakeChoiceDecision(NewSequencer sequencer, Choice choice)
            {
                throw new InvalidProgramException("Not supposed to make a choice");
            }

            #endregion
        }

        private class AIDecisionMaker : IChoiceDecisionMaker
        {
            private readonly object m_choiceResult;

            public AIDecisionMaker(object choiceResult)
            {
                m_choiceResult = choiceResult;
            }

            public object MakeChoiceDecision(NewSequencer sequencer, Choice choice)
            {
                return m_choiceResult;
            }
        }

        #endregion
    }
}

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

        #endregion

        #region Constructor

        public NewMinMaxDriver(AIEvaluationContext context)
        {
            m_context = context;
        }

        #endregion

        #region Methods

        public void RunWithChoice(NewSequencer sequencer, object choice)
        {
            sequencer.RunOnce(new AIDecisionMaker(choice));
            Run(sequencer);
        }

        private void Run(NewSequencer sequencer)
        {
            if (m_context.Algorithm.IsTerminal(m_context.Tree, sequencer.Game))
            {
                return;
            }

            while (!sequencer.IsEmpty)
            {
                var nextPart = sequencer.NextPart;
                var choicePart = nextPart as IChoicePart;

                if (choicePart != null)
                {
                    Choice theChoice = choicePart.GetChoice(sequencer); // Hmm
                    var choiceEnumerator = m_context.ChoiceEnumeratorProvider.GetEnumerator(theChoice);
                    var choices = choiceEnumerator.EnumerateChoices(sequencer.Game, theChoice).ToList();

                    Player player = theChoice.Player.Resolve(sequencer.Game);
                    bool isMaximizingPlayer = m_context.Algorithm.IsMaximizingPlayer(player);

					if (choices.Count == 0)
					{
					    Discard();
                        return;
					}

                    foreach (var choice in choices)
                    {
                        NewSequencer clonedSequencer = sequencer.Clone();
                        using (var choiceScope = BeginChoice(clonedSequencer.Game, isMaximizingPlayer, choice, choice.GetType().Name))
                        {
                            RunWithChoice(clonedSequencer, choice);

                            if (!choiceScope.End())
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var result = sequencer.RunOnce(ms_nullDecisionMaker);

                    switch (result)
                    {
                        case SequencerResult.Continue:
                        case SequencerResult.Stop:
                            break;

                        case SequencerResult.Retry:
                            Discard();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        private void Discard()
        {
            m_context.Tree.Discard();
        }

        private ChoiceScope BeginChoice(Game game, bool isMaximizingPlayer, object choice, string debugInfo)
        {
            return new ChoiceScope(game, m_context.Tree, isMaximizingPlayer, choice, debugInfo);
        }

        #endregion

        #region Inner Types

        private class ChoiceScope : IDisposable
        {
            private readonly IMinimaxTree m_tree;
            private readonly Game m_game;

            public ChoiceScope(Game game, IMinimaxTree tree, bool isMaximizingPlayer, object choice, string debugInfo)
            {
                m_game = game;
                m_tree = tree;

                m_tree.BeginNode(isMaximizingPlayer, choice, debugInfo);
                m_game.Controller.BeginTransaction();
            }

            public void Dispose()
            {
                m_game.Controller.EndTransaction(true);
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

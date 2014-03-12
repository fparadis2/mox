using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mox.Flow;

namespace Mox.AI
{
    public interface ICancellable
    {
        bool Cancel { get; }
    }

    public abstract class MinMaxDriver
    {
        #region Variables

        protected static readonly NullDecisionMaker ms_nullDecisionMaker = new NullDecisionMaker();

        private readonly AIEvaluationContext m_context;
        private readonly ICancellable m_cancellable;

        #endregion

        #region Constructor

        protected MinMaxDriver(AIEvaluationContext context, ICancellable cancellable)
        {
            m_context = context;
            m_cancellable = cancellable;
        }

        #endregion

        #region Properties

        protected bool IsCancelled
        {
            get { return m_cancellable.Cancel; }
        }

        protected IMinMaxAlgorithm Algorithm
        {
            get { return m_context.Algorithm; }
        }

        protected IChoiceEnumeratorProvider ChoiceEnumeratorProvider
        {
            get { return m_context.ChoiceEnumeratorProvider; }
        }

        #endregion

        #region Methods

        public abstract void RunWithChoice(Sequencer sequencer, Choice theChoice, object choiceResult);

        public abstract void Run(Sequencer sequencer);

        protected abstract void TryChoices(Sequencer sequencer, Choice theChoice, bool isMaximizingPlayer, IEnumerable<object> choices);

        protected void RunImpl(Sequencer sequencer)
        {
            while (!IsCancelled)
            {
                if (IsTerminal(sequencer))
                {
                    Evaluate(sequencer.Game);
                    return;
                }

                var nextPart = sequencer.NextPart;

                var choicePart = nextPart as IChoicePart;
                if (choicePart != null)
                {
                    Choice theChoice = choicePart.GetChoice(sequencer);
                    var choiceEnumerator = ChoiceEnumeratorProvider.GetEnumerator(theChoice);

                    var choices = choiceEnumerator.EnumerateChoices(sequencer.Game, theChoice).ToList();

                    if (choices.Count == 0)
                    {
                        Discard();
                        return;
                    }

                    Player player = theChoice.Player.Resolve(sequencer.Game);
                    bool isMaximizingPlayer = Algorithm.IsMaximizingPlayer(player);

                    TryChoices(sequencer, theChoice, isMaximizingPlayer, choices);
                    return;
                }

                var transactionPart = nextPart as TransactionPart;
                if (transactionPart != null)
                {
                    sequencer.Skip();
                    if (!HandleTransactionPart(sequencer.Game, transactionPart))
                    {
                        Discard();
                        return;
                    }
                    continue;
                }

                if (!RunOnce(sequencer, ms_nullDecisionMaker))
                {
                    return;
                }
            }
        }

        protected bool RunOnce(Sequencer sequencer, IChoiceDecisionMaker god)
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

        private static bool HandleTransactionPart(Game game, TransactionPart transactionPart)
        {
            transactionPart.Simulate(game);

            EndTransactionPart endTransactionPart = transactionPart as EndTransactionPart;
            if (endTransactionPart != null)
            {
                if (endTransactionPart.Rollback)
                {
                    return false;
                }
            }

            return true;
        }

        protected void Discard()
        {
            m_context.Tree.Discard();
        }

        protected void Evaluate(Game game)
        {
            var score = m_context.Algorithm.ComputeHeuristic(game, true);
            m_context.Tree.Evaluate(score);
        }

        private bool IsTerminal(Sequencer sequencer)
        {
            if (sequencer.IsEmpty)
                return true;

            if (sequencer.Parts.Any(p => p is IUninterruptiblePart))
                return false;

            if (TransactionPart.IsInTransaction(sequencer.Game))
                return false;

            return m_context.Algorithm.IsTerminal(m_context.Tree, sequencer.Game);
        }

        protected ChoiceScope BeginChoice(Game game, bool isMaximizingPlayer, object choice, string debugInfo)
        {
            return new ChoiceScope(m_context.Tree, game, isMaximizingPlayer, choice, debugInfo);
        }

        #endregion

        #region Inner Types

        protected class ChoiceScope : IDisposable
        {
            private const string TransactionToken = "MinMaxChoice";

            private readonly IMinimaxTree m_tree;
            private readonly Game m_game;

            public ChoiceScope(IMinimaxTree tree, Game game, bool isMaximizingPlayer, object choice, string debugInfo)
            {
                m_tree = tree;
                m_game = game;

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

        protected class NullDecisionMaker : IChoiceDecisionMaker
        {
            #region Implementation of IChoiceDecisionMaker

            public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
            {
                throw new InvalidProgramException("Not supposed to make a choice");
            }

            #endregion
        }

        protected class AIDecisionMaker : IChoiceDecisionMaker
        {
            private readonly object m_choiceResult;

            public AIDecisionMaker(object choiceResult)
            {
                m_choiceResult = choiceResult;
            }

            public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
            {
                return m_choiceResult;
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mox.Flow;

namespace Mox.AI
{
    public abstract class NewMinMaxDriver
    {
        #region Variables

        protected static readonly NullDecisionMaker ms_nullDecisionMaker = new NullDecisionMaker();

        private readonly AIEvaluationContext m_context;
        private readonly ICancellable m_cancellable;

        #endregion

        #region Constructor

        protected NewMinMaxDriver(AIEvaluationContext context, ICancellable cancellable)
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

        public abstract bool RunWithChoice(NewSequencer sequencer, Choice theChoice, object choiceResult);

        public abstract void Run(NewSequencer sequencer);

        protected abstract void TryChoices(NewSequencer sequencer, Choice theChoice, bool isMaximizingPlayer, IEnumerable<object> choices);

        protected void RunImpl(NewSequencer sequencer)
        {
            while (!IsCancelled)
            {
                if (sequencer.IsEmpty || IsTerminal(sequencer.Game))
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

        protected bool RunOnce(NewSequencer sequencer, IChoiceDecisionMaker god)
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

        protected bool IsTerminal(Game game)
        {
            return !TransactionPart.IsInTransaction(game) && m_context.Algorithm.IsTerminal(m_context.Tree, game);
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

            public object MakeChoiceDecision(NewSequencer sequencer, Choice choice)
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

            public object MakeChoiceDecision(NewSequencer sequencer, Choice choice)
            {
                return m_choiceResult;
            }
        }

        #endregion
    }

    public class NewRecursiveMinMaxDriver : NewMinMaxDriver
    {
        #region Constructor

        public NewRecursiveMinMaxDriver(AIEvaluationContext context, ICancellable cancellable)
            : base(context, cancellable)
        {
        }

        #endregion

        #region Methods

        public override bool RunWithChoice(NewSequencer sequencer, Choice theChoice, object choiceResult)
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

        public override void Run(NewSequencer sequencer)
        {
            RunImpl(sequencer);
        }

        protected override void TryChoices(NewSequencer sequencer, Choice theChoice, bool isMaximizingPlayer, IEnumerable<object> choices)
        {
            foreach (var choiceResult in choices)
            {
                NewSequencer clonedSequencer = sequencer.Clone();

                if (!RunWithChoiceImpl(clonedSequencer, theChoice, choiceResult, isMaximizingPlayer) || IsCancelled)
                {
                    break;
                }
            }
        }

        #endregion
    }

    public class NewIterativeMinMaxDriver : NewMinMaxDriver
    {
        #region Variables

        private readonly ChoiceRoundStack m_pendingChoices = new ChoiceRoundStack();

        #endregion

        #region Constructor

        public NewIterativeMinMaxDriver(AIEvaluationContext context, ICancellable cancellable)
            : base(context, cancellable)
        {
        }

        #endregion

        #region Methods

        #endregion

        #region Overrides of NewMinMaxDriver

        public override bool RunWithChoice(NewSequencer sequencer, Choice theChoice, object choiceResult)
        {
            throw new NotImplementedException();
        }

        public override void Run(NewSequencer sequencer)
        {
            while (!IsCancelled)
            {
                ChoiceToTry choice = m_pendingChoices.Pop(this);

                if (choice == null)
                {
                    break;
                }

                if (RunOnce(sequencer, new AIDecisionMaker(choice.ChoiceResult)))
                {
                    RunImpl(choice.Sequencer);
                }
            }

            m_pendingChoices.Clear();
        }

        protected override void TryChoices(NewSequencer sequencer, Choice theChoice, bool isMaximizingPlayer, IEnumerable<object> choices)
        {
            ChoiceRound choiceRound = m_pendingChoices.PushChoiceRound(isMaximizingPlayer, theChoice.GetType().Name);

            // It's important that the first choices be evaluated first because choices are pre-sorted and that helps with pruning the tree.
            foreach (object choice in choices)
            {
                choiceRound.PushChoice(sequencer, choice);
            }
        }

        #endregion

        #region Inner Types

        private class ChoiceToTry
        {
            #region Variables

            private readonly object m_choice;
            private readonly NewSequencer m_sequencer;
            private ChoiceScope m_scope;

            #endregion

            #region Constructor

            public ChoiceToTry(NewSequencer sequencer, object choice)
            {
                m_sequencer = sequencer.Clone();
                m_choice = choice;
            }

            #endregion

            #region Properties

            public object ChoiceResult
            {
                get { return m_choice; }
            }

            public NewSequencer Sequencer
            {
                get { return m_sequencer; }
            }

            #endregion

            #region Methods

            public void Begin(NewIterativeMinMaxDriver driver, bool isMaximizing, string debugInfo)
            {
                Debug.Assert(m_scope == null);
                m_scope = driver.BeginChoice(m_sequencer.Game, isMaximizing, m_choice, debugInfo);
            }

            public bool End()
            {
                return m_scope.End();
            }

            #endregion
        }

        private class ChoiceRound
        {
            #region Variables

            private readonly bool m_isMaximizing;
            private readonly string m_debugInfo;

            private readonly Queue<ChoiceToTry> m_choicesToTry = new Queue<ChoiceToTry>();
            private ChoiceToTry m_currentChoice;

            public ChoiceRound(bool isMaximizing, string debugInfo)
            {
                m_isMaximizing = isMaximizing;
                m_debugInfo = debugInfo;
            }

            internal bool End()
            {
                if (m_currentChoice != null)
                {
                    ChoiceToTry currentChoice = m_currentChoice;
                    m_currentChoice = null;
                    return currentChoice.End();
                }

                return true;
            }

            #endregion

            #region Properties

            private bool IsMaximizing
            {
                get { return m_isMaximizing; }
            }

            private string DebugInfo
            {
                get { return m_debugInfo; }
            }

            public bool IsEmpty
            {
                get { return !m_choicesToTry.Any(); }
            }

            #endregion

            #region Methods

            public void PushChoice(NewSequencer sequencer, object choice)
            {
                ChoiceToTry choiceTotry = new ChoiceToTry(sequencer, choice);
                m_choicesToTry.Enqueue(choiceTotry);
            }

            internal bool Pop(NewIterativeMinMaxDriver driver, out ChoiceToTry choice)
            {
                if (!End())
                {
                    choice = null;
                    return false;
                }

                m_currentChoice = m_choicesToTry.Dequeue();
                m_currentChoice.Begin(driver, IsMaximizing, DebugInfo);
                choice = m_currentChoice;
                return true;
            }

            #endregion
        }

        private class ChoiceRoundStack
        {
            #region Variables

            private readonly Stack<ChoiceRound> m_contextStack = new Stack<ChoiceRound>();

            #endregion

            #region Methods

            public void Clear()
            {
                while (m_contextStack.Any())
                {
                    PopContext();
                }
            }

            public ChoiceRound PushChoiceRound(bool isMaximizing, string debugInfo)
            {
                ChoiceRound context = new ChoiceRound(isMaximizing, debugInfo);
                m_contextStack.Push(context);
                return context;
            }

            public ChoiceToTry Pop(NewIterativeMinMaxDriver driver)
            {
                ChoiceRound context;
                if (!TryGetRound(out context))
                {
                    return null;
                }

                ChoiceToTry choice;
                while (!context.Pop(driver, out choice))
                {
                    PopContext();

                    if (!TryGetRound(out context))
                    {
                        return null;
                    }
                }
                return choice;
            }

            private bool TryGetRound(out ChoiceRound context)
            {
                PopEmptyContexts();

                if (!m_contextStack.Any())
                {
                    context = null;
                    return false;
                }

                context = m_contextStack.Peek();
                return true;
            }

            private void PopEmptyContexts()
            {
                while (m_contextStack.Any() && m_contextStack.Peek().IsEmpty)
                {
                    PopContext();
                }
            }

            private void PopContext()
            {
                m_contextStack.Pop().End();
            }

            #endregion
        }

        #endregion
    }
}

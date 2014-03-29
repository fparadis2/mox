using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mox.Flow;

namespace Mox.AI
{
    public class IterativeMinMaxDriver : MinMaxDriver
    {
        #region Variables

        private readonly ChoiceRoundStack m_pendingChoices = new ChoiceRoundStack();

        #endregion

        #region Constructor

        public IterativeMinMaxDriver(AIEvaluationContext context, ICancellable cancellable)
            : base(context, cancellable)
        {
        }

        #endregion

        #region Methods

        #endregion

        #region Overrides of MinMaxDriver

        public override void RunWithChoice(Sequencer sequencer, Choice theChoice, object choiceResult)
        {
            ChoiceRound choiceRound = m_pendingChoices.PushChoiceRound(theChoice.GetType().Name);
            choiceRound.PushChoice(sequencer, choiceResult);
            RunIterative();
        }

        public override void Run(Sequencer sequencer)
        {
            RunImpl(sequencer);
            RunIterative();
        }

        private void RunIterative()
        {
            while (!IsCancelled)
            {
                ChoiceToTry choice = m_pendingChoices.Pop(this);

                if (choice == null)
                {
                    break;
                }

                if (RunOnce(choice.Sequencer, new AIDecisionMaker(choice.ChoiceResult)))
                {
                    RunImpl(choice.Sequencer);
                }
            }

            m_pendingChoices.Clear();
        }

        protected override void TryChoices(Sequencer sequencer, Choice theChoice, IEnumerable<object> choices)
        {
            ChoiceRound choiceRound = m_pendingChoices.PushChoiceRound(theChoice.GetType().Name);

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
            private readonly Sequencer m_sequencer;
            private ChoiceScope m_scope;

            #endregion

            #region Constructor

            public ChoiceToTry(Sequencer sequencer, object choice)
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

            public Sequencer Sequencer
            {
                get { return m_sequencer; }
            }

            #endregion

            #region Methods

            public void Begin(IterativeMinMaxDriver driver, string debugInfo)
            {
                Debug.Assert(m_scope == null);
                m_scope = driver.BeginChoice(m_sequencer.Game, m_choice, debugInfo);
            }

            public bool End()
            {
                m_scope.Dispose();
                return m_scope.End();
            }

            #endregion
        }

        private class ChoiceRound
        {
            #region Variables

            private readonly string m_debugInfo;

            private readonly Queue<ChoiceToTry> m_choicesToTry = new Queue<ChoiceToTry>();
            private ChoiceToTry m_currentChoice;

            public ChoiceRound(string debugInfo)
            {
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

            public void PushChoice(Sequencer sequencer, object choice)
            {
                ChoiceToTry choiceTotry = new ChoiceToTry(sequencer, choice);
                m_choicesToTry.Enqueue(choiceTotry);
            }

            internal bool Pop(IterativeMinMaxDriver driver, out ChoiceToTry choice)
            {
                if (!End() || driver.IsCancelled)
                {
                    choice = null;
                    return false;
                }

                m_currentChoice = m_choicesToTry.Dequeue();
                m_currentChoice.Begin(driver, DebugInfo);
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

            public ChoiceRound PushChoiceRound(string debugInfo)
            {
                ChoiceRound context = new ChoiceRound(debugInfo);
                m_contextStack.Push(context);
                return context;
            }

            public ChoiceToTry Pop(IterativeMinMaxDriver driver)
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
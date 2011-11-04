// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Mox.Flow;

namespace Mox.AI
{
    public class IterativeMinMaxDriver<TController> : MinMaxDriver<TController>
    {
        #region Inner Types

        private class ChoiceContext
        {
            #region Variables

            private readonly bool m_isMaximizing;
            private readonly string m_debugInfo;

            private readonly Queue<ChoiceToTry> m_choicesToTry = new Queue<ChoiceToTry>();
            private ChoiceToTry m_currentChoice;

            public ChoiceContext(bool isMaximizing, string debugInfo)
            {
                m_isMaximizing = isMaximizing;
                m_debugInfo = debugInfo;
            }

            internal bool Dispose()
            {
                if (m_currentChoice != null)
                {
                    ChoiceToTry currentChoice = m_currentChoice;
                    m_currentChoice = null;
                    return currentChoice.Dispose();
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

            public void PushChoice(Sequencer<TController> sequencer, object choice)
            {
                ChoiceToTry choiceTotry = new ChoiceToTry(sequencer, choice);
                m_choicesToTry.Enqueue(choiceTotry);
            }

            internal bool Pop(IterativeMinMaxDriver<TController> driver, out ChoiceToTry choice)
            {
                if (!Dispose())
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

        private class ChoiceToTry
        {
            #region Variables

            private readonly object m_choice;
            private readonly Sequencer<TController> m_sequencer;
            private IChoiceScope m_scope;

            #endregion

            #region Constructor

            public ChoiceToTry(Sequencer<TController> sequencer, object choice)
            {
                m_sequencer = sequencer.Clone();
                m_choice = choice;
            }

            #endregion

            #region Properties

            public Sequencer<TController> Sequencer
            {
                get { return m_sequencer; }
            }

            #endregion

            #region Methods

            public void Begin(IterativeMinMaxDriver<TController> driver, bool isMaximizing, string debugInfo)
            {
                Debug.Assert(m_scope == null);

                m_scope = driver.BeginChoice(Sequencer.Game, isMaximizing, m_choice, debugInfo);
            }

            public bool Dispose()
            {
                m_sequencer.Dispose();
                return m_scope.End();
            }

            #endregion
        }

        private class ChoicesStack
        {
            #region Variables

            private readonly Stack<ChoiceContext> m_contextStack = new Stack<ChoiceContext>();

            #endregion

            #region Methods

            public void Clear()
            {
                while (m_contextStack.Any())
                {
                    PopContext();
                }
            }

            public ChoiceContext PushChoiceContext(bool isMaximizing, string debugInfo)
            {
                ChoiceContext context = new ChoiceContext(isMaximizing, debugInfo);
                m_contextStack.Push(context);
                return context;
            }

            public ChoiceToTry Pop(IterativeMinMaxDriver<TController> driver)
            {
                ChoiceContext context;
                if (!TryGetContext(out context))
                {
                    return null;
                }

                ChoiceToTry choice;
                while (!context.Pop(driver, out choice))
                {
                    PopContext();

                    if (!TryGetContext(out context))
                    {
                        return null;
                    }
                }
                return choice;
            }

            private bool TryGetContext(out ChoiceContext context)
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
                m_contextStack.Pop().Dispose();
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly ChoicesStack m_choices = new ChoicesStack();
        private int m_versionToken;
        private int m_lastVersionToken;

        private IterativeMinMaxDriver(AIEvaluationContext context, IEnumerable<object> rootChoices)
            : base(context, rootChoices)
        {
        }

        #endregion

        #region Methods

        #region Public methods

        public static MinMaxDriver<TController> CreateController(AIEvaluationContext context)
        {
            return new IterativeMinMaxDriver<TController>(context, null);
        }

        public static MinMaxDriver<TController> CreateRootController(AIEvaluationContext context, params object[] choices)
        {
            return new IterativeMinMaxDriver<TController>(context, choices);
        }

        #endregion

        #region Implementation

        private void IncrementVersion()
        {
            m_versionToken++;
        }

        private void TagVersion()
        {
            m_lastVersionToken = m_versionToken;
        }

        private bool HasPushedChoices
        {
            get { return m_lastVersionToken != m_versionToken; }
        }

        protected override void TryAllChoices(Sequencer<TController> sequencer, bool maximizingPlayer, IEnumerable<object> choices, string debugInfo)
        {
            Debug.Assert(choices.Count() > 0);

            IncrementVersion();
            ChoiceContext choiceContext = m_choices.PushChoiceContext(maximizingPlayer, debugInfo);

            // It's important that the first choices be evaluated first because choices are pre-sorted and that helps with pruning the tree.
            foreach (object choice in choices)
            {
                choiceContext.PushChoice(sequencer, choice);
            }
        }

        protected internal override void RunInternal(ICancellable cancellable)
        {
            while (!cancellable.Cancel)
            {
                ChoiceToTry choice = m_choices.Pop(this);

                if (choice == null)
                {
                    break;
                }

                TryChoice(choice.Sequencer);
            }

            m_choices.Clear(); 
        }

        private void TryChoice(Sequencer<TController> sequencer)
        {
            bool firstPart = true;
            TagVersion();

            while (true)
            {
                // Rerun with the added choice
                using (ITransaction transaction = sequencer.BeginSequencingTransaction())
                {
                    SequencerResult result = sequencer.RunOnce(Controller);

                    if (Recurse(sequencer, result, firstPart))
                    {
                        SafeRollback(transaction);
                        return;
                    }
                }

                firstPart = false;
            }
        }

        private bool Recurse(Sequencer<TController> sequencer, SequencerResult result, bool firstPart)
        {
            if (result == SequencerResult.Retry && firstPart)
            {
                Debug.Assert(!HasPushedChoices, "TODO: We should pop contexts or we will try them next!");
                Discard();
                return true;
            }

            if (HasPushedChoices || HasConsumedChoice)
            {
                return true;
            }

            return EvaluateIf(sequencer.Game, sequencer.IsEmpty);
        }

        private static void SafeRollback(ITransaction transaction)
        {
            if (transaction != null)
            {
                transaction.Rollback();
            }
        }

        #endregion

        #endregion
    }
}

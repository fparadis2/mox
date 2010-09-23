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
using System.Diagnostics;

using Castle.Core.Interceptor;

namespace Mox.Flow
{
    public enum SequencerResult
    {
        Stop,
        Continue,
        Retry
    }

    /// <summary>
    /// A sequencer sequences <see cref="Part{TController}"/>s together.
    /// </summary>
    public partial class Sequencer<TController> : IDisposable
    {
        #region Inner Types

        private sealed class RecorderController : IInterceptor
        {
            #region Variables

            private readonly TController m_originalController;
            private readonly ChoiceRecorder m_recorder;

            #endregion

            #region Constructor

            public RecorderController(TController originalController, ChoiceRecorder recorder)
            {
                m_originalController = originalController;
                m_recorder = recorder;
            }

            #endregion

            #region Methods

            public TController ToController()
            {
                return ProxyGenerator<TController>.CreateInterfaceProxyWithTarget(m_originalController, this);
            }

            #endregion

            public void Intercept(IInvocation invocation)
            {
                object choice;
                if (m_recorder.TryReplay(out choice))
                {
                    invocation.ReturnValue = choice;
                }
                else
                {
                    invocation.Proceed();
                    m_recorder.Record(invocation.ReturnValue);
                }
            }
        }

        #endregion

        #region Variables

        private readonly Game m_game;
        private ImmutableStack<Part<TController>> m_parts = new ImmutableStack<Part<TController>>();
        private ChoiceRecorder m_choiceRecorder = new ChoiceRecorder();

        private ITransaction m_forkTransaction;
        private ITransaction m_activeTransaction;

        #endregion

        #region Constructor

        public Sequencer(Part<TController> initialPart, Game game)
        {
            Throw.IfNull(initialPart, "initialPart");
            Throw.IfNull(game, "game");

            m_game = game;

            Push(initialPart);
        }

        /// <summary>
        /// Clone ctor
        /// </summary>
        private Sequencer(Sequencer<TController> other, Game game)
        {
            m_game = game;
            m_parts = other.m_parts;
            m_argumentStack = other.m_argumentStack;
            m_choiceRecorder = other.m_choiceRecorder.Clone();
        }

        public void Dispose()
        {
            if (m_forkTransaction != null)
            {
                m_forkTransaction.Rollback();
                m_forkTransaction = null;
            }
        }

        #endregion

        #region Properties

        public Game Game
        {
            get { return m_game; }
        }

        public bool IsEmpty
        {
            get { return m_parts.IsEmpty; }
        }

        public Part<TController> NextPart
        {
            get { return m_parts.Peek(); }
        }

        #endregion

        #region Methods

        #region Cloning

        /// <summary>
        /// Clones the current sequencer.
        /// </summary>
        /// <returns></returns>
        public Sequencer<TController> Clone()
        {
            return Clone(Game);
        }

        /// <summary>
        /// Clones the current sequencer.
        /// </summary>
        /// <returns></returns>
        public Sequencer<TController> Clone(Game game)
        {
            return new Sequencer<TController>(this, game);
        }

        /// <summary>
        /// Clones the current sequencer.
        /// </summary>
        /// <returns></returns>
        public Sequencer<TController> Fork()
        {
            var clone = Clone();

            if (m_activeTransaction != null)
            {
                clone.m_forkTransaction = m_activeTransaction.Reverse();
            }

            return clone;
        }

        #endregion

        #region Run

        /// <summary>
        /// Runs all the parts.
        /// </summary>
        /// <remarks>
        /// Returns true if finished 'naturally'.
        /// </remarks>
        public bool Run(TController controller)
        {
            while (!IsEmpty && RunOnce(controller) != SequencerResult.Stop)
            {
            }

            return IsEmpty;
        }

        /// <summary>
        /// Runs the "next" scheduled part.
        /// </summary>
        /// <returns></returns>
        public SequencerResult RunOnce(TController controller)
        {
            Game.EnsureControlModeIs(GameControlMode.Master);

            Debug.Assert(!ReferenceEquals(controller, null), "Invalid controller");

            Part<TController> partToExecute = m_parts.Peek();

            var context = CreateContext(partToExecute.ControllerAccess, controller);
            Part<TController> nextPart = ExecutePart(context, partToExecute);

            if (context.Stop || Game.State.HasEnded)
            {
                m_parts = new ImmutableStack<Part<TController>>();
                return SequencerResult.Stop;
            }
            
            PrepareNextPart(nextPart, context);
            return Equals(nextPart, partToExecute) ? SequencerResult.Retry : SequencerResult.Continue;
        }

        private Part<TController>.Context CreateContext(ControllerAccess controllerAccess, TController controller)
        {
            if (controllerAccess == ControllerAccess.Multiple)
            {
                RecorderController recorderController = new RecorderController(controller, m_choiceRecorder);
                return new Part<TController>.Context(this, recorderController.ToController(), controllerAccess);
            }

            return new Part<TController>.Context(this, controller, controllerAccess);
        }

        private void PrepareNextPart(Part<TController> nextPart, Part<TController>.Context lastContext)
        {
            Pop();

            if (nextPart != null)
            {
                Push(nextPart);
            }

            lastContext.ScheduledParts.ForEach(Push);
        }

        private Part<TController> ExecutePart(Part<TController>.Context context, Part<TController> part)
        {
            // Not in a using to avoid eating up exceptions because .Dispose will most likely rethrow
            IDisposable transactionScope = BeginActiveTransaction(part);
            var result = part.Execute(context);
            transactionScope.Dispose();
            return result;
        }

        private IDisposable BeginActiveTransaction(Part<TController> part)
        {
            Debug.Assert(m_activeTransaction == null);

            m_activeTransaction = BeginSequencingTransaction(part);

            return new DisposableHelper(() =>
            {
                m_choiceRecorder = new ChoiceRecorder();
                DisposableHelper.SafeDispose(ref m_activeTransaction);
            });
        }

        public ITransaction BeginSequencingTransaction()
        {
            return BeginSequencingTransaction(NextPart);
        }

        private ITransaction BeginSequencingTransaction(Part<TController> part)
        {
            if (!(part is ITransactionPart) && part.ControllerAccess != ControllerAccess.None)
            {
                return Game.TransactionStack.BeginTransaction(Transactions.TransactionType.Master);
            }

            return null;
        }

        protected void Push(Part<TController> part)
        {
            m_parts = m_parts.Push(part);
        }

        protected void Pop()
        {
            m_parts = m_parts.Pop();
        }

        #endregion

        #endregion
    }
}

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
using System.Reflection;
using Castle.Core.Interceptor;

using Mox.Flow;
using Mox.Transactions;

namespace Mox.AI
{
    public interface ICancellable
    {
        bool Cancel { get; }
    }

    public abstract class MinMaxDriver<TController>
    {
        #region Inner Types

        protected class Context
        {
            public readonly IMinimaxTree Tree;
            public readonly IMinMaxAlgorithm Algorithm;
            public readonly IChoiceResolverProvider ChoiceResolverProvider;
            public readonly ITransientScope TransientScope;

            public Context(IMinimaxTree minmaxTree, IMinMaxAlgorithm algorithm, IChoiceResolverProvider choiceResolverProvider, ITransientScope transientScope)
            {
                Throw.IfNull(minmaxTree, "minmaxTree");
                Throw.IfNull(algorithm, "algorithm");
                Throw.IfNull(choiceResolverProvider, "choiceResolverProvider");
                Throw.IfNull(transientScope, "transientScope");

                Tree = minmaxTree;
                Algorithm = algorithm;
                ChoiceResolverProvider = choiceResolverProvider;
                TransientScope = transientScope;
            }

            public Context(IMinimaxTree minmaxTree, IMinMaxAlgorithm algorithm, IChoiceResolverProvider choiceResolverProvider, ObjectManager game)
                : this(minmaxTree, algorithm, choiceResolverProvider, game.TransactionStack.CreateTransientScope())
            {
            }
        }

        private class Interceptor : IInterceptor
        {
            private readonly MinMaxDriver<TController> m_owner;

            public Interceptor(MinMaxDriver<TController> owner)
            {
                m_owner = owner;
            }

            public void Intercept(IInvocation invocation)
            {
                invocation.ReturnValue = m_owner.ProxyController_Delegate(invocation.Method, invocation.Arguments);
            }
        }

        private class RetainedChoice
        {
            public readonly object Choice;

            public RetainedChoice(object choice)
            {
                Choice = choice;
            }

            public static readonly RetainedChoice Consumed = new RetainedChoice(null);
        }

        protected interface IChoiceScope
        {
            bool End();
        }

        private class ChoiceScope : IChoiceScope
        {
            private readonly Func<bool> m_endFunc;

            public ChoiceScope(Func<bool> endFunc)
            {
                Debug.Assert(endFunc != null);

                m_endFunc = endFunc;
            }

            public bool End()
            {
                return m_endFunc();
            }
        }

        #endregion

        #region Variables

        private readonly Context m_context;
        private readonly TController m_proxyController;

        private List<object> m_rootChoices;
        private RetainedChoice m_nextChoice;

        #endregion

        #region Constructor

        protected MinMaxDriver(Context context, IEnumerable<object> choices)
        {
            Debug.Assert(context != null);
            m_context = context;

            m_proxyController = ProxyGenerator<TController>.CreateInterfaceProxyWithoutTarget(new Interceptor(this));

            if (choices != null)
            {
                m_rootChoices = new List<object>(choices);
            }
        }

        #endregion

        #region Properties

        protected IMinimaxTree Tree
        {
            get { return m_context.Tree; }
        }

        protected IMinMaxAlgorithm Algorithm
        {
            get { return m_context.Algorithm; }
        }

        private IChoiceResolverProvider ChoiceResolverProvider
        {
            get { return m_context.ChoiceResolverProvider; }
        }

        protected ITransientScope TransientScope
        {
            get { return m_context.TransientScope; }
        }

        public TController Controller
        {
            get { return m_proxyController; }
        }

        /// <summary>
        /// Whether the 'choice' has been made for this driver.
        /// </summary>
        protected bool HasConsumedChoice
        {
            get { return m_nextChoice == RetainedChoice.Consumed; }
        }

        #endregion

        #region Methods

        public void Run(MethodBase method, object[] args, Sequencer<TController> sequencer, ControllerAccess controllerAccess, ICancellable cancellable)
        {
            ChoiceResolver resolver = ChoiceResolverProvider.GetResolver(method);
            args = (object[])args.Clone();
            resolver.SetContext(method, args, new Part<TController>.Context(sequencer, Controller, controllerAccess));

            using (ITransaction transaction = sequencer.BeginSequencingTransaction())
            {
                method.Invoke(Controller, args);
                transaction.Rollback();
            }

            RunInternal(cancellable);
        }

        protected internal virtual void RunInternal(ICancellable cancellable)
        {
        }

        private object ProxyController_Delegate(MethodBase method, object[] args)
        {
            object resultChoice;
            if (TryConsumeNextChoice(out resultChoice))
            {
                return resultChoice;
            }

            ChoiceResolver choiceResolver = GetChoiceResolver(method);
            Debug.Assert(choiceResolver != null);
            object defaultChoice = choiceResolver.GetDefaultChoice(method, args);

            if (m_nextChoice == RetainedChoice.Consumed)
            {
                return defaultChoice;
            }

            Part<TController>.Context context = choiceResolver.GetContext<TController>(method, args);

            context.Stop = true;

            m_nextChoice = RetainedChoice.Consumed;

            if (TransientScope.TransactionRolledback)
            {
                //Discard();
                return defaultChoice;
            }

            if (!TransientScope.IsInUserTransaction && Algorithm.IsTerminal(Tree, context.Game))
            {
                Evaluate(context.Game);
                return defaultChoice;
            }

            // Must start a new node and try all the choices
            Player player = choiceResolver.GetPlayer(method, args);
            bool isMaximizingPlayer = Algorithm.IsMaximizingPlayer(player);
            IEnumerable<object> choices = EnumeratePossibleChoices(choiceResolver, method, args);
            TryAllChoices(context.Sequencer, isMaximizingPlayer, choices, method.Name);
            return defaultChoice;
        }

        private IEnumerable<object> EnumeratePossibleChoices(ChoiceResolver choiceResolver, MethodBase method, object[] args)
        {
            if (m_rootChoices != null)
            {
                IEnumerable<object> rootChoices = m_rootChoices;
                m_rootChoices = null;
                return rootChoices;
            }

            return choiceResolver.ResolveChoices(method, args);
        }

        protected abstract void TryAllChoices(Sequencer<TController> sequencer, bool maximizingPlayer, IEnumerable<object> choices, string debugInfo);

        private bool TryConsumeNextChoice(out object choice)
        {
            if (m_nextChoice != null && m_nextChoice != RetainedChoice.Consumed)
            {
                choice = m_nextChoice.Choice;
                m_nextChoice = null;
                return true;
            }

            choice = null;
            return false;
        }

        private ChoiceResolver GetChoiceResolver(MethodBase method)
        {
            return ChoiceResolverProvider.GetResolver(method);
        }

        protected void Evaluate(Game game)
        {
            Tree.Evaluate(Algorithm.ComputeHeuristic(game, true));
        }

        protected void Discard()
        {
            Tree.Discard();
        }

        protected bool EvaluateIf(Game game, bool condition)
        {
            if (TransientScope.TransactionRolledback)
            {
                Tree.Discard();
                return true;
            }

            if (condition)
            {
                Evaluate(game);
                return true;
            }

            return false;
        }

        private IDisposable AssignNextChoice(object nextChoice)
        {
            var oldChoice = m_nextChoice;
            m_nextChoice = new RetainedChoice(nextChoice);

            return new DisposableHelper(() => m_nextChoice = oldChoice);
        }

        protected IChoiceScope BeginChoice(Game game, bool maximizingPlayer, object choice, string debugInfo)
        {
            Tree.BeginNode(maximizingPlayer, choice, debugInfo);

            var transientHandle = TransientScope.Use();
            var transactionHandle = BeginRollbackTransaction(game);
            var choiceHandle = AssignNextChoice(choice);

            return new ChoiceScope(() =>
            {
                choiceHandle.Dispose();
                transactionHandle.Dispose();
                transientHandle.Dispose();
                return Tree.EndNode();
            });
        }

        private static IDisposable BeginRollbackTransaction(ObjectManager game)
        {
            ITransaction transaction = game.TransactionStack.BeginTransaction(TransactionType.Atomic | TransactionType.Master);
            return new DisposableHelper(() =>
            {
                transaction.Rollback();
                transaction.Dispose();
            });
        }

        #endregion
    }
}

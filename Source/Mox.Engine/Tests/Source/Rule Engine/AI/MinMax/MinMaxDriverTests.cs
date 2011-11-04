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
using System.Linq;
using System.Reflection;
using Mox.Flow;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Interfaces;

using Is = Rhino.Mocks.Constraints.Is;

namespace Mox.AI
{
    using Part = Part<MinMaxDriverTestsBase.IMockController>;
    using Sequencer = Sequencer<MinMaxDriverTestsBase.IMockController>;

    public abstract class MinMaxDriverTestsBase : BaseGameTests
    {
        #region Inner Types

        #region Misc

        private static class TurnDataExtensions
        {
            public static readonly Property<int> MyProperty = Property<int>.RegisterAttachedProperty("MyProperty", typeof(TurnData));
        }

        public enum ChoiceA
        {
            ResultX,
            ResultY
        }

        public enum ChoiceB
        {
            Result1,
            Result2
        }

        public interface IMockController
        {
            ChoiceA ChoiceA(Part.Context context);
            ChoiceB ChoiceB(Part.Context context);
        }

        public interface IChoiceVerifier
        {
            void ChoiceA(ChoiceA choice);
            void ChoiceB(ChoiceB choice);
        }

        protected enum ChoiceExpectationBehavior
        {
            Direct,
            Delayed
        }

        protected abstract ChoiceExpectationBehavior Behavior { get; }
        protected abstract ChoiceExpectationBehavior RootBehavior { get; }

        protected class ChoiceExpectation : IDisposable
        {
            private readonly MinMaxDriverTestsBase m_owner;
            private readonly bool m_rootChoice;
            private readonly List<IDisposable> m_disposables = new List<IDisposable>();

            public ChoiceExpectation(MinMaxDriverTestsBase owner, bool rootChoice, IDisposable disposable)
            {
                m_owner = owner;
                m_rootChoice = rootChoice;

                AddDisposable(disposable);
            }

            private ChoiceExpectationBehavior Behavior
            {
                get
                {
                    if (m_rootChoice)
                    {
                        return m_owner.RootBehavior;
                    }
                    return m_owner.Behavior;
                }
            }

            private void AddDisposable(IDisposable disposable)
            {
                if (disposable != null)
                {
                    switch (Behavior)
                    {
                        case ChoiceExpectationBehavior.Delayed:
                            m_disposables.Add(disposable);
                            break;

                        case ChoiceExpectationBehavior.Direct:
                            disposable.Dispose();
                            break;
                        default:
                            throw new InvalidProgramException();
                    }
                }
            }

            public void Dispose()
            {
                foreach (var action in m_disposables)
                {
                    action.Dispose();
                }
            }

            public ChoiceExpectation And_Garbage<TChoice>()
            {
                var disposable = m_owner.Expect_Garbage<TChoice>();
                AddDisposable(disposable);
                return this;
            }
        }

        #endregion

        #region Parts

        protected abstract class PartBase : Part<IMockController>
        {
            public IChoiceVerifier ChoiceVerifier
            {
                get;
                set;
            }

            protected TPart CreatePart<TPart>()
                where TPart : PartBase, new()
            {
                return new TPart
                {
                    ChoiceVerifier = ChoiceVerifier
                };
            }

            protected static void ModifyData(Context context, int initial, int final)
            {
                // This tests that parts are always ran with the same data.
                Assert.AreEqual(initial, context.Game.TurnData.GetValue(TurnDataExtensions.MyProperty));
                context.Game.TurnData.SetValue(TurnDataExtensions.MyProperty, final);
            }
        }

        protected class SingleChoicePart : PartBase
        {
            #region Overrides of Part<IMockController>

            public override ControllerAccess ControllerAccess
            {
                get
                {
                    return ControllerAccess.Single;
                }
            }

            public override Part<IMockController> Execute(Context context)
            {
                ChoiceVerifier.ChoiceA(context.Controller.ChoiceA(context));
                return null;
            }

            #endregion
        }

        protected class SingleChoicePart_Chained : SingleChoicePart
        {
            #region Overrides of Part<IMockController>

            public override Part<IMockController> Execute(Context context)
            {
                base.Execute(context);
                return CreatePart<SingleChoicePart>();
            }

            #endregion
        }

        protected class SingleChoicePart_ChainedWithEmpty : SingleChoicePart
        {
            #region Inner Types

            private class EmptyPart : Part<IMockController>
            {
                public override Part Execute(Context context)
                {
                    return null;
                }
            }

            #endregion

            #region Overrides of Part<IMockController>

            public override Part<IMockController> Execute(Context context)
            {
                base.Execute(context);
                return new EmptyPart();
            }

            #endregion
        }

        protected class MultiChoicePart_Simple : PartBase
        {
            #region Overrides of Part<IMockController>

            public override ControllerAccess ControllerAccess
            {
                get
                {
                    return ControllerAccess.Multiple;
                }
            }

            public override Part<IMockController> Execute(Context context)
            {
                ChoiceVerifier.ChoiceA(context.Controller.ChoiceA(context));
                ChoiceVerifier.ChoiceB(context.Controller.ChoiceB(context));
                return null;
            }

            #endregion
        }

        protected class MultiChoicePart_Complex : PartBase
        {
            #region Overrides of Part<IMockController>

            public override ControllerAccess ControllerAccess
            {
                get
                {
                    return ControllerAccess.Multiple;
                }
            }

            public override Part<IMockController> Execute(Context context)
            {
                ChoiceVerifier.ChoiceA(context.Controller.ChoiceA(context));

                ChoiceB bResult = context.Controller.ChoiceB(context);
                ChoiceVerifier.ChoiceB(bResult);

                if (bResult == ChoiceB.Result1)
                {
                    ChoiceVerifier.ChoiceA(context.Controller.ChoiceA(context));
                }

                return null;
            }

            #endregion
        }

        protected class MultiChoiceMultiPart : MultiChoicePart_Simple
        {
            #region Overrides of Part<IMockController>

            public override Part<IMockController> Execute(Context context)
            {
                base.Execute(context);
                return CreatePart<MultiChoicePart_Simple>();
            }

            #endregion
        }

        protected class OverflowPart : PartBase
        {
            #region Overrides of Part<IMockController>

            public override ControllerAccess ControllerAccess
            {
                get
                {
                    return ControllerAccess.Single;
                }
            }

            public override Part<IMockController> Execute(Context context)
            {
                ChoiceA a = context.Controller.ChoiceA(context);
                ChoiceVerifier.ChoiceA(a);

                return a == ChoiceA.ResultX ? null : this;
            }

            #endregion
        }

        protected class ModifyingPart : PartBase
        {
            public override ControllerAccess ControllerAccess
            {
                get
                {
                    return ControllerAccess.Single;
                }
            }

            public override Part Execute(Context context)
            {
                var part1 = CreatePart<ModifyingPartWithChoice>();
                part1.StartIndex = 0;

                var part2 = CreatePart<ModifyingPartWithChoice>();
                part2.StartIndex = 4;

                ChoiceVerifier.ChoiceA(context.Controller.ChoiceA(context));
                context.Schedule(part1);
                context.Schedule(CreatePart<ModifyingPart_Impl>());
                context.Schedule(part2);
                return null;
            }

            private class ModifyingPart_Impl : PartBase
            {
                public override Part Execute(Context context)
                {
                    ModifyData(context, 3, 4);
                    return null;
                }
            }
        }

        protected class ModifyingPartWithChoice : PartBase
        {
            #region Overrides of Part<IMockController>

            public override ControllerAccess ControllerAccess
            {
                get
                {
                    return ControllerAccess.Multiple;
                }
            }

            public int StartIndex { get; set; }

            public override Part<IMockController> Execute(Context context)
            {
                ModifyData(context, StartIndex + 0, StartIndex + 1);
                ChoiceVerifier.ChoiceA(context.Controller.ChoiceA(context));

                ModifyData(context, StartIndex + 1, StartIndex + 2);
                ChoiceVerifier.ChoiceB(context.Controller.ChoiceB(context));

                ModifyData(context, StartIndex + 2, StartIndex + 3);
                return null;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Variables

        private IChoiceVerifier m_mockChoiceVerifier;

        private IMinimaxTree m_tree;
        private IMinMaxAlgorithm m_algorithm;
        private IChoiceResolverProvider m_choiceResolverProvider;
        private ChoiceResolver m_choiceResolver;

        private MinMaxDriver<IMockController> m_driver;
        private ICancellable m_cancellable;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_mockChoiceVerifier = m_mockery.StrictMock<IChoiceVerifier>();

            m_tree = m_mockery.StrictMock<IMinimaxTree>();
            m_algorithm = m_mockery.StrictMock<IMinMaxAlgorithm>();
            m_choiceResolverProvider = new MockRepository().StrictMock<IChoiceResolverProvider>();
            m_choiceResolver = m_mockery.StrictMock<ChoiceResolver>();

            SetupResult.For(m_choiceResolverProvider.GetResolver(null)).IgnoreArguments().Return(m_choiceResolver);
            m_choiceResolverProvider.Replay();

            m_driver = CreateMinMaxDriver();
            m_cancellable = new NotCancellable();
        }

        #endregion

        #region Utilities

        #region Execute

        protected void Execute<TPart>()
            where TPart : PartBase, new()
        {
            Execute<TPart>(m_driver);
        }

        protected void Execute<TPart>(MinMaxDriver<IMockController> driver)
            where TPart : PartBase, new()
        {
            Execute<TPart>(driver, driver.Controller);
        }

        protected void Execute<TPart>(IMockController controller)
            where TPart : PartBase, new()
        {
            Execute<TPart>(m_driver, controller);
        }

        private void Execute<TPart>(MinMaxDriver<IMockController> driver, IMockController controller)
            where TPart : PartBase, new()
        {
            TPart part = CreatePart<TPart>();
            Sequencer sequencer = new Sequencer<IMockController>(part, m_game);
            m_mockery.Test(() =>
            {
                using (ITransaction transaction = sequencer.BeginSequencingTransaction())
                {
                    sequencer.RunOnce(controller);
                    transaction.Rollback();
                }

                driver.RunInternal(m_cancellable);
            });
        }

        private MinMaxDriver<IMockController> CreateMinMaxDriver(params object[] firstChoices)
        {
            AIEvaluationContext context = new AIEvaluationContext(m_tree, m_algorithm, m_choiceResolverProvider);
            return CreateMinMaxDriver(context, firstChoices);
        }

        protected abstract MinMaxDriver<IMockController> CreateMinMaxDriver(AIEvaluationContext context, params object[] firstChoices);

        private TPart CreatePart<TPart>()
            where TPart : PartBase, new()
        {
            TPart part = new TPart
            {
                ChoiceVerifier = m_mockChoiceVerifier
            };

            return part;
        }

        #endregion

        #region Expectations

        #region Tree

        protected IDisposable BeginNode(bool maximizingPlayer, object result)
        {
            return BeginNode(maximizingPlayer, result, false);
        }

        protected IDisposable BeginNode(bool maximizingPlayer, object result, bool cutoff)
        {
            m_tree.BeginNode(maximizingPlayer, result, null);
            LastCall.IgnoreArguments().Constraints(Is.Equal(maximizingPlayer), Is.Equal(result), Is.Anything());

            return new DisposableHelper(() => Expect.Call(m_tree.EndNode()).Return(!cutoff));
        }

        #endregion

        #region Verifier

        protected IMethodOptions<object> Try_Choice<TChoice>(TChoice choice)
        {
            GetMethod<TChoice>().Invoke(m_mockChoiceVerifier, new object[] { choice });
            return LastCall.On(m_mockChoiceVerifier);
        }

        protected void Try_Choice_Anything<TChoice>()
        {
            TChoice anything = default(TChoice);
            Try_Choice(anything);
            LastCall.IgnoreArguments();
        }

        private static MethodInfo GetMethod<TChoice>()
        {
            return typeof(IChoiceVerifier).GetMethod(typeof(TChoice).Name);
        }

        #endregion

        #region Choice Resolving

        protected void Expect_Evaluate_heuristic()
        {
            Expect.Call(m_algorithm.ComputeHeuristic(m_game, true)).Return(66);
            m_tree.Evaluate(66);
        }

        protected void Expect_Discard()
        {
            m_tree.Discard();
        }

        private void Expect_IsMaximizingPlayer<TChoice>(bool isMaximizing)
        {
            Player player = m_playerA;
            Expect.Call(m_choiceResolver.GetPlayer(null, null)).
                   IgnoreArguments().
                   Constraints(GetMethodConstraint<TChoice>(), Is.Anything()).
                   Return(player).
                   Message(string.Format("Expected IsMaximizing={0} for {1} choice", isMaximizing, typeof(TChoice).Name));
            Expect.Call(m_algorithm.IsMaximizingPlayer(player)).Return(isMaximizing);
        }

        protected void Expect_GetDefaultChoice<TChoice>(TChoice choice)
        {
            Expect.Call(m_choiceResolver.GetDefaultChoice(null, null)).
                IgnoreArguments().
                Constraints(GetMethodConstraint<TChoice>(), Is.Anything()).
                Return(choice).
                Message(string.Format("Expected DefaultChoice={0} for {1} choice", choice, typeof(TChoice).Name));
        }

        private delegate Part.Context GetContextDelegate(MethodBase method, object[] args);

        private void Expect_GetContext<TChoice>()
        {
            Expect.Call(m_choiceResolver.GetContext<IMockController>(null, null)).IgnoreArguments().Do(new GetContextDelegate(delegate(MethodBase method, object[] args)
            {
                Assert.That(GetMethodConstraint<TChoice>().Eval(method));
                Assert.IsInstanceOf<Part.Context>(args[0]);
                return (Part.Context)args[0];
            }));
        }

        private void Expect_ResolveChoices<TChoice>(params TChoice[] choices)
        {
            Expect.Call(m_choiceResolver.ResolveChoices(null, null)).
                   IgnoreArguments().
                   Constraints(GetMethodConstraint<TChoice>(), Is.Anything()).
                   Return(choices.Cast<object>()).
                   Message(string.Format("Expected Choices={0} for {1} choice", choices, typeof(TChoice).Name));
        }

        protected void Expect_IsTerminal<TChoice>()
        {
            Expect_GetContext<TChoice>();
            Expect.Call(m_algorithm.IsTerminal(m_tree, m_game)).Return(true);
        }

        protected ChoiceExpectation Expect_Choice<TChoice>(bool isMaximizing, params TChoice[] choices)
        {
            return Expect_Choice(isMaximizing, true, choices);
        }

        protected ChoiceExpectation Expect_Choice<TChoice>(bool isMaximizing, bool askIsTerminal, params TChoice[] choices)
        {
            Expect_Choice_Impl(isMaximizing, askIsTerminal, choices);
            var disposable = Expect_Choice_Impl<TChoice>();
            return new ChoiceExpectation(this, false, disposable);
        }

        protected ChoiceExpectation Expect_Root_Choice<TChoice>(bool isMaximizing, params TChoice[] choices)
        {
            Expect_Choice_Impl(isMaximizing, true, choices);
            var disposable = Expect_Choice_Impl<TChoice>();
            return new ChoiceExpectation(this, true, disposable);
        }

        private void Expect_Choice_Impl<TChoice>(bool isMaximizing, bool askIsTerminal, params TChoice[] choices)
        {
            Expect_GetDefaultChoice(default(TChoice));
            Expect_GetContext<TChoice>();

            if (askIsTerminal)
            {
                Expect.Call(m_algorithm.IsTerminal(m_tree, m_game)).Return(false);
            }

            Expect_IsMaximizingPlayer<TChoice>(isMaximizing);

            if (choices.Length > 0)
            {
                Expect_ResolveChoices(choices);
            }
        }

        private IDisposable Expect_Choice_Impl<TChoice>()
        {
            return new DisposableHelper(Try_Choice_Anything<TChoice>);
        }

        private IDisposable Expect_Garbage<TChoice>()
        {
            return new DisposableHelper(() =>
            {
                Expect_GetDefaultChoice(default(TChoice));
                Try_Choice_Anything<TChoice>();
            });
        }

        //protected abstract IDisposable Expect_Choice_Impl<TChoice>();

        //protected abstract IDisposable Expect_Root_Choice_Impl<TChoice>();

        //protected abstract IDisposable Expect_Garbage<TChoice>();

        private static AbstractConstraint GetMethodConstraint<TChoice>()
        {
            return Is.Matching<MethodBase>(method => method.Name == typeof(TChoice).Name);
        }

        #endregion

        #endregion

        #region Helpers

        private void Try_Simple_Multichoice(bool rootChoice, System.Action subAction)
        {
            #region First decision

            var firstChoice = rootChoice ? Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY) : Expect_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY);
            using (firstChoice.And_Garbage<ChoiceB>())
            {
                using (BeginNode(true, ChoiceA.ResultX))
                {
                    Try_Choice(ChoiceA.ResultX);

                    using (Expect_Choice(true, ChoiceB.Result1, ChoiceB.Result2))
                    {
                        using (BeginNode(true, ChoiceB.Result1))
                        {
                            Try_Choice(ChoiceA.ResultX);
                            Try_Choice(ChoiceB.Result1);

                            subAction();
                        }

                        using (BeginNode(true, ChoiceB.Result2))
                        {
                            Try_Choice(ChoiceA.ResultX);
                            Try_Choice(ChoiceB.Result2);

                            subAction();
                        }
                    }
                }

                using (BeginNode(true, ChoiceA.ResultY))
                {
                    Try_Choice(ChoiceA.ResultY);

                    using (Expect_Choice(true, ChoiceB.Result1, ChoiceB.Result2))
                    {
                        using (BeginNode(true, ChoiceB.Result1))
                        {
                            Try_Choice(ChoiceA.ResultY);
                            Try_Choice(ChoiceB.Result1);

                            subAction();
                        }

                        using (BeginNode(true, ChoiceB.Result2))
                        {
                            Try_Choice(ChoiceA.ResultY);
                            Try_Choice(ChoiceB.Result2);

                            subAction();
                        }
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Cancel

        private class NotCancellable : ICancellable
        {
            public bool Cancel
            {
                get { return false; }
            }
        }

        protected void CreateMockCancellable()
        {
            m_cancellable = m_mockery.StrictMock<ICancellable>();
        }

        protected void Expect_Is_Cancelled(bool result)
        {
            Assert.IsNotInstanceOf<NotCancellable>(m_cancellable, "Cancellable is not mocked!");
            Expect.Call(m_cancellable.Cancel).Return(result);
        }

        #endregion

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => CreateMinMaxDriver(null, m_algorithm, m_choiceResolverProvider));
            Assert.Throws<ArgumentNullException>(() => CreateMinMaxDriver(m_tree, null, m_choiceResolverProvider));
            Assert.Throws<ArgumentNullException>(() => CreateMinMaxDriver(m_tree, m_algorithm, null));
        }

        [Test]
        public void Test_Execute_tries_all_the_given_possibilities_simple_case()
        {
            using (OrderedExpectations)
            {
                using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
                {
                    using (BeginNode(true, ChoiceA.ResultX))
                    {
                        Try_Choice(ChoiceA.ResultX);

                        Expect_Evaluate_heuristic();
                    }

                    using (BeginNode(true, ChoiceA.ResultY))
                    {
                        Try_Choice(ChoiceA.ResultY);

                        Expect_Evaluate_heuristic();
                    }
                }
            }

            Execute<SingleChoicePart>();
        }

        [Test]
        public void Test_Evaluation_takes_place_at_the_end_even_if_no_choice_is_made_on_recursion()
        {
            using (OrderedExpectations)
            {
                using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
                {
                    using (BeginNode(true, ChoiceA.ResultX))
                    {
                        Try_Choice(ChoiceA.ResultX);

                        Expect_Evaluate_heuristic();
                    }

                    using (BeginNode(true, ChoiceA.ResultY))
                    {
                        Try_Choice(ChoiceA.ResultY);

                        Expect_Evaluate_heuristic();
                    }
                }
            }

            Execute<SingleChoicePart_ChainedWithEmpty>();
        }

        [Test]
        public void Test_Can_force_the_first_choices_to_be_tried()
        {
            using (OrderedExpectations)
            {
                using (Expect_Root_Choice<ChoiceA>(true))
                {
                    using (BeginNode(true, ChoiceA.ResultX))
                    {
                        Try_Choice(ChoiceA.ResultX);

                        Expect_Evaluate_heuristic();
                    }
                }
            }

            Execute<SingleChoicePart>(CreateMinMaxDriver(ChoiceA.ResultX));
        }

        [Test]
        public void Test_Execute_breaks_when_cutoff()
        {
            using (OrderedExpectations)
            {
                using (Expect_Root_Choice(false, ChoiceA.ResultX, ChoiceA.ResultY))
                {
                    using (BeginNode(false, ChoiceA.ResultX, true))
                    {
                        Try_Choice(ChoiceA.ResultX);
                        Expect_Evaluate_heuristic();
                    }
                }
            }

            Execute<SingleChoicePart>();
        }

        [Test]
        public void Test_Execute_supports_nested_choices_in_the_same_part_simple()
        {
            using (OrderedExpectations)
            {
                Try_Simple_Multichoice(true, Expect_Evaluate_heuristic);
            }

            Execute<MultiChoicePart_Simple>();
        }

        [Test]
        public void Test_Execute_supports_nested_choices_in_the_same_part_complex()
        {
            using (OrderedExpectations)
            {
                #region First decision

                using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY)
                      .And_Garbage<ChoiceB>()
                      .And_Garbage<ChoiceA>())
                {
                    using (BeginNode(true, ChoiceA.ResultX))
                    {
                        Try_Choice(ChoiceA.ResultX);

                        using (Expect_Choice(true, ChoiceB.Result1, ChoiceB.Result2)
                              .And_Garbage<ChoiceA>())
                        {
                            using (BeginNode(true, ChoiceB.Result1))
                            {
                                Try_Choice(ChoiceA.ResultX);
                                Try_Choice(ChoiceB.Result1);

                                using (Expect_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
                                {
                                    using (BeginNode(true, ChoiceA.ResultX))
                                    {
                                        Try_Choice(ChoiceA.ResultX);
                                        Try_Choice(ChoiceB.Result1);
                                        Try_Choice(ChoiceA.ResultX);

                                        Expect_Evaluate_heuristic();
                                    }

                                    using (BeginNode(true, ChoiceA.ResultY))
                                    {
                                        Try_Choice(ChoiceA.ResultX);
                                        Try_Choice(ChoiceB.Result1);
                                        Try_Choice(ChoiceA.ResultY);

                                        Expect_Evaluate_heuristic();
                                    }
                                }
                            }

                            using (BeginNode(true, ChoiceB.Result2))
                            {
                                Try_Choice(ChoiceA.ResultX);
                                Try_Choice(ChoiceB.Result2);

                                Expect_Evaluate_heuristic();
                            }
                        }
                    }

                    using (BeginNode(true, ChoiceA.ResultY))
                    {
                        Try_Choice(ChoiceA.ResultY);

                        using (Expect_Choice(true, ChoiceB.Result1, ChoiceB.Result2)
                              .And_Garbage<ChoiceA>())
                        {
                            using (BeginNode(true, ChoiceB.Result1))
                            {
                                Try_Choice(ChoiceA.ResultY);
                                Try_Choice(ChoiceB.Result1);

                                using (Expect_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
                                {
                                    using (BeginNode(true, ChoiceA.ResultX))
                                    {
                                        Try_Choice(ChoiceA.ResultY);
                                        Try_Choice(ChoiceB.Result1);
                                        Try_Choice(ChoiceA.ResultX);

                                        Expect_Evaluate_heuristic();
                                    }

                                    using (BeginNode(true, ChoiceA.ResultY))
                                    {
                                        Try_Choice(ChoiceA.ResultY);
                                        Try_Choice(ChoiceB.Result1);
                                        Try_Choice(ChoiceA.ResultY);

                                        Expect_Evaluate_heuristic();
                                    }
                                }
                            }

                            using (BeginNode(true, ChoiceB.Result2))
                            {
                                Try_Choice(ChoiceA.ResultY);
                                Try_Choice(ChoiceB.Result2);

                                Expect_Evaluate_heuristic();
                            }
                        }
                    }
                }

                #endregion
            }

            Execute<MultiChoicePart_Complex>();
        }

        private class SecondChoiceController : IMockController
        {
            private readonly IMockController m_underlyingController;
            private ChoiceA? m_firstChoice;

            public SecondChoiceController(IMockController underlyingController, ChoiceA firstChoice)
            {
                m_underlyingController = underlyingController;
                m_firstChoice = firstChoice;
            }

            public ChoiceA ChoiceA(Part.Context context)
            {
                if (m_firstChoice.HasValue)
                {
                    ChoiceA result = m_firstChoice.Value;
                    m_firstChoice = null;
                    return result;
                }

                Assert.IsNull(m_firstChoice);
                return m_underlyingController.ChoiceA(context);
            }

            public ChoiceB ChoiceB(Part.Context context)
            {
                Assert.IsNull(m_firstChoice);
                return m_underlyingController.ChoiceB(context);
            }
        }

        [Test]
        public void Test_Execute_supports_nested_choices_in_the_same_part_when_on_second_choice()
        {
            using (OrderedExpectations)
            {
                Try_Choice(ChoiceA.ResultX); // Not actually part of test

                using (Expect_Root_Choice(true, ChoiceB.Result1, ChoiceB.Result2))
                {
                    using (BeginNode(true, ChoiceB.Result1))
                    {
                        Try_Choice(ChoiceA.ResultX);
                        Try_Choice(ChoiceB.Result1);

                        Expect_Evaluate_heuristic();
                    }

                    using (BeginNode(true, ChoiceB.Result2))
                    {
                        Try_Choice(ChoiceA.ResultX);
                        Try_Choice(ChoiceB.Result2);

                        Expect_Evaluate_heuristic();
                    }
                }
            }

            Execute<MultiChoicePart_Simple>(new SecondChoiceController(m_driver.Controller, ChoiceA.ResultX));
        }

        [Test]
        public void Test_If_tree_is_terminal_the_driver_evaluates_the_heuristic()
        {
            using (OrderedExpectations)
            {
                Expect_GetDefaultChoice(ChoiceA.ResultX);
                Expect_IsTerminal<ChoiceA>();
                Expect_Evaluate_heuristic();

                Try_Choice_Anything<ChoiceA>();
            }

            Execute<SingleChoicePart>();
        }

        [Test]
        public void Test_Execute_tries_all_the_given_possibilities_with_Multi_part_case()
        {
            using (OrderedExpectations)
            {
                using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
                {
                    using (BeginNode(true, ChoiceA.ResultX))
                    {
                        Try_Choice(ChoiceA.ResultX);

                        using (Expect_Choice(false, ChoiceA.ResultX, ChoiceA.ResultY))
                        {
                            using (BeginNode(false, ChoiceA.ResultX))
                            {
                                Try_Choice(ChoiceA.ResultX);

                                Expect_Evaluate_heuristic();
                            }

                            using (BeginNode(false, ChoiceA.ResultY))
                            {
                                Try_Choice(ChoiceA.ResultY);

                                Expect_Evaluate_heuristic();
                            }
                        }
                    }

                    using (BeginNode(true, ChoiceA.ResultY))
                    {
                        Try_Choice(ChoiceA.ResultY);

                        using (Expect_Choice(false, ChoiceA.ResultX, ChoiceA.ResultY))
                        {
                            using (BeginNode(false, ChoiceA.ResultX))
                            {
                                Try_Choice(ChoiceA.ResultX);

                                Expect_Evaluate_heuristic();
                            }

                            using (BeginNode(false, ChoiceA.ResultY))
                            {
                                Try_Choice(ChoiceA.ResultY);

                                Expect_Evaluate_heuristic();
                            }
                        }
                    }
                }
            }

            Execute<SingleChoicePart_Chained>();
        }

        [Test]
        public void Test_Execute_tries_all_possibilities_in_a_multi_part_multi_choice_case()
        {
            using (OrderedExpectations)
            {
                Try_Simple_Multichoice(true, () => Try_Simple_Multichoice(false, Expect_Evaluate_heuristic));
            }

            Execute<MultiChoiceMultiPart>();
        }

#warning TODO

        //#region Transactions

        //[Test]
        //public void Test_When_a_transaction_is_ended_during_the_driver_is_active_the_search_continues_and_the_transaction_remains_active()
        //{
        //    ITransaction activeTransaction = m_game.TransactionStack.BeginTransaction();

        //    using (OrderedExpectations)
        //    {
        //        using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
        //        {
        //            using (BeginNode(true, ChoiceA.ResultX))
        //            {
        //                Try_Choice(ChoiceA.ResultX).Callback(delegate(ChoiceA choice)
        //                {
        //                    activeTransaction.Dispose();
        //                    return true;
        //                });

        //                Expect_Evaluate_heuristic();
        //            }

        //            using (BeginNode(true, ChoiceA.ResultY))
        //            {
        //                Try_Choice(ChoiceA.ResultY);

        //                Expect_Evaluate_heuristic();
        //            }
        //        }
        //    }

        //    Execute<SingleChoicePart>();

        //    Assert.AreEqual(activeTransaction, m_game.TransactionStack.CurrentTransaction);
        //}

        //[Test]
        //public void Test_When_a_transaction_is_rollbacked_during_the_driver_is_active_the_search_stops_and_the_transaction_remains_active()
        //{
        //    ITransaction activeTransaction = m_game.TransactionStack.BeginTransaction();

        //    using (OrderedExpectations)
        //    {
        //        using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
        //        {
        //            using (BeginNode(true, ChoiceA.ResultX))
        //            {
        //                Try_Choice(ChoiceA.ResultX).Callback(delegate(ChoiceA choice)
        //                {
        //                    activeTransaction.Rollback();
        //                    return true;
        //                });

        //                Expect_Discard();
        //            }

        //            using (BeginNode(true, ChoiceA.ResultY))
        //            {
        //                Try_Choice(ChoiceA.ResultY);

        //                Expect_Evaluate_heuristic();
        //            }
        //        }
        //    }

        //    Execute<SingleChoicePart>();

        //    Assert.AreEqual(activeTransaction, m_game.TransactionStack.CurrentTransaction);
        //}

        //[Test]
        //public void Test_When_a_transaction_is_started_in_a_part_and_ended_during_another_nothing_happens()
        //{
        //    ITransaction activeTransaction = m_game.TransactionStack.BeginTransaction();
        //    ITransaction transaction = null;

        //    using (OrderedExpectations)
        //    {
        //        using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
        //        {
        //            using (BeginNode(true, ChoiceA.ResultX))
        //            {
        //                Try_Choice(ChoiceA.ResultX).Callback(delegate(ChoiceA choice)
        //                {
        //                    transaction = m_game.TransactionStack.BeginTransaction();
        //                    return true;
        //                });

        //                // IsTerminal not asked because in user transaction
        //                using (Expect_Choice(false, false, ChoiceA.ResultX, ChoiceA.ResultY))
        //                {
        //                    using (BeginNode(false, ChoiceA.ResultX))
        //                    {
        //                        Try_Choice(ChoiceA.ResultX).Callback(delegate(ChoiceA choice)
        //                        {
        //                            transaction.Dispose();
        //                            return true;
        //                        });

        //                        Expect_Evaluate_heuristic();
        //                    }

        //                    using (BeginNode(false, ChoiceA.ResultY))
        //                    {
        //                        Try_Choice(ChoiceA.ResultY).Callback(delegate(ChoiceA choice)
        //                        {
        //                            transaction.Dispose();
        //                            return true;
        //                        });

        //                        Expect_Evaluate_heuristic();
        //                    }
        //                }
        //            }

        //            using (BeginNode(true, ChoiceA.ResultY))
        //            {
        //                Try_Choice(ChoiceA.ResultY).Callback(delegate(ChoiceA choice)
        //                {
        //                    transaction = m_game.TransactionStack.BeginTransaction();
        //                    return true;
        //                });

        //                // IsTerminal not asked because in user transaction
        //                using (Expect_Choice(false, false, ChoiceA.ResultX, ChoiceA.ResultY))
        //                {
        //                    using (BeginNode(false, ChoiceA.ResultX))
        //                    {
        //                        Try_Choice(ChoiceA.ResultX).Callback(delegate(ChoiceA choice)
        //                        {
        //                            transaction.Rollback();
        //                            return true;
        //                        });

        //                        Expect_Discard();
        //                    }

        //                    using (BeginNode(false, ChoiceA.ResultY))
        //                    {
        //                        Try_Choice(ChoiceA.ResultY).Callback(delegate(ChoiceA choice)
        //                        {
        //                            transaction.Rollback();
        //                            return true;
        //                        });

        //                        Expect_Discard();
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    Execute<SingleChoicePart_Chained>();

        //    Assert.AreEqual(activeTransaction, m_game.TransactionStack.CurrentTransaction);
        //}

        //[Test]
        //public void Test_Transactions_started_during_AI_are_not_really_real()
        //{
        //    ITransaction activeTransaction = m_game.TransactionStack.BeginTransaction();

        //    using (OrderedExpectations)
        //    {
        //        using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
        //        {
        //            using (BeginNode(true, ChoiceA.ResultX))
        //            {
        //                Try_Choice(ChoiceA.ResultX).Callback(delegate(ChoiceA choice)
        //                {
        //                    ITransaction newTransaction = m_game.TransactionStack.BeginTransaction();
        //                    Assert.AreNotEqual(activeTransaction, newTransaction);
        //                    Assert.AreNotEqual(newTransaction, m_game.TransactionStack.CurrentTransaction);
        //                    newTransaction.Dispose();
        //                    return true;
        //                });

        //                Expect_Evaluate_heuristic();
        //            }

        //            using (BeginNode(true, ChoiceA.ResultY))
        //            {
        //                Try_Choice(ChoiceA.ResultY);

        //                Expect_Evaluate_heuristic();
        //            }
        //        }
        //    }

        //    Execute<SingleChoicePart>();

        //    Assert.AreEqual(activeTransaction, m_game.TransactionStack.CurrentTransaction);
        //}

        //[Test]
        //public void Test_Transactions_started_during_AI_must_be_ended_or_rollbacked_before_evaluation()
        //{
        //    using (OrderedExpectations)
        //    {
        //        using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
        //        {
        //            using (BeginNode(true, ChoiceA.ResultX))
        //            {
        //                Try_Choice(ChoiceA.ResultX).Callback<ChoiceA>(choice =>
        //                {
        //                    m_game.TransactionStack.BeginTransaction();
        //                    return true;
        //                });

        //                // Won't ask if the tree is terminal here
        //                using (Expect_Choice(true, false, ChoiceA.ResultX, ChoiceA.ResultY))
        //                {
        //                    using (BeginNode(true, ChoiceA.ResultX))
        //                    {
        //                        Try_Choice(ChoiceA.ResultX);
        //                        Expect_Evaluate_heuristic();
        //                    }

        //                    using (BeginNode(true, ChoiceA.ResultY))
        //                    {
        //                        Try_Choice(ChoiceA.ResultY);
        //                        Expect_Evaluate_heuristic();
        //                    }
        //                }
        //            }

        //            using (BeginNode(true, ChoiceA.ResultY))
        //            {
        //                Try_Choice(ChoiceA.ResultY);

        //                using (Expect_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
        //                {
        //                    using (BeginNode(true, ChoiceA.ResultX))
        //                    {
        //                        Try_Choice(ChoiceA.ResultX);
        //                        Expect_Evaluate_heuristic();
        //                    }

        //                    using (BeginNode(true, ChoiceA.ResultY))
        //                    {
        //                        Try_Choice(ChoiceA.ResultY);
        //                        Expect_Evaluate_heuristic();
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    Execute<SingleChoicePart_Chained>();
        //}

        //#endregion

        [Test]
        public void Test_a_part_that_returns_itself_is_considered_a_rollback()
        {
            using (OrderedExpectations)
            {
                using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
                {
                    using (BeginNode(true, ChoiceA.ResultX))
                    {
                        Try_Choice(ChoiceA.ResultX);

                        Expect_Evaluate_heuristic();
                    }

                    using (BeginNode(true, ChoiceA.ResultY))
                    {
                        Try_Choice(ChoiceA.ResultY);

                        Expect_Discard();
                    }
                }
            }

            Execute<OverflowPart>();
        }

        [Test]
        public void Test_A_part_is_always_executed_from_the_same_data_point()
        {
            using (OrderedExpectations)
            {
                Try_Simple_Multichoice(true, Expect_Evaluate_heuristic);
            }

            Execute<ModifyingPartWithChoice>();
        }

        [Test]
        public void Test_Parts_that_modify_data_but_dont_contain_choices_are_correctly_handled()
        {
            using (OrderedExpectations)
            {
                using (Expect_Root_Choice(true, ChoiceA.ResultX, ChoiceA.ResultY))
                {
                    using (BeginNode(true, ChoiceA.ResultX))
                    {
                        Try_Choice(ChoiceA.ResultX);

                        Try_Simple_Multichoice(true, () => Try_Simple_Multichoice(false, Expect_Evaluate_heuristic));
                    }

                    using (BeginNode(true, ChoiceA.ResultY))
                    {
                        Try_Choice(ChoiceA.ResultY);

                        Try_Simple_Multichoice(true, () => Try_Simple_Multichoice(false, Expect_Evaluate_heuristic));
                    }
                }
            }

            Execute<ModifyingPart>();
        }

        [Test]
        public void Test_A_part_is_always_executed_from_the_same_data_point_on_second_choice()
        {
            using (OrderedExpectations)
            {
                Try_Choice(ChoiceA.ResultX); // Not actually part of test

                using (Expect_Root_Choice(true, ChoiceB.Result1, ChoiceB.Result2))
                {
                    using (BeginNode(true, ChoiceB.Result1))
                    {
                        Try_Choice(ChoiceA.ResultX);
                        Try_Choice(ChoiceB.Result1);

                        Expect_Evaluate_heuristic();
                    }

                    using (BeginNode(true, ChoiceB.Result2))
                    {
                        Try_Choice(ChoiceA.ResultX);
                        Try_Choice(ChoiceB.Result2);

                        Expect_Evaluate_heuristic();
                    }
                }
            }

            Execute<ModifyingPartWithChoice>(new SecondChoiceController(m_driver.Controller, ChoiceA.ResultX));
        }

        #endregion
    }
}



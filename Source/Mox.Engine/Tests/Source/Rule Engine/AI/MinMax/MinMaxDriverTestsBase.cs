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
using System.Linq;

using Mox.Flow;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

using Is = Rhino.Mocks.Constraints.Is;

namespace Mox.AI
{
    public abstract class MinMaxDriverTestsBase : BaseGameTests
    {
        #region Inner Types

        #region Misc

        private static class TurnDataExtensions
        {
            public static readonly Property<int> MyProperty = Property<int>.RegisterAttachedProperty("MyProperty", typeof(TurnData));
        }

        public enum ChoiceAResult
        {
            ResultX,
            ResultY
        }

        public class ChoiceA : Choice
        {
            public ChoiceA(Resolvable<Player> player)
                : base(player)
            {
            }

            #region Overrides of Choice

            public override object DefaultValue
            {
                get { return ChoiceAResult.ResultX; }
            }

            #endregion
        }

        public interface IChoiceVerifier
        {
            void ChoiceA(ChoiceAResult choice);
        }

        private class NullDecisionMaker : IChoiceDecisionMaker
        {
            #region Implementation of IChoiceDecisionMaker

            public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
            {
                throw new InvalidProgramException();
            }

            #endregion
        }

        #endregion

        #region Parts

        protected abstract class PartBase : Part
        {
            public IChoiceVerifier ChoiceVerifier
            {
                get;
                set;
            }

            protected static void ModifyData(Context context, int initial, int final)
            {
                // This tests that parts are always ran with the same data.
                Assert.AreEqual(initial, context.Game.TurnData.GetValue(TurnDataExtensions.MyProperty));
                context.Game.TurnData.SetValue(TurnDataExtensions.MyProperty, final);
            }
        }

        protected abstract class ChoicePart : PartBase, IChoicePart
        {
            #region Variables

            private readonly Resolvable<Player> m_player;

            #endregion

            #region Constructor

            protected ChoicePart(Resolvable<Player> player)
            {
                m_player = player;
            }

            #endregion

            #region Properties

            protected Resolvable<Player> Player
            {
                get { return m_player; }
            }

            #endregion

            #region Overrides of Part<IMockController>

            public override sealed Part Execute(Context context)
            {
                return Execute(context, this.PopChoiceResult<ChoiceAResult>(context));
            }

            protected abstract Part Execute(Context context, ChoiceAResult result);

            #endregion

            #region Implementation of IChoicePart

            public Choice GetChoice(Sequencer sequencer)
            {
                return new ChoiceA(m_player);
            }

            #endregion
        }

        protected class SingleChoicePart : ChoicePart
        {
            #region Constructor

            public SingleChoicePart(Resolvable<Player> player)
                : base(player)
            {
            }

            #endregion

            #region Overrides of ChoicePart

            protected override Part Execute(Context context, ChoiceAResult result)
            {
                ChoiceVerifier.ChoiceA(result);
                return null;
            }

            #endregion
        }

        protected class SingleChoicePart_Chained : SingleChoicePart
        {
            public SingleChoicePart_Chained(Resolvable<Player> player)
                : base(player)
            {
            }

            protected override Part Execute(Context context, ChoiceAResult result)
            {
                base.Execute(context, result);
                return new SingleChoicePart(Player) { ChoiceVerifier = ChoiceVerifier };
            }
        }

        protected class SingleChoicePart_ChainedWithEmpty : SingleChoicePart
        {
            #region Inner Types

            private class EmptyPart : Part
            {
                public override Part Execute(Context context)
                {
                    return null;
                }
            }

            #endregion

            public SingleChoicePart_ChainedWithEmpty(Resolvable<Player> player)
                : base(player)
            {
            }

            protected override Part Execute(Context context, ChoiceAResult result)
            {
                base.Execute(context, result);
                return new EmptyPart();
            }
        }

        protected class SingleChoicePart_Chained_WithEndTransaction : SingleChoicePart_Chained
        {
            private readonly bool m_rollback;
            private readonly object m_token;

            public SingleChoicePart_Chained_WithEndTransaction(Resolvable<Player> player, object token, bool rollback)
                : base(player)
            {
                m_token = token;
                m_rollback = rollback;
            }

            protected override Part Execute(Context context, ChoiceAResult result)
            {
                var nextPart = base.Execute(context, result);
                context.Schedule(new EndTransactionPart(m_rollback, m_token));
                return nextPart;
            }
        }

        protected class OverflowPart : ChoicePart
        {
            public OverflowPart(Resolvable<Player> player)
                : base(player)
            {
            }

            protected override Part Execute(Context context, ChoiceAResult result)
            {
                ChoiceVerifier.ChoiceA(result);

                return result == ChoiceAResult.ResultX ? null : this;
            }
        }

        protected class ModifyingPart : ChoicePart
        {
            public ModifyingPart(Resolvable<Player> player)
                : base(player)
            {
            }

            protected override Part Execute(Context context, ChoiceAResult result)
            {
                var part1 = new ModifyingPartWithChoice(Player)
                {
                    ChoiceVerifier = ChoiceVerifier, 
                    StartIndex = 0
                };

                var part2 = new ModifyingPartWithChoice(Player)
                {
                    ChoiceVerifier = ChoiceVerifier, 
                    StartIndex = 3
                };

                ChoiceVerifier.ChoiceA(result);
                context.Schedule(part1);
                context.Schedule(new ModifyingPart_Impl());
                context.Schedule(part2);
                return null;
            }

            private class ModifyingPart_Impl : PartBase
            {
                public override Part Execute(Context context)
                {
                    ModifyData(context, 2, 3);
                    return null;
                }
            }
        }

        protected class ModifyingPartWithChoice : ChoicePart
        {
            public ModifyingPartWithChoice(Resolvable<Player> player)
                : base(player)
            {
            }

            public int StartIndex { get; set; }

            protected override Part Execute(Context context, ChoiceAResult result)
            {
                ModifyData(context, StartIndex + 0, StartIndex + 1);
                ChoiceVerifier.ChoiceA(result);
                ModifyData(context, StartIndex + 1, StartIndex + 2);
                return null;
            }
        }

        #endregion

        #endregion

        #region Variables

        private IChoiceVerifier m_mockChoiceVerifier;

        private IMinimaxTree m_tree;
        private IMinMaxAlgorithm m_algorithm;
        private IChoiceEnumeratorProvider m_choiceEnumeratorProvider;
        private ChoiceEnumerator m_choiceEnumerator;

        private Cancellable m_cancellable;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_mockChoiceVerifier = m_mockery.StrictMock<IChoiceVerifier>();

            m_tree = m_mockery.StrictMock<IMinimaxTree>();
            m_algorithm = m_mockery.StrictMock<IMinMaxAlgorithm>();
            m_choiceEnumeratorProvider = new MockRepository().StrictMock<IChoiceEnumeratorProvider>();
            m_choiceEnumerator = m_mockery.StrictMock<ChoiceEnumerator>();

            SetupResult.For(m_choiceEnumeratorProvider.GetEnumerator(null)).IgnoreArguments().Return(m_choiceEnumerator);
            m_choiceEnumeratorProvider.Replay();

            m_cancellable = new Cancellable();
        }

        #endregion

        #region Utilities

        #region Execute

        private void Execute(PartBase part)
        {
            var driver = CreateMinMaxDriver();

            part.ChoiceVerifier = m_mockChoiceVerifier;
            Sequencer sequencer = new Sequencer(m_game, part);
            m_mockery.Test(() => driver.Run(sequencer));
        }

        private void ExecuteWithChoice(PartBase part, Choice choice, object choiceResult)
        {
            var driver = CreateMinMaxDriver();

            part.ChoiceVerifier = m_mockChoiceVerifier;
            Sequencer sequencer = new Sequencer(m_game, part);
            m_mockery.Test(() => driver.RunWithChoice(sequencer, choice, choiceResult));
        }

        private MinMaxDriver CreateMinMaxDriver()
        {
            AIEvaluationContext context = new AIEvaluationContext(m_tree, m_algorithm, m_choiceEnumeratorProvider);
            return CreateMinMaxDriver(context, m_cancellable);
        }

        protected abstract MinMaxDriver CreateMinMaxDriver(AIEvaluationContext context, ICancellable cancellable);

        #endregion

        #region Expectations

        #region Tree

        private IDisposable BeginNode(object result, bool cutoff = false)
        {
            m_tree.BeginNode(result, null);
            LastCall.IgnoreArguments().Constraints(Is.Equal(result), Is.Anything());

            return new DisposableHelper(() => Expect.Call(m_tree.EndNode()).Return(!cutoff));
        }

        private void InitializeNode(bool isMaximizing)
        {
            m_tree.InitializeNode(isMaximizing);
        }

        #endregion

        #region Verifier

        protected IMethodOptions<object> Try_Choice(ChoiceAResult choice)
        {
            m_mockChoiceVerifier.ChoiceA(choice);
            return LastCall.On(m_mockChoiceVerifier);
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

        private void Expect_IsMaximizingPlayer(bool isMaximizing)
        {
            Expect.Call(m_algorithm.IsMaximizingPlayer(m_playerA)).Return(isMaximizing);
        }

        private void Expect_EnumerateChoices<TChoice>(params TChoice[] choices)
        {
            Expect.Call(m_choiceEnumerator.EnumerateChoices(null, null)).
                   IgnoreArguments().
                   Return(choices.Cast<object>()).
                   Message(string.Format("Expected Choices={0} for {1} choice", choices, typeof(TChoice).Name));
        }

        protected void Expect_IsTerminal()
        {
            Expect_IsTerminal(true);
        }

        protected void Expect_IsTerminal(bool result)
        {
            Expect.Call(m_algorithm.IsTerminal(m_tree, m_game)).Return(result);
        }

        protected IDisposable Expect_Choice<TChoice>(bool isMaximizing, params TChoice[] choices)
        {
            return Expect_Choice(isMaximizing, true, choices);
        }

        protected IDisposable Expect_Choice<TChoice>(bool isMaximizing, bool askIsTerminal, params TChoice[] choices)
        {
            Expect_Choice_Impl(isMaximizing, askIsTerminal, choices);
            return new EmptyDisposable();
        }

        protected IDisposable Expect_Root_Choice()
        {
            return new EmptyDisposable();
        }
        
        private void Expect_Choice_Impl<TChoice>(bool isMaximizing, bool askIsTerminal, params TChoice[] choices)
        {
            if (askIsTerminal)
            {
                Expect_IsTerminal(false);
            }

            Expect_EnumerateChoices(choices);
            Expect_IsMaximizingPlayer(isMaximizing);
            InitializeNode(isMaximizing);
        }

        private class EmptyDisposable : IDisposable
        {
            public void Dispose()
            {}
        }

        #endregion

        #endregion

        #region Helpers

        private void Try_Simple_Multichoice(System.Action subAction, bool askIsTerminal = true)
        {
            using (Expect_Choice(true, askIsTerminal, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
            {
                using (BeginNode(ChoiceAResult.ResultX))
                {
                    Try_Choice(ChoiceAResult.ResultX);

                    subAction();
                }

                using (BeginNode(ChoiceAResult.ResultY))
                {
                    Try_Choice(ChoiceAResult.ResultY);

                    subAction();
                }
            }
        }

        #endregion

        #region Cancel

        private class Cancellable : ICancellable
        {
            public bool Cancel
            {
                get; set; 
            }
        }

        #endregion

        #endregion

        #region Tests
        
        [Test]
        public void Test_Execute_tries_all_the_given_possibilities_simple_case()
        {
            using (OrderedExpectations)
            {
                using (Expect_Choice(true, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX))
                    {
                        Try_Choice(ChoiceAResult.ResultX);

                        Expect_Evaluate_heuristic();
                    }

                    using (BeginNode(ChoiceAResult.ResultY))
                    {
                        Try_Choice(ChoiceAResult.ResultY);

                        Expect_Evaluate_heuristic();
                    }
                }
            }

            Execute(new SingleChoicePart(m_playerA));
        }

        [Test]
        public void Test_Evaluation_takes_place_at_the_end_even_if_no_choice_is_made_on_recursion()
        {
            using (OrderedExpectations)
            {
                using (Expect_Choice(true, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX))
                    {
                        Try_Choice(ChoiceAResult.ResultX);

                        Expect_IsTerminal();
                        Expect_Evaluate_heuristic();
                    }

                    using (BeginNode(ChoiceAResult.ResultY))
                    {
                        Try_Choice(ChoiceAResult.ResultY);

                        Expect_IsTerminal();
                        Expect_Evaluate_heuristic();
                    }
                }
            }

            Execute(new SingleChoicePart_ChainedWithEmpty(m_playerA));
        }

        [Test]
        public void Test_Can_force_the_first_choices_to_be_tried()
        {
            using (OrderedExpectations)
            {
                using (Expect_Root_Choice())
                {
                    using (BeginNode(ChoiceAResult.ResultY))
                    {
                        Try_Choice(ChoiceAResult.ResultY);

                        Expect_Evaluate_heuristic();
                    }
                }
            }

            ExecuteWithChoice(new SingleChoicePart(m_playerA), new ChoiceA(m_playerA), ChoiceAResult.ResultY);
        }

        [Test]
        public void Test_Can_force_the_first_choices_to_be_tried_with_subsequent_choices()
        {
            using (OrderedExpectations)
            {
                using (Expect_Root_Choice())
                using (BeginNode(ChoiceAResult.ResultY))
                {
                    Try_Choice(ChoiceAResult.ResultY);

                    using (Expect_Choice(true, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                    {
                        using (BeginNode(ChoiceAResult.ResultX))
                        {
                            Try_Choice(ChoiceAResult.ResultX);

                            Expect_Evaluate_heuristic();
                        }

                        using (BeginNode(ChoiceAResult.ResultY))
                        {
                            Try_Choice(ChoiceAResult.ResultY);

                            Expect_Evaluate_heuristic();
                        }
                    }
                }
            }

            ExecuteWithChoice(new SingleChoicePart_Chained(m_playerA), new ChoiceA(m_playerA), ChoiceAResult.ResultY);
        }

        [Test]
        public void Test_Execute_breaks_when_cutoff()
        {
            using (OrderedExpectations)
            {
                using (Expect_Choice(false, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX, true))
                    {
                        Try_Choice(ChoiceAResult.ResultX);
                        Expect_Evaluate_heuristic();
                    }
                }
            }

            Execute(new SingleChoicePart(m_playerA));
        }

        [Test]
        public void Test_If_tree_is_terminal_the_driver_evaluates_the_heuristic()
        {
            using (OrderedExpectations)
            {
                Expect_IsTerminal();
                Expect_Evaluate_heuristic();
            }

            Execute(new SingleChoicePart(m_playerA));
        }

        [Test]
        public void Test_Execute_tries_all_the_given_possibilities_with_Multi_part_case()
        {
            using (OrderedExpectations)
            {
                using (Expect_Choice(true, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX))
                    {
                        Try_Choice(ChoiceAResult.ResultX);

                        using (Expect_Choice(false, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                        {
                            using (BeginNode(ChoiceAResult.ResultX))
                            {
                                Try_Choice(ChoiceAResult.ResultX);

                                Expect_Evaluate_heuristic();
                            }

                            using (BeginNode(ChoiceAResult.ResultY))
                            {
                                Try_Choice(ChoiceAResult.ResultY);

                                Expect_Evaluate_heuristic();
                            }
                        }
                    }

                    using (BeginNode(ChoiceAResult.ResultY))
                    {
                        Try_Choice(ChoiceAResult.ResultY);

                        using (Expect_Choice(false, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                        {
                            using (BeginNode(ChoiceAResult.ResultX))
                            {
                                Try_Choice(ChoiceAResult.ResultX);

                                Expect_Evaluate_heuristic();
                            }

                            using (BeginNode(ChoiceAResult.ResultY))
                            {
                                Try_Choice(ChoiceAResult.ResultY);

                                Expect_Evaluate_heuristic();
                            }
                        }
                    }
                }
            }

            Execute(new SingleChoicePart_Chained(m_playerA));
        }

        [Test]
        public void Test_Execution_will_stop_when_job_is_cancelled()
        {
            using (OrderedExpectations)
            {
                Expect_IsTerminal(false);

                using (Expect_Choice(true, false, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX))
                    {
                        Try_Choice(ChoiceAResult.ResultX);

                        Expect_Evaluate_heuristic();
                    }

                    LastCall.Callback(() => m_cancellable.Cancel = true);
                }
            }

            Execute(new SingleChoicePart(m_playerA));
        }

        #region Transactions

        [Test]
        public void Test_When_a_transaction_is_ended_during_the_driver_is_active_the_search_continues_and_the_transaction_remains_active()
        {
            const string Token = "Banane";

            m_game.Controller.BeginTransaction(Token);

            using (OrderedExpectations)
            {
                Try_Simple_Multichoice(() =>
                {
                    Expect_IsTerminal(false);
                    Try_Simple_Multichoice(Expect_Evaluate_heuristic);
                });
            }

            Execute(new SingleChoicePart_Chained_WithEndTransaction(m_playerA, Token, false));

            m_game.Controller.EndTransaction(false, Token); // Check that the transaction is intact
        }

        [Test]
        public void Test_When_a_transaction_is_rollbacked_during_the_driver_is_active_the_search_stops_and_the_transaction_remains_active()
        {
            const string Token = "Banane";

            m_game.Controller.BeginTransaction(Token);

            using (OrderedExpectations)
            {
                using (Expect_Choice(true, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX))
                    {
                        Try_Choice(ChoiceAResult.ResultX);

                        Expect_IsTerminal(false);
                        Expect_Discard();
                    }

                    using (BeginNode(ChoiceAResult.ResultY))
                    {
                        Try_Choice(ChoiceAResult.ResultY);

                        Expect_IsTerminal(false);
                        Expect_Discard();
                    }
                }
            }

            Execute(new SingleChoicePart_Chained_WithEndTransaction(m_playerA, Token, true));

            m_game.Controller.EndTransaction(false, Token); // Check that the transaction is intact
        }

        private class Test_Transactions_started_during_AI_must_be_ended_or_rollbacked_before_evaluation_Part : SingleChoicePart
        {
            private const string Token = "zeToken";

            public Test_Transactions_started_during_AI_must_be_ended_or_rollbacked_before_evaluation_Part(Resolvable<Player> player)
                : base(player)
            {
            }

            protected override Part Execute(Context context, ChoiceAResult result)
            {
                base.Execute(context, result);

                if (result == ChoiceAResult.ResultX)
                {
                    context.Schedule(new BeginTransactionPart(Token));
                }

                context.Schedule(new SingleChoicePart(Player) { ChoiceVerifier = ChoiceVerifier });
                return new SingleChoicePart(Player) { ChoiceVerifier = ChoiceVerifier };
            }
        }

        [Test]
        public void Test_Transactions_started_during_AI_must_be_ended_or_rollbacked_before_evaluation()
        {
            const string Token = "Banane";

            m_game.Controller.BeginTransaction(Token);
            
            using (OrderedExpectations)
            {
                using (Expect_Choice(true, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX))
                    {
                        Try_Choice(ChoiceAResult.ResultX);

                        Try_Simple_Multichoice(() => Try_Simple_Multichoice(Expect_Evaluate_heuristic, false));
                    }

                    using (BeginNode(ChoiceAResult.ResultY))
                    {
                        Try_Choice(ChoiceAResult.ResultY);

                        Try_Simple_Multichoice(() => Try_Simple_Multichoice(Expect_Evaluate_heuristic));
                    }
                }
            }

            Execute(new Test_Transactions_started_during_AI_must_be_ended_or_rollbacked_before_evaluation_Part(m_playerA));
        }

        [Test]
        public void Test_Transactions_started_before_AI_must_be_ended_or_rollbacked_before_evaluation()
        {
            const string Token = "Banane";

            Sequencer sequencer = new Sequencer(m_game, new BeginTransactionPart(Token));
            sequencer.Run(new NullDecisionMaker());

            using (OrderedExpectations)
            {
                using (Expect_Choice(true, false, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX))
                    {
                        Try_Choice(ChoiceAResult.ResultX);

                        Try_Simple_Multichoice(Expect_Evaluate_heuristic, false);
                    }

                    using (BeginNode(ChoiceAResult.ResultY))
                    {
                        Try_Choice(ChoiceAResult.ResultY);

                        Try_Simple_Multichoice(Expect_Evaluate_heuristic, false);
                    }
                }
            }

            Execute(new SingleChoicePart_Chained(m_playerA));

            m_game.Controller.EndTransaction(false, Token); // Check that the original is still valid.
        }

        #endregion

        [Test]
        public void Test_a_part_that_returns_itself_is_considered_a_rollback()
        {
            using (OrderedExpectations)
            {
                using (Expect_Choice(true, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX))
                    {
                        Try_Choice(ChoiceAResult.ResultX);

                        Expect_Evaluate_heuristic();
                    }

                    using (BeginNode(ChoiceAResult.ResultY))
                    {
                        Try_Choice(ChoiceAResult.ResultY);

                        Expect_Discard();
                    }
                }
            }

            Execute(new OverflowPart(m_playerA));
        }

        [Test]
        public void Test_Parts_that_modify_data_but_dont_contain_choices_are_correctly_handled()
        {
            using (OrderedExpectations)
            {
                using (Expect_Choice(true, ChoiceAResult.ResultX, ChoiceAResult.ResultY))
                {
                    using (BeginNode(ChoiceAResult.ResultX))
                    {
                        Try_Choice(ChoiceAResult.ResultX);

                        Try_Simple_Multichoice(() =>
                        {
                            Expect_IsTerminal(false);
                            Try_Simple_Multichoice(Expect_Evaluate_heuristic);
                        });
                    }

                    using (BeginNode(ChoiceAResult.ResultY))
                    {
                        Try_Choice(ChoiceAResult.ResultY);

                        Try_Simple_Multichoice(() =>
                        {
                            Expect_IsTerminal(false);
                            Try_Simple_Multichoice(Expect_Evaluate_heuristic);
                        });
                    }
                }
            }

            Execute(new ModifyingPart(m_playerA));
        }

        #endregion
    }
}



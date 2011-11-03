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
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Flow
{
    [TestFixture]
    public class SequencerTests : PartTestUtilities
    {
        #region Inner Types

        public interface IMockController
        {
            int MakeChoice(int choice);
        }

        public abstract class MyPart : Part<IMockController>
        {
        }

        public class MakeChoicePart : Part<IMockController>
        {
            private readonly int[] m_expectedChoices;

            public MakeChoicePart(params int[] expectedChoices)
            {
                m_expectedChoices = expectedChoices;
            }

            public override ControllerAccess ControllerAccess
            {
                get
                {
                    return ControllerAccess.Multiple;
                }
            }

            public override Part<IMockController> Execute(Context context)
            {
                foreach (int choice in m_expectedChoices)
                {
                    Assert.AreEqual(choice, context.Controller.MakeChoice(choice));
                }
                return null;
            }
        }

        #endregion

        #region Variables

        private Sequencer<IMockController> m_sequencer;
        private Part<IMockController> m_initialPart;

        private new IMockController m_controller;

        #endregion

        #region Setup

        public override void Setup()
        {
            base.Setup();

            m_initialPart = CreateMockPart();
            m_sequencer = new Sequencer<IMockController>(m_initialPart, m_game);

            m_controller = m_mockery.StrictMock<IMockController>();
        }

        #endregion

        #region Utility

        private Part<IMockController> CreateMockPart()
        {
            return CreateMockPart<IMockController>();
        }

        private void Expect_Part_Execute(Part<IMockController> part)
        {
            Expect_Part_Execute(part, null);
        }

        private void Expect_Part_Execute(Part<IMockController> part, Part<IMockController> nextPart)
        {
            Expect_Part_Execute(part, nextPart, null);
        }

        private void Expect_Part_Execute(Part<IMockController> part, Part<IMockController> nextPart, Action<Part<IMockController>.Context> action)
        {
            SetupResult.For(part.ControllerAccess).Return(ControllerAccess.Multiple);

            Expect.Call(part.Execute(null)).Return(nextPart).IgnoreArguments().Callback<Part<IMockController>.Context>(context =>
            {
                Assert.AreEqual(m_sequencer, context.Sequencer);

                if (action != null)
                {
                    action(context);
                }
                return true;
            });
        }

        private void Assert_RunOnce(SequencerResult expectedResult)
        {
            Assert_RunOnce(m_sequencer, expectedResult);
        }

        private void Assert_RunOnce(Sequencer<IMockController> sequencer, SequencerResult expectedResult)
        {
            m_mockery.Test(() => Assert.AreEqual(expectedResult, sequencer.RunOnce(m_controller)));
        }

        private void Test_Run()
        {
            m_mockery.Test(() => m_sequencer.Run(m_controller));
        }

        #endregion

        #region Tests

        #region Sequencing

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_game, m_sequencer.Game);
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new Sequencer<IMockController>(null, m_game); });
            Assert.Throws<ArgumentNullException>(delegate { new Sequencer<IMockController>(m_initialPart, null); });
        }

        [Test]
        public void Test_RunOnce_runs_the_next_scheduled_task()
        {
            Expect_Part_Execute(m_initialPart);

            Assert_RunOnce(SequencerResult.Continue);
        }

        [Test]
        public void Test_RunOnce_returns_stop_if_the_Context_is_stopped_during_execution()
        {
            Part<IMockController> part = CreateMockPart();

            Expect_Part_Execute(m_initialPart, null, context =>
            {
                context.Schedule(part);
                context.Stop = true;
            });

            Assert_RunOnce(SequencerResult.Stop);
            Assert.That(m_sequencer.IsEmpty);
        }

        [Test]
        public void Test_RunOnce_returns_stop_if_the_game_ends_and_all_scheduled_parts_are_cancelled()
        {
            Part<IMockController> part = CreateMockPart();

            Expect_Part_Execute(m_initialPart, null, context =>
            {
                context.Schedule(part);
                context.Game.State.Winner = m_playerA;
            });

            Assert_RunOnce(SequencerResult.Stop);
            Assert.That(m_sequencer.IsEmpty);
        }

        [Test]
        public void Test_RunOnce_returns_retry_if_part_returns_itself()
        {
            Part<IMockController> part = CreateMockPart();

            Expect_Part_Execute(m_initialPart, m_initialPart, context => context.Schedule(part));

            Assert_RunOnce(SequencerResult.Retry);
            Assert.IsFalse(m_sequencer.IsEmpty);
        }

        [Test]
        public void Test_Can_schedule_additional_tasks_during_execution()
        {
            var subTask1 = CreateMockPart();
            var subTask2 = CreateMockPart();
            Expect_Part_Execute(m_initialPart, null, context =>
            {
                context.Schedule(subTask1);
                context.Schedule(subTask2);
            });

            Assert_RunOnce(SequencerResult.Continue);
            Assert.IsFalse(m_sequencer.IsEmpty);

            Expect_Part_Execute(subTask1);
            Assert_RunOnce(SequencerResult.Continue);
            Assert.IsFalse(m_sequencer.IsEmpty);

            Expect_Part_Execute(subTask2);
            Assert_RunOnce(SequencerResult.Continue);
            Assert.IsTrue(m_sequencer.IsEmpty);
        }

        [Test]
        public void Test_Part_returns_the_next_part_to_execute()
        {
            var nextTask = CreateMockPart();

            Expect_Part_Execute(m_initialPart, nextTask);
            Assert_RunOnce(SequencerResult.Continue);

            Expect_Part_Execute(nextTask);
            Assert_RunOnce(SequencerResult.Continue);
        }

        [Test]
        public void Test_NextPart_returns_the_next_part_to_execute()
        {
            var nextTask = CreateMockPart();

            Expect_Part_Execute(m_initialPart, nextTask);
            Assert_RunOnce(SequencerResult.Continue);

            Assert.AreEqual(nextTask, m_sequencer.NextPart);
        }

        [Test]
        public void Test_Complex_case()
        {
            var subTask1 = CreateMockPart();
            var subTask2 = CreateMockPart();
            var nextPart = CreateMockPart();

            Expect_Part_Execute(m_initialPart, nextPart, context =>
            {
                context.Schedule(subTask1);
                context.Schedule(subTask2);
            });
            Assert_RunOnce(SequencerResult.Continue);

            Expect_Part_Execute(subTask1);
            Assert_RunOnce(SequencerResult.Continue);

            Expect_Part_Execute(subTask2);
            Assert_RunOnce(SequencerResult.Continue);

            Expect_Part_Execute(nextPart);
            Assert_RunOnce(SequencerResult.Continue);
        }

        [Test]
        public void Test_RunOnce_throws_if_no_more_part_to_execute()
        {
            Expect_Part_Execute(m_initialPart);
            Assert_RunOnce(SequencerResult.Continue);

            Assert.Throws<InvalidOperationException>(() => m_sequencer.RunOnce(m_controller));
        }

        [Test]
        public void Test_Run_runs_until_all_parts_have_been_executed()
        {
            Expect_Part_Execute(m_initialPart);
            Test_Run();
        }

        [Test]
        public void Test_Run_runs_until_all_parts_have_been_executed_2()
        {
            var subTask1 = CreateMockPart();
            var subTask2 = CreateMockPart();
            var nextPart = CreateMockPart();

            Expect_Part_Execute(m_initialPart, nextPart, context =>
            {
                context.Schedule(subTask1);
                context.Schedule(subTask2);
            });
            Expect_Part_Execute(subTask1);
            Expect_Part_Execute(subTask2);
            Expect_Part_Execute(nextPart);

            Test_Run();
        }

        #region Cloning

        private void Test_Clone(Converter<Sequencer<IMockController>, Sequencer<IMockController>> cloner, Game expectedGame)
        {
            Test_Clone(cloner, expectedGame, null);
        }

        private void Test_Clone(Converter<Sequencer<IMockController>, Sequencer<IMockController>> cloner, Game expectedGame, System.Action callback)
        {
            var subTask1 = CreateMockPart();
            var subTask2 = CreateMockPart();

            Expect_Part_Execute(m_initialPart, null, context =>
            {
                context.Schedule(subTask1);
                context.Schedule(subTask2);
            });

            Assert_RunOnce(SequencerResult.Continue);

            object tokenA = new object();
            object tokenB = new object();

            m_sequencer.PushArgument(1, tokenA);
            m_sequencer.PushArgument(2, tokenB);

            using (Sequencer<IMockController> clone = cloner(m_sequencer))
            {
                Assert.AreEqual(expectedGame, clone.Game);

                SetupResult.For(subTask1.ControllerAccess).Return(ControllerAccess.Multiple);
                Expect.Call(subTask1.Execute(null)).IgnoreArguments().Return(null).Callback<Part<IMockController>.Context>(context =>
                {
                    Assert.AreEqual(clone, context.Sequencer);
                    Assert.AreEqual(2, context.PopArgument<int>(tokenB));
                    Assert.AreEqual(1, context.PopArgument<int>(tokenA));
                    return true;
                });

                if (callback != null)
                {
                    callback();
                }

                Assert_RunOnce(clone, SequencerResult.Continue);
            }

            // Make sure the original sequencer is left untouched
            Assert.AreEqual(2, m_sequencer.PopArgument<int>(tokenB));
            Expect_Part_Execute(subTask1);
            Assert_RunOnce(SequencerResult.Continue);
        }

        [Test]
        public void Test_Clone_returns_a_new_Sequencer_with_the_same_current_parts_and_properties_and_argument_stack()
        {
            Test_Clone(s => s.Clone(), m_game);
        }

        [Test]
        public void Test_Can_clone_using_a_new_game()
        {
            Game newGame = new Game();
            Test_Clone(s => s.Clone(newGame), newGame);
        }

        [Test]
        public void Test_Fork_does_everything_Clone_does()
        {
            Test_Clone(s => s.Fork(), m_game);
        }

        [Test]
        public void Test_Fork_clones_and_reverses_all_commands_made_until_beginning_of_part()
        {
            m_card.Power = 0;

            Expect_Part_Execute(m_initialPart, null, c =>
            {
                m_card.Power = 1;

                using (c.Sequencer.Fork())
                {
                    Assert.AreEqual(0, m_card.Power);
                }

                Assert.AreEqual(1, m_card.Power);
            });
            Assert_RunOnce(SequencerResult.Continue);
        }

        [Test]
        public void Test_Fork_clones_and_does_nothing_when_executing_ITransactionParts()
        {
            m_card.Power = 0;

            Part<IMockController> transactionPart = m_mockery.StrictMultiMock<Part<IMockController>>(typeof(ITransactionPart));

            m_sequencer = new Sequencer<IMockController>(transactionPart, m_game);

            Expect_Part_Execute(transactionPart, null, c =>
            {
                m_card.Power = 1;

                using (c.Sequencer.Fork())
                {
                    Assert.AreEqual(1, m_card.Power);
                }

                Assert.AreEqual(1, m_card.Power);
            });
            Assert_RunOnce(SequencerResult.Continue);
        }

        [Test]
        public void Test_BeginSequencingTransaction_returns_null_if_next_part_is_a_transaction_part()
        {
            Part<IMockController> transactionPart = m_mockery.StrictMultiMock<Part<IMockController>>(typeof(ITransactionPart));

            m_sequencer = new Sequencer<IMockController>(transactionPart, m_game);

            m_mockery.Test(() => Assert.IsNull(m_sequencer.BeginSequencingTransaction()));
        }

        [Test]
        public void Test_BeginSequencingTransaction_returns_null_if_next_part_doesnt_access_the_controller()
        {
            var subTask1 = CreateMockPart();
            SetupResult.For(subTask1.ControllerAccess).Return(ControllerAccess.None);

            m_sequencer = new Sequencer<IMockController>(subTask1, m_game);

            m_mockery.Test(() => Assert.IsNull(m_sequencer.BeginSequencingTransaction()));
        }

        [Test]
        public void Test_BeginSequencingTransaction_returns_a_valid_transaction_otherwise()
        {
            var subTask1 = CreateMockPart();
            SetupResult.For(subTask1.ControllerAccess).Return(ControllerAccess.Multiple);

            m_sequencer = new Sequencer<IMockController>(subTask1, m_game);

            m_mockery.Test(() =>
            {
                using (ITransaction transaction = m_sequencer.BeginSequencingTransaction())
                {
                    Assert.IsNotNull(transaction);
                    Assert.Fail();
#warning TODO
                    //Assert.AreEqual(transaction, m_game.TransactionStack.CurrentTransaction);
                    //Assert.AreEqual(Transactions.TransactionType.Master, transaction.Type);
                }
            });
        }

        #endregion

        #endregion

        #region Choice Recording

        [Test]
        public void Test_Choices_can_be_replayed_by_cloning_the_sequencer()
        {
            IMockController newController = new MockRepository().StrictMock<IMockController>();

            MakeChoicePart choicePart = new MakeChoicePart(3, 4);
            Expect_Part_Execute(m_initialPart, choicePart);

            Expect.Call(newController.MakeChoice(4)).Return(4).Message("2nd pass 4");
            newController.Replay();

            Expect.Call(m_controller.MakeChoice(3)).Return(3).Message("1st pass 3"); // Only asked once
            Expect.Call(m_controller.MakeChoice(4)).Message("1st pass 4").Do(new Converter<int, int>(delegate(int callback)
            {
                using (Sequencer<IMockController> clone = m_sequencer.Fork())
                {
                    Assert.AreEqual(SequencerResult.Continue, clone.RunOnce(newController));
                }
                return callback;
            }));

            Test_Run();
        }

        #endregion

        #endregion
    }
}

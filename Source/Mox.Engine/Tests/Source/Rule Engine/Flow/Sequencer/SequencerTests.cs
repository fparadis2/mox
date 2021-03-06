﻿// Copyright (c) François Paradis
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
    public class SequencerTests : BaseGameTests
    {
        #region Inner Types

        public abstract class MyPart : Part
        {
        }

        private class OtherPart : Part 
        {
            private readonly int m_hash;

            public OtherPart(int hash)
            {
                m_hash = hash;
            }

            public override Part Execute(Context context)
            {
                return null;
            }

            public override void ComputeHash(Hash hash)
            {
                hash.Add(m_hash);
            }
        }

        #endregion

        #region Variables

        private Sequencer m_sequencer;
        private Part m_initialPart;
        private IChoiceDecisionMaker m_decisionMaker;

        #endregion

        #region Setup

        public override void Setup()
        {
            base.Setup();

            m_initialPart = CreateMockPart();
            m_decisionMaker = m_mockery.StrictMock<IChoiceDecisionMaker>();
            m_sequencer = new Sequencer(m_game, m_initialPart);
        }

        #endregion

        #region Utility

        private Part CreateMockPart()
        {
            return m_mockery.StrictMock<Part>();
        }

        private Part CreateOtherPart(int hash)
        {
            return new OtherPart(hash);
        }

        private static void Expect_Part_Execute(Part part, Part nextPart = null, Action<Part.Context> action = null)
        {
            Expect.Call(part.Execute(null)).Return(nextPart).IgnoreArguments().Callback<Part.Context>(context =>
            {
                if (action != null)
                {
                    action(context);
                }
                return true;
            });
        }

        private void Expect_Make_Decision(Choice choice, object result)
        {
            Expect.Call(m_decisionMaker.MakeChoiceDecision(m_sequencer, choice)).Return(result);
        }

        private void Assert_RunOnce(SequencerResult expectedResult)
        {
            Assert_RunOnce(m_sequencer, expectedResult);
        }

        private void Assert_RunOnce(Sequencer sequencer, SequencerResult expectedResult)
        {
            m_mockery.Test(() => Assert.AreEqual(expectedResult, sequencer.RunOnce(m_decisionMaker)));
        }

        private void Test_Run()
        {
            m_mockery.Test(() => m_sequencer.Run(m_decisionMaker));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_game, m_sequencer.Game);
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new Sequencer(m_game, null); });
            Assert.Throws<ArgumentNullException>(delegate { new Sequencer(null, m_initialPart); });
        }

        [Test]
        public void Test_RunOnce_runs_the_next_scheduled_task()
        {
            Expect_Part_Execute(m_initialPart);

            Assert_RunOnce(SequencerResult.Continue);
        }

        [Test]
        public void Test_RunOnce_returns_stop_if_the_game_ends_and_all_scheduled_parts_are_cancelled()
        {
            Part part = CreateMockPart();

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
            Part part = CreateMockPart();

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
        public void Test_Parts_returns_all_parts_on_the_stack()
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

            Assert.Collections.AreEqual(new[] { subTask1, subTask2, nextPart }, m_sequencer.Parts);
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

            Assert.Throws<Exception>(() => m_sequencer.RunOnce(m_decisionMaker));
        }

        [Test]
        public void Test_RunOnce_decides_the_choice_outcome_before_execution()
        {
            var choicePart = new MockChoicePart(m_playerA);

            Expect_Part_Execute(m_initialPart, choicePart);
            Assert_RunOnce(SequencerResult.Continue);

            Expect_Make_Decision(choicePart.Choice, 3);
            Assert_RunOnce(SequencerResult.Continue);

            Assert.AreEqual(3, choicePart.Result);
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

        [Test]
        public void Test_Skip_skips_the_next_part()
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

            m_sequencer.Skip();
            Assert.AreEqual(subTask2, m_sequencer.NextPart);

            Expect_Part_Execute(subTask2);
            Expect_Part_Execute(nextPart);

            Test_Run();
        }

        #region Arguments

        public void Test_Can_push_and_pop_arguments()
        {
            m_sequencer.PushArgument("a string", "DebugString");
            m_sequencer.PushArgument(3, "DebugInt");

            Assert.AreEqual(3, m_sequencer.PeekArgument<int>("DebugInt"));
            Assert.AreEqual(3, m_sequencer.PopArgument<int>("DebugInt"));

            Assert.AreEqual("a string", m_sequencer.PeekArgument<string>("DebugString"));
            Assert.AreEqual("a string", m_sequencer.PopArgument<string>("DebugString"));
        }

        #endregion

        #region Cloning

        private void Test_Clone(Converter<Sequencer, Sequencer> cloner, Game expectedGame, System.Action callback = null)
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

            Sequencer clone = cloner(m_sequencer);
            {
                Assert.AreEqual(expectedGame, clone.Game);

                Expect.Call(subTask1.Execute(null)).IgnoreArguments().Return(null).Callback<Part.Context>(context =>
                {
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

        #endregion

        #region Hash

        [Test]
        public void Test_Sequencers_with_the_same_parts_have_the_same_hash()
        {
            Hash hash1 = new Hash();
            m_sequencer = new Sequencer(m_game, CreateOtherPart(1));
            m_sequencer.ComputeHash(hash1);

            Hash hash2 = new Hash();
            m_sequencer = new Sequencer(m_game, CreateOtherPart(1));
            m_sequencer.ComputeHash(hash2);

            Assert.AreEqual(hash1.Value, hash2.Value);
        }

        [Test]
        public void Test_Different_parts_produce_different_hashes()
        {
            Hash hash1 = new Hash();
            m_sequencer.ComputeHash(hash1);

            Hash hash2 = new Hash();
            m_sequencer = new Sequencer(m_game, CreateOtherPart(1));
            m_sequencer.ComputeHash(hash2);

            Assert.AreNotEqual(hash1.Value, hash2.Value);
        }

        [Test]
        public void Test_Same_part_with_different_hashes_produce_different_hashes()
        {
            Hash hash1 = new Hash();
            m_sequencer = new Sequencer(m_game, CreateOtherPart(1));
            m_sequencer.ComputeHash(hash1);

            Hash hash2 = new Hash();
            m_sequencer = new Sequencer(m_game, CreateOtherPart(2));
            m_sequencer.ComputeHash(hash2);

            Assert.AreNotEqual(hash1.Value, hash2.Value);
        }

        private void CheckHashForArgument(Hash expectedHash, object argument)
        {
            Assert.That(m_sequencer.IsEmpty);

            Hash testHash = new Hash();
            m_sequencer.PushArgument(argument, "Hash test");
            m_sequencer.ComputeHash(testHash);
            m_sequencer.PopArgument<object>("Hash test");
        }

        [Test]
        public void Test_Arguments_contribute_to_the_hash()
        {
            Expect_Part_Execute(m_initialPart, null);
            Assert_RunOnce(SequencerResult.Continue);

            Hash hash = new Hash(); hash.Add(2);
            CheckHashForArgument(hash, 2);

            hash = new Hash(); hash.Add(4);
            CheckHashForArgument(hash, 4);
        }

        #endregion

        #endregion

        #region Mock Types

        private class MockChoicePart : ChoicePart<int>
        {
            private readonly MockChoice m_choice;

            public MockChoicePart(Player player)
                : base(player)
            {
                m_choice = new MockChoice(player);
            }

            public int Result
            {
                get;
                set;
            }

            public Choice Choice
            {
                get 
                {
                    return m_choice;
                }
            }

            public override Choice GetChoice(Sequencer sequencer)
            {
                return m_choice;
            }

            public override Part Execute(Context context, int choice)
            {
                Result = choice;
                return null;
            }
        }

        private class MockChoice : Choice
        {
            public MockChoice(Resolvable<Player> player)
                : base(player)
            {
            }

            #region Overrides of Choice

            public override object DefaultValue
            {
                get { return null; }
            }

            #endregion
        }

        #endregion
    }
}

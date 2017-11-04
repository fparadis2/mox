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

using Rhino.Mocks;

using Mox.Flow;

namespace Mox
{
    public class NewSequencerTester
    {
        #region Inner Types

        private class DummyPart : Part
        {
            #region Properties

            public Part InnerPart { get; set; }

            #endregion

            #region Overrides of Part

            public override Part Execute(Context context)
            {
                if (InnerPart != null)
                {
                    return InnerPart.Execute(context);
                }

                return null;
            }

            #endregion
        }

        public interface IExpectation
        {
            IExpectation RepeatAny();
            IExpectation Callback(System.Action action);
        }

        private class MockDecisionMaker : IChoiceDecisionMaker
        {
            #region Variables

            private readonly Game m_game;
            private readonly List<Player> m_mockedPlayers = new List<Player>();
            private readonly Queue<Expectation> m_expectations = new Queue<Expectation>();

            #endregion

            #region Constructor

            public MockDecisionMaker(Game game)
            {
                m_game = game;
            }

            #endregion

            #region Methods

            public bool IsMocked(Player player)
            {
                return m_mockedPlayers.Contains(player);
            }

            public void Mock(Player player)
            {
                m_mockedPlayers.Add(player);
            }

            public IExpectation Expect<TChoice>(Player player, object result, Action<TChoice> validation = null)
                where TChoice : Choice
            {
                var expectation = new TypedExpectation<TChoice>(player, result, validation);
                m_expectations.Enqueue(expectation);
                return expectation;
            }

            public void Expect_All_Players_Pass()
            {
                m_expectations.Enqueue(new AnyPlayerPassExpectation());
            }

            public void VerifyExpectations()
            {
                Assert.Collections.IsEmpty(m_expectations);
            }

            #endregion

            #region Implementation of IChoiceDecisionMaker

            public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
            {
                var player = choice.Player.Resolve(m_game);

                if (!IsMocked(player))
                {
                    return choice.DefaultValue;
                }

                var expectation = FindCorrespondingExpectation(choice, player);
                return expectation.Result;
            }

            private Expectation FindCorrespondingExpectation(Choice choice, Player player)
            {
                Expectation nextExpectation;
                do
                {
                    nextExpectation = FindCorrespondingExpectationImpl(choice, player);
                }
                while (nextExpectation == null);

                return nextExpectation;
            }

            private Expectation FindCorrespondingExpectationImpl(Choice choice, Player player)
            {
                if (m_expectations.Count == 0)
                {
                    Assert.Fail("No expectation found for choice {0}", choice);
                }

                var nextExpectation = m_expectations.Peek();

                if (!nextExpectation.Repeat)
                {
                    m_expectations.Dequeue();
                }

                string message;
                bool valid = nextExpectation.Validate(choice, player, out message);

                if (valid)
                {
                    return nextExpectation;
                }

                if (nextExpectation.Repeat)
                {
                    m_expectations.Dequeue();
                }
                else
                {
                    Assert.Fail(message);
                }

                return null;
            }

            #endregion

            #region Inner Types

            private class TypedExpectation<TChoice> : PlayerExpectation
                where TChoice : Choice
            {
                private readonly Action<TChoice> m_validateAction;

                public TypedExpectation(Player expectedPlayer, object result, Action<TChoice> validateAction)
                    : base(expectedPlayer, result)
                {
                    m_validateAction = validateAction;
                }

                public override bool Validate(Choice choice, Player player, out string message)
                {
                    if (!base.Validate(choice, player, out message))
                    {
                        return false;
                    }

                    if (!(choice is TChoice))
                    {
                        message = string.Format("Expected choice of type {0} but got {1}", typeof (TChoice).Name, choice);
                        return false;
                    }

                    if (m_validateAction != null)
                    {
                        m_validateAction((TChoice)choice);
                    }

                    return true;
                }

                public override string ToString()
                {
                    return string.Format("Expectation of choice {0} for player {1}", typeof(TChoice).Name, ExpectedPlayer);
                }
            }

            private class PlayerExpectation : Expectation
            {
                private readonly Player m_expectedPlayer;

                protected PlayerExpectation(Player expectedPlayer, object result)
                    : base(result)
                {
                    Throw.IfNull(expectedPlayer, "expectedPlayer");
                    m_expectedPlayer = expectedPlayer;
                }

                protected Player ExpectedPlayer
                {
                    get { return m_expectedPlayer; }
                }

                public override bool Validate(Choice choice, Player player, out string message)
                {
                    if (!base.Validate(choice, player, out message))
                    {
                        return false;
                    }

                    if (m_expectedPlayer != player)
                    {
                        message = string.Format("Player in choice {0} does not match expected player", choice);
                        return false;
                    }

                    return true;
                }
            }

            private class Expectation : IExpectation
            {
                private readonly object m_result;

                private bool m_repeat;
                private System.Action m_action;

                protected Expectation(object result)
                {
                    m_result = result;
                }

                public object Result
                {
                    get { return m_result; }
                }

                public bool Repeat
                {
                    get { return m_repeat; }
                }

                public virtual bool Validate(Choice choice, Player player, out string message)
                {
                    message = null;
                    return true;
                }

                #region Implementation of IExpectation

                public IExpectation RepeatAny()
                {
                    m_repeat = true;
                    return this;
                }

                public IExpectation Callback(System.Action action)
                {
                    Throw.InvalidOperationIf(m_action != null, "A callback is already set on this expectation");
                    m_action = action;
                    return this;
                }

                #endregion
            }

            private class AnyPlayerPassExpectation : Expectation
            {
                public AnyPlayerPassExpectation()
                    : base(null)
                {
                    RepeatAny();
                }

                public override bool Validate(Choice choice, Player player, out string message)
                {
                    if (!base.Validate(choice, player, out message))
                    {
                        return false;
                    }

                    if (!(choice is GivePriorityChoice))
                    {
                        message = string.Format("Expected choice of type {0} but got {1}", typeof (GivePriorityChoice).Name, choice);
                        return false;
                    }

                    return true;
                }
            }

            #endregion
        }

        private class MockSequencer : Sequencer
        {
            public MockSequencer(Game game, Part initialPart)
                : base(game, initialPart)
            {
            }

            public new void Push(Part part)
            {
                base.Push(part);
            }
        }

        #endregion

        #region Variables

        private readonly Game m_game;

        private readonly MockRepository m_mockery;
        private readonly MockSequencer m_sequencer;

        private readonly MockDecisionMaker m_mockDecisionMaker;

        private readonly DummyPart m_initialPart;

        #endregion

        #region Constructor

        public NewSequencerTester(MockRepository mockery, Game game)
        {
            Throw.IfNull(mockery, "mockery");
            Throw.IfNull(game, "game");

            m_mockery = mockery;
            m_game = game;

            m_initialPart = new DummyPart();

            m_mockDecisionMaker = new MockDecisionMaker(m_game);
            m_sequencer = new MockSequencer(game, m_initialPart);
        }

        #endregion

        #region Properties

        internal Sequencer Sequencer
        {
            get { return m_sequencer; }
        }

        #endregion

        #region Methods

        #region Choice mocking

        public void MockPlayerChoices(Player player)
        {
            m_mockDecisionMaker.Mock(player);
        }

        public void MockAllPlayersChoices()
        {
            m_game.Players.ForEach(MockPlayerChoices);
        }

        public bool IsMocked(Player player)
        {
            return m_mockDecisionMaker.IsMocked(player);
        }

        #endregion

        #region Choice Expectations

        #region Mulligan

        public IExpectation Expect_Player_Mulligan(Player player, bool result)
        {
            Assert.IsTrue(IsMocked(player), "Player choices are not mocked");

            return m_mockDecisionMaker.Expect<MulliganChoice>(player, result);
        }

        #endregion

        #region GivePriority

        public IExpectation Expect_Player_GivePriority(Player player, Action action)
        {
            Assert.IsTrue(IsMocked(player), "Player choices are not mocked");
            return m_mockDecisionMaker.Expect<GivePriorityChoice>(player, action);
        }

        public void Expect_All_Players_Pass()
        {
            m_mockDecisionMaker.Expect_All_Players_Pass();
        }

        public void Expect_Everyone_passes_once(Player startingPlayer)
        {
            foreach (Player player in Player.Enumerate(startingPlayer, false))
            {
                Expect_Player_GivePriority(player, null);
            }
        }

        #endregion

        #region PayMana

        public IExpectation Expect_Player_PayMana(Player player, ManaCost manaCost, Action action)
        {
            Assert.IsTrue(IsMocked(player), "Player choices are not mocked");

            return m_mockDecisionMaker.Expect<PayManaChoice>(player, action, choice =>
                Assert.AreEqual(manaCost, choice.ManaCost, "Expected mana cost requirement of {0} but got {1}", manaCost, choice.ManaCost));
        }

        public void Expect_Player_PayDummyMana(Player player, ManaCost manaCost)
        {
            ManaPaymentNew payment = ManaPaymentNew.CreateAnyFromCost(manaCost);
            var totalAmount = payment.GetTotalAmount();

            player.ManaPool.Colorless += totalAmount.Colorless;
            player.ManaPool.White += totalAmount.White;
            player.ManaPool.Blue += totalAmount.Blue;
            player.ManaPool.Black += totalAmount.Black;
            player.ManaPool.Red += totalAmount.Red;
            player.ManaPool.Green += totalAmount.Green;

            Expect_Player_PayMana(player, manaCost, new PayManaAction(payment));
        }

        #endregion

        #region Target

        public IExpectation Expect_Player_Target(Player player, bool allowCancel, IEnumerable<ITargetable> targetables, ITargetable result, TargetContextType targetContextType)
        {
            Assert.IsTrue(IsMocked(player), "Player choices are not mocked");

            int[] identifiers = targetables == null ? null : targetables.Select(targetable => targetable.Identifier).ToArray();

            return m_mockDecisionMaker.Expect<TargetChoice>(player, GetTargetResult(result), choice =>
            {
                Assert.AreEqual(allowCancel, choice.Context.AllowCancel);
                Assert.Collections.AreEqual(identifiers, choice.Context.Targets);
                Assert.AreEqual(targetContextType, choice.Context.Type);
            });
        }

        private static TargetResult GetTargetResult(ITargetable targetable)
        {
            return targetable == null ? TargetResult.Invalid : new TargetResult(targetable.Identifier);
        }

        #endregion

        #region ModalChoice

        public IExpectation Expect_Player_AskModalChoice(Player player, ModalChoiceContext context, ModalChoiceResult result)
        {
            Assert.IsTrue(IsMocked(player), "Player choices are not mocked");

            return m_mockDecisionMaker.Expect<ModalChoice>(player, result, choice =>
            {
                Assert.AreEqual(context.Question, choice.Context.Question);
                Assert.AreEqual(context.Importance, choice.Context.Importance);
                Assert.AreEqual(context.DefaultChoice, choice.Context.DefaultChoice);
                Assert.Collections.AreEqual(context.Choices, choice.Context.Choices);
            });
        }

        #endregion

        #region Combat

        public void Expect_Player_DeclareAttackers(Player player, DeclareAttackersContext attackInfo, DeclareAttackersResult result)
        {
            Assert.IsTrue(IsMocked(player), "Player choices are not mocked");

            m_mockDecisionMaker.Expect<DeclareAttackersChoice>(player, result, choice => Assert.Collections.AreEqual(attackInfo.LegalAttackers, choice.AttackContext.LegalAttackers));
        }

        public void Expect_Player_DeclareBlockers(Player player, DeclareBlockersContext blockInfo, DeclareBlockersResult result)
        {
            Assert.IsTrue(IsMocked(player), "Player choices are not mocked");

            m_mockDecisionMaker.Expect<DeclareBlockersChoice>(player, result, choice =>
            {
                Assert.Collections.AreEqual(blockInfo.Attackers, choice.BlockContext.Attackers);
                Assert.Collections.AreEqual(blockInfo.LegalBlockers, choice.BlockContext.LegalBlockers);
            });
        }

        #endregion

        #region Verification

        public void VerifyExpectations()
        {
            m_mockDecisionMaker.VerifyExpectations();
            Assert.That(m_sequencer.IsArgumentStackEmpty, "Argument stack should be empty after tests!");
        }

        #endregion

        #endregion

        #region Running

        public void Run(Part part)
        {
            m_mockery.Test(() => RunWithoutMock(part));
        }

        public void RunWithoutMock(Part part)
        {
            m_sequencer.Push(part);
            m_sequencer.Run(m_mockDecisionMaker);
        }

        public Part.Context CreateContext()
        {
            return new Part.Context(m_sequencer);
        }

        public void RunOnce(Part part)
        {
            m_mockery.Test(() =>
            {
                m_sequencer.Push(part);
                m_sequencer.RunOnce(m_mockDecisionMaker);
            });
        }

        #endregion

        #endregion
    }
}

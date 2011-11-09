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

        private class DummyPart : NewPart
        {
            #region Properties

            public NewPart InnerPart { get; set; }

            #endregion

            #region Overrides of Part

            public override NewPart Execute(Context context)
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

            public void VerifyExpectations()
            {
                Assert.Collections.IsEmpty(m_expectations);
            }

            #endregion

            #region Implementation of IChoiceDecisionMaker

            public object MakeChoiceDecision(Choice choice)
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
                if (m_expectations.Count == 0)
                {
                    Assert.Fail("No expectation found for choice {0}", choice);
                }

                var nextExpectation = m_expectations.Peek();

                if (!nextExpectation.Repeat)
                {
                    m_expectations.Dequeue();
                }

                nextExpectation.Validate(choice, player);
                return nextExpectation;
            }

            #endregion

            #region Inner Types

            private class TypedExpectation<TChoice> : Expectation
                where TChoice : Choice
            {
                private readonly Action<TChoice> m_validateAction;

                public TypedExpectation(Player expectedPlayer, object result, Action<TChoice> validateAction)
                    : base(expectedPlayer, result)
                {
                    m_validateAction = validateAction;
                }

                public override void Validate(Choice choice, Player player)
                {
                    base.Validate(choice, player);

                    Assert.IsInstanceOf<TChoice>(choice, "Expected choice of type {0} but got {1}", typeof (TChoice).Name, choice);

                    if (m_validateAction != null)
                    {
                        m_validateAction((TChoice)choice);
                    }
                }

                public override string ToString()
                {
                    return string.Format("Expectation of choice {0} for player {1}", typeof(TChoice).Name, ExpectedPlayer);
                }
            }

            private class Expectation : IExpectation
            {
                private readonly Player m_expectedPlayer;
                private readonly object m_result;

                private bool m_repeat;
                private System.Action m_action;

                protected Expectation(Player expectedPlayer, object result)
                {
                    Throw.IfNull(expectedPlayer, "expectedPlayer");
                    m_expectedPlayer = expectedPlayer;
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

                protected Player ExpectedPlayer
                {
                    get { return m_expectedPlayer; }
                }

                public virtual void Validate(Choice choice, Player player)
                {
                    Assert.AreEqual(m_expectedPlayer, player, "Player in choice {0} does not match expected player", choice);
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

	        #endregion
        }

        private class MockSequencer : NewSequencer
        {
            public MockSequencer(Game game, NewPart initialPart)
                : base(game, initialPart)
            {
            }

            public new void Push(NewPart part)
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

        internal NewSequencer Sequencer
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

        public void Expect_Player_Play(Player player, Action action, ExecutionEvaluationContext expectedContext = new ExecutionEvaluationContext())
        {
            Assert.IsNotNull(action);

            Expect.Call(action.CanExecute(player, expectedContext)).Return(true);
            action.Execute(null, player);

            LastCall.IgnoreArguments().Callback<NewPart.Context, Player>((callbackContext, callbackPlayer) =>
            {
                Assert.AreEqual(player, callbackPlayer);
                return true;
            });
        }

        public void Expect_Player_PlayInvalid(Player player, Action action, ExecutionEvaluationContext expectedContext = new ExecutionEvaluationContext())
        {
            Assert.IsNotNull(action);

            Expect.Call(action.CanExecute(player, expectedContext)).Return(false);
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
            ManaPayment payment = new ManaPayment();

            player.ManaPool[Color.None] += manaCost.Colorless;
            payment.Pay(Color.None, manaCost.Colorless);

            foreach (ManaSymbol symbol in manaCost.Symbols)
            {
                Color color = ManaSymbolHelper.GetColor(symbol);
                player.ManaPool[color] += 1;
                payment.Pay(color, 1);
            }

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

        public void Run(NewPart part)
        {
            m_mockery.Test(() => RunWithoutMock(part));
        }

        public void RunWithoutMock(NewPart part)
        {
            m_sequencer.Push(part);
            m_sequencer.Run(m_mockDecisionMaker);
        }

        public NewPart.Context CreateContext()
        {
            return new NewPart.Context(m_sequencer);
        }

        public void RunOnce(NewPart part)
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

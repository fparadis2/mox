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
using Rhino.Mocks.Constraints;
using Rhino.Mocks.Interfaces;

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

        public NewPart.Context CreateContext(object choiceResult = null)
        {
            return new NewPart.Context(m_sequencer, choiceResult);
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

#warning remove
    public class SequencerTester
    {
        #region Inner Types

        private class DummyPart : Part<IGameController>
        {
            #region Properties

            public Part<IGameController> InnerPart { get; set; }

            #endregion

            #region Overrides of Part

            public override Part<IGameController> Execute(Context context)
            {
                if (InnerPart != null)
                {
                    return InnerPart.Execute(context);
                }

                return null;
            }

            #endregion
        }

        private class MockController : IGameController
        {
            #region Variables

            private readonly List<Player> m_mockedPlayers = new List<Player>();
            private readonly IGameController m_realController;

            #endregion

            #region Constructor

            public MockController(IGameController controller)
            {
                Throw.IfNull(controller, "controller");
                m_realController = controller;
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

            #endregion

            #region Implementation of IGameController

            public ModalChoiceResult AskModalChoice(Part<IGameController>.Context context, Player player, ModalChoiceContext choiceContext)
            {
                if (IsMocked(player))
                {
                    return m_realController.AskModalChoice(context, player, choiceContext);
                }

                return choiceContext.DefaultChoice;
            }

            /// <summary>
            /// Gives the priority to the given <paramref name="player"/>.
            /// </summary>
            /// <returns>The action to do, null otherwise (to pass).</returns>
            public Action GivePriority(MTGPart.Context context, Player player)
            {
                if (IsMocked(player))
                {
                    return m_realController.GivePriority(context, player);
                }

                return null;
            }

            /// <summary>
            /// Asks the given <paramref name="player"/> to pay for mana.
            /// </summary>
            /// <param name="context"></param>
            /// <param name="player"></param>
            /// <param name="manaCost">The cost to pay.</param>
            /// <returns>The action to do (either a mana ability or a mana payment), null otherwise (to cancel).</returns>
            public Action PayMana(Part<IGameController>.Context context, Player player, ManaCost manaCost)
            {
                if (IsMocked(player))
                {
                    return m_realController.PayMana(context, player, manaCost);
                }

                return null;
            }

            /// <summary>
            /// Asks the player to choose a target from the possible targets.
            /// </summary>
            /// <param name="context"></param>
            /// <param name="player"></param>
            /// <param name="targetInfo"></param>
            /// <returns></returns>
            public int Target(Part<IGameController>.Context context, Player player, TargetContext targetInfo)
            {
                if (IsMocked(player))
                {
                    return m_realController.Target(context, player, targetInfo);
                }

                return targetInfo.Targets.Count > 0 ? targetInfo.Targets.First() : ObjectManager.InvalidIdentifier;
            }

            /// <summary>
            /// Asks the player whether to mulligan.
            /// </summary>
            /// <returns>True to mulligan.</returns>
            public bool Mulligan(Part<IGameController>.Context context, Player player)
            {
                if (IsMocked(player))
                {
                    return m_realController.Mulligan(context, player);
                }

                return false;
            }

            public DeclareAttackersResult DeclareAttackers(Part<IGameController>.Context context, Player player, DeclareAttackersContext attackInfo)
            {
                if (IsMocked(player))
                {
                    return m_realController.DeclareAttackers(context, player, attackInfo);
                }

                return DeclareAttackersResult.Empty;
            }

            public DeclareBlockersResult DeclareBlockers(Part<IGameController>.Context context, Player player, DeclareBlockersContext blockInfo)
            {
                if (IsMocked(player))
                {
                    return m_realController.DeclareBlockers(context, player, blockInfo);
                }

                return DeclareBlockersResult.Empty;
            }

            #endregion
        }

        private class MockSequencer : Sequencer<IGameController>
        {
            public MockSequencer(Part<IGameController> initialPart, Game game) : base(initialPart, game)
            {
            }

            public new void Push(Part<IGameController> part)
            {
                base.Push(part);
            }
        }

        #endregion

        #region Variables

        private readonly Game m_game;

        private readonly MockRepository m_mockery;
        private readonly MockSequencer m_sequencer;

        private readonly MockController m_mockController;
        private readonly IGameController m_controller;

        private readonly MTGPart.Context m_context;
        private readonly DummyPart m_initialPart;

        #endregion

        #region Constructor

        public SequencerTester(MockRepository mockery, Game game)
        {
            Throw.IfNull(mockery, "mockery");
            Throw.IfNull(game, "game");

            m_mockery = mockery;
            m_game = game;

            m_initialPart = new DummyPart();

            m_controller = m_mockery.StrictMock<IGameController>();
            m_mockController = new MockController(m_controller);
            m_sequencer = new MockSequencer(m_initialPart, game);
            m_context = new MTGPart.Context(m_sequencer, m_mockController, ControllerAccess.Multiple);
        }

        #endregion

        #region Properties

        public Game Game
        {
            get { return m_sequencer.Game; }
        }

        public MTGPart.Context Context
        {
            get { return m_context; }
        }

        public Sequencer<IGameController> Sequencer
        {
            get { return m_sequencer; }
        }

        public IGameController Controller
        {
            get { return m_mockController; }
        }

        #endregion

        #region Methods

        #region Creation
        
        public MTGPart.Context CreateContext()
        {
            return new MTGPart.Context(m_sequencer, m_controller, ControllerAccess.Multiple);
        }

        public void MockPlayerController(Player player)
        {
            m_mockController.Mock(player);
        }

        public void MockAllPlayers()
        {
            Game.Players.ForEach(MockPlayerController);
        }

        public bool IsMocked(Player player)
        {
            return m_mockController.IsMocked(player);
        }

        #endregion

        #region Expectations

        public bool ValidateContext(MTGPart.Context context)
        {
            Assert.AreEqual(m_game, context.Game);
            return true;
        }

        #region Mulligan

        public IMethodOptions<ModalChoiceResult> Expect_AskModalChoice(Player player, ModalChoiceContext context, ModalChoiceResult result)
        {
            Assert.IsTrue(m_mockController.IsMocked(player), "Player controller is not mocked");

            return Expect.Call(m_controller.AskModalChoice(m_context, player, context))
                .Return(result)
                .Message(string.Format("Expected asking modal choice {0} to {1}", context.Question, player.Name))
                .IgnoreArguments()
                .Callback<MTGPart.Context, Player, ModalChoiceContext>((callbackContext, callbackPlayer, choiceContext) =>
                {
                    Assert.AreEqual(player, callbackPlayer);

                    Assert.AreEqual(context.Question, choiceContext.Question);
                    Assert.AreEqual(context.Importance, choiceContext.Importance);
                    Assert.AreEqual(context.DefaultChoice, choiceContext.DefaultChoice);
                    Assert.Collections.AreEqual(context.Choices, choiceContext.Choices);

                    return ValidateContext(callbackContext);
                });
        }

        #endregion

        #region GivePriority

        public IMethodOptions<Action> Expect_Player_Action(Player player, Action action)
        {
            return Expect_Player_Action(player, action, null);
        }

        public IMethodOptions<Action> Expect_Player_Action(Player player, Action action, System.Action callbackAction)
        {
            callbackAction = callbackAction ?? (() => { });

            Assert.IsTrue(m_mockController.IsMocked(player), "Player controller is not mocked");
            return Expect.Call(m_controller.GivePriority(m_context, player))
                .Return(action)
                .Message("Expected priority to " + player.Name)
                .Constraints(Is.Matching<MTGPart.Context>(ValidateContext), Is.Same(player))
                .Repeat.Once()
                .WhenCalled(mi => callbackAction());
        }

        public void Expect_Player_MockAction(Player player, Action action, ExecutionEvaluationContext expectedContext)
        {
            Expect_Player_Action(player, action);

            if (action != null)
            {
                Expect.Call(action.CanExecute(player, expectedContext)).Return(true);
                //action.Execute(m_context, player);

                LastCall.IgnoreArguments()
                    .Callback<MTGPart.Context, Player>((callbackContext, callbackPlayer) =>
                {
                    Assert.AreEqual(player, callbackPlayer);
                    return ValidateContext(callbackContext);
                });
            }
        }

        public void Expect_Player_Invalid_MockAction(Player player, Action action, ExecutionEvaluationContext expectedContext)
        {
            Expect_Player_Action(player, action);

            if (action != null)
            {
                Expect.Call(action.CanExecute(player, expectedContext)).Return(false);
            }
        }

        #endregion

        #region Target

        public void Expect_Player_Target(Player player, bool allowCancel, IEnumerable<ITargetable> targetables, ITargetable result, TargetContextType targetContextType)
        {
            Assert.IsTrue(m_mockController.IsMocked(player), "Player controller is not mocked");

            int[] identifiers = targetables == null ? null : targetables.Select(targetable => targetable.Identifier).ToArray();
            Expect.Call(m_controller.Target(m_context, player, null))
                .Return(GetIdentifier(result))
                .Message("Expected targeting from " + player.Name)
                .IgnoreArguments()
                .Callback<MTGPart.Context, Player, TargetContext>((callbackContext, callbackPlayer, callbackTargetInfo) =>
            {
                Assert.AreEqual(allowCancel, callbackTargetInfo.AllowCancel);
                Assert.Collections.AreEqual(identifiers, callbackTargetInfo.Targets);
                Assert.AreEqual(targetContextType, callbackTargetInfo.Type);
                Assert.AreEqual(player, callbackPlayer);
                return ValidateContext(callbackContext);
            });
        }

        private static int GetIdentifier(ITargetable targetable)
        {
            return targetable == null ? ObjectManager.InvalidIdentifier : targetable.Identifier;
        }

        #endregion

        #region PayMana

        public void Expect_Player_PayMana(Player player, ManaCost manaCost, Action action)
        {
            Assert.IsTrue(m_mockController.IsMocked(player), "Player controller is not mocked");

            Expect.Call(m_controller.PayMana(m_context, player, manaCost))
                .Return(action)
                .Message("Expected pay mana priority to " + player.Name)
                .IgnoreArguments()
                .Callback<MTGPart.Context, Player, ManaCost>((callbackContext, callbackPlayer, callbackManaCost) =>
            {
                Assert.AreEqual(manaCost, callbackManaCost);
                Assert.AreEqual(player, callbackPlayer);
                return ValidateContext(callbackContext);
            });
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

        #region Mulligan

        public IMethodOptions<bool> Expect_Player_Mulligan(Player player, bool result)
        {
            Assert.IsTrue(m_mockController.IsMocked(player), "Player controller is not mocked");

            return Expect.Call(m_controller.Mulligan(m_context, player))
                .Return(result)
                .Message("Expected asking mulligan to " + player.Name)
                .IgnoreArguments()
                .Callback<MTGPart.Context, Player>((callbackContext, callbackPlayer) =>
                {
                    Assert.AreEqual(player, callbackPlayer);
                    return ValidateContext(callbackContext);
                });
        }

        #endregion

        #region Combat

        public void Expect_Player_DeclareAttackers(Player player, DeclareAttackersContext attackInfo, DeclareAttackersResult result)
        {
            Assert.IsTrue(m_mockController.IsMocked(player), "Player controller is not mocked");

            Expect.Call(m_controller.DeclareAttackers(m_context, player, attackInfo))
                .Return(result)
                .Message("Expected declare attackers to " + player.Name)
                .IgnoreArguments()
                .Callback<MTGPart.Context, Player, DeclareAttackersContext>((callbackContext, callbackPlayer, callbackAttackInfo) =>
                {
                    Assert.AreEqual(player, callbackPlayer);
                    Assert.Collections.AreEqual(attackInfo.LegalAttackers, callbackAttackInfo.LegalAttackers);
                    return ValidateContext(callbackContext);
                });
        }

        public void Expect_Player_DeclareBlockers(Player player, DeclareBlockersContext blockInfo, DeclareBlockersResult result)
        {
            Assert.IsTrue(m_mockController.IsMocked(player), "Player controller is not mocked");

            Expect.Call(m_controller.DeclareBlockers(m_context, player, blockInfo))
                .Return(result)
                .Message("Expected declare blockers to " + player.Name)
                .IgnoreArguments()
                .Callback<MTGPart.Context, Player, DeclareBlockersContext>((callbackContext, callbackPlayer, callbackBlockInfo) =>
                {
                    Assert.AreEqual(player, callbackPlayer);
                    Assert.Collections.AreEqual(blockInfo.Attackers, callbackBlockInfo.Attackers);
                    Assert.Collections.AreEqual(blockInfo.LegalBlockers, callbackBlockInfo.LegalBlockers);
                    return ValidateContext(callbackContext);
                });
        }

        #endregion

        #endregion

        #region Running

        public void Run(Part<IGameController> part)
        {
            m_mockery.Test(() => RunWithoutMock(part));
        }

        public void RunWithoutMock(Part<IGameController> part)
        {
            m_sequencer.Push(part);
            m_sequencer.Run(m_mockController);
        }

        #endregion

        #endregion
    }
}

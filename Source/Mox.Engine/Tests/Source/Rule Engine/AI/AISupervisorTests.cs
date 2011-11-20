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
using System.Text;
using Mox.Flow;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.AI
{
    /// <summary>
    /// Also serves as a demonstration for the AI, through a simple game.
    /// On each turn, the computer and the player must guess one of 2 numbers.
    /// They score the number they chose, expect if the last digit of their score is that number, in which case, they only score half of that number.
    /// </summary>
    [TestFixture]
    public class AISupervisorTests : BaseGameTests
    {
        #region Inner Types

        private class GuessingGameBoard : Object
        {
            #region Variables

            private static readonly Property<int> Player1Score = Property<int>.RegisterProperty("Player1Score", typeof(GuessingGameBoard));
            private static readonly Property<int> Player2Score = Property<int>.RegisterProperty("Player2Score", typeof(GuessingGameBoard));

            private static readonly Property<int> VerifyDataConsistencyProperty = Property<int>.RegisterAttachedProperty("VerifyDataConsistency", typeof (GuessingGameBoard));

            #endregion

            #region Properties

            public int VerifyDataConsistency
            {
                get { return GetValue(VerifyDataConsistencyProperty); }
                set { SetValue(VerifyDataConsistencyProperty, value); }
            }

            #endregion

            #region Methods

            public int GetScore(Player player)
            {
                Property<int> scoreProperty = GetScoreProperty(player);
                return GetValue(scoreProperty);
            }

            public void AddScore(Player player, int value)
            {
                int currentValue = GetScore(player);

                Property<int> scoreProperty = GetScoreProperty(player);
                SetValue(scoreProperty, currentValue + value);
            }

            public bool HasEnded
            {
                get 
                {
                    return GetValue(Player1Score) > 50 || GetValue(Player2Score) > 50;
                }
            }

            private Property<int> GetScoreProperty(Player player)
            {
                Assert.AreEqual(player.Manager, Manager, "Cross-game operation!");

                if (player.Name == "Player A")
                {
                    return Player1Score;
                }

                Assert.AreEqual("Player B", player.Name);
                return Player2Score;
            }

            public static GuessingGameBoard GetBoard(Game game)
            {
                return (GuessingGameBoard)game.Objects.First(obj => obj is GuessingGameBoard);
            }

            #endregion
        }

        [ChoiceEnumerator(typeof(GuessingGameChoiceEnumerator))]
        public class GuessingGameChoice : Choice
        {
            public GuessingGameChoice(Resolvable<Player> player)
                : base(player)
            {
            }

            #region Overrides of Choice

            public override object DefaultValue
            {
                get { return 0; }
            }

            #endregion
        }
        
        private class GuessingGameChoiceEnumerator : ChoiceEnumerator
        {
            #region Implementation of ChoiceEnumerator

            /// <summary>
            /// Returns the possible choices for the choice context.
            /// </summary>
            public override IEnumerable<object> EnumerateChoices(Game game, Choice choice)
            {
                for(int i = 9; i >= 0; i--)
                {
                    yield return i;
                }
            }

            #endregion
        }

        private class GuessingGameAlgorithm : BaseMinMaxAlgorithm
        {
            private readonly int m_depth;

            public GuessingGameAlgorithm(Player maximizingPlayer, int depth)
                : base(maximizingPlayer, new AIParameters())
            {
                m_depth = depth;
            }

            #region Overrides of BaseMinMaxAlgorithm

            /// <summary>
            /// Returns true if the current search should be stopped at this depth.
            /// </summary>
            /// <remarks>
            /// Normally this would return true if the game has ended, or if the search reached a specific depth.
            /// </remarks>
            /// <param name="tree"></param>
            /// <param name="game"></param>
            /// <returns></returns>
            public override bool IsTerminal(IMinimaxTree tree, Game game)
            {
                if (tree.Depth > m_depth)
                {
                    return true;
                }

                return GuessingGameBoard.GetBoard(game).HasEnded;
            }

            /// <summary>
            /// Computes the "value" of the current "game state".
            /// </summary>
            /// <returns></returns>
            public override float ComputeHeuristic(Game game, bool considerGameEndingState)
            {
                GuessingGameBoard board = GuessingGameBoard.GetBoard(game);

                float value = 0;
                foreach (Player player in game.Players)
                {
                    value += board.GetScore(player) * (IsMaximizingPlayer(player) ? 1 : -1);
                }
                return value;
            }

            #endregion
        }

        private class GuessingGameSupervisor : AISupervisor
        {
            public GuessingGameSupervisor(Game game)
                : base(game)
            {
            }

            public int MaxDepth
            {
                get;
                set;
            }

            public override IMinMaxAlgorithm CreateAlgorithm(Player maximizingPlayer)
            {
                return new GuessingGameAlgorithm(maximizingPlayer, MaxDepth);
            }
        }

        #region Parts

        private interface IRandomNumberProvider
        {
            /// <summary>
            /// Provides a number between 0 and 9 inclusive
            /// </summary>
            /// <returns></returns>
            int Provide(int turn);
        }

        private class MockRandomNumberProvider : IRandomNumberProvider
        {
            private readonly System.Random m_random = new System.Random();
            private readonly List<int> m_providedNumbers = new List<int>();

            public MockRandomNumberProvider()
            {
                AllowGrowing = false;
            }

            public IList<int> ProvidedNumbers
            {
                get { return m_providedNumbers; }
            }

            public bool AllowGrowing
            {
                get;
                set;
            }

            #region Implementation of IRandomNumberProvider

            /// <summary>
            /// Provides a number between 0 and 9 inclusive
            /// </summary>
            /// <returns></returns>
            public int Provide(int turn)
            {
                if (turn >= m_providedNumbers.Count)
                {
                    Assert.That(AllowGrowing, "Cannot grow (turn {0})", turn);
                    Grow(turn + 1);
                }

                return m_providedNumbers[turn];
            }

            private void Grow(int count)
            {
                while (m_providedNumbers.Count < count)
                {
                    m_providedNumbers.Add(m_random.Next(0, 10));
                }
            }

            #endregion
        }

        private class MainPart : PlayerPart
        {
            private readonly IRandomNumberProvider m_provider;
            private readonly int m_turn = 0;

            public MainPart(Player player, IRandomNumberProvider provider)
                : this(player, provider, 0)
            {
            }

            private MainPart(Player player, IRandomNumberProvider provider, int inc)
                : base(player)
            {
                m_provider = provider;
                m_turn = inc;
            }

            #region Overrides of Part<IGuessingGame>

            public override Part Execute(Context context)
            {
                GuessingGameBoard board = GuessingGameBoard.GetBoard(context.Game);
                Player player = GetPlayer(context.Game);

                if (board.HasEnded)
                {
                    return null;
                }

                int turn = m_turn;
                int number1 = m_provider.Provide(turn++);
                int number2 = m_provider.Provide(turn++);

                context.Schedule(new OneGuessPart(player, m_turn, number1, number2));

                return new MainPart(Player.GetNextPlayer(player), m_provider, turn);
            }

            #endregion
        }

        private class OneGuessPart : ChoicePart<int>
        {
            private readonly int m_turn;
            private readonly int m_number1;
            private readonly int m_number2;

            public OneGuessPart(Player player, int turn, int number1, int number2)
                : base(player)
            {
                m_turn = turn;
                m_number1 = number1;
                m_number2 = number2;
            }

            public override Choice GetChoice(Sequencer sequencer)
            {
                return new GuessingGameChoice(ResolvablePlayer);
            }

            public override Part Execute(Context context, int guess)
            {
                GuessingGameBoard board = GuessingGameBoard.GetBoard(context.Game);
                Player player = GetPlayer(context.Game);

                VerifyDataConsistency(board, m_turn, m_turn + 1);

                if (guess == m_number1 || guess == m_number2) 
                {
                    int increment = guess;
                    if (guess == GetLastDigit(board.GetScore(player)))
                    {
                        increment /= 2;
                    }
                    board.AddScore(player, increment);
                }

                VerifyDataConsistency(board, m_turn + 1, m_turn + 2);

                return null;
            }

            private static void VerifyDataConsistency(GuessingGameBoard board, int initial, int final)
            {
                Assert.AreEqual(initial, board.VerifyDataConsistency);
                board.VerifyDataConsistency = final;
            }

            private static int GetLastDigit(int number)
            {
                return number % 10;
            }
        }

        #endregion

        #endregion

        #region Variables

        private GuessingGameBoard m_board;
        private GuessingGameSupervisor m_supervisor;
        private IChoiceDecisionMaker m_mockChoiceDecisionMaker;

        private MockRandomNumberProvider m_randomNumberProvider;
        private Sequencer m_sequencer;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_game.UseRandom(Random.New());

            m_board = m_game.Create<GuessingGameBoard>();
            m_game.Objects.Add(m_board);

            m_supervisor = new GuessingGameSupervisor(m_game);
            m_supervisor.MaxDepth = 2;
            m_mockChoiceDecisionMaker = m_mockery.StrictMock<IChoiceDecisionMaker>();

            m_randomNumberProvider = new MockRandomNumberProvider();
            m_sequencer = new Sequencer(m_game, new MainPart(m_playerA, m_randomNumberProvider));
        }

        #endregion

        #region Utilities

        private void Provide_Random_Numbers(params int[] numbers)
        {
            numbers.ForEach(m_randomNumberProvider.ProvidedNumbers.Add);
        }

        private void Expect_Guess(Player expectedPlayer, int answer)
        {
            Expect.Call(m_mockChoiceDecisionMaker.MakeChoiceDecision(null, null)).IgnoreArguments().Callback<Sequencer, Choice>((s, c) =>
            {
                Assert.IsInstanceOf<GuessingGameChoice>(c);
                Assert.AreEqual(expectedPlayer, c.Player.Resolve(m_game));
                return true;
            }).Return(answer);
        }

        private void Do_Turn_Normal(Player player, int answer)
        {
            m_sequencer.RunOnce(m_mockChoiceDecisionMaker); // Main part

            Expect_Guess(player, answer);
            m_mockery.Test(() => m_sequencer.RunOnce(m_mockChoiceDecisionMaker)); // Guess part
        }

        private void Do_Turn_AI(Player player, int expectedAnswer)
        {
            m_sequencer.RunOnce(m_mockChoiceDecisionMaker); // Main part

            Assert.AreEqual(expectedAnswer, m_supervisor.MakeChoiceDecision(m_sequencer, new GuessingGameChoice(player)));

            Expect_Guess(player, expectedAnswer);
            m_mockery.Test(() => m_sequencer.RunOnce(m_mockChoiceDecisionMaker)); // Guess part
        }

        private void Assert_Score_Is(int playerA, int playerB)
        {
            Assert.AreEqual(playerA, m_board.GetScore(m_playerA));
            Assert.AreEqual(playerB, m_board.GetScore(m_playerB));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_access_and_change_parameters()
        {
            Assert.IsNotNull(m_supervisor.Parameters);
            m_supervisor.Parameters.MinimumTreeDepth = 10;
            Assert.AreEqual(10, m_supervisor.Parameters.MinimumTreeDepth);
        }

        [Test]
        public void Test_General_game_without_AI()
        {
            Provide_Random_Numbers(
                0, 1,
                8, 9,
                4, 8,
                5, 8);

            Do_Turn_Normal(m_playerA, 1);
            Assert_Score_Is(1, 0);

            Do_Turn_Normal(m_playerB, 8);
            Assert_Score_Is(1, 8);

            Do_Turn_Normal(m_playerA, 9);
            Assert_Score_Is(1, 8);

            Do_Turn_Normal(m_playerB, 8);
            Assert_Score_Is(1, 12);
        }

        [Test]
        public void Test_AI_will_generally_choose_the_highest_number()
        {
            Provide_Random_Numbers(
                0, 1,
                8, 9,
                4, 8,
                0, 0, 0, 0, 0, 0); // not important, give some space for AI digging

            Do_Turn_AI(m_playerA, 1);
            Assert_Score_Is(1, 0);

            Do_Turn_AI(m_playerB, 9);
            Assert_Score_Is(1, 9);

            Do_Turn_AI(m_playerA, 8);
            Assert_Score_Is(9, 9);
        }

        [Test]
        public void Test_AI_will_choose_the_lowest_number_if_it_gives_more_score()
        {
            Provide_Random_Numbers(
                8, 9,
                0, 0, 0, 0, 0, 0); // not important, give some space for AI digging

            m_board.AddScore(m_playerA, 9);
            Assert_Score_Is(9, 0);
            Do_Turn_AI(m_playerA, 8);
            Assert_Score_Is(17, 0);
        }

        #endregion
    }
}

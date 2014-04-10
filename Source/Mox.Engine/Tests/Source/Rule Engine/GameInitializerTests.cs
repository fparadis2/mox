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
using Mox.Database;
using NUnit.Framework;
using Rhino.Mocks;

using Is = Rhino.Mocks.Constraints.Is;

namespace Mox
{
    [TestFixture]
    public class GameInitializerTests : BaseGameTests
    {
        #region Variables

        private ICardFactory m_cardFactory;
        private ICardDatabase m_cardDatabase;
        private GameInitializer m_gameInitializer;

        private Deck m_deckA;
        private Deck m_deckB;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            CreateGame(2);
            m_cardFactory = m_mockery.StrictMock<ICardFactory>();
            m_cardDatabase = m_mockery.StrictMock<ICardDatabase>();
            m_gameInitializer = new GameInitializer(m_cardFactory, m_cardDatabase);

            m_deckA = new Deck();
            m_deckA.Cards.Add(new CardIdentifier { Card = "1", Set = "MySet" });
            m_deckA.Cards.Add(new CardIdentifier { Card = "2" });

            m_deckB = new Deck();
            m_deckB.Cards.Add(new CardIdentifier { Card = "3" });

            m_gameInitializer.AssignDeck(m_playerA, m_deckA);
            m_gameInitializer.AssignDeck(m_playerB, m_deckB);
        }

        #endregion

        #region Utilities

        private void Initialize()
        {
            using (m_mockery.Unordered())
            {
                m_deckA.Cards.ForEach(Expect_GetCardInstance);
                m_deckB.Cards.ForEach(Expect_GetCardInstance);

                m_deckA.Cards.ForEach(Expect_InitializeCard);
                m_deckB.Cards.ForEach(Expect_InitializeCard);
            }

            m_mockery.Test(() => m_gameInitializer.Initialize(m_game));
        }

        private void Expect_GetCardInstance(CardIdentifier identifier)
        {
            Expect.Call(m_cardDatabase.ResolveCardIdentifier(identifier)).Return(identifier);
        }

        private void Expect_InitializeCard(CardIdentifier identifier)
        {
            m_cardFactory.InitializeCard(null);
            LastCall.Constraints(Is.Matching<Card>(card => card.CardIdentifier == identifier));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new GameInitializer(null, m_cardDatabase));
            Assert.Throws<ArgumentNullException>(() => new GameInitializer(m_cardFactory, null));
        }

        [Test]
        public void Test_Invalid_Initialize_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_gameInitializer.Initialize(null));
        }

        [Test]
        public void Test_Default_starting_life_is_20()
        {
            Assert.AreEqual(20, m_gameInitializer.StartingPlayerLife);
            m_gameInitializer.StartingPlayerLife = 50;
            Assert.AreEqual(50, m_gameInitializer.StartingPlayerLife);
        }

        [Test]
        public void Test_Players_have_the_starting_life_after_initialization()
        {
            m_gameInitializer.StartingPlayerLife = 42;

            Initialize();

            foreach (Player player in m_game.Players)
            {
                Assert.AreEqual(42, player.Life);
            }
        }

        [Test]
        public void Test_AssignDeck_invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_gameInitializer.AssignDeck(null, new Deck()));
        }

        [Test]
        public void Test_Initializer_initializes_cards_from_decks()
        {
            Initialize();

            Assert.AreEqual(2, m_playerA.Library.Count);
            Assert.IsTrue(m_playerA.Library.Count(card => card.CardIdentifier == m_deckA.Cards.First()) == 1);
            Assert.IsTrue(m_playerA.Library.Count(card => card.CardIdentifier == m_deckA.Cards.Skip(1).First()) == 1);

            Assert.AreEqual(1, m_playerB.Library.Count);
            Assert.IsTrue(m_playerB.Library[0].CardIdentifier == m_deckB.Cards.First());
        }

        [Test]
        public void Test_Initializer_initializes_the_random_generator_for_the_game()
        {
            Assert.AreEqual(-1, m_gameInitializer.Seed);
            
            Initialize();

            Assert.IsNotNull(m_game.Random);
        }

        [Test]
        public void Test_Initializer_initializes_the_random_generator_for_the_game_with_the_provided_seed()
        {
            m_gameInitializer.Seed = 10;

            Initialize();

            Assert.IsNotNull(m_game.Random);
            Assert.AreEqual(m_game.Random.Next(), Random.New(10).Next());
        }

        #endregion
    }
}

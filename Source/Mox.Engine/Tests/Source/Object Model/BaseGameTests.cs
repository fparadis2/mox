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
using NUnit.Framework;
using Rhino.Mocks;

using Mox.Abilities;

namespace Mox
{
    public class BaseGameTests
    {
        #region Variables

        protected MockRepository m_mockery;
        public Game m_game;

        protected Player m_playerA;
        protected Player m_playerB;
        protected Player m_playerC;

        protected Card m_card;
        protected MockAbility m_mockAbility;

        #endregion

        #region Utilities

        #region Cards

        /// <summary>
        /// Creates a new card.
        /// </summary>
        /// <returns></returns>
        protected Card CreateCard(Player owner)
        {
            return CreateCard(owner, "My Card");
        }

        /// <summary>
        /// Creates a new card.
        /// </summary>
        /// <returns></returns>
        protected Card CreateCard(Player owner, string name)
        {
            return m_game.CreateCard(owner, new CardIdentifier { Card = name });
        }

        #endregion

        #region Abilities

        protected MockAbility CreateMockAbility(Card source, AbilityType type)
        {
            MockAbility ability = m_game.CreateAbility<MockAbility>(source);
            ability.Implementation = m_mockery.PartialMock<MockAbility.Impl>();
            ability.MockedAbilityType = type;
            return ability;
        }

        protected MockTriggeredAbility CreateMockTriggeredAbility(Card source)
        {
            MockTriggeredAbility ability = m_game.CreateAbility<MockTriggeredAbility>(source);
            ability.Implementation = m_mockery.PartialMock<MockTriggeredAbility.Impl>();
            return ability;
        }

        protected MockEvasionAbility CreateMockEvasionAbility(Card source)
        {
            MockEvasionAbility ability = m_game.CreateAbility<MockEvasionAbility>(source);
            ability.Implementation = m_mockery.PartialMock<MockEvasionAbility.Impl>();
            return ability;
        }

        #endregion

        #region Sequencing

        protected IDisposable OrderedExpectations
        {
            get
            {
                return m_mockery.Ordered();
            }
        }

        protected IDisposable UnorderedExpectations
        {
            get
            {
                return m_mockery.Ordered();
            }
        }

        #endregion

        #region Asserts

        /// <summary>
        /// Tests the equality between objects.
        /// </summary>
        protected static void Assert_Objects_have_same_identifier(Object expected, Object actual)
        {
            NUnit.Framework.Assert.AreEqual(expected.Identifier, actual.Identifier);
        }

        protected void Assert_HashIsEqual(IHashable hashable1, IHashable hashable2)
        {
            HashContext context = new HashContext(m_game);

            Hash hash1 = new Hash();
            hashable1.ComputeHash(hash1, context);

            Hash hash2 = new Hash();
            hashable2.ComputeHash(hash2, context);

            NUnit.Framework.Assert.AreEqual(hash1.Value, hash2.Value);
        }

        protected void Assert_HashIsNotEqual(IHashable hashable1, IHashable hashable2)
        {
            HashContext context = new HashContext(m_game);

            Hash hash1 = new Hash();
            hashable1.ComputeHash(hash1, context);

            Hash hash2 = new Hash();
            hashable2.ComputeHash(hash2, context);

            NUnit.Framework.Assert.AreNotEqual(hash1.Value, hash2.Value);
        }

        #endregion

        #region Targeting

        protected IEnumerable<GameObject> GetTargetables(Predicate<GameObject> filter)
        {
            foreach (Object obj in GetTargetables())
            {
                GameObject targetable = obj as GameObject;
                if (targetable != null && filter(targetable))
                {
                    yield return targetable;
                }
            }
        }

        private IEnumerable<GameObject> GetTargetables()
        {
            foreach (Player player in m_game.Players)
            {
                yield return player;
            }

            foreach (Card card in m_game.Zones.Battlefield.AllCards)
            {
                yield return card;
            }
        }

        #endregion

        #endregion

        #region Setup / Teardown

        [SetUp]
        public virtual void Setup()
        {
            m_mockery = new MockRepository();

            CreateGame(2);
            m_card = CreateCard(m_playerA);
            m_mockAbility = CreateMockAbility(m_card, AbilityType.Normal);
        }

        [TearDown]
        public virtual void Teardown()
        {
        }

        protected void CreateGame(int numPlayers)
        {
            NUnit.Framework.Assert.IsTrue(numPlayers == 2 || numPlayers == 3);

            m_game = new Game();
            m_game.UseRandom(new MockRandom());

            m_playerA = m_game.CreatePlayer(); m_playerA.Name = "Player A";
            m_playerB = m_game.CreatePlayer(); m_playerB.Name = "Player B";

            if (numPlayers == 2)
            {
                m_playerC = null;
            }
            else
            {
                m_playerC = m_game.CreatePlayer(); m_playerC.Name = "Player C";
            }
        }

        #endregion
    }
}

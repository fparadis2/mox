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

namespace Mox
{
    [TestFixture]
    public class GameTests : BaseGameTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        #endregion

        #region Utilities

        private int[] Test_Shuffle(int n)
        {
            Expect_Shuffle_Reverse(n);

            int[] result = null;
            m_mockery.Test(() => result = m_random.Shuffle(n));
            return result;
        }

        #endregion

        #region Tests

        /// <summary>
        /// I don't trust that utility method.
        /// </summary>
        [Test]
        public void Test_Expect_Shuffle_Reverse()
        {
            Assert.AreEqual(new[] { 0 }, Test_Shuffle(1));
            Assert.AreEqual(new[] { 0, 0 }, Test_Shuffle(2));
            Assert.AreEqual(new[] { 0, 1, 0 }, Test_Shuffle(3));
            Assert.AreEqual(new[] { 0, 2, 2, 0 }, Test_Shuffle(4));
            Assert.AreEqual(new[] { 0, 3, 2, 3, 0 }, Test_Shuffle(5));
        }

        [Test]
        public void Test_Game_is_composed_of_players()
        {
            Assert.Collections.AreEquivalent(new[] { m_playerA, m_playerB }, m_game.Players);
            Assert.IsTrue(m_game.Players.IsReadOnly);
        }

        [Test]
        public void Test_Game_is_composed_of_cards()
        {
            Assert.Collections.AreEquivalent(new[] { m_card }, m_game.Cards);

            Card newCard = CreateCard(m_playerA);
            Assert.Collections.AreEquivalent(new[] { m_card, newCard }, m_game.Cards);
            m_game.Cards.Remove(newCard);

            Assert.Collections.AreEquivalent(new[] { m_card }, m_game.Cards);
        }

        [Test]
        public void Test_Game_is_composed_of_abilities()
        {
            m_game.Abilities.Clear();

            Assert.Collections.IsEmpty(m_game.Abilities);

            PlayCardAbility newAbility = m_game.CreateAbility<PlayCardAbility>(m_card);
            Assert.Collections.AreEquivalent(new[] { newAbility }, m_game.Abilities);
            m_game.Abilities.Remove(newAbility);

            Assert.Collections.IsEmpty(m_game.Abilities);
        }

        [Test]
        public void Test_Game_has_a_spell_stack()
        {
            Assert.IsNotNull(m_game.SpellStack);
        }

        [Test]
        public void Test_Random_is_not_set_by_default()
        {
            Assert.Throws<InvalidOperationException>(() => new Game().Random.Next());
        }

        [Test]
        public void Test_Can_set_Random_afterwards()
        {
            m_game = new Game();

            IRandom random1 = Random.New();
            IRandom random2 = Random.New();
            using (m_game.UseRandom(random1))
            {
                Assert.AreSame(random1, m_game.Random);

                using (m_game.UseRandom(random2))
                {
                    Assert.AreSame(random2, m_game.Random);
                }

                Assert.AreSame(random1, m_game.Random);
            }

            Assert.Throws<InvalidOperationException>(() => m_game.Random.Next());
        }

        [Test]
        public void Test_Cannot_access_an_invalid_zone()
        {
            Assert.Throws<ArgumentException>(() => m_game.Zones[(Zone.Id)1234].ToString());
        }

        [Test]
        public void Test_Contains_an_event_repository()
        {
            Assert.IsNotNull(m_game.Events);
        }

        [Test]
        public void Test_Log_never_null()
        {
            m_game.Log = null;
            Assert.IsNotNull(m_game.Log);
        }

        [Test]
        public void Test_Can_assign_Log()
        {
            var context = new LogContext();
            m_game.Log = context;
            Assert.AreEqual(context, m_game.Log);
        }

        #endregion
    }
}

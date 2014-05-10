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
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class GameHashTests : BaseGameTests
    {
        #region Utilities

        private Hash ComputeHash()
        {
            Hash hash = new Hash();
            m_game.ComputeHash(hash);
            return hash;
        }

        private void Assert_Hash_changes(System.Action action)
        {
            Hash baseHash = ComputeHash();

            action();

            Hash newHash = ComputeHash();

            Assert.AreNotEqual(baseHash.Value, newHash.Value);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_ComputeHash_takes_cards_into_account()
        {
            Card card = null;
            Assert_Hash_changes(() => card = CreateCard(m_playerA));
            Assert_Hash_changes(() => card.Zone = m_game.Zones.Battlefield);
            Assert_Hash_changes(() => card.Power = 3);
        }

        [Test]
        public void Test_ComputeHash_takes_players_into_account()
        {
            m_playerA.Life = 10;
            Assert_Hash_changes(() => m_playerA.Life = 5);
        }

        [Test]
        public void Test_ComputeHash_takes_abilities_into_account()
        {
            m_mockAbility.MockProperty = 3;
            Assert_Hash_changes(() => m_mockAbility.MockProperty = 4);
        }

        [Test]
        public void Test_ComputeHash_takes_targets_into_accounts()
        {
            TargetCost cost = TargetCost.Player();
            cost.SetSourceAbility(m_mockAbility);
            Assert_Hash_changes(() => cost.SetResult(m_game, m_playerA));
        }

        #endregion
    }
}

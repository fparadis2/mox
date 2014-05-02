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
        public void Test_Can_compute_a_hash_for_all_object_types()
        {
            var objectTypes = typeof (Game).Assembly.GetTypes().Where(type => typeof (Object).IsAssignableFrom(type) && !type.IsAbstract);

            foreach (var objectType in objectTypes)
            {
                var obj = (Object)Activator.CreateInstance(objectType);

                Hash hash = new Hash();
                ObjectManipulators.GetManipulator(obj).ComputeHash(obj, hash);
            }
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

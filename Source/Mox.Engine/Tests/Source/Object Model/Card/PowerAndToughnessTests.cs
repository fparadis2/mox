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
    public class PowerAndToughnessTests : BaseGameTests
    {
        #region Tests

        [Test]
        public void Test_can_set_power()
        {
            PowerAndToughness pw = new PowerAndToughness();

            pw.Power = 3;
            Assert.AreEqual(3, pw.Power);
        }

        [Test]
        public void Test_can_set_toughness()
        {
            PowerAndToughness pw = new PowerAndToughness();

            pw.Toughness = 3;
            Assert.AreEqual(3, pw.Toughness);
        }

        [Test]
        public void Test_ComputeHash()
        {
            Assert_HashIsEqual(new PowerAndToughness(), new PowerAndToughness());
            Assert_HashIsEqual(new PowerAndToughness { Power = 1, Toughness = 2 }, new PowerAndToughness { Power = 1, Toughness = 2 });

            Assert_HashIsNotEqual(new PowerAndToughness { Power = 1, Toughness = 2 }, new PowerAndToughness { Power = 2, Toughness = 2 });
            Assert_HashIsNotEqual(new PowerAndToughness { Power = 1, Toughness = 2 }, new PowerAndToughness { Power = 1, Toughness = 3 });
        }

        #endregion
    }
}

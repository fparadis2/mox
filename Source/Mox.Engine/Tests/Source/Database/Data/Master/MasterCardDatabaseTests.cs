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
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Mox.Database
{
    [TestFixture]
    public class MasterCardDatabaseTests
    {
        #region Tests

        [Test]
        public void Test_Can_access_static_instance()
        {
            Assert.IsNotNull(MasterCardDatabase.Instance);
        }

        [Test]
        public void Test_Can_access_sets()
        {
            Assert.IsNotNull(MasterCardDatabase.Instance.Sets["10E"]);
        }

        [Test]
        public void Test_Every_card_has_at_least_an_instance()
        {
            var database = MasterCardDatabase.Instance;

            foreach (CardInfo card in database.Cards)
            {
                Assert.That(card.Instances.Any(), "Card {0} has no instances!", card.Name);
            }
        }

        #endregion
    }
}

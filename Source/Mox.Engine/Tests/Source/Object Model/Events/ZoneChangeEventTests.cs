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
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Events
{
    [TestFixture]
    public class ZoneChangeEventTests : BaseGameTests
    {
        #region Variables

        private ZoneChangeEvent m_zoneChangeEvent;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_zoneChangeEvent = new ZoneChangeEvent(m_card, m_game.Zones.Graveyard, m_game.Zones.Hand);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new ZoneChangeEvent(null, m_game.Zones.Graveyard, m_game.Zones.Hand));
            Assert.Throws<ArgumentNullException>(() => new ZoneChangeEvent(m_card, null, m_game.Zones.Hand));
            Assert.Throws<ArgumentNullException>(() => new ZoneChangeEvent(m_card, m_game.Zones.Graveyard, null));
        }

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.AreEqual(m_card, m_zoneChangeEvent.Card);
            Assert.AreEqual(m_game.Zones.Graveyard, m_zoneChangeEvent.OldZone);
            Assert.AreEqual(m_game.Zones.Hand, m_zoneChangeEvent.NewZone);
        }

        [Test]
        public void Changing_the_zone_of_a_card_triggers_a_ZoneChange_event()
        {
            Assert.AreEqual(m_game.Zones.Library, m_card.Zone, "Sanity check");

            m_game.AssertTriggers<ZoneChangeEvent>(() => { m_card.Zone = m_game.Zones.Hand; }, e =>
            {
                Assert.AreEqual(m_card, e.Card);
                Assert.AreEqual(m_game.Zones.Library, e.OldZone);
                Assert.AreEqual(m_game.Zones.Hand, e.NewZone);
            });
        }

        [Test]
        public void Changing_the_zone_of_a_card_doesnt_trigger_a_ZoneChange_event_during_a_rollback()
        {
            Assert.AreEqual(m_game.Zones.Library, m_card.Zone, "Sanity check");

            using (ITransaction transaction = m_game.TransactionStack.BeginTransaction())
            {
                m_card.Zone = m_game.Zones.Hand;

                m_game.AssertDoesNotTrigger<ZoneChangeEvent>(transaction.Rollback);
            }
        }

        #endregion
    }
}

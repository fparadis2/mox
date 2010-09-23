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

namespace Mox
{
    [TestFixture]
    public class ZoneRulesTests : BaseGameTests
    {
        #region Tests

        /// <summary>
        /// 217.1. 
        /// A zone is a place where objects can be during a game. 
        /// There are normally six zones: library, hand, graveyard, in play, stack, and removed from the game. 
        /// Some older cards also use the ante and phased-out zones.
        /// 
        /// Each player has his or her own library, hand, and graveyard. 
        /// The other zones are shared by all players.
        /// </summary>
        [Test]
        public void Test_217_1()
        {
            Assert.IsNotNull(m_game.Zones);
            Assert.IsNotNull(m_game.Zones.Library);
            Assert.IsNotNull(m_game.Zones.Hand);
            Assert.IsNotNull(m_game.Zones.Graveyard);
            Assert.IsNotNull(m_game.Zones.Battlefield);
            Assert.IsNotNull(m_game.Zones.Stack);
            Assert.IsNotNull(m_game.Zones.Exile);
            Assert.IsNotNull(m_game.Zones.PhasedOut);

            Assert.IsNotNull(m_game.Zones.Stack);
        }

        /// <summary>
        /// 217.1a If an object would go to any library, graveyard, or hand other than its owner's, it goes to its owner's corresponding zone. 
        /// If an instant or sorcery card would come into play, it remains in its previous zone.
        /// </summary>
        [Test]
        public void Test_217_1a()
        {
            new Zone[] { m_game.Zones.Hand, m_game.Zones.Library, m_game.Zones.Graveyard }.ForEach(zone =>
            {
                Card card = CreateCard(m_playerA);
                card.Zone = m_game.Zones.Battlefield;
                card.Controller = m_playerB;
                card.Zone = zone;
                Assert.AreSame(m_playerA, card.Controller);
            });

            new Zone[] { m_game.Zones.Hand, m_game.Zones.Library, m_game.Zones.Graveyard }.ForEach(zone =>
            {
                Card card = CreateCard(m_playerA);
                card.Zone = zone;
                card.Controller = m_playerB;
                Assert.AreSame(m_playerA, card.Controller);
                
                // Check that the controller can be changed after...
                card.Zone = m_game.Zones.Battlefield;
                card.Controller = m_playerB;
                Assert.AreSame(m_playerB, card.Controller);
            });

            Card instant = CreateCard(m_playerA); instant.Type = Type.Instant;
            Card sorcery = CreateCard(m_playerA); sorcery.Type = Type.Sorcery;

            instant.Zone = m_game.Zones.Hand;
            sorcery.Zone = m_game.Zones.Hand;

            instant.Zone = m_game.Zones.Battlefield;
            sorcery.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(m_game.Zones.Hand, instant.Zone);
            Assert.AreEqual(m_game.Zones.Hand, sorcery.Zone);
        }

        #endregion
    }
}

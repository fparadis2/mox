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

namespace Mox.Replication
{
    [TestFixture]
    public class OpenVisibilityStrategyTests : BaseGameTests
    {
        #region Variables

        private OpenVisibilityStrategy m_strategy;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_strategy = new OpenVisibilityStrategy();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_IsVisible_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_strategy.IsVisible(null, m_playerA));
        }

        [Test]
        public void Test_Players_are_always_visible_to_one_another()
        {
            Assert.IsTrue(m_strategy.IsVisible(m_playerA, m_playerA));
            Assert.IsTrue(m_strategy.IsVisible(m_playerA, m_playerB));
            Assert.IsTrue(m_strategy.IsVisible(m_playerA, null));
        }

        [Test]
        public void Test_Cards_are_always_visible_to_all_players()
        {
            foreach (Zone zone in new[] { m_game.Zones.Battlefield, m_game.Zones.PhasedOut, m_game.Zones.Graveyard, m_game.Zones.Exile, m_game.Zones.Stack, m_game.Zones.Library, m_game.Zones.Hand })
            {
                m_card.Zone = m_game.Zones.Battlefield;

                Assert.IsTrue(m_strategy.IsVisible(m_card, m_playerA));
                Assert.IsTrue(m_strategy.IsVisible(m_card, m_playerB));
                Assert.IsTrue(m_strategy.IsVisible(m_card, null));
            }
        }

        [Test]
        public void Test_Abilities_are_always_visible_to_all_players()
        {
            Assert.IsTrue(m_strategy.IsVisible(m_card, m_playerA));
            Assert.IsTrue(m_strategy.IsVisible(m_mockAbility, m_playerB));
            Assert.IsTrue(m_strategy.IsVisible(m_mockAbility, null));
        }

        [Test]
        public void Test_Can_attach_and_detach_to_ObjectVisibilityChanged_event()
        {
            EventSink<VisibilityChangedEventArgs> sink = new EventSink<VisibilityChangedEventArgs>();
            m_strategy.ObjectVisibilityChanged += sink;
            m_strategy.ObjectVisibilityChanged -= sink;
        }

        #endregion
    }
}

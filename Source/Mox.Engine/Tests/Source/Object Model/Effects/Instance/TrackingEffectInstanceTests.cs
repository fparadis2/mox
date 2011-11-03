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
    public class TrackingEffectInstanceTests : BaseGameTests
    {
        #region Inner Types

        private class MyEffect : Effect<PowerAndToughness>
        {
            public MyEffect()
                : base(Card.PowerAndToughnessProperty)
            {
            }

            public override PowerAndToughness Modify(Object owner, PowerAndToughness value)
            {
                value.Power += 1;
                return value;
            }
        }

        private class MyCondition : Condition
        {
            public override bool Matches(Card card)
            {
                return card.Is(Color.Blue);
            }

            protected internal override bool Invalidate(PropertyBase property)
            {
                return base.Invalidate(property) || property == Card.ColorProperty;
            }
        }

        #endregion

        #region Variables

        private Card m_card1;
        private Card m_card2;
        private MyEffect m_effect;
        private MyCondition m_condition;

        private TrackingEffectInstance m_instanceWithoutCondition;
        private TrackingEffectInstance m_instanceWithCondition;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_card1 = CreateCard(m_playerA);
            m_card1.Zone = m_game.Zones.Battlefield;
            m_card1.Color = Color.Blue;

            m_card2 = CreateCard(m_playerA);
            m_card2.Zone = m_game.Zones.Battlefield;
            m_card2.Color = Color.Blue;

            m_effect = new MyEffect();
            m_condition = new MyCondition();

            m_instanceWithoutCondition = m_game.CreateTrackingEffect(m_effect, Condition.True, m_game.Zones.Battlefield);
            m_instanceWithCondition = m_game.CreateTrackingEffect(m_effect, m_condition, m_game.Zones.Battlefield);
        }

        #endregion

        #region Utilities

        private int CountOfGlobalEffectInstances
        {
            get { return m_game.Objects.Where(obj => obj is TrackingEffectInstance).Count(); }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(() => m_game.CreateTrackingEffect(null, m_condition, m_game.Zones.Battlefield));
            Assert.Throws<ArgumentNullException>(() => m_game.CreateTrackingEffect(m_effect, null, m_game.Zones.Battlefield));
            Assert.Throws<ArgumentNullException>(() => m_game.CreateTrackingEffect(m_effect, m_condition, null));
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_effect, m_instanceWithoutCondition.Effect);
            Assert.AreEqual(m_condition, m_instanceWithCondition.Condition);
        }

        [Test]
        public void Test_Creation_automatically_adds_in_manager_objects()
        {
            Assert.Collections.Contains(m_instanceWithoutCondition, m_game.Objects);
        }

        [Test]
        public void Test_Creation_is_undoable()
        {
            int initialCount = CountOfGlobalEffectInstances;

            Assert.IsUndoRedoable(m_game.Controller, 
                () => Assert.AreEqual(initialCount, CountOfGlobalEffectInstances),
                () => m_game.CreateTrackingEffect(m_effect, m_condition, m_game.Zones.Battlefield), 
                () => Assert.AreEqual(initialCount + 1, CountOfGlobalEffectInstances));
        }

        [Test]
        public void Test_Effect_affects_cards_entering_and_leaving_tracking_zone()
        {
            Assert.AreEqual(2, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);

            m_card1.Zone = m_game.Zones.Hand;

            Assert.AreEqual(0, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);

            m_card1.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(2, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);
        }

        [Test]
        public void Test_Effect_affects_cards_that_satisfy_or_not_condition()
        {
            Assert.AreEqual(2, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);

            m_card1.Color = Color.Red;

            Assert.AreEqual(1, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);

            m_card1.Color = Color.Blue;

            Assert.AreEqual(2, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);
        }

        [Test]
        public void Test_Complex_case()
        {
            Assert.AreEqual(2, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);

            m_card1.Zone = m_game.Zones.Hand;
            m_card1.Color = Color.Red;

            Assert.AreEqual(0, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);

            m_card1.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(1, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);

            m_card1.Color = Color.Blue;

            Assert.AreEqual(2, m_card1.Power);
            Assert.AreEqual(2, m_card2.Power);
        }

        #endregion
    }
}
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

namespace Mox.Abilities
{
    [TestFixture]
    public class TrackingEffectInstanceTests : BaseGameTests
    {
        #region Variables

        private Card m_source;
        private Card m_card1;
        private Card m_card2;
        private ModifyPowerAndToughnessEffect m_effect;
        private ObjectResolver m_targets;

        private ContinuousAbility m_ability;
        private TrackingEffectInstance m_instance;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_source = CreateCard(m_playerA);
            m_source.Zone = m_game.Zones.Battlefield;

            m_card1 = CreateCard(m_playerA);
            m_card1.Type = Type.Creature;
            m_card1.Zone = m_game.Zones.Battlefield;

            m_card2 = CreateCard(m_playerA);
            m_card2.Type = Type.Creature;
            m_card2.Zone = m_game.Zones.Battlefield;

            m_ability = m_game.CreateAbility<ContinuousAbility>(m_source);
            m_effect = new ModifyPowerAndToughnessEffect(m_ability, 1, 0);
            m_targets = new FilterObjectResolver(PermanentFilter.AnyCreature & PermanentFilter.ControlledByYou);

            m_instance = CreateInstance();
        }

        #endregion

        #region Utilities

        private int CountOfGlobalEffectInstances
        {
            get { return m_game.Objects.Where(obj => obj is TrackingEffectInstance).Count(); }
        }

        private TrackingEffectInstance CreateInstance()
        {
            return m_game.CreateTrackingEffect(m_ability, m_targets, m_effect, null);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_Construction_values()
        {
            Assert.Throws<ArgumentNullException>(() => m_game.CreateTrackingEffect(null, m_targets, m_effect, null));
            Assert.Throws<ArgumentNullException>(() => m_game.CreateTrackingEffect(m_ability, null, m_effect, null));
            Assert.Throws<ArgumentNullException>(() => m_game.CreateTrackingEffect(m_ability, m_targets, null, null));
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_targets, m_instance.Targets);
            Assert.AreEqual(m_ability, m_instance.Ability);
        }

        [Test]
        public void Test_Creation_automatically_adds_in_manager_objects()
        {
            Assert.Collections.Contains(m_instance, m_game.Objects);
        }

        [Test]
        public void Test_Creation_is_undoable()
        {
            int initialCount = CountOfGlobalEffectInstances;

            Assert.IsUndoRedoable(m_game.Controller, 
                () => Assert.AreEqual(initialCount, CountOfGlobalEffectInstances),
                () => CreateInstance(), 
                () => Assert.AreEqual(initialCount + 1, CountOfGlobalEffectInstances));
        }

        [Test]
        public void Test_Effect_affects_cards_entering_and_leaving_the_battlefield()
        {
            Assert.AreEqual(1, m_card1.Power);
            Assert.AreEqual(1, m_card2.Power);

            m_card1.Zone = m_game.Zones.Hand;

            Assert.AreEqual(0, m_card1.Power);
            Assert.AreEqual(1, m_card2.Power);

            m_card1.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(1, m_card1.Power);
            Assert.AreEqual(1, m_card2.Power);
        }

        [Test]
        public void Test_Effect_affects_cards_that_satisfy_or_not_condition()
        {
            Assert.AreEqual(1, m_card1.Power);
            Assert.AreEqual(1, m_card2.Power);

            m_card1.Controller = m_playerB;

            Assert.AreEqual(0, m_card1.Power);
            Assert.AreEqual(1, m_card2.Power);

            m_card1.Controller = m_playerA;

            Assert.AreEqual(1, m_card1.Power);
            Assert.AreEqual(1, m_card2.Power);
        }

        #endregion
    }
}
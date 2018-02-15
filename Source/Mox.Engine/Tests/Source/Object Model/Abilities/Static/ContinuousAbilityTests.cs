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
using NUnit.Framework;

namespace Mox.Abilities
{
    [TestFixture]
    public class ContinuousAbilityTests : BaseGameTests
    {
        #region Variables

        private ContinuousAbility m_ability;

        private Card m_otherCard1;
        private Card m_otherCard2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            var spellDefinition = CreateSpellDefinition(m_card);
            var targets = new FilterObjectResolver(PermanentFilter.AnyCreature & PermanentFilter.ControlledByYou);
            spellDefinition.AddAction(new ModifyPowerAndToughnessAction(targets, null, 1, 0));

            m_otherCard1 = CreateCard(m_playerA);
            m_otherCard1.Type = Type.Creature;
            m_otherCard1.Zone = m_game.Zones.Battlefield;

            m_otherCard2 = CreateCard(m_playerB);
            m_otherCard2.Type = Type.Creature;
            m_otherCard2.Zone = m_game.Zones.Battlefield;

            m_ability = m_game.CreateAbility<ContinuousAbility>(m_card, spellDefinition);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Ability_only_has_an_effect_when_card_is_in_play()
        {
            m_card.Zone = m_game.Zones.Hand;
            Assert.AreEqual(0, m_otherCard1.Power);
            Assert.AreEqual(0, m_otherCard2.Power);

            m_card.Zone = m_game.Zones.Battlefield;
            Assert.AreEqual(1, m_otherCard1.Power);
            Assert.AreEqual(0, m_otherCard2.Power);

            m_card.Zone = m_game.Zones.Hand;
            Assert.AreEqual(0, m_otherCard1.Power);
            Assert.AreEqual(0, m_otherCard2.Power);
        }

        [Test]
        public void Test_Ability_only_has_an_effect_on_targeted_objects()
        {
            m_card.Zone = m_game.Zones.Battlefield;
            Assert.AreEqual(1, m_otherCard1.Power);
            Assert.AreEqual(0, m_otherCard2.Power);

            m_otherCard2.Controller = m_playerA;
            Assert.AreEqual(1, m_otherCard1.Power);
            Assert.AreEqual(1, m_otherCard2.Power);

            m_otherCard2.Controller = m_playerB;
            Assert.AreEqual(1, m_otherCard1.Power);
            Assert.AreEqual(0, m_otherCard2.Power);
        }

        #endregion
    }
}

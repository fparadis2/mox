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

namespace Mox
{
    [TestFixture]
    public class AttachmentAbilityTests : BaseGameTests
    {
        #region Mock Types

        private class MockAttachmentAbility : AttachmentAbility
        {
            protected override IEnumerable<IEffectCreator> Attach(ILocalEffectHost<Card> cardEffectHost)
            {
                yield return cardEffectHost.ModifyPowerAndToughness(+1, +1);
            }
        }

        #endregion

        #region Variables

        private Card m_otherCard1;
        private Card m_otherCard2;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_card.SubTypes |= SubType.Aura;

            m_otherCard1 = CreateCard(m_playerA); m_otherCard1.Type = Type.Creature;
            m_otherCard2 = CreateCard(m_playerB); m_otherCard2.Type = Type.Creature;
            m_game.CreateAbility<MockAttachmentAbility>(m_card);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Attachment_abilities_have_an_effect_when_attached()
        {
            m_card.Zone = m_otherCard1.Zone = m_otherCard2.Zone = m_game.Zones.Battlefield;

            Assert.AreEqual(0, m_otherCard1.Power);
            Assert.AreEqual(0, m_otherCard2.Power);

            m_card.Attach(m_otherCard1);

            Assert.AreEqual(1, m_otherCard1.Power);
            Assert.AreEqual(0, m_otherCard2.Power);

            m_card.Attach(m_otherCard2);

            Assert.AreEqual(0, m_otherCard1.Power);
            Assert.AreEqual(1, m_otherCard2.Power);

            m_card.Attach(null);

            Assert.AreEqual(0, m_otherCard1.Power);
            Assert.AreEqual(0, m_otherCard2.Power);
        }

        #endregion
    }
}

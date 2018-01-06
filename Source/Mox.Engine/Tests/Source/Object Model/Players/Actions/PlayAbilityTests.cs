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

using NUnit.Framework;
using Rhino.Mocks;

using Mox.Abilities;

namespace Mox
{
    [TestFixture]
    public class PlayAbilityTests : BaseGameTests
    {
        #region Variables

        private PlayAbility m_action;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_action = new PlayAbility(m_mockAbility);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_mockAbility, m_action.Ability.Resolve(m_game));
        }

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(delegate { new PlayAbility(null); });
        }

        [Test]
        public void Test_CanExecute_checks_whether_the_ability_can_be_played()
        {
            m_mockAbility.Expect_CanPlay();
            m_mockery.Test(() => Assert.IsTrue(m_action.CanExecute(new AbilityEvaluationContext(m_playerA, AbilityEvaluationContextType.Normal))));
        }

        #endregion
    }
}

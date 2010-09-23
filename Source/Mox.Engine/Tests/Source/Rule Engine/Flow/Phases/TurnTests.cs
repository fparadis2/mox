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
using System.Collections.Generic;
using Rhino.Mocks;

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class TurnTests : BaseGameTests
    {
        #region Variables

        private Turn m_turn;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_turn = new Turn();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Contains_no_phases_by_default()
        {
            Assert.Collections.IsEmpty(m_turn.Phases);
            Assert.IsFalse(m_turn.Phases.IsReadOnly);
        }

        #endregion
    }
}

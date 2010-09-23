﻿// Copyright (c) François Paradis
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
using Mox.Flow.Parts;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.Flow
{
    [TestFixture]
    public class MTGPartTests : PartTestUtilities
    {
        #region Variables

        private MTGPart m_part;

        #endregion

        #region Setup

        public override void Setup()
        {
            base.Setup();

            m_part = m_mockery.StrictMock<MTGPart>(m_playerA);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_Player()
        {
            Assert.AreEqual(m_playerA, m_part.GetPlayer(m_sequencerTester.Context));
        }

        #endregion
    }
}

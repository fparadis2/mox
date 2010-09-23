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

namespace Mox.Flow.Phases
{
    [TestFixture]
    public class DrawStepTests : BaseStepTests<DrawStep>
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_step = new DrawStep();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(Steps.Draw, m_step.Type);
        }

        [Test]
        public void Test_Active_player_draws_a_card()
        {
            Card theCard = CreateCard(m_playerA);
            m_playerA.Library.MoveToTop(new[] { theCard });

            m_game.State.CurrentTurn = 10;

            RunStep(m_playerA);

            Assert.Collections.Contains(theCard, m_playerA.Hand);
        }

        [Test]
        public void Test_First_player_skips_his_draw_step()
        {
            Card theCard = CreateCard(m_playerA);
            m_playerA.Library.MoveToTop(new[] { theCard });

            m_game.State.CurrentTurn = 0;

            RunStep(m_playerA);

            Assert.Collections.Contains(theCard, m_playerA.Library);
        }

        #endregion
    }
}

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

namespace Mox.UI.Game
{
    [TestFixture]
    public class GameElementViewModelTests : BaseGameViewModelTests
    {
        #region Variables

        private GameElementViewModel m_gameElementViewModel;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_gameElementViewModel = new GameElementViewModel(m_gameViewModel);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new CardViewModel(null));
        }

        [Test]
        public void Test_construction_arguments()
        {
            Assert.AreEqual(m_gameViewModel, m_gameElementViewModel.GameViewModel);
        }

        #endregion
    }
}

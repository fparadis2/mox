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

using NUnit.Framework;

namespace Mox.UI.Game
{
    [TestFixture]
    public class GameStateViewModelTests : BaseGameViewModelTests
    {
        #region Variables

        private GameStateViewModel m_stateViewModel;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_stateViewModel = new GameStateViewModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_ActivePlayer()
        {
            PlayerViewModel player = new PlayerViewModel(m_gameViewModel);
            m_stateViewModel.ActivePlayer = player;
            Assert.AreEqual(player, m_stateViewModel.ActivePlayer);
        }

        #endregion
    }
}

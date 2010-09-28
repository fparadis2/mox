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

namespace Mox.UI.Browser
{
    [TestFixture]
    public class DeckCardViewModelTests : DeckViewModelTestsBase
    {
        #region Variables

        private DeckCardViewModel m_model;

        #endregion

        #region Setup

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            DeckViewModel owner = new DeckViewModel(m_editor, m_deck);

            m_model = new DeckCardViewModel(owner, m_card1);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(3, m_model.Quantity);
        }

        [Test]
        public void Test_Can_change_quantity()
        {
            m_editor.IsEnabled = true;
            m_model.Quantity = 7;
            Assert.AreEqual(7, m_model.Quantity);
            Assert.AreEqual(7, m_deck.Cards[m_card1]);
        }

        [Test]
        public void Test_Setting_an_invalid_quantity_does_nothing()
        {
            m_editor.IsEnabled = true;
            m_model.Quantity = 0;
            Assert.AreEqual(3, m_model.Quantity);

            m_model.Quantity = -1;
            Assert.AreEqual(3, m_model.Quantity);
        }

        [Test]
        public void Test_Cannot_change_properties_when_not_enabled()
        {
            m_editor.IsEnabled = false;
            Assert.Throws<InvalidOperationException>(() => m_model.Quantity = 2);
        }

        #endregion
    }
}

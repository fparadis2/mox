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

namespace Mox
{
    [TestFixture]
    public class CardCollectionTests : BaseGameTests
    {
        #region Variables

        private Card m_cardInLibrary1;
        private Card m_cardInLibrary2;

        #endregion

        #region Setup

        public override void Setup()
        {
            base.Setup();

            m_game.Cards.Clear();

            m_cardInLibrary1 = CreateCard(m_playerA);
            m_cardInLibrary2 = CreateCard(m_playerA);
            m_playerA.Library.MoveToTop(new[] { m_cardInLibrary1 });
            m_playerA.Library.MoveToTop(new[] { m_cardInLibrary2 });
        }

        #endregion

        #region Tests

        #region Top & Bottom

        [Test]
        public void Test_Top_returns_the_top_cards_of_the_zone()
        {
            Assert.AreEqual(m_cardInLibrary2, m_playerA.Library.Top());

            Assert.Collections.IsEmpty(m_playerA.Library.Top(0));
            Assert.Collections.AreEqual(new[] {m_cardInLibrary2}, m_playerA.Library.Top(1));
            Assert.Collections.AreEqual(new[] {m_cardInLibrary2, m_cardInLibrary1}, m_playerA.Library.Top(2));
            Assert.Collections.AreEqual(new[] {m_cardInLibrary2, m_cardInLibrary1}, m_playerA.Library.Top(3));
        }

        [Test]
        public void Test_Bottom_returns_the_bottom_cards_of_the_zone()
        {
            Assert.AreEqual(m_cardInLibrary1, m_playerA.Library.Bottom());

            Assert.Collections.IsEmpty(m_playerA.Library.Bottom(0));
            Assert.Collections.AreEqual(new[] { m_cardInLibrary1 }, m_playerA.Library.Bottom(1));
            Assert.Collections.AreEqual(new[] { m_cardInLibrary1, m_cardInLibrary2 }, m_playerA.Library.Bottom(2));
            Assert.Collections.AreEqual(new[] { m_cardInLibrary1, m_cardInLibrary2 }, m_playerA.Library.Bottom(3));
        }

        #endregion

        #endregion
    }
}

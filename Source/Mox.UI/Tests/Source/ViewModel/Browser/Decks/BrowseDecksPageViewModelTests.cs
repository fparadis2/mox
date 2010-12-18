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
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class BrowseDecksPageViewModelTests
    {
        #region Variables

        private BrowseDecksPageViewModel m_pageModel;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            CardDatabase database = new CardDatabase();
            DeckLibrary library = new DeckLibrary();

            m_pageModel = new BrowseDecksPageViewModel(library, new EditDeckViewModel(database, null));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNotNull(m_pageModel.Library);
            Assert.AreEqual("Deck library", m_pageModel.Title);
            Assert.IsNotNull(m_pageModel.Editor);
            Assert.IsNotNull(m_pageModel.Editor.Database);
        }
        
        #endregion
    }
}

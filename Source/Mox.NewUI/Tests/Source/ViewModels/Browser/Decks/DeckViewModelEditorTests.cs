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
    public class DeckViewModelEditorTests
    {
        #region Variables

        private CardDatabase m_database;
        private IMasterCardFactory m_cardFactory;
        private DeckViewModelEditor m_model;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_database = new CardDatabase();
            m_cardFactory = new CompoundCardFactory();
            m_model = new DeckViewModelEditor(m_database, m_cardFactory);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_database, m_model.Database);
            Assert.AreEqual(m_cardFactory, m_model.CardFactory);
        }

        [Test]
        public void Test_PropertyChangeNotification()
        {
            Assert.ThatAllPropertiesOn(m_model).RaiseChangeNotification();
        }

        [Test]
        public void Test_Can_get_set_IsReadOnly()
        {
            Assert.IsFalse(m_model.IsEnabled);
            m_model.IsEnabled = true;
            Assert.IsTrue(m_model.IsEnabled);
        }

        [Test]
        public void Test_Can_get_set_IsDirty()
        {
            Assert.IsFalse(m_model.IsDirty);
            m_model.IsDirty = true;
            Assert.IsTrue(m_model.IsDirty);
        }

        #endregion
    }
}

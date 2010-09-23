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

namespace Mox.UI
{
    [TestFixture]
    public class MainMenuItemModelTests
    {
        #region Variables

        private int m_value;
        private System.Action m_action;

        private MainMenuItemModel m_parentMenuItem;
        private MainMenuItemModel m_finalMenuItem;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_value = 0;
            m_action = () => { m_value++; };

            m_parentMenuItem = MainMenuItemModel.Create();
            m_finalMenuItem = MainMenuItemModel.Create(m_action);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_set_Text()
        {
            Assert.AreEqual(null, m_parentMenuItem.Text);
            m_parentMenuItem.Text = "New Value";
            Assert.AreEqual("New Value", m_parentMenuItem.Text);
        }

        [Test]
        public void Test_Can_get_ClickAction()
        {
            Assert.IsNull(m_parentMenuItem.ClickAction);
            Assert.AreEqual(m_action, m_finalMenuItem.ClickAction);
        }

        [Test]
        public void Test_Can_get_and_modify_ChildItems()
        {
            Assert.IsNotNull(m_parentMenuItem.Items);

            m_parentMenuItem.Items.Add(m_finalMenuItem);

            Assert.Collections.Contains(m_finalMenuItem, m_parentMenuItem.Items);
        }

        [Test]
        public void Test_SelectedItem_is_null_by_default()
        {
            Assert.IsNull(m_parentMenuItem.SelectedItem);
        }

        [Test]
        public void Test_Can_set_SelectedItem()
        {
            m_parentMenuItem.Items.Add(m_parentMenuItem);
            m_parentMenuItem.SelectedItem = m_parentMenuItem;

            Assert.AreEqual(m_parentMenuItem, m_parentMenuItem.SelectedItem);
        }

        [Test]
        public void Test_Setting_a_final_item_as_SelectedItem_executes_its_action_and_nulls_SelectedItem()
        {
            m_parentMenuItem.Items.Add(m_finalMenuItem);
            m_parentMenuItem.SelectedItem = m_finalMenuItem;

            Assert.AreEqual(1, m_value);

            Assert.IsNull(m_parentMenuItem.SelectedItem);
        }

        #endregion
    }
}

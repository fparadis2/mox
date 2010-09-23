using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mox.Database;
using NUnit.Framework;

namespace Mox.UI.Browser
{
    [TestFixture]
    public class EditDeckViewModelTests
    {
        #region Variables

        private CardDatabase m_database;
        private EditDeckViewModel m_model;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_database = new CardDatabase();
            m_model = new EditDeckViewModel(m_database);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.AreEqual(m_database, m_model.Database);
        }

        [Test]
        public void Test_Can_get_set_DetailsExpanded()
        {
            Assert.IsFalse(m_model.DetailsExpanded);
            m_model.DetailsExpanded = true;
            Assert.IsTrue(m_model.DetailsExpanded);
        }

        [Test]
        public void Test_Can_get_set_IsReadOnly()
        {
            Assert.IsFalse(m_model.IsEnabled);
            m_model.IsEnabled = true;
            Assert.IsTrue(m_model.IsEnabled);
        }

        [Test]
        public void Test_Can_get_DetailsExpanderText()
        {
            m_model.DetailsExpanded = false;
            Assert.AreEqual("Show Details", m_model.DetailsExpanderText);

            m_model.DetailsExpanded = true;
            Assert.AreEqual("Hide Details", m_model.DetailsExpanderText);

            m_model.IsEnabled = true;
            Assert.AreEqual("Hide Details", m_model.DetailsExpanderText);

            m_model.DetailsExpanded = false;
            Assert.AreEqual("Edit Details", m_model.DetailsExpanderText);
        }

        #endregion
    }
}

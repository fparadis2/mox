using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox.UI.Game
{
    [TestFixture]
    public class SpellStackViewModelTests
    {
        private SpellStackViewModel m_model;

        [SetUp]
        public void Setup()
        {
            m_model = new SpellStackViewModel();
        }

        [Test]
        public void Test_Spells_can_be_added()
        {
            Assert.Collections.IsEmpty(m_model.Spells);

            var spell = new SpellViewModel();
            m_model.Spells.Add(spell);
            Assert.Collections.AreEqual(new[] { spell }, m_model.Spells);
        }
    }
}

using System;
using NUnit.Framework;

namespace Mox.UI.Game
{
    [TestFixture]
    public class SpellViewModelTests
    {
        private SpellViewModel m_model;

        [SetUp]
        public void Setup()
        {
            m_model = new SpellViewModel();
        }

        [Test]
        public void Test_Image_can_be_set()
        {
            Assert.IsNull(m_model.Image);

            var image = ImageKey.ForManaSymbol(ManaSymbol.B);
            m_model.Image = image;
            Assert.AreEqual(image, m_model.Image);
        }

        [Test]
        public void Test_Text_can_be_set()
        {
            Assert.IsNull(m_model.AbilityText);
            Assert.IsFalse(m_model.ShowAbilityText);

            m_model.AbilityText = "Test";
            Assert.AreEqual("Test", m_model.AbilityText);
            Assert.IsTrue(m_model.ShowAbilityText);
        }
    }
}

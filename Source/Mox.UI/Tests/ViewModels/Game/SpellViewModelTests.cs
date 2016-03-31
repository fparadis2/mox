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
        public void Test_CardIdentifier_can_be_set()
        {
            Assert.That(m_model.CardIdentifier.IsInvalid);

            var identifier = new CardIdentifier { Card = "My Card" };
            m_model.CardIdentifier = identifier;
            Assert.AreEqual(identifier, m_model.CardIdentifier);
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

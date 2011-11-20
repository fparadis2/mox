using System;

using NUnit.Framework;

namespace Mox.Flow
{
    [TestFixture]
    public class TargetResultTests : BaseGameTests
    {
        #region Tests

        [Test]
        public void Test_Invalid_returns_an_invalid_result()
        {
            var invalid = TargetResult.Invalid;
            Assert.That(!invalid.IsValid);
            Assert.Throws<InvalidOperationException>(() => invalid.Resolve<Card>(m_game));
        }

        [Test]
        public void Test_Can_resolve_a_result()
        {
            var result = new TargetResult(m_card.Identifier);
            Assert.That(result.IsValid);
            Assert.AreEqual(m_card, result.Resolve<Card>(m_game));
        }

        [Test]
        public void Test_Is_serializable()
        {
            var result = new TargetResult(m_card.Identifier);
            result = Assert.IsSerializable(result);
            Assert.AreEqual(m_card, result.Resolve<Card>(m_game));
        }

        #endregion
    }
}

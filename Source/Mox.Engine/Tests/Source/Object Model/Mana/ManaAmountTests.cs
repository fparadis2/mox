using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    [TestFixture]
    public class ManaAmountTests
    {
        #region Tests

        [Test]
        public void Test_TryParse_Single()
        {
            ManaAmount amount;

            Assert.That(ManaAmount.TryParse("{W}", out amount));
            Assert.AreEqual(new ManaAmount { White = 1 }, amount);

            Assert.That(ManaAmount.TryParse("{U}", out amount));
            Assert.AreEqual(new ManaAmount { Blue = 1 }, amount);

            Assert.That(ManaAmount.TryParse("{B}", out amount));
            Assert.AreEqual(new ManaAmount { Black = 1 }, amount);

            Assert.That(ManaAmount.TryParse("{R}", out amount));
            Assert.AreEqual(new ManaAmount { Red = 1 }, amount);

            Assert.That(ManaAmount.TryParse("{G}", out amount));
            Assert.AreEqual(new ManaAmount { Green = 1 }, amount);

            Assert.That(ManaAmount.TryParse("{C}", out amount));
            Assert.AreEqual(new ManaAmount { Colorless = 1 }, amount);
        }

        [Test]
        public void Test_TryParse_Multiple()
        {
            ManaAmount amount;

            Assert.That(ManaAmount.TryParse("{W}{U}", out amount));
            Assert.AreEqual(new ManaAmount { White = 1, Blue = 1 }, amount);

            Assert.That(ManaAmount.TryParse("{U}{U}", out amount));
            Assert.AreEqual(new ManaAmount { Blue = 2 }, amount);
        }

        [Test]
        public void Test_TryParse_Invalid()
        {
            ManaAmount amount;

            Assert.IsFalse(ManaAmount.TryParse("R", out amount));
            Assert.IsFalse(ManaAmount.TryParse("{T}", out amount));
            Assert.IsFalse(ManaAmount.TryParse("{G/W}", out amount));
        }

        #endregion
    }
}

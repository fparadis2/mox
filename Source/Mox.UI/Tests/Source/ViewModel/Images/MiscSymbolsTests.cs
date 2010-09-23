using System;
using NUnit.Framework;

namespace Mox.UI
{
    [TestFixture]
    public class MiscSymbolsTests
    {
        #region Utilities

        private static void Test_Parse(string str, MiscSymbols expectedSymbol)
        {
            MiscSymbols symbol;
            bool result = MiscSymbols.TryParse(str, out symbol);

            Assert.AreEqual(expectedSymbol != null, result);
            Assert.AreEqual(expectedSymbol, symbol);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Can_get_Tap()
        {
            var tap = MiscSymbols.Tap;
            Assert.IsNotNull(tap);
            Assert.AreEqual("{T}", tap.ToString());
        }

        [Test]
        public void Test_Can_get_Untap()
        {
            var untap = MiscSymbols.Untap;
            Assert.IsNotNull(untap);
            Assert.AreEqual("{U}", untap.ToString());
        }

        [Test]
        public void Test_Can_parse_symbols()
        {
            Test_Parse("Any", null);
            Test_Parse("T", null);
            Test_Parse("{T}", MiscSymbols.Tap);
            Test_Parse("{U}", MiscSymbols.Untap);
        }

        #endregion
    }
}

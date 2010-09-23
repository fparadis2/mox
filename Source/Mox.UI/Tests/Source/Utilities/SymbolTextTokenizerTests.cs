using System;
using NUnit.Framework;

namespace Mox.UI
{
    [TestFixture]
    public class SymbolTextTokenizerTests
    {
        #region Tests

        [Test]
        public void Test_Tokenize_empty_string()
        {
            Assert.Collections.IsEmpty(SymbolTextTokenizer.Tokenize(null, ManaSymbolNotation.Compact));
            Assert.Collections.IsEmpty(SymbolTextTokenizer.Tokenize(string.Empty, ManaSymbolNotation.Compact));
        }

        [Test]
        public void Test_Tokenize_simple_mana_symbol()
        {
            Assert.Collections.AreEqual(new object[] { ManaSymbol.R }, SymbolTextTokenizer.Tokenize("R", ManaSymbolNotation.Compact));
            Assert.Collections.AreEqual(new object[] { ManaSymbol.U }, SymbolTextTokenizer.Tokenize("{U}", ManaSymbolNotation.Compact));
            Assert.Collections.AreEqual(new object[] { "W" }, SymbolTextTokenizer.Tokenize("W", ManaSymbolNotation.Long));
            Assert.Collections.AreEqual(new object[] { ManaSymbol.U }, SymbolTextTokenizer.Tokenize("{U}", ManaSymbolNotation.Long));
        }

        [Test]
        public void Test_Tokenize_mana_cost_with_text()
        {
            Assert.Collections.AreEqual(new object[] { "This will cost you ", 1, ManaSymbol.R, " and then ", 2, ManaSymbol.G, ManaSymbol.U, ", you understand me?" }, SymbolTextTokenizer.Tokenize("This will cost you 1R and then 2GU, you understand me?", ManaSymbolNotation.Compact));
            Assert.Collections.AreEqual(new object[] { 1, ManaSymbol.R, " ", 2, ManaSymbol.G, ManaSymbol.U }, SymbolTextTokenizer.Tokenize("1R 2GU", ManaSymbolNotation.Compact));
        }

        [Test]
        public void Test_Tokenize_handles_whitespaces()
        {
            Assert.Collections.AreEqual(new object[] { "This  will\t ", 1, "\nand ", 2, ManaSymbol.G }, SymbolTextTokenizer.Tokenize("This  will\t 1\nand 2G", ManaSymbolNotation.Compact));
        }

        [Test]
        public void Test_Tokenize_will_return_modal_costs_before_the_rst()
        {
            Assert.Collections.AreEqual(new object[] { "Cost: ", ManaSymbol.X, 1, ManaSymbol.B }, SymbolTextTokenizer.Tokenize("Cost: X1B", ManaSymbolNotation.Compact));
        }

        [Test]
        public void Test_Tokenize_handles_colons()
        {
            Assert.Collections.AreEqual(new object[] { 2, ManaSymbol.R, ": Do something" }, SymbolTextTokenizer.Tokenize("2R: Do something", ManaSymbolNotation.Compact));
        }

        [Test]
        public void Test_Tokenize_can_handles_a_string_ending_with_separator()
        {
            Assert.Collections.AreEqual(new object[] { "Do something." }, SymbolTextTokenizer.Tokenize("Do something.", ManaSymbolNotation.Compact));
        }

        [Test]
        public void Test_Tokenize_can_handle_misc_symbols()
        {
            Assert.Collections.AreEqual(new object[] { MiscSymbols.Tap, ": Do something" }, SymbolTextTokenizer.Tokenize("{T}: Do something", ManaSymbolNotation.Compact));
        }

        #endregion
    }
}

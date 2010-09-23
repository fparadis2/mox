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

namespace Mox
{
    [TestFixture]
    public class ManaSymbolTests
    {
        #region Utilities

        private static void Test_Parse(ManaSymbol expectedSymbol, string stringToParse)
        {
            Test_Parse(expectedSymbol, stringToParse, ManaSymbolNotation.Compact);
            Test_Parse(expectedSymbol, stringToParse, ManaSymbolNotation.Long);
        }

        private static void Test_Parse(ManaSymbol expectedSymbol, string stringToParse, ManaSymbolNotation notation)
        {
            ManaSymbol actual;
            Assert.IsTrue(ManaSymbolHelper.TryParse(stringToParse, notation, out actual), "Could not parse the symbol {0}", stringToParse);
            Assert.AreEqual(expectedSymbol, actual);
            Assert.AreEqual(expectedSymbol, ManaSymbolHelper.Parse(stringToParse, notation));
        }

        private static void Test_DoesntParse(string stringToParse, ManaSymbolNotation notation)
        {
            ManaSymbol actual;
            Assert.IsFalse(ManaSymbolHelper.TryParse(stringToParse, notation, out actual), "{0} was parsed into {1}", stringToParse, actual);
            Assert.Throws<ArgumentException>(() => ManaSymbolHelper.Parse(stringToParse, notation));
        }

        private static void Test_ToString(string expectedString, ManaSymbol symbol, bool longForm)
        {
            Assert.AreEqual(expectedString, ManaSymbolHelper.ToString(symbol, longForm ? ManaSymbolNotation.Long : ManaSymbolNotation.Compact));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_The_converted_value_of_a_mana_symbol_is_the_numerical_value_of_the_largest_component()
        {
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.W));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.U));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.B));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.R));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.G));

            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.BG));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.BR));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.GU));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.GW));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.RG));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.RW));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.UB));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.UR));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.WB));
            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.WU));

            Assert.AreEqual(2, ManaSymbolHelper.GetConvertedValue(ManaSymbol.W2));
            Assert.AreEqual(2, ManaSymbolHelper.GetConvertedValue(ManaSymbol.U2));
            Assert.AreEqual(2, ManaSymbolHelper.GetConvertedValue(ManaSymbol.B2));
            Assert.AreEqual(2, ManaSymbolHelper.GetConvertedValue(ManaSymbol.R2));
            Assert.AreEqual(2, ManaSymbolHelper.GetConvertedValue(ManaSymbol.G2));

            Assert.AreEqual(0, ManaSymbolHelper.GetConvertedValue(ManaSymbol.X));
            Assert.AreEqual(0, ManaSymbolHelper.GetConvertedValue(ManaSymbol.Y));
            Assert.AreEqual(0, ManaSymbolHelper.GetConvertedValue(ManaSymbol.Z));

            Assert.AreEqual(1, ManaSymbolHelper.GetConvertedValue(ManaSymbol.S));
        }

        [Test]
        public void Test_Cannot_get_the_converted_value_of_an_invalid_mana_symbol()
        {
            Assert.Throws<ArgumentException>(() => ManaSymbolHelper.GetConvertedValue((ManaSymbol) (-1)));
        }

        [Test]
        public void Test_All_Mana_symbols_are_covered_by_GetConvertedValue()
        {
            foreach (ManaSymbol symbol in Enum.GetValues(typeof(ManaSymbol)))
            {
                ManaSymbol symbol1 = symbol;
                Assert.DoesntThrow(() => ManaSymbolHelper.GetConvertedValue(symbol1));
            }
        }

        [Test]
        public void Test_TryParse_LongForm()
        {
            Test_Parse(ManaSymbol.W, "{W}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.U, "{U}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.B, "{B}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.R, "{R}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.G, "{G}", ManaSymbolNotation.Long);

            Test_Parse(ManaSymbol.BG, "{B/G}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.BR, "{B/R}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.GU, "{G/U}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.GW, "{G/W}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.RG, "{R/G}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.RW, "{R/W}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.UB, "{U/B}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.UR, "{U/R}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.WB, "{W/B}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.WU, "{W/U}", ManaSymbolNotation.Long);

            Test_Parse(ManaSymbol.W2, "{2/W}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.U2, "{2/U}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.B2, "{2/B}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.R2, "{2/R}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.G2, "{2/G}", ManaSymbolNotation.Long);

            Test_Parse(ManaSymbol.X, "{X}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.Y, "{Y}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.Z, "{Z}", ManaSymbolNotation.Long);

            Test_Parse(ManaSymbol.S, "{S}", ManaSymbolNotation.Long);

            Test_DoesntParse("W", ManaSymbolNotation.Long);
            Test_DoesntParse("WW", ManaSymbolNotation.Long);
            Test_DoesntParse("{WW}", ManaSymbolNotation.Long);
            Test_DoesntParse("{T}", ManaSymbolNotation.Long);
            Test_DoesntParse("{}", ManaSymbolNotation.Long);
            Test_DoesntParse("{", ManaSymbolNotation.Long);
            Test_DoesntParse("}", ManaSymbolNotation.Long);
            Test_DoesntParse("", ManaSymbolNotation.Long);
            Test_DoesntParse(null, ManaSymbolNotation.Long);
        }

        [Test]
        public void Test_TryParse_CompactForm()
        {
            Test_Parse(ManaSymbol.W, "W", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.U, "U", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.B, "B", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.R, "R", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.G, "G", ManaSymbolNotation.Compact);

            Test_Parse(ManaSymbol.BG, "{B/G}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.BR, "{B/R}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.GU, "{G/U}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.GW, "{G/W}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.RG, "{R/G}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.RW, "{R/W}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.UB, "{U/B}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.UR, "{U/R}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.WB, "{W/B}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.WU, "{W/U}", ManaSymbolNotation.Compact);

            Test_Parse(ManaSymbol.W2, "{2/W}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.U2, "{2/U}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.B2, "{2/B}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.R2, "{2/R}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.G2, "{2/G}", ManaSymbolNotation.Compact);

            Test_Parse(ManaSymbol.X, "X", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.Y, "Y", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.Z, "Z", ManaSymbolNotation.Compact);

            Test_Parse(ManaSymbol.S, "S", ManaSymbolNotation.Compact);
        }

        [Test]
        public void Test_Parsing_is_case_insensitive_LongForm()
        {
            Test_Parse(ManaSymbol.W, "{w}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.U, "{u}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.B, "{b}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.R, "{r}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.G, "{g}", ManaSymbolNotation.Long);

            Test_Parse(ManaSymbol.BG, "{b/g}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.BR, "{b/r}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.GU, "{g/u}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.GW, "{g/w}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.RG, "{r/g}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.RW, "{r/w}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.UB, "{u/b}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.UR, "{u/r}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.WB, "{w/b}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.WU, "{w/u}", ManaSymbolNotation.Long);

            Test_Parse(ManaSymbol.W2, "{2/w}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.U2, "{2/u}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.B2, "{2/b}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.R2, "{2/r}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.G2, "{2/g}", ManaSymbolNotation.Long);

            Test_Parse(ManaSymbol.X, "{x}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.Y, "{y}", ManaSymbolNotation.Long);
            Test_Parse(ManaSymbol.Z, "{z}", ManaSymbolNotation.Long);

            Test_Parse(ManaSymbol.S, "{s}", ManaSymbolNotation.Long);
        }

        [Test]
        public void Test_Parsing_is_case_insensitive_CompactForm()
        {
            Test_Parse(ManaSymbol.W, "w", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.U, "u", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.B, "b", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.R, "r", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.G, "g", ManaSymbolNotation.Compact);

            Test_Parse(ManaSymbol.BG, "{b/g}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.BR, "{b/r}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.GU, "{g/u}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.GW, "{g/w}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.RG, "{r/g}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.RW, "{r/w}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.UB, "{u/b}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.UR, "{u/r}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.WB, "{w/b}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.WU, "{w/u}", ManaSymbolNotation.Compact);

            Test_Parse(ManaSymbol.W2, "{2/w}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.U2, "{2/u}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.B2, "{2/b}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.R2, "{2/r}", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.G2, "{2/g}", ManaSymbolNotation.Compact);

            Test_Parse(ManaSymbol.X, "x", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.Y, "y", ManaSymbolNotation.Compact);
            Test_Parse(ManaSymbol.Z, "z", ManaSymbolNotation.Compact);

            Test_Parse(ManaSymbol.S, "s", ManaSymbolNotation.Compact);
        }

        [Test]
        public void Test_Cannot_parse_hybrid_mana_without_the_brackets()
        {
            Test_DoesntParse("W/U", ManaSymbolNotation.Compact);
            Test_DoesntParse("2/W", ManaSymbolNotation.Long);
        }

        [Test]
        public void Test_ToString_CompactForm()
        {
            Test_ToString("W", ManaSymbol.W, false);
            Test_ToString("U", ManaSymbol.U, false);
            Test_ToString("B", ManaSymbol.B, false);
            Test_ToString("R", ManaSymbol.R, false);
            Test_ToString("G", ManaSymbol.G, false);

            Test_ToString("{B/G}", ManaSymbol.BG, false);
            Test_ToString("{B/R}", ManaSymbol.BR, false);
            Test_ToString("{G/U}", ManaSymbol.GU, false);
            Test_ToString("{G/W}", ManaSymbol.GW, false);
            Test_ToString("{R/G}", ManaSymbol.RG, false);
            Test_ToString("{R/W}", ManaSymbol.RW, false);
            Test_ToString("{U/B}", ManaSymbol.UB, false);
            Test_ToString("{U/R}", ManaSymbol.UR, false);
            Test_ToString("{W/B}", ManaSymbol.WB, false);
            Test_ToString("{W/U}", ManaSymbol.WU, false);

            Test_ToString("{2/W}", ManaSymbol.W2, false);
            Test_ToString("{2/U}", ManaSymbol.U2, false);
            Test_ToString("{2/B}", ManaSymbol.B2, false);
            Test_ToString("{2/R}", ManaSymbol.R2, false);
            Test_ToString("{2/G}", ManaSymbol.G2, false);

            Test_ToString("X", ManaSymbol.X, false);
            Test_ToString("Y", ManaSymbol.Y, false);
            Test_ToString("Z", ManaSymbol.Z, false);

            Test_ToString("S", ManaSymbol.S, false);

            Assert.Throws<ArgumentException>(
                () => ManaSymbolHelper.ToString((ManaSymbol) (-1), ManaSymbolNotation.Compact));
        }

        [Test]
        public void Test_ToString_LongForm()
        {
            Test_ToString("{W}", ManaSymbol.W, true);
            Test_ToString("{U}", ManaSymbol.U, true);
            Test_ToString("{B}", ManaSymbol.B, true);
            Test_ToString("{R}", ManaSymbol.R, true);
            Test_ToString("{G}", ManaSymbol.G, true);

            Test_ToString("{B/G}", ManaSymbol.BG, true);
            Test_ToString("{B/R}", ManaSymbol.BR, true);
            Test_ToString("{G/U}", ManaSymbol.GU, true);
            Test_ToString("{G/W}", ManaSymbol.GW, true);
            Test_ToString("{R/G}", ManaSymbol.RG, true);
            Test_ToString("{R/W}", ManaSymbol.RW, true);
            Test_ToString("{U/B}", ManaSymbol.UB, true);
            Test_ToString("{U/R}", ManaSymbol.UR, true);
            Test_ToString("{W/B}", ManaSymbol.WB, true);
            Test_ToString("{W/U}", ManaSymbol.WU, true);

            Test_ToString("{2/W}", ManaSymbol.W2, true);
            Test_ToString("{2/U}", ManaSymbol.U2, true);
            Test_ToString("{2/B}", ManaSymbol.B2, true);
            Test_ToString("{2/R}", ManaSymbol.R2, true);
            Test_ToString("{2/G}", ManaSymbol.G2, true);

            Test_ToString("{X}", ManaSymbol.X, true);
            Test_ToString("{Y}", ManaSymbol.Y, true);
            Test_ToString("{Z}", ManaSymbol.Z, true);

            Test_ToString("{S}", ManaSymbol.S, true);

            Assert.Throws<ArgumentException>(() => ManaSymbolHelper.ToString((ManaSymbol) (-1), ManaSymbolNotation.Long));
        }

        [Test]
        public void Test_All_Mana_symbols_can_be_converted_to_and_from_string_in_compact_form()
        {
            foreach (ManaSymbol symbol in Enum.GetValues(typeof(ManaSymbol)))
            {
                string symbolString = ManaSymbolHelper.ToString(symbol, ManaSymbolNotation.Compact);
                Assert.AreEqual(symbol, ManaSymbolHelper.Parse(symbolString, ManaSymbolNotation.Compact));
            }
        }

        [Test]
        public void Test_All_Mana_symbols_can_be_converted_to_and_from_string_in_long_form()
        {
            foreach (ManaSymbol symbol in Enum.GetValues(typeof(ManaSymbol)))
            {
                string symbolString = ManaSymbolHelper.ToString(symbol, ManaSymbolNotation.Long);
                Assert.AreEqual(symbol, ManaSymbolHelper.Parse(symbolString, ManaSymbolNotation.Long));
            }
        }

        [Test]
        public void Test_IsHybrid()
        {
            Assert.IsFalse(ManaSymbolHelper.IsHybrid(ManaSymbol.W));
            Assert.IsFalse(ManaSymbolHelper.IsHybrid(ManaSymbol.U));
            Assert.IsFalse(ManaSymbolHelper.IsHybrid(ManaSymbol.B));
            Assert.IsFalse(ManaSymbolHelper.IsHybrid(ManaSymbol.R));
            Assert.IsFalse(ManaSymbolHelper.IsHybrid(ManaSymbol.G));

            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.BG));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.BR));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.GU));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.GW));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.RG));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.RW));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.UB));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.UR));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.WB));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.WU));

            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.W2));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.U2));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.B2));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.R2));
            Assert.IsTrue(ManaSymbolHelper.IsHybrid(ManaSymbol.G2));

            Assert.IsFalse(ManaSymbolHelper.IsHybrid(ManaSymbol.X));
            Assert.IsFalse(ManaSymbolHelper.IsHybrid(ManaSymbol.Y));
            Assert.IsFalse(ManaSymbolHelper.IsHybrid(ManaSymbol.Z));

            Assert.IsFalse(ManaSymbolHelper.IsHybrid(ManaSymbol.S));

            Assert.Throws<ArgumentException>(() => ManaSymbolHelper.IsHybrid((ManaSymbol)999));
        }

        [Test]
        public void Test_GetColor()
        {
            Assert.AreEqual(Color.Black, ManaSymbolHelper.GetColor(ManaSymbol.B));
            Assert.AreEqual(Color.Blue, ManaSymbolHelper.GetColor(ManaSymbol.U));
            Assert.AreEqual(Color.Green, ManaSymbolHelper.GetColor(ManaSymbol.G));
            Assert.AreEqual(Color.Red, ManaSymbolHelper.GetColor(ManaSymbol.R));
            Assert.AreEqual(Color.White, ManaSymbolHelper.GetColor(ManaSymbol.W));

            Assert.AreEqual(Color.Black | Color.Green, ManaSymbolHelper.GetColor(ManaSymbol.BG));
            Assert.AreEqual(Color.Black | Color.Red, ManaSymbolHelper.GetColor(ManaSymbol.BR));
            Assert.AreEqual(Color.Green | Color.Blue, ManaSymbolHelper.GetColor(ManaSymbol.GU));
            Assert.AreEqual(Color.Green | Color.White, ManaSymbolHelper.GetColor(ManaSymbol.GW));
            Assert.AreEqual(Color.Red | Color.Green, ManaSymbolHelper.GetColor(ManaSymbol.RG));
            Assert.AreEqual(Color.Red | Color.White, ManaSymbolHelper.GetColor(ManaSymbol.RW));
            Assert.AreEqual(Color.Blue | Color.Black, ManaSymbolHelper.GetColor(ManaSymbol.UB));
            Assert.AreEqual(Color.Blue | Color.Red, ManaSymbolHelper.GetColor(ManaSymbol.UR));
            Assert.AreEqual(Color.White | Color.Black, ManaSymbolHelper.GetColor(ManaSymbol.WB));
            Assert.AreEqual(Color.White | Color.Blue, ManaSymbolHelper.GetColor(ManaSymbol.WU));

            Assert.AreEqual(Color.Black, ManaSymbolHelper.GetColor(ManaSymbol.B2));
            Assert.AreEqual(Color.Green, ManaSymbolHelper.GetColor(ManaSymbol.G2));
            Assert.AreEqual(Color.Red, ManaSymbolHelper.GetColor(ManaSymbol.R2));
            Assert.AreEqual(Color.Blue, ManaSymbolHelper.GetColor(ManaSymbol.U2));
            Assert.AreEqual(Color.White, ManaSymbolHelper.GetColor(ManaSymbol.W2));

            Assert.AreEqual(Color.None, ManaSymbolHelper.GetColor(ManaSymbol.X));
            Assert.AreEqual(Color.None, ManaSymbolHelper.GetColor(ManaSymbol.Y));
            Assert.AreEqual(Color.None, ManaSymbolHelper.GetColor(ManaSymbol.Z));
            Assert.AreEqual(Color.None, ManaSymbolHelper.GetColor(ManaSymbol.S));
        }

        [Test]
        public void Test_GetSymbol()
        {
            Assert.AreEqual(ManaSymbol.B, ManaSymbolHelper.GetSymbol(Color.Black));
            Assert.AreEqual(ManaSymbol.U, ManaSymbolHelper.GetSymbol(Color.Blue));
            Assert.AreEqual(ManaSymbol.G, ManaSymbolHelper.GetSymbol(Color.Green));
            Assert.AreEqual(ManaSymbol.R, ManaSymbolHelper.GetSymbol(Color.Red));
            Assert.AreEqual(ManaSymbol.W, ManaSymbolHelper.GetSymbol(Color.White));
            Assert.AreEqual(ManaSymbol.X, ManaSymbolHelper.GetSymbol(Color.None)); // TODO

            Assert.Throws<ArgumentException>(() => ManaSymbolHelper.GetSymbol(Color.Red | Color.White));
        }

        #endregion
    }
}
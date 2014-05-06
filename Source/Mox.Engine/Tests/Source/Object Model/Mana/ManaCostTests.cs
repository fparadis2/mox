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
    public class ManaCostTests : BaseGameTests
    {
        #region Utilities

        private static void Test_Parse(string text, ManaCost expectedCost)
        {
            ManaCost parsedCost;
            if (expectedCost != null)
            {
                Assert.AreEqual(expectedCost, ManaCost.Parse(text));
                Assert.IsTrue(ManaCost.TryParse(text, out parsedCost));
                Assert.AreEqual(expectedCost, parsedCost);
            }
            else
            {
                Assert.Throws<ArgumentException>(() => ManaCost.Parse(text));
                Assert.IsFalse(ManaCost.TryParse(text, out parsedCost));
                Assert.IsNull(parsedCost);
            }
        }

        #endregion

        #region Tests

        #region General

        [Test]
        public void Test_Constructor_values()
        {
            ManaCost cost = new ManaCost(5, ManaSymbol.B, ManaSymbol.G2);

            Assert.AreEqual(5, cost.Colorless);
            Assert.Collections.AreEquivalent(new[] { ManaSymbol.B, ManaSymbol.G2 }, cost.Symbols);
        }

        [Test]
        public void Test_Symbols_are_readonly()
        {
            ManaCost cost = new ManaCost(5);
            Assert.IsTrue(cost.Symbols.IsReadOnly);
        }

        [Test]
        public void Test_Cannot_construct_with_an_invalid_colorless_value()
        {
            Assert.DoesntThrow(delegate { new ManaCost(100000); });
            Assert.DoesntThrow(delegate { new ManaCost(1); });
            Assert.DoesntThrow(delegate { new ManaCost(0); });

            Assert.Throws<ArgumentOutOfRangeException>(delegate { new ManaCost(-1); });
            Assert.Throws<ArgumentOutOfRangeException>(delegate { new ManaCost(-10); });
        }

        [Test]
        public void Test_Converted_value_of_a_cost_is_the_sum_of_the_converted_value_of_each_symbol_plus_the_colorless_value()
        {
            Assert.AreEqual(0, new ManaCost(0).ConvertedValue);
            Assert.AreEqual(1, new ManaCost(1).ConvertedValue);
            Assert.AreEqual(2, new ManaCost(2).ConvertedValue);

            Assert.AreEqual(2, new ManaCost(0, ManaSymbol.B, ManaSymbol.BR).ConvertedValue);
            Assert.AreEqual(6, new ManaCost(3, ManaSymbol.B, ManaSymbol.G2).ConvertedValue);
        }

        [Test]
        public void Test_ToString_uses_the_compact_form_by_default()
        {
            Assert.AreEqual("0", new ManaCost(0).ToString());
            Assert.AreEqual("5", new ManaCost(5).ToString());

            Assert.AreEqual("1B", new ManaCost(1, ManaSymbol.B).ToString());
            Assert.AreEqual("X1{W/U}", new ManaCost(1, ManaSymbol.X, ManaSymbol.WU).ToString());

            Assert.AreEqual("B", new ManaCost(0, ManaSymbol.B).ToString());
            Assert.AreEqual("X{W/U}", new ManaCost(0, ManaSymbol.X, ManaSymbol.WU).ToString());
        }

        [Test]
        public void Test_ToString_with_compact_form()
        {
            Assert.AreEqual("0", new ManaCost(0).ToString(ManaSymbolNotation.Compact));
            Assert.AreEqual("5", new ManaCost(5).ToString(ManaSymbolNotation.Compact));

            Assert.AreEqual("1B", new ManaCost(1, ManaSymbol.B).ToString(ManaSymbolNotation.Compact));
            Assert.AreEqual("X1{W/U}", new ManaCost(1, ManaSymbol.X, ManaSymbol.WU).ToString(ManaSymbolNotation.Compact));

            Assert.AreEqual("B", new ManaCost(0, ManaSymbol.B).ToString(ManaSymbolNotation.Compact));
            Assert.AreEqual("X{W/U}", new ManaCost(0, ManaSymbol.X, ManaSymbol.WU).ToString(ManaSymbolNotation.Compact));
        }

        [Test]
        public void Test_ToString_with_long_form()
        {
            Assert.AreEqual("{0}", new ManaCost(0).ToString(ManaSymbolNotation.Long));
            Assert.AreEqual("{5}", new ManaCost(5).ToString(ManaSymbolNotation.Long));

            Assert.AreEqual("{1}{B}", new ManaCost(1, ManaSymbol.B).ToString(ManaSymbolNotation.Long));
            Assert.AreEqual("{X}{1}{W/U}", new ManaCost(1, ManaSymbol.X, ManaSymbol.WU).ToString(ManaSymbolNotation.Long));

            Assert.AreEqual("{B}", new ManaCost(0, ManaSymbol.B).ToString(ManaSymbolNotation.Long));
            Assert.AreEqual("{X}{W/U}", new ManaCost(0, ManaSymbol.X, ManaSymbol.WU).ToString(ManaSymbolNotation.Long));
        }

        [Test]
        public void Test_Equals()
        {
            Assert.AreEqual(new ManaCost(0), new ManaCost(0));
            Assert.AreEqual(new ManaCost(1), new ManaCost(1));
            Assert.AreEqual(new ManaCost(3, ManaSymbol.R, ManaSymbol.S), new ManaCost(3, ManaSymbol.R, ManaSymbol.S));
            Assert.AreEqual(new ManaCost(0, ManaSymbol.S, ManaSymbol.R), new ManaCost(0, ManaSymbol.R, ManaSymbol.S));

            Assert.IsFalse(new ManaCost(0).Equals(null));
            Assert.AreNotEqual(new ManaCost(0), new ManaCost(1));
            Assert.AreNotEqual(new ManaCost(3, ManaSymbol.R, ManaSymbol.S), new ManaCost(2, ManaSymbol.R, ManaSymbol.S));
            Assert.AreNotEqual(new ManaCost(0, ManaSymbol.S, ManaSymbol.R, ManaSymbol.S), new ManaCost(0, ManaSymbol.R, ManaSymbol.S));
            Assert.AreNotEqual(new ManaCost(2, ManaSymbol.R, ManaSymbol.S), new ManaCost(2, ManaSymbol.R, ManaSymbol.W));
        }

        [Test]
        public void Test_Equality_operator()
        {
            Assert.IsTrue(new ManaCost(0) == new ManaCost(0));
            Assert.IsTrue(new ManaCost(1) == new ManaCost(1));
            Assert.IsTrue(new ManaCost(3, ManaSymbol.R, ManaSymbol.S) == new ManaCost(3, ManaSymbol.R, ManaSymbol.S));
            Assert.IsTrue(new ManaCost(0, ManaSymbol.S, ManaSymbol.R) == new ManaCost(0, ManaSymbol.R, ManaSymbol.S));

            Assert.IsFalse(new ManaCost(0) == new ManaCost(1));
            Assert.IsFalse(new ManaCost(3, ManaSymbol.R, ManaSymbol.S) == new ManaCost(2, ManaSymbol.R, ManaSymbol.S));
            Assert.IsFalse(new ManaCost(0, ManaSymbol.S, ManaSymbol.R, ManaSymbol.S) == new ManaCost(0, ManaSymbol.R, ManaSymbol.S));
        }

        [Test]
        public void Test_Inequality_operator()
        {
            Assert.IsFalse(new ManaCost(0) != new ManaCost(0));
            Assert.IsFalse(new ManaCost(1) != new ManaCost(1));
            Assert.IsFalse(new ManaCost(3, ManaSymbol.R, ManaSymbol.S) != new ManaCost(3, ManaSymbol.R, ManaSymbol.S));
            Assert.IsFalse(new ManaCost(0, ManaSymbol.S, ManaSymbol.R) != new ManaCost(0, ManaSymbol.R, ManaSymbol.S));

            Assert.IsTrue(new ManaCost(0) != new ManaCost(1));
            Assert.IsTrue(new ManaCost(3, ManaSymbol.R, ManaSymbol.S) != new ManaCost(2, ManaSymbol.R, ManaSymbol.S));
            Assert.IsTrue(new ManaCost(0, ManaSymbol.S, ManaSymbol.R, ManaSymbol.S) != new ManaCost(0, ManaSymbol.R, ManaSymbol.S));
        }

        [Test]
        public void Test_GetHashCode()
        {
            Assert.AreEqual(0.GetHashCode(), new ManaCost(0).GetHashCode());
            Assert.AreEqual(3.GetHashCode(), new ManaCost(3).GetHashCode());
        }

        [Test]
        public void Test_IsConcrete()
        {
            Assert.IsTrue(new ManaCost(0).IsConcrete);
            Assert.IsTrue(new ManaCost(1, ManaSymbol.R).IsConcrete);
            Assert.IsFalse(new ManaCost(1, ManaSymbol.GW).IsConcrete);
            Assert.IsFalse(new ManaCost(0, ManaSymbol.G2).IsConcrete);
            Assert.IsFalse(new ManaCost(0, ManaSymbol.X).IsConcrete);
        }

        [Test]
        public void Test_IsEmpty()
        {
            Assert.IsTrue(new ManaCost(0).IsEmpty);
            Assert.IsFalse(new ManaCost(1, ManaSymbol.R).IsEmpty);
            Assert.IsFalse(new ManaCost(1, ManaSymbol.GW).IsEmpty);
            Assert.IsFalse(new ManaCost(0, ManaSymbol.G2).IsEmpty);
            Assert.IsFalse(new ManaCost(0, ManaSymbol.X).IsEmpty);
        }

        [Test]
        public void Test_Empty_returns_an_empty_mana_cost()
        {
            Assert.AreEqual(new ManaCost(0), ManaCost.Empty);
            Assert.IsTrue(ManaCost.Empty.IsEmpty);
        }

        [Test]
        public void Test_Remove_returns_a_new_cost_with_one_less_symbol()
        {
            Assert.AreEqual(new ManaCost(0), new ManaCost(0).Remove(ManaSymbol.R));
            Assert.AreEqual(new ManaCost(0, ManaSymbol.R2), new ManaCost(0, ManaSymbol.X, ManaSymbol.R2).Remove(ManaSymbol.X));
        }

        [Test]
        public void Test_RemoveColorless_returns_a_new_cost_with_that_much_less_colorless_mana()
        {
            Assert.AreEqual(new ManaCost(0), new ManaCost(0).RemoveColorless(5));
            Assert.AreEqual(new ManaCost(0, ManaSymbol.X, ManaSymbol.R2), new ManaCost(5, ManaSymbol.X, ManaSymbol.R2).RemoveColorless(5));
            Assert.AreEqual(new ManaCost(3, ManaSymbol.R), new ManaCost(5, ManaSymbol.R).RemoveColorless(2));
        }

        [Test]
        public void Test_Cannot_remove_a_negative_amount_of_colorless_mana()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ManaCost(0).RemoveColorless(-1));
        }

        [Test]
        public void Test_Is_serializable()
        {
            ManaCost cost = new ManaCost(3, ManaSymbol.R, ManaSymbol.W);
            Assert.AreEqual(cost, Assert.IsSerializable(cost));
        }

        #endregion

        #region Parsing

        [Test]
        public void Test_Parse_invalid_string()
        {
            Test_Parse("I", null);
            Test_Parse("RG[3)", null);
            Test_Parse("5RG3)", null);
        }

        [Test]
        public void Test_Parse_empty_cost()
        {
            Test_Parse(null, ManaCost.Empty);
            Test_Parse(string.Empty, ManaCost.Empty);
        }

        [Test]
        public void Test_Parse_simple_strings()
        {
            Test_Parse("3", new ManaCost(3));
            Test_Parse("R", new ManaCost(0, ManaSymbol.R));
            Test_Parse("3RW", new ManaCost(3, ManaSymbol.R, ManaSymbol.W));
            Test_Parse("R3W", new ManaCost(3, ManaSymbol.R, ManaSymbol.W));
            Test_Parse("RW32", new ManaCost(32, ManaSymbol.R, ManaSymbol.W));
        }

        [Test]
        public void Test_Parse_in_long_format()
        {
            Test_Parse("{3}", new ManaCost(3));
            Test_Parse("{R}", new ManaCost(0, ManaSymbol.R));
            Test_Parse("{3}{R}{W}", new ManaCost(3, ManaSymbol.R, ManaSymbol.W));
            Test_Parse("{3}{R/G}{W}", new ManaCost(3, ManaSymbol.RG, ManaSymbol.W));

            ManaCost cost;
            Assert.IsFalse(ManaCost.TryParse("3", ManaSymbolNotation.Long, out cost));
        }

        [Test]
        public void Test_Parse_complex_strings()
        {
            Test_Parse("X", new ManaCost(0, ManaSymbol.X));
            Test_Parse("XY2R", new ManaCost(2, ManaSymbol.X, ManaSymbol.Y, ManaSymbol.R));
        }

        [Test]
        public void Test_Parse_hybrid_mana()
        {
            Test_Parse("{W/U}", new ManaCost(0, ManaSymbol.WU));
            Test_Parse("2{W/B}{2/R}", new ManaCost(2, ManaSymbol.WB, ManaSymbol.R2));
        }

        #endregion

        #region Hash

        [Test]
        public void Test_Hash_changes_if_cost_changes()
        {
            Assert_HashIsEqual(new ManaCost(1, ManaSymbol.B, ManaSymbol.B, ManaSymbol.R), new ManaCost(1, ManaSymbol.B, ManaSymbol.B, ManaSymbol.R));
            Assert_HashIsEqual(new ManaCost(1, ManaSymbol.B, ManaSymbol.B, ManaSymbol.R), new ManaCost(1, ManaSymbol.B, ManaSymbol.R, ManaSymbol.B));

            Assert_HashIsNotEqual(new ManaCost(1), new ManaCost(2));
            Assert_HashIsNotEqual(new ManaCost(1), new ManaCost(1, ManaSymbol.B));
            Assert_HashIsNotEqual(new ManaCost(1, ManaSymbol.B), new ManaCost(1, ManaSymbol.R));
        }

        #endregion

        #endregion
    }
}

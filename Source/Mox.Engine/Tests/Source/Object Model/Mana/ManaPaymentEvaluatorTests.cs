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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;

namespace Mox
{
    [TestFixture]
    public class ManaPaymentEvaluatorTests
    {
        #region Helpers

        private static readonly Color[] ms_colors = { Color.White, Color.Blue, Color.Black, Color.Red, Color.Green };

        private static List<ManaPayment> EnumerateCompletePayments(ManaCost cost, ManaAmount amount, out ManaColors missingMana)
        {
            ManaPaymentEvaluator evaluator = new ManaPaymentEvaluator(cost);
            var canPayResult = evaluator.CanPay(amount);
            var enumerateResult = evaluator.EnumerateCompletePayments(amount);

            missingMana = evaluator.MissingMana;

            Assert.AreEqual(enumerateResult, canPayResult);

            if (!enumerateResult)
            {
                Assert.Collections.IsEmpty(evaluator.CompletePayments);
                Assert.AreNotEqual(ManaColors.None, evaluator.MissingMana);
            }

            return evaluator.CompletePayments.ToList();
        }

        private static List<ManaPaymentAmount> EnumerateTotalPayments(ManaCost cost, ManaAmount amount)
        {
            var microPayments = EnumerateCompletePayments(cost, amount, out ManaColors missingMana);

            List<ManaPaymentAmount> totalPayments = new List<ManaPaymentAmount>(microPayments.Count);

            foreach (var microPayment in microPayments)
            {
                totalPayments.Add(microPayment.GetTotalAmount());
            }

            return totalPayments;
        }

        private static ManaPaymentAmount GetTotalPayment(ManaCost cost, ManaAmount amount)
        {
            List<ManaPaymentAmount> payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(1, payments.Count, "Expected only one payment");
            return payments[0];
        }

        private static ManaColors GetMissingMana(ManaCost cost, ManaAmount amount)
        {
            var payments = EnumerateCompletePayments(cost, amount, out ManaColors missingMana);
            Assert.AreEqual(0, payments.Count, "Expected no complete payment");
            return missingMana;
        }

        private static bool TryGetHybridSymbol(Color colorA, Color colorB, out ManaSymbol symbol)
        {
            symbol = ManaSymbol.X;

            string a = GetColorShortString(colorA);
            string b = GetColorShortString(colorB);

            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;

            return Enum.TryParse(a + b, out symbol);
        }

        private static string GetColorShortString(Color c)
        {
            switch (c)
            {
                case Color.White: return "W";
                case Color.Blue: return "U";
                case Color.Black: return "B";
                case Color.Red: return "R";
                case Color.Green: return "G";
                default:
                    return null;
            }
        }

        private static Color GetOtherColor(params Color[] colors)
        {
            foreach (var color in ms_colors)
            {
                if (!colors.Contains(color))
                    return color;
            }

            throw new InvalidProgramException();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_EnumerateCompletePayments_returns_nothing_if_a_generic_cost_is_unpayable()
        {
            ManaCost cost = new ManaCost(10);
            ManaAmount amount = new ManaAmount();

            Assert.AreEqual(ManaColors.All, GetMissingMana(cost, amount));
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_nothing_if_a_single_colored_cost_is_unpayable()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.B);
            ManaAmount amount = new ManaAmount();

            Assert.AreEqual(ManaColors.Black, GetMissingMana(cost, amount));
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_the_only_possible_payment_when_obvious_with_only_colorless()
        {
            ManaCost cost = new ManaCost(10);
            ManaAmount amount = new ManaAmount { Colorless = 10 };

            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { Colorless = 10 }, payment);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_the_only_possible_payment_when_obvious_with_mixed_amount()
        {
            ManaCost cost = new ManaCost(10);
            ManaAmount amount = new ManaAmount { Colorless = 7, Blue = 2, Red = 1 };

            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { Colorless = 7, Blue = 2, Red = 1 }, payment);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_the_only_possible_payment_when_obvious_with_only_color()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.U);
            ManaAmount amount = new ManaAmount { Red = 1, Blue = 1 };

            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { Red = 1, Blue = 1 }, payment);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_the_only_possible_payment_when_obvious_with_more_mana_than_needed()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.U);
            ManaAmount amount = new ManaAmount { Red = 2, Blue = 2 };

            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { Red = 1, Blue = 1 }, payment);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_the_only_possible_payment_when_obvious_with_mixed()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.U);
            ManaAmount amount = new ManaAmount { Colorless = 2, Red = 1, Blue = 1 };

            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { Colorless = 2, Red = 1, Blue = 1 }, payment);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_with_only_colorless()
        {
            ManaCost cost = new ManaCost(1);
            ManaAmount amount = new ManaAmount { Red = 1, Blue = 1 };

            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(2, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Blue = 1 }, payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_1()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaAmount amount = new ManaAmount { Red = 1, Blue = 1 };

            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { Red = 1, Blue = 1 }, payment);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_2()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaAmount amount = new ManaAmount { Red = 2, Blue = 2 };

            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(2, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Blue = 1 }, payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_3()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaAmount amount = new ManaAmount { Red = 3 };

            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { Red = 2 }, payment);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_4()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaAmount amount = new ManaAmount { Red = 2, Green = 1 };

            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(2, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Green = 1 }, payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_5()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaAmount amount = new ManaAmount { Red = 2, Green = 2 };

            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(2, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Green = 1 }, payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_6()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R);
            ManaAmount amount = new ManaAmount { Red = 2, Green = 2 };

            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(2, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2, Green = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Green = 2 }, payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_7()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.U);
            ManaAmount amount = new ManaAmount { Red = 2, Blue = 2, Green = 2 };

            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(4, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2, Blue = 2 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2, Blue = 1, Green = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Blue = 2, Green = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Blue = 1, Green = 2 }, payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_8()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.U);
            ManaAmount amount = new ManaAmount { Red = 2, Blue = 2, Green = 2, Black = 2 };

            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(8, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2, Blue = 2 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2, Blue = 1, Green = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2, Blue = 1, Black = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Blue = 2, Green = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Blue = 2, Black = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Blue = 1, Green = 2 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Blue = 1, Black = 2 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Blue = 1, Green = 1, Black = 1 }, payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_doesnt_return_duplicate_payments()
        {
            ManaCost cost = new ManaCost(2);
            ManaAmount amount = new ManaAmount { Red = 2, Blue = 2 };

            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(3, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 2 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1, Blue = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Blue = 2 }, payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_for_all_single_mana()
        {
            foreach (var color in ms_colors)
            {
                ManaSymbol symbol;
                if (!Enum.TryParse(GetColorShortString(color), out symbol))
                    throw new InvalidProgramException();

                Trace.WriteLine($"Testing {symbol}");

                ManaCost cost = new ManaCost(0, symbol);

                var amountA = new ManaAmount();
                amountA.Add(color, 1);
                var payment = GetTotalPayment(cost, amountA);
                Assert.AreEqual((ManaPaymentAmount)amountA, payment);

                var amountB = new ManaAmount();
                amountB.Add(GetOtherColor(color), 1);
                var payments = EnumerateTotalPayments(cost, amountB);
                Assert.AreEqual(0, payments.Count);
            }
        }

        [Test]
        public void Test_EnumerateCompletePayments_dont_hang_when_pool_is_huge_and_colorless_is_zero()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.W);
            ManaAmount amount = new ManaAmount { Colorless = 200, Red = 200, Blue = 200, Green = 200, White = 200, Black = 200 };

            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { White = 1 }, payment);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_for_hybrid_mana()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.UR);

            var amount = new ManaAmount { Red = 1 };
            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { Red = 1 }, payment);

            amount = new ManaAmount { Blue = 1 };
            payment = GetTotalPayment(cost, amount);
            Assert.AreEqual(new ManaPaymentAmount { Blue = 1 }, payment);

            amount = new ManaAmount { Red = 1, Blue = 1 };
            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(2, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Blue = 1 }, payments);

            amount = new ManaAmount { Black = 1 };
            Assert.AreEqual(ManaColors.Blue | ManaColors.Red, GetMissingMana(cost, amount));
        }

        [Test]
        public void Test_EnumerateCompletePayments_for_all_hybrid_mana()
        {
            foreach (var colorA in ms_colors)
            {
                foreach (var colorB in ms_colors)
                {
                    ManaSymbol symbol;
                    if (!TryGetHybridSymbol(colorA, colorB, out symbol))
                        continue;

                    Trace.WriteLine($"Testing {symbol}");

                    ManaCost cost = new ManaCost(0, symbol);

                    var amountA = new ManaAmount();
                    amountA.Add(colorA, 1);
                    var payment = GetTotalPayment(cost, amountA);
                    Assert.AreEqual((ManaPaymentAmount)amountA, payment);

                    var amountB = new ManaAmount();
                    amountB.Add(colorB, 1);
                    payment = GetTotalPayment(cost, amountB);
                    Assert.AreEqual((ManaPaymentAmount)amountB, payment);

                    var amountC = new ManaAmount();
                    amountC.Add(colorA, 1);
                    amountC.Add(colorB, 1);
                    var payments = EnumerateTotalPayments(cost, amountC);
                    Assert.AreEqual(2, payments.Count);
                    Assert.Collections.Contains(amountA, payments);
                    Assert.Collections.Contains(amountB, payments);

                    var amountD = new ManaAmount();
                    amountD.Add(GetOtherColor(colorA, colorB), 1);
                    payments = EnumerateTotalPayments(cost, amountD);
                    Assert.AreEqual(0, payments.Count);
                }
            }
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_for_phyrexian_mana()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.RP);

            var amount = new ManaAmount { Red = 1 };
            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(2, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Red = 1 }, payments);
            Assert.Collections.Contains(new ManaPaymentAmount { Phyrexian = 1 }, payments);

            amount = new ManaAmount { Black = 1 };
            payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(1, payments.Count);
            Assert.Collections.Contains(new ManaPaymentAmount { Phyrexian = 1 }, payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_for_all_phyrexian_mana()
        {
            foreach (var color in ms_colors)
            {
                ManaSymbol symbol;
                if (!Enum.TryParse(GetColorShortString(color) + "P", out symbol))
                    throw new InvalidProgramException();

                Trace.WriteLine($"Testing {symbol}");

                ManaCost cost = new ManaCost(0, symbol);

                var amount = new ManaAmount();
                amount.Add(color, 1);
                var payments = EnumerateTotalPayments(cost, amount);
                Assert.AreEqual(2, payments.Count);
                Assert.Collections.Contains(amount, payments);
                Assert.Collections.Contains(new ManaPaymentAmount { Phyrexian = 1 }, payments);

                amount = new ManaAmount();
                amount.Add(GetOtherColor(color), 1);
                payments = EnumerateTotalPayments(cost, amount);
                Assert.AreEqual(1, payments.Count);
                Assert.Collections.Contains(new ManaPaymentAmount { Phyrexian = 1 }, payments);
            }
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_for_double_hybrid_mana()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.U2);

            var amount = new ManaAmount { Blue = 1 };
            var payment = GetTotalPayment(cost, amount);
            Assert.AreEqual((ManaPaymentAmount)amount, payment);

            amount = new ManaAmount { Blue = 2 };
            var payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(2, payments.Count);
            Assert.Collections.Contains(new ManaAmount { Blue = 1 }, payments);
            Assert.Collections.Contains(new ManaAmount { Blue = 2 }, payments); // Sadly it also returns this as a valid payment but I don't see how to avoid it

            amount = new ManaAmount { Black = 2 };
            payment = GetTotalPayment(cost, amount);
            Assert.AreEqual((ManaPaymentAmount)amount, payment);

            amount = new ManaAmount { Red = 1, Black = 1 };
            payment = GetTotalPayment(cost, amount);
            Assert.AreEqual((ManaPaymentAmount)amount, payment);

            amount = new ManaAmount { Black = 1 };
            Assert.AreEqual(ManaColors.All, GetMissingMana(cost, amount));

            amount = new ManaAmount { Black = 1, Red = 1, Green = 1 };
            payments = EnumerateTotalPayments(cost, amount);
            Assert.AreEqual(3, payments.Count);
        }

        [Test]
        public void Test_EnumerateCompletePayments_for_all_double_hybrid_mana()
        {
            foreach (var color in ms_colors)
            {
                ManaSymbol symbol;
                if (!Enum.TryParse(GetColorShortString(color) + "2", out symbol))
                    throw new InvalidProgramException();

                var colorB = GetOtherColor(color);
                var colorC = GetOtherColor(color, colorB);
                var colorD = GetOtherColor(color, colorB, colorC);

                Trace.WriteLine($"Testing {symbol}");

                ManaCost cost = new ManaCost(0, symbol);

                var amountA = new ManaAmount();
                amountA.Add(color, 1);
                var payment = GetTotalPayment(cost, amountA);
                Assert.AreEqual((ManaPaymentAmount)amountA, payment);

                var amountB = new ManaAmount();
                amountB.Add(color, 2);
                var payments = EnumerateTotalPayments(cost, amountB);
                Assert.AreEqual(2, payments.Count);
                Assert.Collections.Contains(amountA, payments);
                Assert.Collections.Contains(amountB, payments); // Sadly it also returns this as a valid payment but I don't see how to avoid it

                var amountC = new ManaAmount();
                amountC.Add(colorB, 2);
                payment = GetTotalPayment(cost, amountC);
                Assert.AreEqual((ManaPaymentAmount)amountC, payment);

                var amountD = new ManaAmount();
                amountD.Add(colorB, 1);
                amountD.Add(colorC, 1);
                payment = GetTotalPayment(cost, amountD);
                Assert.AreEqual((ManaPaymentAmount)amountD, payment);

                var amountE = new ManaAmount();
                amountE.Add(colorB, 1);
                payments = EnumerateTotalPayments(cost, amountE);
                Assert.AreEqual(0, payments.Count);

                var amountF = new ManaAmount();
                amountF.Add(colorB, 1);
                amountF.Add(colorC, 1);
                amountF.Add(colorD, 1);
                payments = EnumerateTotalPayments(cost, amountF);
                Assert.AreEqual(3, payments.Count);

                var amountG = new ManaAmount();
                amountG.Add(color, 1);
                amountG.Add(colorB, 2);
                payments = EnumerateTotalPayments(cost, amountG);
                Assert.AreEqual(3, payments.Count);
                Assert.Collections.Contains(amountA, payments);
                Assert.Collections.Contains(amountC, payments);

                var amountH = new ManaAmount();
                amountH.Add(color, 1);
                amountH.Add(colorB, 1);
                Assert.Collections.Contains(amountH, payments);
            }
        }

        [Test]
        public void Test_MissingMana_returns_colors_that_can_be_used_to_complete_the_payment()
        {
            var cost = new ManaCost(0, ManaSymbol.B, ManaSymbol.R, ManaSymbol.G, ManaSymbol.C);
            var amount = new ManaAmount { Black = 1, Green = 1 };
            Assert.AreEqual(ManaColors.Red | ManaColors.Colorless, GetMissingMana(cost, amount));

            cost = new ManaCost(0, ManaSymbol.WB, ManaSymbol.BG);
            amount = new ManaAmount { Black = 1 };
            Assert.AreEqual(ManaColors.White | ManaColors.Black | ManaColors.Green, GetMissingMana(cost, amount));

            cost = new ManaCost(0, ManaSymbol.W2);
            amount = new ManaAmount { Black = 1 };
            Assert.AreEqual(ManaColors.All, GetMissingMana(cost, amount));

            cost = new ManaCost(0, ManaSymbol.W, ManaSymbol.BP);
            amount = new ManaAmount();
            Assert.AreEqual(ManaColors.White | ManaColors.Black, GetMissingMana(cost, amount)); // Phyrexian always considered "missing" if not provided explicitly

            cost = new ManaCost(1, ManaSymbol.W);
            amount = new ManaAmount { White = 1 };
            Assert.AreEqual(ManaColors.All, GetMissingMana(cost, amount));

            cost = new ManaCost(1, ManaSymbol.W);
            amount = new ManaAmount { Red = 1 };
            Assert.AreEqual(ManaColors.White, GetMissingMana(cost, amount));
        }

        #endregion
    }
}

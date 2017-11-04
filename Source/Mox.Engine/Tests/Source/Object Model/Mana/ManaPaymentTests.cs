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

namespace Mox
{
    [TestFixture]
    public class ManaPaymentTests
    {
        #region Variables

        private ManaPayment m_payment;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_payment = new ManaPayment();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsTrue(m_payment.Payments.IsReadOnly);
            Assert.Collections.IsEmpty(m_payment.Payments);
        }

        [Test]
        public void Test_Can_pay_with_any_kind_of_mana()
        {
            m_payment.Pay(Color.Black);
            m_payment.Pay(Color.Red);
            m_payment.Pay(Color.None);

            Assert.Collections.AreEquivalent(new[] 
            { 
                Color.Black,
                Color.Red,
                Color.None
            }, m_payment.Payments);
        }

        [Test]
        public void Test_IsEmpty_returns_true_if_no_payments_have_been_made()
        {
            Assert.IsTrue(m_payment.IsEmpty);
            m_payment.Pay(Color.Black);
            Assert.IsFalse(m_payment.IsEmpty);
        }

        [Test]
        public void Test_Is_serializable()
        {
            ManaPayment result = Assert.IsSerializable(m_payment);
            Assert.Collections.AreEqual(result.Payments, m_payment.Payments);
        }

        [Test]
        public void Test_Can_pay_multiple_mana_in_one_call()
        {
            m_payment.Pay(Color.Black, 2);
            m_payment.Pay(Color.Red, 3);

            Assert.Collections.AreEquivalent(new[] 
            { 
                Color.Black,
                Color.Black,
                Color.Red,
                Color.Red,
                Color.Red
            }, m_payment.Payments);
        }

        [Test]
        public void Test_Can_pay_with_0_tokens()
        {
            m_payment.Pay(Color.Black, 0);
            Assert.Collections.IsEmpty(m_payment.Payments);
        }

        [Test]
        public void Test_Number_of_tokens_must_be_positive()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => m_payment.Pay(Color.Black, -1));
        }

        #region EnumerateCompletePayments

        private static List<ManaPayment> EnumerateCompletePayments(ManaCost cost, ManaPool pool)
        {
            return new List<ManaPayment>(ManaPayment.EnumerateCompletePayments(cost, pool));
        }

        private static ManaPayment GetCompletePayment(ManaCost cost, ManaPool pool)
        {
            List<ManaPayment> payments = EnumerateCompletePayments(cost, pool);
            Assert.AreEqual(1, payments.Count, "Expected only one payment");
            return payments[0];
        }

        private static void Assert_CompletePayments_Are(ManaCost cost, ManaPool pool, IEnumerable<IEnumerable<Color>> expectedPayments)
        {
            List<List<Color>> expectedColorsList = new List<List<Color>>();
            expectedPayments.ForEach(payment =>
            {
                List<Color> colors = new List<Color>(payment);
                colors.Sort();
                expectedColorsList.Add(colors);
            });

            List<ManaPayment> actualPayments = EnumerateCompletePayments(cost, pool);

            if (actualPayments.Count != expectedColorsList.Count)
            {
                StringBuilder receivedPayments = new StringBuilder();

                foreach (var actual in actualPayments)
                {
                    receivedPayments.AppendLine();
                    receivedPayments.Append("[");
                    receivedPayments.Append(actual.Payments.Join(", "));
                    receivedPayments.Append("]");
                }

                Assert.Fail("Expected {0} payments but got {1}. Received payments: {2}", expectedColorsList.Count, actualPayments.Count, receivedPayments);
            }

            foreach (ManaPayment actualPayment in actualPayments)
            {
                Assert.IsFalse(actualPayment.IsEmpty, "Received empty payment");
                var actualColorsInOrder = new List<Color>(actualPayment.Payments);
                actualColorsInOrder.Sort();

                bool found = false;
                foreach (var expectedColors in expectedColorsList)
                {
                    if (actualColorsInOrder.SequenceEqual(expectedColors))
                    {
                        expectedColorsList.Remove(expectedColors);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Assert.Fail("Received unexpected payment: {0}", actualColorsInOrder.Join(", "));
                }
            }

            if (expectedColorsList.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                expectedColorsList.ForEach(payment =>
                {
                    builder.Append("[");
                    builder.Append(payment.Join(", "));
                    builder.Append("], ");
                });
                builder.Remove(builder.Length - 2, 2);

                Assert.Fail("Expected payment(s): {0}", builder);
            }
        }

        [Test]
        public void Test_Invalid_EnumerateCompletePayments_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => EnumerateCompletePayments(null, new ManaPool()));
            Assert.Throws<ArgumentNullException>(() => EnumerateCompletePayments(new ManaCost(0), null));
            Assert.Throws<ArgumentException>(() => EnumerateCompletePayments(new ManaCost(0), new ManaPool())); // Cannot enumerate with an empty cost
            Assert.Throws<ArgumentException>(() => EnumerateCompletePayments(new ManaCost(0, ManaSymbol.X), new ManaPool())); // Cannot enumerate with an abstract cost
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_nothing_if_the_cost_is_unpayable_with_colorless()
        {
            ManaCost cost = new ManaCost(10);
            ManaPool pool = new ManaPool();

            Assert.Collections.IsEmpty(EnumerateCompletePayments(cost, pool));
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_nothing_if_the_cost_is_unpayable_with_color()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.B);
            ManaPool pool = new ManaPool();

            Assert.Collections.IsEmpty(EnumerateCompletePayments(cost, pool));
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_the_only_possible_payment_when_obvious_with_only_colorless()
        {
            ManaCost cost = new ManaCost(10);
            ManaPool pool = new ManaPool();
            pool[Color.None] = 10;

            ManaPayment payment = GetCompletePayment(cost, pool);
            Assert.AreEqual(10, payment.Payments.Count);
            payment.Payments.ForEach(color => Assert.AreEqual(Color.None, color));
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_the_only_possible_payment_when_obvious_with_only_color()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 1;
            pool[Color.Blue] = 1;

            ManaPayment payment = GetCompletePayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red, Color.Blue }, payment.Payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_the_only_possible_payment_when_obvious_with_mixed()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.None] = 2;
            pool[Color.Red] = 1;
            pool[Color.Blue] = 1;

            ManaPayment payment = GetCompletePayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.None, Color.None, Color.Red, Color.Blue }, payment.Payments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_with_only_colorless()
        {
            ManaCost cost = new ManaCost(1);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 1;
            pool[Color.Blue] = 1;

            var expectedPayments = new[]
            {
                new []{ Color.Red },
                new []{ Color.Blue }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_1()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 1;
            pool[Color.Blue] = 1;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Blue }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_2()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 2;
            pool[Color.Blue] = 2;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Blue },
                new []{ Color.Red, Color.Red }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_3()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 3;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Red }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_4()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 2;
            pool[Color.Green] = 1;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Red },
                new []{ Color.Red, Color.Green }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_5()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 2;
            pool[Color.Green] = 2;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Red },
                new []{ Color.Red, Color.Green }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_6()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 2;
            pool[Color.Green] = 2;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Red, Color.Green },
                new []{ Color.Red, Color.Green, Color.Green }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_7()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 2;
            pool[Color.Blue] = 2;
            pool[Color.Green] = 2;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Blue, Color.Red, Color.Blue },
                new []{ Color.Red, Color.Blue, Color.Red, Color.Green },
                new []{ Color.Red, Color.Blue, Color.Blue, Color.Green },
                new []{ Color.Red, Color.Blue, Color.Green, Color.Green },
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_returns_all_the_possible_payments_8()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 2;
            pool[Color.Blue] = 2;
            pool[Color.Green] = 2;
            pool[Color.Black] = 2;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Blue, Color.Red, Color.Blue },
                new []{ Color.Red, Color.Blue, Color.Red, Color.Green },
                new []{ Color.Red, Color.Blue, Color.Red, Color.Black },
                new []{ Color.Red, Color.Blue, Color.Blue, Color.Green },
                new []{ Color.Red, Color.Blue, Color.Blue, Color.Black },
                new []{ Color.Red, Color.Blue, Color.Green, Color.Green },
                new []{ Color.Red, Color.Blue, Color.Green, Color.Black },
                new []{ Color.Red, Color.Blue, Color.Black, Color.Black },
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_When_enough_colorless_is_available_EnumerateCompletePayments_will_use_it_always_1()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.None] = 2;
            pool[Color.Red] = 2;
            pool[Color.Blue] = 2;
            pool[Color.Green] = 2;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Blue, Color.None, Color.None }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_When_enough_colorless_is_available_EnumerateCompletePayments_will_use_it_always_2()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.None] = 1;
            pool[Color.Red] = 2;
            pool[Color.Blue] = 2;
            pool[Color.Green] = 2;

            var expectedPayments = new[]
            {
                new []{ Color.Red, Color.Blue, Color.None, Color.Red },
                new []{ Color.Red, Color.Blue, Color.None, Color.Blue },
                new []{ Color.Red, Color.Blue, Color.None, Color.Green }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        [Test]
        public void Test_EnumerateCompletePayments_dont_hang_when_pool_is_huge_and_colorless_is_zero()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.W);
            ManaPool pool = new ManaPool();
            pool[Color.None] = 200;
            pool[Color.Red] = 200;
            pool[Color.Blue] = 200;
            pool[Color.Green] = 200;
            pool[Color.White] = 200;
            pool[Color.Black] = 200;

            var expectedPayments = new[]
            {
                new []{ Color.White }
            };
            Assert_CompletePayments_Are(cost, pool, expectedPayments);
        }

        #endregion

        #region GetMaximalRemainingPayment

        [Test]
        public void Test_GetMaximalRemainingPayment_will_return_an_empty_payment_if_cost_can_be_paid_entirely_from_mana_pool()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 1;
            pool[Color.Blue] = 1;

            ManaPayment payment = ManaPayment.GetMaximalRemainingPayment(cost, pool);
            Assert.Collections.AreEquivalent(new Color[0], payment.Payments);
        }

        [Test]
        public void Test_GetMaximalRemainingPayment_with_simple_case_1()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 0;
            pool[Color.Blue] = 1;

            ManaPayment payment = ManaPayment.GetMaximalRemainingPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red }, payment.Payments);
        }

        [Test]
        public void Test_GetMaximalRemainingPayment_with_simple_case_2()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.Red] = 2;
            pool[Color.Blue] = 1;

            ManaPayment payment = ManaPayment.GetMaximalRemainingPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.None }, payment.Payments);
        }

        [Test]
        public void Test_GetMaximalRemainingPayment_with_simple_case_3()
        {
            ManaCost cost = new ManaCost(1, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.Black] = 1;
            pool[Color.Blue] = 1;

            ManaPayment payment = ManaPayment.GetMaximalRemainingPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red }, payment.Payments);
        }

        [Test]
        public void Test_GetMaximalRemainingPayment_with_simple_case_4()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool();
            pool[Color.Black] = 1;
            pool[Color.Blue] = 1;

            ManaPayment payment = ManaPayment.GetMaximalRemainingPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red, Color.Red, Color.None }, payment.Payments);
        }

        #endregion

        #region GetMaximalTrivialPayment

        [Test]
        public void Test_GetMaximalTrivialPayment_RR()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.R);
            ManaPool pool = new ManaPool { Red = 2, Blue = 1 };

            ManaPayment payment = ManaPayment.GetMaximalTrivialPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red, Color.Red }, payment.Payments);
        }

        [Test]
        public void Test_GetMaximalTrivialPayment_BR()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.U);
            ManaPool pool = new ManaPool { Red = 2, Blue = 1 };

            ManaPayment payment = ManaPayment.GetMaximalTrivialPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red, Color.Blue }, payment.Payments);
        }

        [Test]
        public void Test_GetMaximalTrivialPayment_RR_cant_pay_all()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.R);
            ManaPool pool = new ManaPool { Red = 1, Blue = 1 };

            ManaPayment payment = ManaPayment.GetMaximalTrivialPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red }, payment.Payments);
        }

        [Test]
        public void Test_GetMaximalTrivialPayment_UBRR_cant_pay_all()
        {
            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.R, ManaSymbol.B, ManaSymbol.U);
            ManaPool pool = new ManaPool { Red = 1, Blue = 1 };

            ManaPayment payment = ManaPayment.GetMaximalTrivialPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red, Color.Blue }, payment.Payments);
        }

        [Test]
        public void Test_GetMaximalTrivialPayment_can_pay_colorless_with_any_color()
        {
            ManaCost cost = new ManaCost(5);
            ManaPool pool = new ManaPool { Red = 3 };

            ManaPayment payment = ManaPayment.GetMaximalTrivialPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red, Color.Red, Color.Red }, payment.Payments);
        }

        [Test]
        public void Test_GetMaximalTrivialPayment_can_pay_colorless_with_colored_if_non_ambiguous()
        {
            ManaCost cost = new ManaCost(5, ManaSymbol.C);
            ManaPool pool = new ManaPool { Colorless = 1, Blue = 2 };

            ManaPayment payment = ManaPayment.GetMaximalTrivialPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.None, Color.Blue, Color.Blue }, payment.Payments);
        }

        [Test]
        public void Test_GetMaximalTrivialPayment_cannot_pay_colorless_with_colored_if_ambiguous()
        {
            ManaCost cost = new ManaCost(5);
            ManaPool pool = new ManaPool { Colorless = 1, Blue = 2 };

            ManaPayment payment = ManaPayment.GetMaximalTrivialPayment(cost, pool);
            Assert.Collections.AreEquivalent(new Color[0], payment.Payments);
        }

        [Test]
        public void Test_GetMaximalTrivialPayment_colored_is_payed_before_colorless()
        {
            ManaCost cost = new ManaCost(2, ManaSymbol.R);
            ManaPool pool = new ManaPool { Red = 2 };

            ManaPayment payment = ManaPayment.GetMaximalTrivialPayment(cost, pool);
            Assert.Collections.AreEquivalent(new[] { Color.Red, Color.Red }, payment.Payments);
        }

        #endregion

        #endregion
    }

#warning [Mana] Rename
    [TestFixture]
    public class ManaPaymentNewTests
    {
        #region Helpers

        private void CheckIsExactPayment(ManaCost cost, ManaPaymentNew payment)
        {
            Assert.That(payment.TryPay(cost, out ManaCost remaining));
            Assert.That(remaining.IsEmpty);
        }

        private void CheckIsExactAtomPayment(ManaSymbol symbol, ManaPaymentAmount payment)
        {
            ManaCost cost = new ManaCost(0, symbol);

            ManaPaymentNew fullPayment = new ManaPaymentNew();
            fullPayment.Atoms = new[] { payment };

            CheckIsExactPayment(cost, fullPayment);
        }

        private void CheckIsExactGenericPayment(byte generic, ManaPaymentAmount payment)
        {
            ManaCost cost = new ManaCost(generic);
            ManaPaymentNew fullPayment = new ManaPaymentNew { Generic = payment };

            CheckIsExactPayment(cost, fullPayment);
        }

        private void CheckCreateAnyFromCost(ManaCost cost)
        {
            var payment = ManaPaymentNew.CreateAnyFromCost(cost);
            Assert.That(payment.TryPay(cost, out ManaCost remaining));
            Assert.That(remaining.IsEmpty);
        }

        private void CheckNoTrivialPayment(ManaCost cost, ManaAmount amount)
        {
            bool result = ManaPaymentNew.TryGetTrivialPayment(cost, amount, out ManaPaymentNew payment);
            Assert.IsFalse(result);
        }

        private ManaPaymentNew GetTrivialPayment(ManaCost cost, ManaAmount amount)
        {
            bool result = ManaPaymentNew.TryGetTrivialPayment(cost, amount, out ManaPaymentNew payment);
            Assert.That(result, $"No trivial payment for cost {cost} with amount {amount}");
            return payment;
        }

        private void AssertIsEmpty(ManaPaymentAmount paymentAmount)
        {
            Assert.AreEqual(new ManaPaymentAmount(), paymentAmount);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Clone_returns_a_copy()
        {
            var emptyOriginal = new ManaPaymentNew();
            var emptyCopy = emptyOriginal.Clone();
            Assert.AreEqual(emptyOriginal.Generic, emptyCopy.Generic);
            Assert.IsNull(emptyCopy.Atoms);

            var fullOriginal = new ManaPaymentNew();
            fullOriginal.Generic = new ManaPaymentAmount { Red = 1 };
            fullOriginal.Atoms = new[] { new ManaPaymentAmount { Blue = 1 }, new ManaPaymentAmount { Colorless = 2 } };

            var fullCopy = fullOriginal.Clone();
            Assert.AreEqual(fullOriginal.Generic, fullCopy.Generic);
            Assert.AreEqual(2, fullCopy.Atoms.Length);
            Assert.AreEqual(new ManaPaymentAmount { Blue = 1 }, fullCopy.Atoms[0]);
            Assert.AreEqual(new ManaPaymentAmount { Colorless = 2 }, fullCopy.Atoms[1]);

            // Check that they are independant
            fullCopy.Atoms[0] = new ManaPaymentAmount { White = 1 };
            Assert.AreEqual(new ManaPaymentAmount { Blue = 1 }, fullOriginal.Atoms[0]);
        }

        [Test]
        public void Test_Prepare_returns_a_payment_with_the_correct_atom_count()
        {
            var payment = ManaPaymentNew.Prepare(new ManaCost(2));
            Assert.IsNull(payment.Atoms);

            payment = ManaPaymentNew.Prepare(new ManaCost(2, ManaSymbol.R, ManaSymbol.GW));
            Assert.AreEqual(2, payment.Atoms.Length);
        }

        [Test]
        public void Test_CreateAnyFromCost_returns_any_payment_that_fullfills_exactly_the_cost()
        {
            CheckCreateAnyFromCost(new ManaCost(2));
            CheckCreateAnyFromCost(new ManaCost(2, ManaSymbol.R, ManaSymbol.W));
            CheckCreateAnyFromCost(new ManaCost(2, ManaSymbol.R2, ManaSymbol.WP, ManaSymbol.WB));
        }

        [Test]
        public void Test_GetTotalAmount_returns_the_sum_of_all_atoms_and_the_generic_amount()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Generic = new ManaPaymentAmount { Red = 1, Blue = 1 };
            payment.Atoms = new ManaPaymentAmount[2];
            payment.Atoms[0] = new ManaPaymentAmount { Red = 1 };
            payment.Atoms[1] = new ManaPaymentAmount { Phyrexian = 1 };

            Assert.AreEqual(new ManaPaymentAmount { Red = 2, Blue = 1, Phyrexian = 1 }, payment.GetTotalAmount());
        }

        [Test]
        public void Test_GetTotalAmount_works_if_its_only_a_generic_payment()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Generic = new ManaPaymentAmount { Red = 1, Blue = 1 };

            Assert.AreEqual(new ManaPaymentAmount { Red = 1, Blue = 1 }, payment.GetTotalAmount());
        }

        [Test]
        public void Test_TryPay_throws_if_the_cost_is_not_concrete()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Atoms = new[] { new ManaPaymentAmount { Red = 1 } };

            ManaCost cost = new ManaCost(0, ManaSymbol.X);
            Assert.Throws<InvalidOperationException>(() => payment.TryPay(cost, out ManaCost remaining));
        }

        [Test]
        public void Test_TryPay_returns_true_if_the_payment_is_exact()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Generic = new ManaPaymentAmount { Blue = 1, Black = 1 };
            payment.Atoms = new ManaPaymentAmount[2];
            payment.Atoms[0] = new ManaPaymentAmount { Blue = 1 };
            payment.Atoms[1] = new ManaPaymentAmount { Red = 1 };            

            ManaCost cost = new ManaCost(2, ManaSymbol.R, ManaSymbol.UR);
            Assert.That(payment.TryPay(cost, out ManaCost remaining));
            Assert.That(remaining.IsEmpty);
        }

        [Test]
        public void Test_TryPay_returns_false_if_the_payment_is_not_valid()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Atoms = new [] { new ManaPaymentAmount { Black = 1 } };

            ManaCost cost = new ManaCost(0, ManaSymbol.UR);
            Assert.That(!payment.TryPay(cost, out ManaCost remaining));
        }

        [Test]
        public void Test_TryPay_returns_false_if_there_is_not_enough_atoms()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Atoms = new[] { new ManaPaymentAmount { Red = 1 } };

            ManaCost cost = new ManaCost(0, ManaSymbol.R, ManaSymbol.R);
            Assert.That(!payment.TryPay(cost, out ManaCost remaining));
        }

        [Test]
        public void Test_TryPay_returns_false_if_there_is_too_many_atoms()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Atoms = new[] { new ManaPaymentAmount { Red = 1 }, new ManaPaymentAmount { Red = 1 } };

            ManaCost cost = new ManaCost(0, ManaSymbol.R);
            Assert.That(!payment.TryPay(cost, out ManaCost remaining));
        }

        [Test]
        public void Test_TryPay_returns_false_if_an_atom_payment_is_too_much()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Atoms = new[] { new ManaPaymentAmount { Black = 2 } };

            ManaCost cost = new ManaCost(0, ManaSymbol.UR);
            Assert.That(!payment.TryPay(cost, out ManaCost remaining));
        }

        [Test]
        public void Test_TryPay_returns_false_if_the_generic_payment_is_too_much()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Generic = new ManaPaymentAmount { Blue = 3 };

            ManaCost cost = new ManaCost(2);
            Assert.That(!payment.TryPay(cost, out ManaCost remaining));
        }

        [Test]
        public void Test_TryPay_returns_false_if_an_atom_payment_contains_phyrexian_but_is_not_valid()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Atoms = new[] { new ManaPaymentAmount { Red = 1, Phyrexian = 1 } };

            ManaCost cost = new ManaCost(0, ManaSymbol.R);
            Assert.That(!payment.TryPay(cost, out ManaCost remaining));
        }

        [Test]
        public void Test_TryPay_returns_false_if_the_generic_payment_contains_phyrexian()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Generic = new ManaPaymentAmount { Phyrexian = 1 };

            ManaCost cost = new ManaCost(1);
            Assert.That(!payment.TryPay(cost, out ManaCost remaining));
        }

        [Test]
        public void Test_TryPay_returns_the_remaining_cost_when_its_a_partial_payment()
        {
            ManaPaymentNew payment = new ManaPaymentNew();
            payment.Generic = new ManaPaymentAmount { Blue = 1, Black = 1 };
            payment.Atoms = new ManaPaymentAmount[2];
            payment.Atoms[0] = new ManaPaymentAmount { Blue = 1 };

            ManaCost cost = new ManaCost(5, ManaSymbol.R, ManaSymbol.UR);
            Assert.That(payment.TryPay(cost, out ManaCost remaining));
            Assert.AreEqual(new ManaCost(3, ManaSymbol.R), remaining);
        }

        [Test]
        public void Test_Exact_payments_single()
        {
            CheckIsExactAtomPayment(ManaSymbol.W, new ManaPaymentAmount { White = 1 });
            CheckIsExactAtomPayment(ManaSymbol.U, new ManaPaymentAmount { Blue = 1 });
            CheckIsExactAtomPayment(ManaSymbol.B, new ManaPaymentAmount { Black = 1 });
            CheckIsExactAtomPayment(ManaSymbol.R, new ManaPaymentAmount { Red = 1 });
            CheckIsExactAtomPayment(ManaSymbol.G, new ManaPaymentAmount { Green = 1 });
            CheckIsExactAtomPayment(ManaSymbol.C, new ManaPaymentAmount { Colorless = 1 });
        }

        [Test]
        public void Test_Exact_payments_hybrid()
        {
            CheckIsExactAtomPayment(ManaSymbol.WU, new ManaPaymentAmount { White = 1 });
            CheckIsExactAtomPayment(ManaSymbol.WU, new ManaPaymentAmount { Blue = 1 });

            CheckIsExactAtomPayment(ManaSymbol.WB, new ManaPaymentAmount { White = 1 });
            CheckIsExactAtomPayment(ManaSymbol.WB, new ManaPaymentAmount { Black = 1 });

            CheckIsExactAtomPayment(ManaSymbol.UB, new ManaPaymentAmount { Blue = 1 });
            CheckIsExactAtomPayment(ManaSymbol.UB, new ManaPaymentAmount { Black = 1 });

            CheckIsExactAtomPayment(ManaSymbol.UR, new ManaPaymentAmount { Blue = 1 });
            CheckIsExactAtomPayment(ManaSymbol.UR, new ManaPaymentAmount { Red = 1 });

            CheckIsExactAtomPayment(ManaSymbol.BR, new ManaPaymentAmount { Black = 1 });
            CheckIsExactAtomPayment(ManaSymbol.BR, new ManaPaymentAmount { Red = 1 });

            CheckIsExactAtomPayment(ManaSymbol.BG, new ManaPaymentAmount { Black = 1 });
            CheckIsExactAtomPayment(ManaSymbol.BG, new ManaPaymentAmount { Green = 1 });

            CheckIsExactAtomPayment(ManaSymbol.RG, new ManaPaymentAmount { Red = 1 });
            CheckIsExactAtomPayment(ManaSymbol.RG, new ManaPaymentAmount { Green = 1 });

            CheckIsExactAtomPayment(ManaSymbol.RW, new ManaPaymentAmount { Red = 1 });
            CheckIsExactAtomPayment(ManaSymbol.RW, new ManaPaymentAmount { White = 1 });

            CheckIsExactAtomPayment(ManaSymbol.GW, new ManaPaymentAmount { Green = 1 });
            CheckIsExactAtomPayment(ManaSymbol.GW, new ManaPaymentAmount { White = 1 });

            CheckIsExactAtomPayment(ManaSymbol.GU, new ManaPaymentAmount { Green = 1 });
            CheckIsExactAtomPayment(ManaSymbol.GU, new ManaPaymentAmount { Blue = 1 });
        }

        [Test]
        public void Test_Exact_payments_hybrid_2()
        {
            CheckIsExactAtomPayment(ManaSymbol.W2, new ManaPaymentAmount { White = 1 });
            CheckIsExactAtomPayment(ManaSymbol.W2, new ManaPaymentAmount { Blue = 2 });
            CheckIsExactAtomPayment(ManaSymbol.W2, new ManaPaymentAmount { Blue = 1, White = 1 });
            CheckIsExactAtomPayment(ManaSymbol.W2, new ManaPaymentAmount { Blue = 1, Red = 1 });

            CheckIsExactAtomPayment(ManaSymbol.U2, new ManaPaymentAmount { Blue = 1 });
            CheckIsExactAtomPayment(ManaSymbol.U2, new ManaPaymentAmount { Colorless = 2 });

            CheckIsExactAtomPayment(ManaSymbol.B2, new ManaPaymentAmount { Black = 1 });
            CheckIsExactAtomPayment(ManaSymbol.B2, new ManaPaymentAmount { Colorless = 2 });

            CheckIsExactAtomPayment(ManaSymbol.R2, new ManaPaymentAmount { Red = 1 });
            CheckIsExactAtomPayment(ManaSymbol.R2, new ManaPaymentAmount { Colorless = 2 });

            CheckIsExactAtomPayment(ManaSymbol.G2, new ManaPaymentAmount { Green = 1 });
            CheckIsExactAtomPayment(ManaSymbol.G2, new ManaPaymentAmount { Colorless = 2 });
        }

        [Test]
        public void Test_Exact_payments_phyrexian()
        {
            CheckIsExactAtomPayment(ManaSymbol.WP, new ManaPaymentAmount { White = 1 });
            CheckIsExactAtomPayment(ManaSymbol.WP, new ManaPaymentAmount { Phyrexian = 1 });

            CheckIsExactAtomPayment(ManaSymbol.UP, new ManaPaymentAmount { Blue = 1 });
            CheckIsExactAtomPayment(ManaSymbol.UP, new ManaPaymentAmount { Phyrexian = 1 });

            CheckIsExactAtomPayment(ManaSymbol.BP, new ManaPaymentAmount { Black = 1 });
            CheckIsExactAtomPayment(ManaSymbol.BP, new ManaPaymentAmount { Phyrexian = 1 });

            CheckIsExactAtomPayment(ManaSymbol.RP, new ManaPaymentAmount { Red = 1 });
            CheckIsExactAtomPayment(ManaSymbol.RP, new ManaPaymentAmount { Phyrexian = 1 });

            CheckIsExactAtomPayment(ManaSymbol.GP, new ManaPaymentAmount { Green = 1 });
            CheckIsExactAtomPayment(ManaSymbol.GP, new ManaPaymentAmount { Phyrexian = 1 });
        }

        [Test]
        public void Test_Exact_payments_generic()
        {
            CheckIsExactGenericPayment(1, new ManaPaymentAmount { White = 1 });
            CheckIsExactGenericPayment(2, new ManaPaymentAmount { White = 2 });
            CheckIsExactGenericPayment(2, new ManaPaymentAmount { White = 1, Colorless = 1 });
            CheckIsExactGenericPayment(5, new ManaPaymentAmount { White = 2, Colorless = 3 });
        }

        [Test]
        public void Test_ToString()
        {
            var payment = new ManaPaymentNew();
            Assert.AreEqual("0", payment.ToString());

            payment.Generic = new ManaPaymentAmount { White = 1, Colorless = 2 };
            Assert.AreEqual("(WCC)", payment.ToString());

            payment.Atoms = new ManaPaymentAmount[3];
            payment.Atoms[0] = new ManaPaymentAmount { Red = 1 };
            payment.Atoms[1] = new ManaPaymentAmount { Red = 1, Green = 1 };
            payment.Atoms[2] = new ManaPaymentAmount { Phyrexian = 1 };
            Assert.AreEqual("(WCC)R(RG)P", payment.ToString());

            payment.Atoms[1] = new ManaPaymentAmount();
            Assert.AreEqual("(WCC)R0P", payment.ToString());

            payment.Generic = new ManaPaymentAmount();
            Assert.AreEqual("0R0P", payment.ToString());
        }
        
        [Test]
        public void Test_TryGetTrivialPayment_returns_false_when_there_is_no_trivial_payment()
        {
            CheckNoTrivialPayment(new ManaCost(2, ManaSymbol.R), new ManaAmount());
            CheckNoTrivialPayment(new ManaCost(0, ManaSymbol.R), new ManaAmount { Blue = 1 });
            CheckNoTrivialPayment(new ManaCost(0, ManaSymbol.RW), new ManaAmount { Blue = 1 });
        }

        [Test]
        public void Test_TryGetTrivialPayment_returns_the_trivial_payment_when_there_is_one()
        {
            var payment = GetTrivialPayment(new ManaCost(0, ManaSymbol.B, ManaSymbol.B), new ManaAmount { Black = 1 });
            AssertIsEmpty(payment.Generic);
            Assert.AreEqual(new ManaPaymentAmount { Black = 1 }, payment.Atoms[0]);
            AssertIsEmpty(payment.Atoms[1]);

            payment = GetTrivialPayment(new ManaCost(0, ManaSymbol.WB), new ManaAmount { Black = 1 });
            AssertIsEmpty(payment.Generic);
            Assert.AreEqual(new ManaPaymentAmount { Black = 1 }, payment.Atoms[0]);

            payment = GetTrivialPayment(new ManaCost(0, ManaSymbol.WB), new ManaAmount { Black = 1 });
            AssertIsEmpty(payment.Generic);
            Assert.AreEqual(new ManaPaymentAmount { Black = 1 }, payment.Atoms[0]);
        }

        [Test]
        public void Test_TryGetTrivialPayment_Single_mana_is_always_trivial_to_pay()
        {
            var payment = GetTrivialPayment(new ManaCost(10, ManaSymbol.B), new ManaAmount { Black = 1 });
            AssertIsEmpty(payment.Generic);
            Assert.AreEqual(new ManaPaymentAmount { Black = 1 }, payment.Atoms[0]);
        }

        [Test]
        public void Test_TryGetTrivialPayment_Generic_cost_is_always_trivial_to_pay_if_the_rest_is_already_paid()
        {
            var payment = GetTrivialPayment(new ManaCost(2), new ManaAmount { Black = 1 });
            Assert.AreEqual(new ManaPaymentAmount { Black = 1 }, payment.Generic);
        }

        [Test]
        public void Test_TryGetTrivialPayment_Generic_cost_is_trivial_if_the_mana_cannot_be_used_for_something_else_anyway()
        {
            var payment = GetTrivialPayment(new ManaCost(2, ManaSymbol.R), new ManaAmount { Black = 1 });
            Assert.AreEqual(new ManaPaymentAmount { Black = 1 }, payment.Generic);
            AssertIsEmpty(payment.Atoms[0]);
        }

        [Test]
        public void Test_TryGetTrivialPayment_Hybrid_cost_is_not_trivial_if_more_than_one_symbol_can_be_paid_with_same_mana()
        {
            CheckNoTrivialPayment(new ManaCost(0, ManaSymbol.RW, ManaSymbol.WB), new ManaAmount { White = 1 });

            // But R is trivial
            var payment = GetTrivialPayment(new ManaCost(0, ManaSymbol.WB, ManaSymbol.RW), new ManaAmount { Red = 1 });
            AssertIsEmpty(payment.Generic);
            AssertIsEmpty(payment.Atoms[0]);
            Assert.AreEqual(new ManaPaymentAmount { Red = 1 }, payment.Atoms[1]);
        }

        #endregion
    }
}

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
    public class ManaAbilityOutcomeTests : BaseGameTests
    {
        #region Utilities

        private static ManaPayment CreatePayment(params Color[] colors)
        {
            ManaPayment payment = new ManaPayment();
            foreach (Color color in colors)
            {
                payment.Pay(color);
            }
            return payment;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Any_represents_an_outcome_that_can_always_possibly_provide_a_payment()
        {
            Assert.IsTrue(ManaAbilityOutcome.Any.CanProvide(CreatePayment()));
            Assert.IsTrue(ManaAbilityOutcome.Any.CanProvide(CreatePayment(Color.None, Color.None, Color.Red, Color.White)));
        }

        [Test]
        public void Test_OfColor_can_provide_for_payments_that_contain_the_color()
        {
            Assert.IsTrue(ManaAbilityOutcome.OfColor(Color.Red).CanProvide(CreatePayment(Color.Red)));
            Assert.IsTrue(ManaAbilityOutcome.OfColor(Color.Red).CanProvide(CreatePayment(Color.White, Color.Red)));
            Assert.IsTrue(ManaAbilityOutcome.OfColor(Color.Red | Color.Blue).CanProvide(CreatePayment(Color.White, Color.Red)));
            Assert.IsFalse(ManaAbilityOutcome.OfColor(Color.Red | Color.Blue).CanProvide(CreatePayment(Color.White, Color.Black)));
            Assert.IsFalse(ManaAbilityOutcome.OfColor(Color.None).CanProvide(CreatePayment(Color.White, Color.Black)));
        }

        [Test]
        public void Test_OfColor_can_always_provide_for_payments_that_contain_colorless_mana()
        {
            Assert.IsTrue(ManaAbilityOutcome.OfColor(Color.Red).CanProvide(CreatePayment(Color.None)));
            Assert.IsTrue(ManaAbilityOutcome.OfColor(Color.Red).CanProvide(CreatePayment(Color.None, Color.Blue)));
            Assert.IsTrue(ManaAbilityOutcome.OfColor(Color.None).CanProvide(CreatePayment(Color.None, Color.Blue)));
        }

        #endregion
    }
}

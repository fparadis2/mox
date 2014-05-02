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
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox
{
    public abstract class CompositeConditionTests : BaseGameTests
    {
        #region Variables

        private Condition m_conditionA;
        private Condition m_conditionB;

        private Condition m_compositeCondition;

        #endregion

        #region Setup / Teardown

        public override void Setup()
        {
            base.Setup();

            m_conditionA = m_mockery.StrictMock<Condition>();
            m_conditionB = m_mockery.StrictMock<Condition>();

            m_compositeCondition = Compose(m_conditionA, m_conditionB);
        }

        #endregion

        #region Utilities

        private struct Combination
        {
            public bool A;
            public bool B;
        }

        protected abstract Condition Compose(Condition a, Condition b);

        protected abstract bool Compose(bool a, bool b);

        private static void Expect_Matches(Condition mockCondition, Card card, bool result)
        {
            Expect.Call(mockCondition.Matches(card)).Return(result).Repeat.Any();
        }

        private static void Expect_Invalidate(Condition mockCondition, PropertyBase property, bool result)
        {
            Expect.Call(mockCondition.Invalidate(property)).Return(result).Repeat.Any();
        }

        private static void Expect_ComputeHash(Condition mockCondition, Hash hash)
        {
            mockCondition.ComputeHash(hash);
        }

        private static IEnumerable<Combination> Combinations
        {
            get
            {
                yield return new Combination { A = true, B = true };
                yield return new Combination { A = true, B = false };
                yield return new Combination { A = false, B = true };
                yield return new Combination { A = false, B = false };
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_construction_values()
        {
            Assert.Throws<ArgumentNullException>(() => Compose(null, m_conditionB));
            Assert.Throws<ArgumentNullException>(() => Compose(m_conditionA, null));
        }

        [Test]
        public void Test_Matches()
        {
            foreach (var combination in Combinations)
            {
                Expect_Matches(m_conditionA, m_card, combination.A);
                Expect_Matches(m_conditionB, m_card, combination.B);

                bool expectedResult = Compose(combination.A, combination.B);

                m_mockery.Test(() => Assert.AreEqual(expectedResult, m_compositeCondition.Matches(m_card)));
            }
        }

        [Test]
        public void Test_Invalidate_always_composes_like_or()
        {
            foreach (var combination in Combinations)
            {
                Expect_Invalidate(m_conditionA, Card.ColorProperty, combination.A);
                Expect_Invalidate(m_conditionB, Card.ColorProperty, combination.B);

                bool expectedResult = combination.A || combination.B;

                m_mockery.Test(() => Assert.AreEqual(expectedResult, m_compositeCondition.Invalidate(Card.ColorProperty)));
            }
        }

        [Test]
        public void Test_ComputeHash_computes_the_hash_of_every_condition()
        {
            foreach (var combination in Combinations)
            {
                Hash hash = new Hash();

                Expect_ComputeHash(m_conditionA, hash);
                Expect_ComputeHash(m_conditionB, hash);

                m_mockery.Test(() => m_compositeCondition.ComputeHash(hash));
            }
        }

        #endregion
    }

    [TestFixture]
    public class AndConditionTests : CompositeConditionTests
    {
        protected override Condition Compose(Condition a, Condition b)
        {
            return a & b;
        }

        protected override bool Compose(bool a, bool b)
        {
            return a && b;
        }
    }
}
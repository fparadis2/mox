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
using Rhino.Mocks;

namespace Mox
{
    [TestFixture]
    public class RandomTests
    {
        #region Variables

        private MockRepository m_mockery;
        private IRandom m_random;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_random = m_mockery.StrictMock<IRandom>();
        }

        #endregion

        #region Utilities

        private void Expect_Next(int result, int max)
        {
            Expect.Call(m_random.Next(max)).Return(result);
        }

        private int[] Shuffle(int n)
        {
            int[] result = null;
            m_mockery.Test(() => result = m_random.Shuffle(n));
            return result;
        }

        private static void Assert_Have_same_seed(IRandom r1, IRandom r2)
        {
            Assert.AreEqual(r1.Next(10), r2.Next(10));
            Assert.AreEqual(r1.Next(), r2.Next());
            Assert.AreEqual(r1.Next(20), r2.Next(20));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_New_returns_a_new_random()
        {
            IRandom random = Random.New();
            Assert.IsNotNull(random);
            Assert.GreaterOrEqual(random.Next(10), 0);
            Assert.Less(random.Next(10), 10);
        }

        [Test]
        public void Test_New_with_a_seed_returns_a_seeded_random()
        {
            IRandom random1 = Random.New(5);
            IRandom random2 = Random.New(5);

            Assert_Have_same_seed(random1, random2);
        }

        [Test]
        public void Test_Shuffling_an_empty_collection_does_nothing()
        {
            int[] array = Shuffle(0);
            Assert.Collections.IsEmpty(array);
        }

        [Test]
        public void Test_Shuffle_returns_an_array_where_each_element_is_a_repositioning_index()
        {
            using (m_mockery.Ordered())
            {
                Expect_Next(1, 4);
                Expect_Next(2, 3);
                Expect_Next(1, 2);
            }

            int[] array = Shuffle(4);
            Assert.Collections.AreEqual(new[] { 0, 1, 2, 1 }, array);
        }

        [Test]
        public void Test_Shuffling_a_real_collection_doesnt_change_the_collection_elements_but_only_reorders_them()
        {
            const int TestLength = 1000;
            int[] array = Random.New().Shuffle(TestLength);

            Assert.AreEqual(TestLength, array.Length);

            array.ForEach(i =>
            {
                Assert.GreaterOrEqual(i, 0);
                Assert.Less(i, TestLength);
            });
        }

        [Test]
        public void Test_Shuffling_is_always_the_same_when_providing_a_seed()
        {
            const int TestLength = 100;
            const int Seed = 10;

            int[] first = Random.New(Seed).Shuffle(TestLength);
            int[] second = Random.New(Seed).Shuffle(TestLength);

            Assert.Collections.AreEqual(first, second);
        }

        [Test]
        public void Test_Choose_returns_one_of_the_elements_at_random()
        {
            string[] collection = new[] {"One", "Two", "Three", "Four"};

            Expect_Next(2, 4);

            m_mockery.Test(() => Assert.AreEqual("Three", m_random.Choose(collection)));
        }

        #endregion
    }
}

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
    public class FlagsTests
    {
        #region Inner Types

        [Flags]
        private enum MyFlags
        {
            None = 0,
            Value1 = 1,
            Value2 = 2,
            Value3 = 4
        }

        private enum NonFlags
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }

        #endregion

        #region Variables

        private Flags<MyFlags> m_flags;

        #endregion

        #region Tests

        [Test]
        public void Test_GetHashCode_returns_hash_code_of_value()
        {
            m_flags = MyFlags.Value1;
            Assert.AreEqual(MyFlags.Value1.GetHashCode(), m_flags.GetHashCode());
        }

        [Test]
        public void Test_Equals_works_with_flags()
        {
            Flags<MyFlags> flags1 = MyFlags.Value1;
            Flags<MyFlags> flags2 = MyFlags.Value1;
            Flags<MyFlags> flags3 = MyFlags.Value2 | MyFlags.Value3;
            Flags<MyFlags> flags4 = MyFlags.Value1 | MyFlags.Value3;

            Assert.AreCompletelyEqual(flags1, flags2);
            Assert.AreCompletelyNotEqual(flags1, flags3);
            Assert.AreCompletelyNotEqual(flags1, flags4);
        }

        [Test]
        public void Test_Can_convert_implicitely_to_and_from_Flags()
        {
            m_flags = MyFlags.Value2 | MyFlags.Value3;
            MyFlags result = m_flags;
            Assert.AreEqual(MyFlags.Value2 | MyFlags.Value3, result);
        }

        [Test]
        public void Test_Default_value_is_default_value_of_enum()
        {
            m_flags = new Flags<MyFlags>();
            Assert.AreEqual(MyFlags.None, (MyFlags)m_flags);
        }

        [Test]
        public void Test_Equality_operator_works_with_normal_flags()
        {
            m_flags = MyFlags.Value1;
            Assert.That(m_flags == MyFlags.Value1);
            Assert.That(MyFlags.Value1 == m_flags);

            Assert.That(m_flags != MyFlags.Value2);
            Assert.That(MyFlags.Value2 != m_flags);
        }

        [Test]
        public void Test_Cannot_use_with_non_flag_enum_types()
        {
            Assert.Throws<TypeInitializationException>(() => { Flags<NonFlags> a = NonFlags.Value1; });
            Assert.Throws<TypeInitializationException>(() => { Flags<int> a = 3; });
        }

        [Test]
        public void Test_Contains_returns_true_if_the_flags_contains_completely_the_given_value()
        {
            m_flags = MyFlags.Value1 | MyFlags.Value2;
            Assert.That(m_flags.Contains(MyFlags.Value1));
            Assert.That(m_flags.Contains(MyFlags.Value2));
            Assert.That(m_flags.Contains(MyFlags.Value1 | MyFlags.Value2));
            Assert.That(m_flags.Contains(MyFlags.None));

            Assert.That(!m_flags.Contains(MyFlags.Value3));
            Assert.That(!m_flags.Contains(MyFlags.Value1 | MyFlags.Value3));
        }

        [Test]
        public void Test_ContainsAny_returns_true_if_the_flags_contains_partially_the_given_value()
        {
            m_flags = MyFlags.Value1 | MyFlags.Value2;
            Assert.That(m_flags.ContainsAny(MyFlags.Value1));
            Assert.That(m_flags.ContainsAny(MyFlags.Value2));
            Assert.That(m_flags.ContainsAny(MyFlags.Value1 | MyFlags.Value2));
            Assert.That(m_flags.ContainsAny(MyFlags.Value1 | MyFlags.Value2 | MyFlags.Value3));
            Assert.That(m_flags.ContainsAny(MyFlags.Value1 | MyFlags.Value3));

            Assert.That(!m_flags.ContainsAny(MyFlags.Value3));
        }

        [Test]
        public void Test_Indexer_allows_to_get_a_specific_bit()
        {
            m_flags = MyFlags.Value1 | MyFlags.Value2;
            Assert.That(m_flags[MyFlags.Value1]);
            Assert.That(m_flags[MyFlags.Value2]);
            Assert.That(m_flags[MyFlags.Value1 | MyFlags.Value2]);
            Assert.That(m_flags[MyFlags.None]);

            Assert.That(!m_flags[MyFlags.Value3]);
            Assert.That(!m_flags[MyFlags.Value1 | MyFlags.Value3]);
        }

        [Test]
        public void Test_Indexer_allows_to_set_a_specific_bit()
        {
            m_flags = MyFlags.Value1 | MyFlags.Value2;
            
            m_flags[MyFlags.Value1] = true;
            Assert.That(m_flags.Equals(MyFlags.Value1 | MyFlags.Value2));

            m_flags[MyFlags.Value1] = false;
            Assert.That(m_flags.Equals(MyFlags.Value2));

            m_flags[MyFlags.None] = true;
            Assert.That(m_flags.Equals(MyFlags.Value2));

            m_flags[MyFlags.Value3] = true;
            Assert.That(m_flags.Equals(MyFlags.Value2 | MyFlags.Value3));

            m_flags[MyFlags.Value1 | MyFlags.Value2] = false;
            Assert.That(m_flags.Equals(MyFlags.Value3));

            m_flags[MyFlags.Value1 | MyFlags.Value2] = true;
            Assert.That(m_flags.Equals(MyFlags.Value1 | MyFlags.Value2 | MyFlags.Value3));
        }

        #endregion
    }
}

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

namespace Mox.AI
{
    [TestFixture]
    public class ChoiceEnumeratorAttributeTests
    {
        #region Variables

        private ChoiceEnumeratorAttribute m_attribute;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_attribute = new ChoiceEnumeratorAttribute(typeof(ChoiceEnumeratorAttributeTests));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(delegate { new ChoiceResolverAttribute(null); });
        }

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.AreEqual(typeof(ChoiceEnumeratorAttributeTests), m_attribute.Type);
        }

        #endregion
    }
}

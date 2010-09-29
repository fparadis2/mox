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
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class TypeExtensionsTests
    {
        #region Tests

        #region GetDominantType

        [Test]
        public void Test_GetDominantType_returns_the_type_directly_if_alone()
        {
            foreach (Type type in Enum.GetValues(typeof(Type)))
            {
                Assert.AreEqual(type, type.GetDominantType());
            }
        }

        [Test]
        public void Test_GetDominantType_returns_the_most_significant_type()
        {
            List<Type> expectedTypes = new List<Type>
            {
                Type.Creature,
                Type.Land,
                Type.Instant,
                Type.Sorcery,
                Type.Enchantment,
                Type.Planeswalker,
                Type.Tribal,
                Type.Artifact,
                Type.Scheme
            };

            while (expectedTypes.Count > 0)
            {
                Type combinedType = expectedTypes.Aggregate(Type.None, (a, b) => a | b);
                Assert.AreEqual(expectedTypes.First(), combinedType.GetDominantType());
                expectedTypes.RemoveAt(0);
            }
        }

        #endregion

        #endregion
    }
}

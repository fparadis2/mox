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
    public class SubTypesTests
    {
        #region Tests

        [Test]
        public void Test_Empty_returns_an_empty_subtypes()
        {
            var subTypes = SubTypes.Empty;
            Assert.IsFalse(subTypes.Is(SubType.Angel));
            Assert.IsFalse(subTypes.Is(SubType.Mountain));
            Assert.IsFalse(subTypes.Is(SubType.Swamp));
        }

        [Test]
        public void Test_The_or_operator_adds_types_to_the_sub_types()
        {
            var subTypes = SubTypes.Empty;
            subTypes |= SubType.Angel;
            subTypes |= SubType.Mountain;
            subTypes |= SubType.Mountain;

            Assert.IsTrue(subTypes.Is(SubType.Angel));
            Assert.IsTrue(subTypes.Is(SubType.Mountain));
            Assert.IsFalse(subTypes.Is(SubType.Swamp));
        }

        [Test]
        public void Test_Is_returns_true_if_the_sub_types_implements_all_the_types()
        {
            var subTypes = SubTypes.Empty;
            subTypes |= SubType.Angel;
            subTypes |= SubType.Mountain;

            Assert.IsTrue(subTypes.Is(SubType.Mountain));
            Assert.IsFalse(subTypes.Is(SubType.Swamp));
        }

        [Test]
        public void Test_IsAll_returns_true_if_the_sub_types_implements_all_the_types()
        {
            var subTypes = SubTypes.Empty;
            subTypes |= SubType.Angel;
            subTypes |= SubType.Mountain;

            Assert.IsTrue(subTypes.IsAll(SubType.Angel, SubType.Mountain));
            Assert.IsFalse(subTypes.IsAll(SubType.Angel, SubType.Swamp));
        }

        [Test]
        public void Test_IsAny_returns_true_if_the_sub_types_implements_any_of_the_types()
        {
            var subTypes = SubTypes.Empty;
            subTypes |= SubType.Angel;
            subTypes |= SubType.Mountain;

            Assert.IsTrue(subTypes.IsAny(SubType.Angel, SubType.Mountain));
            Assert.IsTrue(subTypes.IsAny(SubType.Angel, SubType.Swamp));
            Assert.IsFalse(subTypes.IsAny(SubType.Soldier, SubType.Swamp));
        }

        [Test]
        public void Test_SubTypes_are_serializable()
        {
            var subTypes = SubTypes.Empty;
            subTypes |= SubType.Angel;
            subTypes |= SubType.Mountain;

            subTypes = Assert.IsSerializable(subTypes);

            Assert.Collections.AreEquivalent(new[] {SubType.Angel, SubType.Mountain}, subTypes.ToList());
        }

        [Test]
        public void Test_Can_construct_from_a_list()
        {
            var subTypes = new SubTypes(SubType.Angel, SubType.Mountain);
            Assert.Collections.AreEquivalent(new[] { SubType.Angel, SubType.Mountain }, subTypes.ToList());
        }

        [Test]
        public void Test_Fully_implements_equality()
        {
            var subTypes1 = new SubTypes(SubType.Angel, SubType.Mountain);
            var subTypes2 = new SubTypes(SubType.Mountain, SubType.Angel);
            var subTypes3 = new SubTypes(SubType.Mountain, SubType.Antelope);

            Assert.AreCompletelyEqual(subTypes1, subTypes2);
            Assert.AreCompletelyNotEqual(subTypes1, subTypes3);
            Assert.AreCompletelyNotEqual(subTypes2, subTypes3);
        }

        [Test]
        public void Test_ComputeHash()
        {
            Assert.HashIsEqual(new SubTypes(), new SubTypes());
            Assert.HashIsEqual(new SubTypes(SubType.Angel), new SubTypes(SubType.Angel));
            Assert.HashIsEqual(new SubTypes(SubType.Angel, SubType.Tower), new SubTypes(SubType.Angel, SubType.Tower));

            Assert.HashIsNotEqual(new SubTypes(), new SubTypes(SubType.Angel, SubType.Tower));
            Assert.HashIsNotEqual(new SubTypes(SubType.Angel), new SubTypes(SubType.Angel, SubType.Tower));
            Assert.HashIsNotEqual(new SubTypes(SubType.Angel, SubType.PowerPlant), new SubTypes(SubType.Angel, SubType.Tower));
        }

        #endregion
    }
}

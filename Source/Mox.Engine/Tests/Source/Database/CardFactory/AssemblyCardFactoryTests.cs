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

namespace Mox.Database
{
    [TestFixture]
    public class AssemblyCardFactoryTests
    {
        #region Inner Types

        private class NormalType
        {
        }

        [CardFactory("My Card")]
        private class MyCardFactory : CardFactory
        {
        }

        [CardFactory("My Card")]
        [CardFactory("My Card2")]
        private class MultipleCardFactory : CardFactory
        {
        }

        [CardFactory("My Card")]
        private class InvalidConstructorFactory : CardFactory
        {
            public InvalidConstructorFactory(int i)
            {
            }
        }

        [CardFactory("")]
        private class EmptyCardNameFactory : CardFactory
        {
        }

        #endregion

        #region Utilities

        private AssemblyCardFactory Create(params System.Type[] types)
        {
            return new AssemblyCardFactory(types);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Types_without_the_CardFactoryAttribute_are_ignored()
        {
            AssemblyCardFactory factory = Create(typeof(NormalType));
            Assert.AreEqual(0, factory.Count);
        }

        [Test]
        public void Test_Simple_type()
        {
            AssemblyCardFactory factory = Create(typeof(MyCardFactory));
            Assert.AreEqual(1, factory.Count);
            Assert.IsTrue(factory.IsDefined("My Card"));
        }

        [Test]
        public void Test_Type_can_have_multiple_attributes()
        {
            AssemblyCardFactory factory = Create(typeof(MultipleCardFactory));
            Assert.AreEqual(2, factory.Count);
            Assert.IsTrue(factory.IsDefined("My Card"));
            Assert.IsTrue(factory.IsDefined("My Card2"));
        }

        [Test]
        public void Test_Factories_must_be_instantiable()
        {
            var factory = Create(typeof(InvalidConstructorFactory));
            Assert.Throws<MissingMethodException>(() => factory.CreateFactory("My Card", null));
        }

        [Test]
        public void Test_Attribute_CardName_cannot_be_empty()
        {
            Assert.Throws<ArgumentNullException>(() => Create(typeof(EmptyCardNameFactory)));
        }

        #endregion
    }
}

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
    public class AssemblyCardFactoryTests
    {
        #region Inner Types

        private class NormalType
        {
        }

        [CardFactory("My Card")]
        private class MyCardFactory : ICardFactory
        {
            #region Implementation of ICardFactory

            public void InitializeCard(Card card)
            {
            }

            #endregion
        }

        [CardFactory("My Card")]
        [CardFactory("My Card2")]
        private class MultipleCardFactory : ICardFactory
        {
            #region Implementation of ICardFactory

            public void InitializeCard(Card card)
            {
            }

            #endregion
        }

        [CardFactory("My Card")]
        private class InvalidCardFactory
        {
        }

        [CardFactory("My Card")]
        private class InvalidConstructorFactory : ICardFactory
        {
            public InvalidConstructorFactory(int i)
            {
            }

            #region Implementation of ICardFactory

            public void InitializeCard(Card card)
            {
                throw new System.NotImplementedException();
            }

            #endregion
        }

        [CardFactory("")]
        private class EmptyCardNameFactory : ICardFactory
        {
            #region Implementation of ICardFactory

            public void InitializeCard(Card card)
            {
            }

            #endregion
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
        public void Test_Factories_must_implement_ICardFactory()
        {
            Assert.Throws<InvalidCastException>(() => Create(typeof(InvalidCardFactory)));
        }

        [Test]
        public void Test_Factories_must_be_instantiable()
        {
            Assert.Throws<MissingMethodException>(() => Create(typeof(InvalidConstructorFactory)));
        }

        [Test]
        public void Test_Attribute_CardName_cannot_be_empty()
        {
            Assert.Throws<ArgumentNullException>(() => Create(typeof(EmptyCardNameFactory)));
        }

        #endregion
    }
}

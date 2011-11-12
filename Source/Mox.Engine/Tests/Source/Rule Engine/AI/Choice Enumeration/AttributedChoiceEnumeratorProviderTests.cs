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

using Mox.Flow;
using NUnit.Framework;

namespace Mox.AI
{
    [TestFixture]
    public class AttributedChoiceEnumeratorProviderTests : BaseGameTests
    {
        #region Inner Types

        private class MyEnumerator : ChoiceEnumerator
        {
            #region Implementation of ChoiceEnumerator

            /// <summary>
            /// Returns the possible choices for choice context.
            /// </summary>
            public override IEnumerable<object> EnumerateChoices(Game game, Choice choice)
            {
                throw new System.NotImplementedException();
            }

            #endregion
        }

        #endregion

        #region Variables

        private AttributedChoiceEnumeratorProvider m_provider;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            m_provider = new AttributedChoiceEnumeratorProvider(new AIParameters());
        }

        #endregion

        #region Utilities

        private ChoiceEnumerator GetEnumerator(Choice choice)
        {
            return m_provider.GetEnumerator(choice);
        }

        #endregion

        #region Test Choices

        private class MockChoice : Choice
        {
            public MockChoice(Player player)
                : base(player)
            {
            }
            
            public override object DefaultValue
            {
                get { throw new NotImplementedException(); }
            }
        }

        [ChoiceEnumerator(typeof(MyEnumerator))]
        private class ValidChoice : MockChoice
        {
            public ValidChoice(Player player)
                : base(player)
            {
            }
        }

        [ChoiceEnumerator(typeof(AttributedChoiceEnumeratorProviderTests))]
        private class InvalidChoice : MockChoice
        {
            public InvalidChoice(Player player)
                : base(player)
            {
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_Invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new AttributedChoiceEnumeratorProvider(null));
        }

        [Test]
        public void Test_GetEnumerator_Invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => GetEnumerator(null));
            Assert.Throws<ArgumentException>(() => GetEnumerator(new MockChoice(m_playerA)));
            Assert.Throws<ArgumentException>(() => GetEnumerator(new InvalidChoice(m_playerA)));
        }

        [Test]
        public void Test_GetResolver_uses_attribute_to_find_type()
        {
            Assert.IsInstanceOf<MyEnumerator>(GetEnumerator(new ValidChoice(m_playerA)));
            Assert.AreSame(GetEnumerator(new ValidChoice(m_playerA)), GetEnumerator(new ValidChoice(m_playerA)));
        }

        #endregion
    }
}

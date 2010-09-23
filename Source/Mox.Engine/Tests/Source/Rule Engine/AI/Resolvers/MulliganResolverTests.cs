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
using System.Text;

using NUnit.Framework;

namespace Mox.AI.Resolvers
{
    [TestFixture]
    public class MulliganResolverTests : BaseMTGChoiceResolverTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        internal override BaseMTGChoiceResolver CreateResolver()
        {
            return new MulliganResolver();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Returns_only_false_for_now()
        {
            Assert.Collections.AreEqual(new[] { false }, m_choiceResolver.ResolveChoices(GetMethod(), null));
        }

        [Test]
        public void Test_Default_choice_is_to_not_mulligan()
        {
            Assert.IsFalse((bool)m_choiceResolver.GetDefaultChoice(GetMethod(), null));
        }

        #endregion
    }
}

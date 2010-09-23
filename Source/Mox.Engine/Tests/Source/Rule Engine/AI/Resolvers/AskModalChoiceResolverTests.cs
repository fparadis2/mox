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
    public class AskModalChoiceResolverTests : BaseMTGChoiceResolverTests
    {
        #region Variables

        #endregion

        #region Setup / Teardown

        internal override BaseMTGChoiceResolver CreateResolver()
        {
            return new AskModalChoiceResolver();
        }

        #endregion

        #region Utilities

        private IEnumerable<ModalChoiceResult> ResolveChoices(ModalChoiceContext context)
        {
            return m_choiceResolver.ResolveChoices(GetMethod(), new object[] { null, null, context }).Cast<ModalChoiceResult>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Returns_the_default_choice_only_if_trivial_choice()
        {
            ModalChoiceContext context = new ModalChoiceContext
            {
                DefaultChoice = ModalChoiceResult.No,
                Importance = ModalChoiceImportance.Trivial
            };

            context.Choices.Add(ModalChoiceResult.No);
            context.Choices.Add(ModalChoiceResult.Yes);

            Assert.Collections.AreEqual(new[] { ModalChoiceResult.No }, ResolveChoices(context));
        }

        [Test]
        public void Test_Returns_all_choices_if_not_trivial_choice()
        {
            ModalChoiceContext context = new ModalChoiceContext
            {
                DefaultChoice = ModalChoiceResult.No,
                Importance = ModalChoiceImportance.Important
            };

            context.Choices.Add(ModalChoiceResult.No);
            context.Choices.Add(ModalChoiceResult.Yes);

            Assert.Collections.AreEqual(new[] { ModalChoiceResult.No, ModalChoiceResult.Yes }, ResolveChoices(context));
        }

        [Test]
        public void Test_Default_choice_is_context_default_choice()
        {
            ModalChoiceContext context = new ModalChoiceContext
            {
                DefaultChoice = ModalChoiceResult.No
            };

            Assert.AreEqual(ModalChoiceResult.No, (ModalChoiceResult)m_choiceResolver.GetDefaultChoice(GetMethod(), new object[] { null, null, context }));
        }

        #endregion
    }
}

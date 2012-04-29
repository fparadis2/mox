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
using System.Linq;
using Mox.Flow;
using NUnit.Framework;

namespace Mox.UI.Game
{
    [TestFixture]
    public class InteractionControllerMulliganTests : InteractionControllerTester
    {
        #region Tests

        [Test]
        public void Test_BeginMulligan_returns_true_if_selecting_the_yes_choice()
        {
            InteractionController.BeginInteraction(new MulliganChoice(EmptyPlayer));
            {
                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(2, Model.Interaction.UserChoiceInteraction.Choices.Count);
                UserChoiceModel yesChoice = Model.Interaction.UserChoiceInteraction.Choices.First(choice => string.Equals(choice.Text, "yes", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(yesChoice);
                Model.Interaction.SelectChoice(yesChoice);
            }
            Assert.IsTrue(IsCompleted);
            Assert.IsTrue((bool)Result);
        }

        [Test]
        public void Test_BeginMulligan_returns_false_if_selecting_the_no_choice()
        {
            InteractionController.BeginInteraction(new MulliganChoice(EmptyPlayer));
            {
                Assert.IsNotNull(Model.Interaction.UserChoiceInteraction);
                Assert.AreEqual(2, Model.Interaction.UserChoiceInteraction.Choices.Count);
                UserChoiceModel noChoice = Model.Interaction.UserChoiceInteraction.Choices.First(choice => string.Equals(choice.Text, "no", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(noChoice);
                Model.Interaction.SelectChoice(noChoice);
            }
            Assert.IsTrue(IsCompleted);
            Assert.IsFalse((bool)Result);
        }

        #endregion
    }
}

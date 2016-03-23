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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mox.UI.Game
{
    partial class InteractionController
    {
        #region Inner Types

        private class MulliganInteraction : Interaction
        {
            public override void Run()
            {
                base.Run();

                Model.Interaction.UserChoiceSelected += Interaction_UserChoiceSelected;
                Model.Interaction.UserChoiceInteraction = UserChoiceInteractionModel.YesNo("Would you like to mulligan?");
            }

            protected override void End(object result)
            {
                Model.Interaction.UserChoiceSelected -= Interaction_UserChoiceSelected;

                base.End(result);
            }

            private void Interaction_UserChoiceSelected(object sender, ItemEventArgs<UserChoiceModel> e)
            {
                End(e.Item.Type == UserChoiceType.Yes);
            }
        }

        #endregion
    }
}

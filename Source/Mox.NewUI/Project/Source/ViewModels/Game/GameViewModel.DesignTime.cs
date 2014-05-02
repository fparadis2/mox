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

namespace Mox.UI.Game
{
    public class GameViewModel_DesignTime : GameViewModel_DesignTime_Empty
    {
        #region Constructor

        public GameViewModel_DesignTime()
        {
            MainPlayer = new PlayerViewModel_DesignTime(this);
            State.CurrentStep = Steps.DeclareBlockers;
            State.ActivePlayer = MainPlayer;

            Players.Add(MainPlayer);
            Players.Add(new PlayerViewModel_DesignTime(this));

            Interaction.UserChoiceInteraction = new UserChoiceInteractionModel_DesignTime();
        }

        #endregion
    }

    public class GameViewModel_DesignTime_Empty : GameViewModel
    {
        #region Constructor

        public GameViewModel_DesignTime_Empty()
        {
            Source = new Mox.Game();
        }

        #endregion
    }
}

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
            Initialize(this);
        }

        #endregion

        public static void Initialize(GameViewModel game)
        {
            if (game.Source == null)
                game.Source = new Mox.Game();

            game.MainPlayer = new PlayerViewModel_DesignTime(game);
            game.State.CurrentStep = Steps.DeclareBlockers;
            game.State.ActivePlayer = game.MainPlayer;

            game.Players.Add(game.MainPlayer);
            game.Players.Add(new PlayerViewModel_DesignTime(game));

            game.Interaction.UserChoiceInteraction = new UserChoiceInteractionModel_DesignTime();
        }
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

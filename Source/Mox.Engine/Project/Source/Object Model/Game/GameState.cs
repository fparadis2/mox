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

namespace Mox
{
    /// <summary>
    /// Contains various information about the game state.
    /// </summary>
    public class GameState : GameObject
    {
        #region Variables

        public static readonly Property<Player> WinnerProperty = Property<Player>.RegisterProperty("Winner", typeof(GameState));
        public static readonly Property<Player> ActivePlayerProperty = Property<Player>.RegisterProperty("ActivePlayer", typeof(GameState));
        public static readonly Property<Steps> CurrentStepProperty = Property<Steps>.RegisterProperty("CurrentStep", typeof(GameState));
        public static readonly Property<Phases> CurrentPhaseProperty = Property<Phases>.RegisterProperty("CurrentPhase", typeof(GameState));
        public static readonly Property<int> CurrentTurnProperty = Property<int>.RegisterProperty("CurrentTurn", typeof(GameState));
        
        #endregion

        #region Properties

        /// <summary>
        /// Winner of the game, if any.
        /// </summary>
        public Player Winner
        {
            get { return GetValue(WinnerProperty); }
            set { SetValue(WinnerProperty, value); }
        }

        /// <summary>
        /// Returns true if the game has ended.
        /// </summary>
        public bool HasEnded
        {
            get { return Winner != null; }
        }

        /// <summary>
        /// Active player (player whose turn it is).
        /// </summary>
        public Player ActivePlayer
        {
            get { return GetValue(ActivePlayerProperty); }
            set { SetValue(ActivePlayerProperty, value); }
        }

        /// <summary>
        /// Current step of the game.
        /// </summary>
        public Steps CurrentStep
        {
            get { return GetValue(CurrentStepProperty); }
            set { SetValue(CurrentStepProperty, value); }
        }

        /// <summary>
        /// Current phase of the game.
        /// </summary>
        public Phases CurrentPhase
        {
            get { return GetValue(CurrentPhaseProperty); }
            set { SetValue(CurrentPhaseProperty, value); }
        }

        /// <summary>
        /// Current turn number.
        /// </summary>
        public int CurrentTurn
        {
            get { return GetValue(CurrentTurnProperty); }
            set { SetValue(CurrentTurnProperty, value); }
        }

        #endregion
    }
}

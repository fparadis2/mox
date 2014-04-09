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

namespace Mox
{
    /// <summary>
    /// Contains various information about the game state.
    /// </summary>
    public class GameState : GameObject
    {
        #region Variables

        private Player m_winner;
        public static readonly Property<Player> WinnerProperty = Property<Player>.RegisterProperty<GameState>("Winner", g => g.m_winner);

        private Player m_activePlayer;
        public static readonly Property<Player> ActivePlayerProperty = Property<Player>.RegisterProperty<GameState>("ActivePlayer", g => g.m_activePlayer);

        private Steps m_currentStep;
        public static readonly Property<Steps> CurrentStepProperty = Property<Steps>.RegisterProperty<GameState>("CurrentStep", g => g.m_currentStep);

        private Phases m_currentPhase;
        public static readonly Property<Phases> CurrentPhaseProperty = Property<Phases>.RegisterProperty<GameState>("CurrentPhase", g => g.m_currentPhase);

        private int m_currentTurn;
        public static readonly Property<int> CurrentTurnProperty = Property<int>.RegisterProperty<GameState>("CurrentTurn", g => g.m_currentTurn);

        private int m_transactionCount;
        private static readonly Property<int> TransactionCountProperty = Property<int>.RegisterProperty<GameState>("TransactionCount", g => g.m_transactionCount, PropertyFlags.Private);
        
        #endregion

        #region Properties

        /// <summary>
        /// Winner of the game, if any.
        /// </summary>
        public Player Winner
        {
            get { return m_winner; }
            set { SetValue(WinnerProperty, value, ref m_winner); }
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
            get { return m_activePlayer; }
            set { SetValue(ActivePlayerProperty, value, ref m_activePlayer); }
        }

        /// <summary>
        /// Current step of the game.
        /// </summary>
        public Steps CurrentStep
        {
            get { return m_currentStep; }
            set { SetValue(CurrentStepProperty, value, ref m_currentStep); }
        }

        /// <summary>
        /// Current phase of the game.
        /// </summary>
        public Phases CurrentPhase
        {
            get { return m_currentPhase; }
            set { SetValue(CurrentPhaseProperty, value, ref m_currentPhase); }
        }

        /// <summary>
        /// Current turn number.
        /// </summary>
        public int CurrentTurn
        {
            get { return m_currentTurn; }
            set { SetValue(CurrentTurnProperty, value, ref m_currentTurn); }
        }

        internal int TransactionCount
        {
            get { return m_transactionCount; }
            set { SetValue(TransactionCountProperty, value, ref m_transactionCount); }
        }

        #endregion
    }
}

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

namespace Mox.AI
{
    /// <summary>
    /// Minmax AI parameters
    /// </summary>
    public class AIParameters
    {
        #region Variables

        private int m_minimumTreeDepth;
        private int m_maximumSpellStackDepth;

        #endregion

        #region Constructor

        public AIParameters()
        {
            MinimumTreeDepth = 8;
            MaximumSpellStackDepth = 1;

            DriverType = MinMaxDriverType.Iterative;
            TreeType = MinMaxTreeType.NegaMaxWithTranspositionTable;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The minimum tree depth the minmax algorithm will try to reach before evaluating the game state.
        /// </summary>
        public int MinimumTreeDepth
        {
            get { return m_minimumTreeDepth; }
            set 
            {
                Throw.ArgumentOutOfRangeIf(value <= 0, "Minimum tree depth must be at least 1", "MinimumTreeDepth");
                m_minimumTreeDepth = value; 
            }
        }

        /// <summary>
        /// The maximum number of spells in the spell stack before the AI stops trying to play
        /// </summary>
        public int MaximumSpellStackDepth
        {
            get { return m_maximumSpellStackDepth; }
            set 
            {
                Throw.ArgumentOutOfRangeIf(value < 0, "Maximum spell stack depth must be at least 0", "MinimumTreeDepth");
                m_maximumSpellStackDepth = value; 
            }
        }

        /// <summary>
        /// Driver Type.
        /// </summary>
        public MinMaxDriverType DriverType { get; set; }

        public MinMaxTreeType TreeType { get; set; }

        /// <summary>
        /// The maximum time AI can take to make a single choice.
        /// </summary>
        /// <remarks>
        /// 0 means no timeout.
        /// </remarks>
        public TimeSpan GlobalAITimeout { get; set; }

        #endregion

        #region Inner Types

        public enum MinMaxDriverType
        {
            Iterative,
            Recursive
        }

        public enum MinMaxTreeType
        {
            OldNegaMax,
            NegaMaxWithTranspositionTable
        }

        #endregion
    }
}

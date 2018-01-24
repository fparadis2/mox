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

namespace Mox.Flow
{
    /// <summary>
    /// The different situations in which a controller can be asked to target something.
    /// </summary>
    public enum TargetContextType
    {
        /// <summary>
        /// Normal targetting, through a spell.
        /// </summary>
        Normal,
        /// <summary>
        /// Controller is asked to discard a card, usually during the cleanup phase (but not always).
        /// </summary>
        Discard
    }

    /// <summary>
    /// Target context.
    /// </summary>
    [Serializable]
    public class TargetContext
    {
        #region Variables

        private readonly bool m_allowCancel;
        private readonly int[] m_targets;
        private readonly TargetContextType m_type;

        #endregion

        #region Constructor

        public TargetContext(bool allowCancel, int[] targets, TargetContextType type)
        {
            Throw.IfNull(targets, "targets");

            m_allowCancel = allowCancel;
            m_targets = targets;
            m_type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Whether the player is allowed to cancel.
        /// </summary>
        public bool AllowCancel
        {
            get { return m_allowCancel; }
        }

        /// <summary>
        /// The possible targets that the user can choose.
        /// </summary>
        public IList<int> Targets
        {
            get { return m_targets; }
        }

        public TargetContextType Type
        {
            get { return m_type; }
        }

        #endregion

        #region Methods

        public bool IsValid(TargetResult target)
        {
            return m_targets.Contains(target.Identifier);
        }

        #endregion
    }
}

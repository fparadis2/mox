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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Mox.Abilities;

namespace Mox
{
    /// <summary>
    /// A card in the game.
    /// </summary>
    partial class Card
    {
        #region Variables

        private readonly List<Ability> m_abilities = new List<Ability>();

        #endregion

        #region Properties

        /// <summary>
        /// Abilities of this card.
        /// </summary>
        public IEnumerable<Ability> Abilities
        {
            get { return m_abilities; }
        }

        internal IList<Ability> InternalAbilities
        {
            get { return m_abilities; }
        }

        #endregion
    }
}

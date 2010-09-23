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
    /// Card super types.
    /// </summary>
    [Flags]
    public enum SuperType
    {
        /// <summary>
        /// No super type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Basic.
        /// </summary>
        Basic = 1,

        /// <summary>
        /// Legendary.
        /// </summary>
        Legendary = 2,

        /// <summary>
        /// World.
        /// </summary>
        World = 4,

        /// <summary>
        /// Snow.
        /// </summary>
        Snow = 8
    }
}

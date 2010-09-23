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
    /// Card types.
    /// </summary>
    [Flags]
    public enum Type
    {
        /// <summary>
        /// No type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Artifact.
        /// </summary>
        Artifact = 1,

        /// <summary>
        /// Creature.
        /// </summary>
        Creature = 2,

        /// <summary>
        /// Enchantment.
        /// </summary>
        Enchantment = 4,

        /// <summary>
        /// Instant.
        /// </summary>
        Instant = 8,
        
        /// <summary>
        /// Land.
        /// </summary>
        Land = 16,

        /// <summary>
        /// Planeswalker.
        /// </summary>
        Planeswalker = 32,

        /// <summary>
        /// Sorcery.
        /// </summary>
        Sorcery = 64,

        /// <summary>
        /// Tribal.
        /// </summary>
        Tribal = 128,

        /// <summary>
        /// Scheme.
        /// </summary>
        Scheme = 256
    }
}

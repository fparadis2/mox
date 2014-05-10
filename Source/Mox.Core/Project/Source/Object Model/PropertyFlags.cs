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
    /// Options on properties.
    /// </summary>
    [Flags]
    public enum PropertyFlags
    {
        /// <summary>
        /// Default.
        /// </summary>
        None = 0,
        /// <summary>
        /// Property is private (i.e. only visible to owner)
        /// </summary>
        Private = 1,
        /// <summary>
        /// Property value can be affected by effects
        /// </summary>
        Modifiable = 2,
        /// <summary>
        /// Property doesn't contribute to object hash
        /// </summary>
        IgnoreHash = 4,
    }
}

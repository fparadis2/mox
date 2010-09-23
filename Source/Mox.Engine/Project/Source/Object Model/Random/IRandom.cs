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
    public interface IRandom
    {
        #region Methods

        /// <summary>
        /// Returns a non-negative random value.
        /// </summary>
        int Next();

        /// <summary>
        /// Returns a non-negative random value less than the specified maximum (exclusive).
        /// </summary>
        int Next(int max);

        /// <summary>
        /// Returns a random value between the given min (inclusive) and max (exclusive).
        /// </summary>
        int Next(int min, int max);

        #endregion
    }
}

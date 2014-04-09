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

namespace Mox.Rules
{
    /// <summary>
    /// Implements the "one land per turn" rule
    /// </summary>
    internal static class OneLandPerTurn
    {
        private static readonly Scope m_bypassScope = new Scope();

        public static bool IsBypassed
        {
            get { return m_bypassScope.InScope; }
        }

        /// <summary>
        /// Set this to true to ignore the "one land per turn" rule. Use only in tests.
        /// </summary>
        public static IDisposable Bypass()
        {
            return m_bypassScope.Begin();
        }
    }
}

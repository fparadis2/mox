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
    internal enum TurnDebuggingLevel
    {
        None,
        Light,
        Verbose
    }

    /// <summary>
    /// Contains various constants to modify behavior at compile-time.
    /// </summary>
    internal static class Configuration
    {
        #region AI

        /// <summary>
        /// Whether the AI uses multiple threads
        /// </summary>
        /// <remarks>
        /// Default: true
        /// </remarks> 
        public const bool AI_Multithreaded = false;

        #endregion

        #region Debugging

        /// <summary>
        /// Whether to output debug info on each turn
        /// </summary>
        /// <remarks>
        /// Default: None
        /// </remarks>
        public const TurnDebuggingLevel Debug_Turns = TurnDebuggingLevel.Light;

        /// <summary>
        /// Whether to output debug info when AI makes a decision
        /// </summary>
        /// <remarks>
        /// Default: false
        /// </remarks>
        public const bool Debug_AI_choices = Debug_Minimax_tree || true;

        /// <summary>
        /// Whether to output debug info during minimax evaluation
        /// </summary>
        /// <remarks>
        /// Default: false
        /// </remarks>
        public const bool Debug_Minimax_tree = true;

        /// <summary>
        /// Validates that all driver types produce the same minmax tree.
        /// </summary>
        /// <remarks>
        /// Ignored if <see cref="Debug_Minimax_tree"/> is false.
        /// Default: false
        /// </remarks>
        public const bool Validate_Minimax_drivers = false;

        #endregion
    }
}

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
    /// Log Importance.
    /// </summary>
    public enum LogImportance
    {
        /// <summary>
        /// Error message. Always displayed.
        /// </summary>
        Error,
        /// <summary>
        /// Warning message. Always displayed.
        /// </summary>
        Warning,
        /// <summary>
        /// High-importance message. Should be displayed for most verbosities (expect silent).
        /// </summary>
        High,
        /// <summary>
        /// Normal-importance message. Should be displayed at normal verbosity.
        /// </summary>
        Normal,
        /// <summary>
        /// Low-importance message. Should be displayed only when verbose.
        /// </summary>
        Low,
        /// <summary>
        /// Debug message. Should be displayed only in debug or at diagnostics verbosity.
        /// </summary>
        Debug
    }
}

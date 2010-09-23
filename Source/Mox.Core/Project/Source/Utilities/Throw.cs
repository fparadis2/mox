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
using System.Diagnostics;

namespace Mox
{
    /// <summary>
    /// Utility class to validate arguments and throw exceptions.
    /// </summary>
    public static class Throw
    {
        #region Methods

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given <paramref name="obj"/> is null.
        /// </summary>
        [DebuggerStepThrough]
        public static void IfNull(object obj, string argumentName)
        {
            if (ReferenceEquals(obj, null))
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given <paramref name="string"/> is null or empty.
        /// </summary>
        [DebuggerStepThrough]
        public static void IfEmpty(string @string, string argumentName)
        {
            if (string.IsNullOrEmpty(@string))
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the given <paramref name="condition"/> is met.
        /// </summary>
        [DebuggerStepThrough]
        public static void InvalidArgumentIf(bool condition, string message, string paramName)
        {
            if (condition)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the given <paramref name="condition"/> is met.
        /// </summary>
        [DebuggerStepThrough]
        public static void InvalidOperationIf(bool condition, string message)
        {
            if (condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Throws an <see cref="InvalidProgramException"/> if the given <paramref name="condition"/> is met.
        /// </summary>
        [DebuggerStepThrough]
        public static void InvalidProgramIf(bool condition, string message)
        {
            if (condition)
            {
                throw new InvalidProgramException(message);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if the given <paramref name="condition"/> is met.
        /// </summary>
        [DebuggerStepThrough]
        public static void ArgumentOutOfRangeIf(bool condition, string message, string paramName)
        {
            if (condition)
            {
                throw new ArgumentOutOfRangeException(paramName, message);
            }
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the given <paramref name="condition"/> is met.
        /// </summary>
        [DebuggerStepThrough]
        public static void DisposedIf(bool condition, string objectName)
        {
            if (condition)
            {
                throw new ObjectDisposedException(objectName);
            }
        }

        #endregion
    }
}

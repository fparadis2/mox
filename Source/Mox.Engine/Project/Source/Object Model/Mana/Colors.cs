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
    /// The possible colors.
    /// </summary>
    [Flags]
    public enum Color
    {
        /// <summary>
        /// Colorless.
        /// </summary>
        None = 0,
        /// <summary>
        /// White (W).
        /// </summary>
        White = 1,
        /// <summary>
        /// Blue (U).
        /// </summary>
        Blue = 2,
        /// <summary>
        /// Black (B).
        /// </summary>
        Black = 4,
        /// <summary>
        /// Red (R).
        /// </summary>
        Red = 8,
        /// <summary>
        /// Green (G).
        /// </summary>
        Green = 16
    }

    public static class ColorExtensions
    {
        public static readonly Color AllColors = Color.White | Color.Blue | Color.Black | Color.Red | Color.Green;

        public static bool HasMoreThanOneColor(this Color color)
        {
            switch (color)
            {
                case Color.None:
                case Color.White:
                case Color.Blue:
                case Color.Black:
                case Color.Red:
                case Color.Green:
                    return false;
                default:
                    return true;
            }
        }

        public static int CountColors(this Color color)
        {
            int count = 0;

            if (color.HasFlag(Color.White))
                count += 1;

            if (color.HasFlag(Color.Blue))
                count += 1;

            if (color.HasFlag(Color.Black))
                count += 1;

            if (color.HasFlag(Color.Red))
                count += 1;

            if (color.HasFlag(Color.Green))
                count += 1;

            return count;
        }

        public static Color ParseSingleColor(char c)
        {
            switch (c)
            {
                case 'W':
                    return Color.White;

                case 'U':
                    return Color.Blue;

                case 'B':
                    return Color.Black;

                case 'R':
                    return Color.Red;

                case 'G':
                    return Color.Green;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}

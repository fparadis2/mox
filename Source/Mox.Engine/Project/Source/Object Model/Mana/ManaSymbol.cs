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
    /// Represents the possible mana symbols.
    /// </summary>
    /// <remarks>Those are sorted in order of more generic to less generic.</remarks>
    public enum ManaSymbol
    {
        /// <summary>
        /// {X}
        /// </summary>
        X,
        /// <summary>
        /// {Y}.
        /// </summary>
        Y,
        /// <summary>
        /// {Z}.
        /// </summary>
        Z,

        /// <summary>
        /// {2/W}
        /// </summary>
        W2,
        /// <summary>
        /// {2/U}
        /// </summary>
        U2,
        /// <summary>
        /// {2/B}
        /// </summary>
        B2,
        /// <summary>
        /// {2/R}
        /// </summary>
        R2,
        /// <summary>
        /// {2/G}
        /// </summary>
        G2,

        /// <summary>
        /// {W/P}
        /// </summary>
        WP,
        /// <summary>
        /// {U/P}
        /// </summary>
        UP,
        /// <summary>
        /// {B/P}
        /// </summary>
        BP,
        /// <summary>
        /// {R/P}
        /// </summary>
        RP,
        /// <summary>
        /// {G/P}
        /// </summary>
        GP,

        /// <summary>
        /// White/Blue {W/U}.
        /// </summary>
        WU,
        /// <summary>
        /// White/Black {W/B}.
        /// </summary>
        WB,
        /// <summary>
        /// Blue/Black {U/B}.
        /// </summary>
        UB,
        /// <summary>
        /// Blue/Red {U/R}.
        /// </summary>
        UR,
        /// <summary>
        /// Black/Red {B/R}.
        /// </summary>
        BR,
        /// <summary>
        /// Black/Green {B/G}.
        /// </summary>
        BG,
        /// <summary>
        /// Red/Green {R/G}.
        /// </summary>
        RG,
        /// <summary>
        /// Red/White {R/W}.
        /// </summary>
        RW,
        /// <summary>
        /// Green/White {G/W}.
        /// </summary>
        GW,
        /// <summary>
        /// Green/Blue {G/U}.
        /// </summary>
        GU,

        /// <summary>
        /// White {W}.
        /// </summary>
        W,
        /// <summary>
        /// Blue {U}.
        /// </summary>
        U,
        /// <summary>
        /// Black {B}.
        /// </summary>
        B,
        /// <summary>
        /// Red {R}.
        /// </summary>
        R,
        /// <summary>
        /// Green {G}.
        /// </summary>
        G,

        /// <summary>
        /// Snow {S}.
        /// </summary>
        S
    }

    /// <summary>
    /// Provides utility methods in relation with mana symbols.
    /// </summary>
    public static class ManaSymbolHelper
    {
        #region Methods

        /// <summary>
        /// Returns the converted value of the given <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static int GetConvertedValue(ManaSymbol symbol)
        {
            switch (symbol)
            {
                case ManaSymbol.W:
                case ManaSymbol.U:
                case ManaSymbol.B:
                case ManaSymbol.R:
                case ManaSymbol.G:

                case ManaSymbol.BG:
                case ManaSymbol.BR:
                case ManaSymbol.GU:
                case ManaSymbol.GW:
                case ManaSymbol.RG:
                case ManaSymbol.RW:
                case ManaSymbol.UB:
                case ManaSymbol.UR:
                case ManaSymbol.WB:
                case ManaSymbol.WU:

                case ManaSymbol.WP:
                case ManaSymbol.UP:
                case ManaSymbol.BP:
                case ManaSymbol.RP:
                case ManaSymbol.GP:

                case ManaSymbol.S:
                    return 1;

                case ManaSymbol.W2:
                case ManaSymbol.U2:
                case ManaSymbol.B2:
                case ManaSymbol.R2:
                case ManaSymbol.G2:
                    return 2;

                case ManaSymbol.X:
                case ManaSymbol.Y:
                case ManaSymbol.Z:
                    return 0;

                default:
                    throw new ArgumentException("Invalid mana symbol");
            }
        }

        /// <summary>
        /// Tries to parse the given <paramref name="str"/> into a <see cref="ManaSymbol"/>.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="notation">Notation to enforce.</param>
        /// <param name="symbol">The parsed symbol.</param>
        /// <returns>True if the given <paramref name="str"/> could be parsed.</returns>
        public static bool TryParse(string str, ManaSymbolNotation notation, out ManaSymbol symbol)
        {
            symbol = default(ManaSymbol);

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (TryParseLong(str, out symbol))
            {
                return true;
            }

            return notation == ManaSymbolNotation.Compact && TryParseCompact(str, out symbol);
        }

        private static bool TryParseLong(string str, out ManaSymbol symbol)
        {
            switch (str.ToUpperInvariant())
            {
                case "{W}":
                    symbol = ManaSymbol.W;
                    return true;
                case "{U}":
                    symbol = ManaSymbol.U;
                    return true;
                case "{B}":
                    symbol = ManaSymbol.B;
                    return true;
                case "{R}":
                    symbol = ManaSymbol.R;
                    return true;
                case "{G}":
                    symbol = ManaSymbol.G;
                    return true;

                case "{B/G}":
                    symbol = ManaSymbol.BG;
                    return true;
                case "{B/R}":
                    symbol = ManaSymbol.BR;
                    return true;
                case "{G/U}":
                    symbol = ManaSymbol.GU;
                    return true;
                case "{G/W}":
                    symbol = ManaSymbol.GW;
                    return true;
                case "{R/G}":
                    symbol = ManaSymbol.RG;
                    return true;
                case "{R/W}":
                    symbol = ManaSymbol.RW;
                    return true;
                case "{U/B}":
                    symbol = ManaSymbol.UB;
                    return true;
                case "{U/R}":
                    symbol = ManaSymbol.UR;
                    return true;
                case "{W/B}":
                    symbol = ManaSymbol.WB;
                    return true;
                case "{W/U}":
                    symbol = ManaSymbol.WU;
                    return true;

                case "{2/W}":
                    symbol = ManaSymbol.W2;
                    return true;
                case "{2/U}":
                    symbol = ManaSymbol.U2;
                    return true;
                case "{2/B}":
                    symbol = ManaSymbol.B2;
                    return true;
                case "{2/R}":
                    symbol = ManaSymbol.R2;
                    return true;
                case "{2/G}":
                    symbol = ManaSymbol.G2;
                    return true;

                case "{W/P}":
                    symbol = ManaSymbol.WP;
                    return true;
                case "{U/P}":
                    symbol = ManaSymbol.UP;
                    return true;
                case "{B/P}":
                    symbol = ManaSymbol.BP;
                    return true;
                case "{R/P}":
                    symbol = ManaSymbol.RP;
                    return true;
                case "{G/P}":
                    symbol = ManaSymbol.GP;
                    return true;

                case "{X}":
                    symbol = ManaSymbol.X;
                    return true;
                case "{Y}":
                    symbol = ManaSymbol.Y;
                    return true;
                case "{Z}":
                    symbol = ManaSymbol.Z;
                    return true;

                case "{S}":
                    symbol = ManaSymbol.S;
                    return true;

                default:
                    symbol = default(ManaSymbol);
                    return false;
            }
        }

        private static bool TryParseCompact(string str, out ManaSymbol symbol)
        {
            switch (str.ToUpperInvariant())
            {
                case "W":
                    symbol = ManaSymbol.W;
                    return true;
                case "U":
                    symbol = ManaSymbol.U;
                    return true;
                case "B":
                    symbol = ManaSymbol.B;
                    return true;
                case "R":
                    symbol = ManaSymbol.R;
                    return true;
                case "G":
                    symbol = ManaSymbol.G;
                    return true;

                case "X":
                    symbol = ManaSymbol.X;
                    return true;
                case "Y":
                    symbol = ManaSymbol.Y;
                    return true;
                case "Z":
                    symbol = ManaSymbol.Z;
                    return true;

                case "S":
                    symbol = ManaSymbol.S;
                    return true;

                default:
                    symbol = default(ManaSymbol);
                    return false;
            }
        }

        /// <summary>
        /// Parses the given <paramref name="str"/> into a <see cref="ManaSymbol"/>.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="notation"></param>
        /// <returns>The parsed symbol.</returns>
        public static ManaSymbol Parse(string str, ManaSymbolNotation notation)
        {
            ManaSymbol symbol;
            if (!TryParse(str, notation, out symbol))
            {
                throw new ArgumentException(string.Format("Could not parse the {0} into a mana symbol.", str));
            }
            return symbol;
        }

        /// <summary>
        /// Returns a string representation of the mana symbol.
        /// </summary>
        /// <returns></returns>
        public static string ToString(ManaSymbol symbol, ManaSymbolNotation notation)
        {
            if (notation == ManaSymbolNotation.Long)
            {
                switch (symbol)
                {
                    case ManaSymbol.W: return "{W}";
                    case ManaSymbol.U: return "{U}";
                    case ManaSymbol.B: return "{B}";
                    case ManaSymbol.R: return "{R}";
                    case ManaSymbol.G: return "{G}";

                    case ManaSymbol.BG: return "{B/G}";
                    case ManaSymbol.BR: return "{B/R}";
                    case ManaSymbol.GU: return "{G/U}";
                    case ManaSymbol.GW: return "{G/W}";
                    case ManaSymbol.RG: return "{R/G}";
                    case ManaSymbol.RW: return "{R/W}";
                    case ManaSymbol.UB: return "{U/B}";
                    case ManaSymbol.UR: return "{U/R}";
                    case ManaSymbol.WB: return "{W/B}";
                    case ManaSymbol.WU: return "{W/U}";

                    case ManaSymbol.S: return "{S}";

                    case ManaSymbol.W2: return "{2/W}";
                    case ManaSymbol.U2: return "{2/U}";
                    case ManaSymbol.B2: return "{2/B}";
                    case ManaSymbol.R2: return "{2/R}";
                    case ManaSymbol.G2: return "{2/G}";

                    case ManaSymbol.WP: return "{W/P}";
                    case ManaSymbol.UP: return "{U/P}";
                    case ManaSymbol.BP: return "{B/P}";
                    case ManaSymbol.RP: return "{R/P}";
                    case ManaSymbol.GP: return "{G/P}";

                    case ManaSymbol.X: return "{X}";
                    case ManaSymbol.Y: return "{Y}";
                    case ManaSymbol.Z: return "{Z}";

                    default:
                        throw new ArgumentException("Invalid mana symbol");
                }
            }
            else
            {
                switch (symbol)
                {
                    case ManaSymbol.W: return "W";
                    case ManaSymbol.U: return "U";
                    case ManaSymbol.B: return "B";
                    case ManaSymbol.R: return "R";
                    case ManaSymbol.G: return "G";

                    case ManaSymbol.BG: return "{B/G}";
                    case ManaSymbol.BR: return "{B/R}";
                    case ManaSymbol.GU: return "{G/U}";
                    case ManaSymbol.GW: return "{G/W}";
                    case ManaSymbol.RG: return "{R/G}";
                    case ManaSymbol.RW: return "{R/W}";
                    case ManaSymbol.UB: return "{U/B}";
                    case ManaSymbol.UR: return "{U/R}";
                    case ManaSymbol.WB: return "{W/B}";
                    case ManaSymbol.WU: return "{W/U}";

                    case ManaSymbol.S: return "S";

                    case ManaSymbol.W2: return "{2/W}";
                    case ManaSymbol.U2: return "{2/U}";
                    case ManaSymbol.B2: return "{2/B}";
                    case ManaSymbol.R2: return "{2/R}";
                    case ManaSymbol.G2: return "{2/G}";

                    case ManaSymbol.WP: return "{W/P}";
                    case ManaSymbol.UP: return "{U/P}";
                    case ManaSymbol.BP: return "{B/P}";
                    case ManaSymbol.RP: return "{R/P}";
                    case ManaSymbol.GP: return "{G/P}";

                    case ManaSymbol.X: return "X";
                    case ManaSymbol.Y: return "Y";
                    case ManaSymbol.Z: return "Z";

                    default:
                        throw new ArgumentException("Invalid mana symbol");
                }
            }
        }

        /// <summary>
        /// Returns true if the <paramref name="symbol"/> is hybrid.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static bool IsHybrid(ManaSymbol symbol)
        {
            switch (symbol)
            {
                case ManaSymbol.W:
                case ManaSymbol.U:
                case ManaSymbol.B:
                case ManaSymbol.R:
                case ManaSymbol.G:
                    return false;

                case ManaSymbol.BG:
                case ManaSymbol.BR:
                case ManaSymbol.GU:
                case ManaSymbol.GW:
                case ManaSymbol.RG:
                case ManaSymbol.RW:
                case ManaSymbol.UB:
                case ManaSymbol.UR:
                case ManaSymbol.WB:
                case ManaSymbol.WU:
                    return true;

                case ManaSymbol.S:
                    return false;

                case ManaSymbol.W2:
                case ManaSymbol.U2:
                case ManaSymbol.B2:
                case ManaSymbol.R2:
                case ManaSymbol.G2:
                    return true;

                case ManaSymbol.WP:
                case ManaSymbol.UP:
                case ManaSymbol.BP:
                case ManaSymbol.RP:
                case ManaSymbol.GP:
                    return true;

                case ManaSymbol.X:
                case ManaSymbol.Y:
                case ManaSymbol.Z:
                    return false;

                default:
                    throw new ArgumentException("Invalid mana symbol");
            }
        }

        /// <summary>
        /// Returns the color associated with the <paramref name="symbol"/>.
        /// </summary>
        public static Color GetColor(ManaSymbol symbol)
        {
            switch (symbol)
            {
                case ManaSymbol.W:
                case ManaSymbol.W2:
                    return Color.White;
                case ManaSymbol.U:
                case ManaSymbol.U2:
                    return Color.Blue;
                case ManaSymbol.B:
                case ManaSymbol.B2:
                    return Color.Black;
                case ManaSymbol.R:
                case ManaSymbol.R2:
                    return Color.Red;
                case ManaSymbol.G:
                case ManaSymbol.G2:
                    return Color.Green;

                case ManaSymbol.BG:
                    return Color.Black | Color.Green;
                case ManaSymbol.BR:
                    return Color.Black | Color.Red;
                case ManaSymbol.GU:
                    return Color.Green | Color.Blue;
                case ManaSymbol.GW:
                    return Color.Green | Color.White;
                case ManaSymbol.RG:
                    return Color.Red | Color.Green;
                case ManaSymbol.RW:
                    return Color.Red | Color.White;
                case ManaSymbol.UB:
                    return Color.Blue | Color.Black;
                case ManaSymbol.UR:
                    return Color.Blue | Color.Red;
                case ManaSymbol.WB:
                    return Color.White | Color.Black;
                case ManaSymbol.WU:
                    return Color.White | Color.Blue;

                case ManaSymbol.WP:
                    return Color.White;
                case ManaSymbol.UP:
                    return Color.Blue;
                case ManaSymbol.BP:
                    return Color.Black;
                case ManaSymbol.RP:
                    return Color.Red;
                case ManaSymbol.GP:
                    return Color.Green;

                case ManaSymbol.S:
                case ManaSymbol.X:
                case ManaSymbol.Y:
                case ManaSymbol.Z:
                    return Color.None;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the symbol associated with the <paramref name="color"/>.
        /// </summary>
        public static ManaSymbol GetSymbol(Color color)
        {
            switch (color)
            {
                case Color.White:
                    return ManaSymbol.W;

                case Color.Blue:
                    return ManaSymbol.U;

                case Color.Black:
                    return ManaSymbol.B;

                case Color.Red:
                    return ManaSymbol.R;

                case Color.Green:
                    return ManaSymbol.G;

                case Color.None:
                    return ManaSymbol.X;

                default:
                    throw new ArgumentException("This color is not associated with a specific symbol");
            }
        }

        /// <summary>
        /// Returns whether the symbol is colored.
        /// </summary>
        public static bool IsColored(ManaSymbol symbol)
        {
            switch (symbol)
            {
                case ManaSymbol.S:
                case ManaSymbol.X:
                case ManaSymbol.Y:
                case ManaSymbol.Z:
                    return false;

                default:
                    return true;
            }
        }

        #endregion
    }
}

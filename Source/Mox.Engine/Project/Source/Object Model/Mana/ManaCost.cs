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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox
{
    /// <summary>
    /// Represents a mana cost.
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    [Serializable]
    public sealed class ManaCost : IEquatable<ManaCost>, IHashable
    {
        #region Variables

        private readonly byte m_generic;
        private readonly ManaSymbol[] m_symbols;
        private int? m_hash;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="generic">Amount of generic mana in the cost.</param>
        /// <param name="symbols">The symbols (except the colorless mana) that compose the cost.</param>
        public ManaCost(byte generic, params ManaSymbol[] symbols)
        {
            m_generic = generic;
            m_symbols = new ManaSymbol[symbols.Length];

            Array.Copy(symbols, m_symbols, m_symbols.Length);
            Array.Sort(m_symbols);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="generic">Amount of generic mana in the cost.</param>
        /// <param name="symbols">The symbols (except the colorless mana) that compose the cost.</param>
        public ManaCost(byte generic, ICollection<ManaSymbol> symbols)
        {
            m_generic = generic;
            m_symbols = new ManaSymbol[symbols.Count];

            symbols.CopyTo(m_symbols, 0);
            Array.Sort(m_symbols);
        }

        private static readonly ManaCost ms_empty = new ManaCost(0);

        /// <summary>
        /// Returns an empty cost.
        /// </summary>
        public static ManaCost Empty
        {
            get { return ms_empty; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// How many generic mana this cost contains.
        /// </summary>
        public byte Generic
        {
            get { return m_generic; }
        }

        /// <summary>
        /// Other Symbols contained in this cost.
        /// </summary>
        public IReadOnlyList<ManaSymbol> Symbols
        {
            get { return m_symbols; }
        }

        public int SymbolCount
        {
            get { return m_symbols.Length + (m_generic > 0 ? 1 : 0); }
        }

        /// <summary>
        /// Converted value of this cost.
        /// </summary>
        public int ConvertedValue
        {
            get
            {
                return m_symbols.Aggregate((int)Generic, (result, symbol) => result + ManaSymbolHelper.GetConvertedValue(symbol));
            }
        }

        /// <summary>
        /// Returns true if the cost is concrete (no hybrid mana symbols and no XYZ symbols)
        /// </summary>
        public bool IsConcrete
        {
            get 
            { 
                return !m_symbols.Any(symbol => 
                    symbol == ManaSymbol.X ||
                    symbol == ManaSymbol.Y ||
                    symbol == ManaSymbol.Z
                ); 
            }
        }

        /// <summary>
        /// Returns true if the cost is empty (nothing to pay)
        /// </summary>
        public bool IsEmpty
        {
            get { return m_generic == 0 && m_symbols.Length == 0; }
        }

        #endregion

        #region Methods

        #region Operations

        /// <summary>
        /// Returns the mana colors that can be used to pay this cost. Returns false if any mana
        /// </summary>
        public ManaColors GetPayingColors()
        {
            if (m_generic > 0)
                return ManaColors.All;

            ManaColors result = ManaColors.None;

            foreach (var symbol in Symbols)
            {
                switch (symbol)
                {
                    case ManaSymbol.X:
                    case ManaSymbol.Y:
                    case ManaSymbol.Z:
                    case ManaSymbol.W2:
                    case ManaSymbol.U2:
                    case ManaSymbol.B2:
                    case ManaSymbol.R2:
                    case ManaSymbol.G2:
                        return ManaColors.All;

                    case ManaSymbol.WP: result |= ManaColors.White; break;
                    case ManaSymbol.UP: result |= ManaColors.Blue; break;
                    case ManaSymbol.BP: result |= ManaColors.Black; break;
                    case ManaSymbol.RP: result |= ManaColors.Red; break;
                    case ManaSymbol.GP: result |= ManaColors.Green; break;

                    case ManaSymbol.WU: result |= ManaColors.White | ManaColors.Blue; break;
                    case ManaSymbol.WB: result |= ManaColors.White | ManaColors.Black; break;
                    case ManaSymbol.UB: result |= ManaColors.Blue | ManaColors.Black; break;
                    case ManaSymbol.UR: result |= ManaColors.Blue | ManaColors.Red; break;
                    case ManaSymbol.BR: result |= ManaColors.Black | ManaColors.Red; break;
                    case ManaSymbol.BG: result |= ManaColors.Black | ManaColors.Green; break;
                    case ManaSymbol.RG: result |= ManaColors.Red | ManaColors.Green; break;
                    case ManaSymbol.RW: result |= ManaColors.Red | ManaColors.White; break;
                    case ManaSymbol.GW: result |= ManaColors.Green | ManaColors.White; break;
                    case ManaSymbol.GU: result |= ManaColors.Green | ManaColors.Blue; break;

                    case ManaSymbol.W: result |= ManaColors.White; break;
                    case ManaSymbol.U: result |= ManaColors.Blue; break;
                    case ManaSymbol.B: result |= ManaColors.Black; break;
                    case ManaSymbol.R: result |= ManaColors.Red; break;
                    case ManaSymbol.G: result |= ManaColors.Green; break;
                    case ManaSymbol.C: result |= ManaColors.Colorless; break;

                    case ManaSymbol.S:
                    default:
                        throw new NotSupportedException();
                }
            }

            return result;
        }

        #endregion

        #region Parse

        public static ManaCost Parse(string cost, ManaSymbolNotation notation = ManaSymbolNotation.Compact)
        {
            ManaCost parsedCost;
            Throw.InvalidArgumentIf(!TryParse(cost, out parsedCost), "Invalid cost: " + cost, "cost");
            return parsedCost;
        }

        public static bool TryParse(string text, out ManaCost manaCost)
        {
            return TryParse(text, ManaSymbolNotation.Compact, out manaCost);
        }

        public static bool TryParse(string text, ManaSymbolNotation notation, out ManaCost manaCost)
        {
            return TryParse(text, 0, text != null ? text.Length : 0, notation, out manaCost);
        }

        public static bool TryParse(string text, int start, int end, ManaSymbolNotation notation, out ManaCost manaCost)
        {
            manaCost = null;

            if (start >= end)
            {
                manaCost = Empty;
                return true;
            }

            bool hasGeneric = false;
            byte generic = 0;

            List<ManaSymbol> symbols = new List<ManaSymbol>();
            foreach (string token in Tokenize(text, start, end))
            {
                ManaSymbol symbol;
                if (ManaSymbolHelper.TryParse(token, notation, out symbol))
                {
                    symbols.Add(symbol);
                }
                else
                {
                    // Can't have two generic parts
                    if (hasGeneric)
                        return false;

                    string intToken = token;

                    if (intToken.StartsWith("{") && intToken.EndsWith("}"))
                    {
                        intToken = token.Substring(1, intToken.Length - 2);
                    }
                    else if (notation == ManaSymbolNotation.Long)
                    {
                        return false;
                    }

                    if (!byte.TryParse(intToken, out generic))
                    {
                        return false;
                    }
                }
            }

            manaCost = new ManaCost(generic, symbols);
            return true;
        }

        private static IEnumerable<string> Tokenize(string text, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++)
            {
                char symbolChar = text[i];

                if (symbolChar == '{')
                {
                    // Hybrid mana
                    int end = text.IndexOf('}', i);
                    if (end != -1)
                    {
                        yield return text.Substring(i, end + 1 - i);
                        i = end;
                    }
                }
                else if (char.IsDigit(symbolChar))
                {
                    int end = i;
                    while (end < text.Length - 1 && char.IsDigit(text[end + 1]))
                    {
                        end++;
                    }

                    yield return text.Substring(i, end + 1 - i);
                    i = end;
                }
                else
                {
                    yield return symbolChar.ToString();
                }
            }
        }

        #endregion

        #region Overriden

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public string ToString(ManaSymbolNotation notation)
        {
            StringBuilder result = new StringBuilder();

            int i = 0;
            for (; i < m_symbols.Length; i++)
            {
                if (m_symbols[i] > ManaSymbol.Z)
                    break;

                result.Append(GetSymbolString(m_symbols[i], notation));
            }

            if (Generic > 0 || Symbols.Count == 0)
            {
                if (notation != ManaSymbolNotation.Compact)
                {
                    result.Append('{');
                }
                
                result.Append(Generic);

                if (notation != ManaSymbolNotation.Compact)
                {
                    result.Append('}');
                }
            }

            for (; i < m_symbols.Length; i++)
            {
                result.Append(GetSymbolString(m_symbols[i], notation));
            }

            return result.ToString();
        }

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(ManaSymbolNotation.Compact);
        }

        private static string GetSymbolString(ManaSymbol symbol, ManaSymbolNotation notation)
        {
            return ManaSymbolHelper.ToString(symbol, notation);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ManaCost);
        }

        public override int GetHashCode()
        {
            return m_generic;
        }

        public void ComputeHash(Hash hash, HashContext context)
        {
            if (!m_hash.HasValue)
            {
                Hash ownHash = new Hash();
                ownHash.Add(m_generic);
                foreach (var symbol in m_symbols)
                {
                    ownHash.Add((int)symbol);
                }
                m_hash = ownHash.Value;
            }
                
            hash.Add(m_hash.Value);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(ManaCost other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (m_generic != other.m_generic)
            {
                return false;
            }

            if (m_symbols.Length != other.m_symbols.Length)
            {
                return false;
            }

            // Symbols are sorted so this works.
            for (int i = 0; i < m_symbols.Length; i++)
            {
                if (m_symbols[i] != other.m_symbols[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(ManaCost a, ManaCost b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(ManaCost a, ManaCost b)
        {
            return !(a == b);
        }

        #endregion

        #endregion
    }
}

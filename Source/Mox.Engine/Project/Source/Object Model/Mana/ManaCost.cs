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
        #region Constants

        private const ManaSymbol ThresholdSymbol = ManaSymbol.Z;

        #endregion

        #region Variables

        private readonly int m_colorless;
        private readonly List<ManaSymbol> m_symbols = new List<ManaSymbol>();
        private List<ManaSymbol> m_sortedSymbols;
        private int? m_hash;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="numColorlessMana">Amount of colorless mana in the cost.</param>
        /// <param name="otherSymbols">The other symbols (expect the colorless mana) that compose the cost.</param>
        public ManaCost(int numColorlessMana, params ManaSymbol[] otherSymbols)
            : this(numColorlessMana, (IEnumerable<ManaSymbol>)otherSymbols)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="numColorlessMana">Amount of colorless mana in the cost.</param>
        /// <param name="otherSymbols">The other symbols (expect the colorless mana) that compose the cost.</param>
        private ManaCost(int numColorlessMana, IEnumerable<ManaSymbol> otherSymbols)
        {
            Throw.ArgumentOutOfRangeIf(numColorlessMana < 0, "Colorless mana cannot be negative.", "numColorlessMana");

            m_colorless = numColorlessMana;

            if (otherSymbols != null)
            {
                m_symbols.AddRange(otherSymbols);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// How many colorless mana this cost contains.
        /// </summary>
        public int Colorless
        {
            get { return m_colorless; }
        }

        /// <summary>
        /// Other Symbols contained in this cost.
        /// </summary>
        public IList<ManaSymbol> Symbols
        {
            get { return m_symbols.AsReadOnly(); }
        }

        private IList<ManaSymbol> SortedSymbols
        {
            get
            {
                if (m_sortedSymbols == null)
                {
                    var sortedSymbols = new List<ManaSymbol>(m_symbols);
                    sortedSymbols.Sort();
                    m_sortedSymbols = sortedSymbols;
                }

                return m_sortedSymbols;
            }
        }

        /// <summary>
        /// Converted value of this cost.
        /// </summary>
        public int ConvertedValue
        {
            get
            {
                return Symbols.Aggregate(Colorless, (result, symbol) => result + ManaSymbolHelper.GetConvertedValue(symbol));
            }
        }

        /// <summary>
        /// Returns true if the cost is concrete (no hybrid mana symbols and no XYZ symbols)
        /// </summary>
        public bool IsConcrete
        {
            get 
            { 
                return !Symbols.Any(symbol => 
                    ManaSymbolHelper.IsHybrid(symbol) ||
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
            get { return m_colorless == 0 && Symbols.Count == 0; }
        }

        /// <summary>
        /// Returns an empty cost.
        /// </summary>
        public static ManaCost Empty
        {
            get { return new ManaCost(0); }
        }

        #endregion

        #region Methods

        #region Operations

        /// <summary>
        /// Removes the given <paramref name="symbol"/> and returns the result.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public ManaCost Remove(ManaSymbol symbol)
        {
            List<ManaSymbol> newSymbols = new List<ManaSymbol>(Symbols);
            newSymbols.Remove(symbol);
            return new ManaCost(m_colorless, newSymbols);
        }

        /// <summary>
        /// Removes colorless mana from the cost, returning the result.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public ManaCost RemoveColorless(int amount)
        {
            Throw.ArgumentOutOfRangeIf(amount < 0, "Amount cannot be negative", "amount");
            return new ManaCost(Math.Max(0, m_colorless - amount), Symbols);
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
            manaCost = null;

            if (string.IsNullOrEmpty(text))
            {
                manaCost = Empty;
                return true;
            }

            int colorless = -1;

            List<ManaSymbol> symbols = new List<ManaSymbol>();
            foreach (string token in Tokenize(text))
            {
                ManaSymbol symbol;
                if (ManaSymbolHelper.TryParse(token, notation, out symbol))
                {
                    symbols.Add(symbol);
                }
                else
                {
                    // Can't have two colorless parts
                    if (colorless >= 0)
                    {
                        return false;
                    }

                    string intToken = token;

                    if (intToken.StartsWith("{") && intToken.EndsWith("}"))
                    {
                        intToken = token.Substring(1, intToken.Length - 2);
                    }
                    else if (notation == ManaSymbolNotation.Long)
                    {
                        return false;
                    }

                    if (!int.TryParse(intToken, out colorless))
                    {
                        return false;
                    }
                }
            }

            manaCost = new ManaCost(Math.Max(0, colorless), symbols);
            return true;
        }

        private static IEnumerable<string> Tokenize(string text)
        {
            for (int i = 0; i < text.Length; i++)
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

        #region Misc

        public IEnumerable<object> ToObjects()
        {
            foreach (ManaSymbol symbol in ModalSymbols)
            {
                yield return symbol;
            }

            if (Colorless > 0 || Symbols.Count == 0)
            {
                yield return Colorless;
            }

            foreach (ManaSymbol symbol in NonModalSymbols)
            {
                yield return symbol;
            }
        }

        private IEnumerable<ManaSymbol> ModalSymbols
        {
            get { return SortedSymbols.TakeWhile(s => s <= ThresholdSymbol); }
        }

        private IEnumerable<ManaSymbol> NonModalSymbols
        {
            get { return Symbols.Where(s => s > ThresholdSymbol); }
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

            foreach (ManaSymbol symbol in ModalSymbols)
            {
                result.Append(GetSymbolString(symbol, notation));
            }

            if (Colorless > 0 || Symbols.Count == 0)
            {
                if (notation != ManaSymbolNotation.Compact)
                {
                    result.Append('{');
                }
                
                result.Append(Colorless);

                if (notation != ManaSymbolNotation.Compact)
                {
                    result.Append('}');
                }
            }

            foreach (ManaSymbol symbol in NonModalSymbols)
            {
                result.Append(GetSymbolString(symbol, notation));
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
            return m_colorless;
        }

        public void ComputeHash(Hash hash, HashContext context)
        {
            if (!m_hash.HasValue)
            {
                Hash ownHash = new Hash();
                ownHash.Add(m_colorless);
                foreach (var symbol in SortedSymbols)
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

            if (m_colorless != other.m_colorless)
            {
                return false;
            }

            if (m_symbols.Count != other.m_symbols.Count)
            {
                return false;
            }

            // Symbols are sorted so this works.
            for (int i = 0; i < m_symbols.Count; i++)
            {
                if (SortedSymbols[i] != other.SortedSymbols[i])
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

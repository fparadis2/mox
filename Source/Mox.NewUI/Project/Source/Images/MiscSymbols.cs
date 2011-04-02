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

namespace Mox.UI
{
    /// <summary>
    /// Other MTG symbols that can appear in UI.
    /// </summary>
    public class MiscSymbols
    {
        #region Constants

        private const string TapToken = "{T}";
        private const string UntapToken = "{U}";

        #endregion

        #region Variables

        private static readonly MiscSymbols ms_tap = new MiscSymbols(TapToken);
        private static readonly MiscSymbols ms_untap = new MiscSymbols(UntapToken);

        private readonly string m_token;

        #endregion

        #region Constructor

        private MiscSymbols(string token)
        {
            Throw.IfNull(token, "token");
            m_token = token;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tap symbol.
        /// </summary>
        public static MiscSymbols Tap
        {
            get { return ms_tap; }
        }

        /// <summary>
        /// Untap symbol.
        /// </summary>
        public static MiscSymbols Untap
        {
            get { return ms_untap; }
        }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            MiscSymbols other = obj as MiscSymbols;
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return Equals(m_token, other.m_token);
        }

        public override int GetHashCode()
        {
            return m_token.GetHashCode();
        }

        public override string ToString()
        {
            return m_token;
        }

        public static bool TryParse(string str, out MiscSymbols symbol)
        {
            switch (str)
            {
                case TapToken:
                    symbol = Tap;
                    return true;

                case UntapToken:
                    symbol = Untap;
                    return true;

                default:
                    symbol = default(MiscSymbols);
                    return false;
            }
        }

        #endregion
    }
}

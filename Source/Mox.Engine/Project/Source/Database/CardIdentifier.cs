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
    /// Uniquely identifies a card info.
    /// </summary>
    [Serializable]
    public struct CardIdentifier : IEquatable<CardIdentifier>
    {
        #region Variables

        /// <summary>
        /// Set identifier.
        /// </summary>
        public string Set;

        /// <summary>
        /// Card name.
        /// </summary>
        public string Card;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the card identifier is invalid.
        /// </summary>
        public bool IsInvalid
        {
            get { return string.IsNullOrEmpty(Card); }
        }

        #endregion

        #region Methods

        public bool Equals(CardIdentifier identifier)
        {
            if (!string.IsNullOrEmpty(Set))
            {
                if (Set != identifier.Set)
                {
                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(identifier.Set))
            {
                return false;
            }

            return Card == identifier.Card;
        }

        public override bool Equals(object obj)
        {
            if (obj is CardIdentifier)
            {
                return Equals((CardIdentifier)obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hash = Card.GetHashCode();

            if (!string.IsNullOrEmpty(Set))
            {
                hash ^= Set.GetHashCode();
            }

            return hash;
        }

        public override string ToString()
        {
            if (IsInvalid)
            {
                return "[Invalid]";
            }

            return string.Format("[{0} ({1})]", Card, Set);
        }

        public static bool operator ==(CardIdentifier a, CardIdentifier b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CardIdentifier a, CardIdentifier b)
        {
            return !(a == b);
        }

        #endregion
    }
}

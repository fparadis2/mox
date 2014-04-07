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

namespace Mox.Database
{
    /// <summary>
    /// Contains information about a particular instance of a card in a set.
    /// </summary>
    public class CardInstanceInfo
    {
        #region Variables

        private readonly CardInfo m_card;
        private readonly SetInfo m_set;

        private readonly Rarity m_rarity;
        private readonly int m_multiverseId;
        private readonly string m_artist;

        #endregion

        #region Constructor

        internal CardInstanceInfo(CardInfo card, SetInfo set, Rarity rarity, int multiverseId, string artist)
        {
            Throw.IfNull(card, "card");
            Throw.IfNull(set, "set");
            Debug.Assert(card.Database == set.Database);

            m_card = card;
            m_set = set;

            m_rarity = rarity;
            m_multiverseId = multiverseId;
            m_artist = artist;
        }

        #endregion

        #region Properties

        public CardDatabase Database
        {
            get { return m_card.Database; }
        }

        /// <summary>
        /// Card.
        /// </summary>
        public CardInfo Card
        {
            get { return m_card; }
        }

        /// <summary>
        /// Set.
        /// </summary>
        public SetInfo Set
        {
            get { return m_set; }
        }

        /// <summary>
        /// Rarity
        /// </summary>
        public Rarity Rarity
        {
            get { return m_rarity; }
        }

        /// <summary>
        /// Multiverse Id
        /// </summary>
        public int MultiverseId
        {
            get { return m_multiverseId; }
        }

        /// <summary>
        /// Illustration Artist.
        /// </summary>
        public string Artist
        {
            get { return m_artist; }
        }

        #endregion
    }
}
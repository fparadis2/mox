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

        private readonly int m_index;
        private readonly Rarity m_rarity;
        private readonly int m_multiverseId;
        private readonly string m_artist;
        private readonly string m_flavor;

        #endregion

        #region Constructor

        internal CardInstanceInfo(CardInfo card, SetInfo set, int index, Rarity rarity, int multiverseId, string artist, string flavor = null)
        {
            Throw.IfNull(card, "card");
            Throw.IfNull(set, "set");
            Debug.Assert(card.Database == set.Database);

            m_card = card;
            m_set = set;

            m_index = index;
            m_rarity = rarity;
            m_multiverseId = multiverseId;
            m_artist = artist;
            m_flavor = flavor;
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
        /// Index in the set.
        /// </summary>
        public int Index
        {
            get { return m_index; }
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

        /// <summary>
        /// Flavor Text.
        /// </summary>
        public string Flavor
        {
            get { return m_flavor; }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return string.Format("{0} ({1})", Card.Name, Set.Name);
        }

        public static implicit operator CardIdentifier(CardInstanceInfo instance)
        {
            return new CardIdentifier
            {
                Card = instance.m_card.Name,
                Set = instance.m_set.Identifier,
                MultiverseId = instance.m_multiverseId
            };
        }

        #endregion
    }
}
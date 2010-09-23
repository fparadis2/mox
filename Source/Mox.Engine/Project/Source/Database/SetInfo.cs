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

namespace Mox.Database
{
    /// <summary>
    /// Contains information about a card set.
    /// </summary>
    public class SetInfo
    {
        #region Variables

        private readonly CardDatabase m_database;

        private readonly string m_identifier;
        private readonly string m_name;
        private readonly string m_block;

        private readonly DateTime m_releaseDate;

        #endregion

        #region Constructor

        internal SetInfo(CardDatabase database, string identifier, string name, string block, DateTime releaseDate)
        {
            Throw.IfNull(database, "database");
            Throw.IfEmpty(identifier, "identifier");

            m_database = database;
            m_name = name;
            m_identifier = identifier;
            m_block = block;
            m_releaseDate = releaseDate;
        }

        #endregion

        #region Properties

        public CardDatabase Database
        {
            get { return m_database; }
        }

        public IEnumerable<CardInstanceInfo> CardInstances
        {
            get { return Database.GetCardInstances(this); }
        }

        /// <summary>
        /// Name of the set.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Set Identifier
        /// </summary>
        public string Identifier
        {
            get { return m_identifier; }
        }

        /// <summary>
        /// Set Block
        /// </summary>
        public string Block
        {
            get { return m_block; }
        }

        /// <summary>
        /// Release Date
        /// </summary>
        public DateTime ReleaseDate
        {
            get { return m_releaseDate; }
        }
        
        #endregion

        #region Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}

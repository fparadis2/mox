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
using System.Runtime.Serialization;

namespace Mox.Network
{
    /// <summary>
    /// Represents a client of the game service (either a player or a spectator).
    /// </summary>
    [DataContract]
    public class Client
    {
        #region Variables

        [DataMember]
        private readonly string m_name;

        [DataMember]
        private readonly Guid m_identifier = Guid.NewGuid();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public Client(string name)
        {
            Throw.IfEmpty(name, "name");
            m_name = name;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the client.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_identifier.GetHashCode();
        }

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Client other = obj as Client;

            return !ReferenceEquals(other, null) && other.m_identifier == m_identifier;
        }

        /// <summary>
        /// ==
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Client a, Client b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        /// <summary>
        /// !=
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Client a, Client b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[Client: {0}]", Name);
        }

        #endregion
    }
}

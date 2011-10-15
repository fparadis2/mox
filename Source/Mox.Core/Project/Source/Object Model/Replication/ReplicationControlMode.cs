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
namespace Mox
{
    public enum ReplicationControlMode
    {
        /// <summary>
        /// A master host can be modified by an AI, a sequencer, etc.
        /// </summary>
        Master,
        /// <summary>
        /// A synchronized (replicated, slave) host is only modified through replication. Other objects cannot act upon a synchronized host.
        /// A slave can be upgraded temporarily to a Master (AI does this)
        /// </summary>
        Slave
    }
}
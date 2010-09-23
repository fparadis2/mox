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
    public enum GameControlMode
    {
        /// <summary>
        /// A master game can be modified by a game sequencer.
        /// </summary>
        Master,
        /// <summary>
        /// A synchronized (replicated, slave) game is only modified through replication. A game sequencer cannot act upon a synchronized game.
        /// </summary>
        Synchronized
    }
}
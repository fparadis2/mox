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
using Mox.Rules;

namespace Mox
{
    /// <summary>
    /// Contains data that is reset at the beginning of every turn
    /// </summary>
    public class TurnData : GameObject
    {
        private int m_numberOfLandsPlayed;
        private static readonly Property<int> NumberOfLandsPlayedProperty = Property<int>.RegisterProperty<TurnData>("NumberOfLandsPlayed", t => t.m_numberOfLandsPlayed);

        public bool CanPlayLand
        {
            get
            {
                return m_numberOfLandsPlayed == 0 || OneLandPerTurn.IsBypassed;
            }
        }

        public void PlayOneLand()
        {
            SetValue(NumberOfLandsPlayedProperty, m_numberOfLandsPlayed + 1, ref m_numberOfLandsPlayed);
        }
    }
}

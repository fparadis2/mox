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
using System.Text;

namespace Mox.AI.Arena
{
    public class ArenaResult
    {
        #region Properties

        public int Seed
        {
            get;
            set;
        }

        public Player Winner
        {
            get;
            set;
        }

        public float WinnerScore
        {
            get;
            set;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(string.Format("Seed          : {0}", Seed));
            builder.AppendLine(string.Format("Winner        : {0}", Winner.Name));
            builder.AppendLine(string.Format("Winner Score  : {0}", WinnerScore));

            return builder.ToString();
        }

        #endregion
    }
}
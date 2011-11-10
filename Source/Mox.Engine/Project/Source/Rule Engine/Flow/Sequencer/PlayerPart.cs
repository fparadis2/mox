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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mox.Flow
{
    /// <summary>
    /// A part associated with a player
    /// </summary>
    public abstract class PlayerPart : NewPart
    {
        #region Variables

        private readonly Resolvable<Player> m_player;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="player"></param>
        protected PlayerPart(Player player)
        {
            Throw.IfNull(player, "player");
            m_player = player;
        }

        #endregion

        #region Properties

        protected Resolvable<Player> ResolvablePlayer
        {
            get { return m_player; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Player this part is associated with.
        /// </summary>
        public Player GetPlayer(Context context)
        {
            return GetPlayer(context.Game);
        }

        /// <summary>
        /// Player this part is associated with.
        /// </summary>
        public Player GetPlayer(Game game)
        {
            return m_player.Resolve(game);
        }

        #endregion
    }
}

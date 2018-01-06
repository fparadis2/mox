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

namespace Mox
{
    /// <summary>
    /// Represents a player in a game.
    /// </summary>
    public class Player : GameObject, IObjectName
    {
        #region Variables

        private readonly int m_index = 0;
        public static Property<int> IndexProperty = Property<int>.RegisterProperty<Player>("Index", p => p.m_index);

        private string m_name;
        public static Property<string> NameProperty = Property<string>.RegisterProperty<Player>("Name", p => p.m_name, PropertyFlags.IgnoreHash);

        private int m_life = 20;
        public static Property<int> LifeProperty = Property<int>.RegisterProperty<Player>("Life", p => p.m_life);

        private int m_maximumHandSize = 7;
        public static Property<int> MaximumHandSizeProperty = Property<int>.RegisterProperty<Player>("MaximumHandSize", p => p.m_maximumHandSize);

        private bool m_hasDrawnMoreCardsThanAvailable;
        public static Property<bool> HasDrawnMoreCardsThanAvailableProperty = Property<bool>.RegisterProperty<Player>("HasDrawnMoreCardsThanAvailable", p => p.m_hasDrawnMoreCardsThanAvailable);

        private readonly PlayerManaPool m_manaPool;

        #endregion

        #region Constructor
        
        public Player()
        {
            m_manaPool = new PlayerManaPool(this);
        }

        #endregion

        #region Properties

        public int Index
        {
            get { return m_index; }
        }

        /// <summary>
        /// Name of the player.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { SetValue(NameProperty, value, ref m_name); }
        }

        /// <summary>
        /// Life of the player.
        /// </summary>
        public int Life
        {
            get { return m_life; }
            set { SetValue(LifeProperty, value, ref m_life); }
        }

        /// <summary>
        /// Maximum hand size of the player.
        /// </summary>
        public int MaximumHandSize
        {
            get { return m_maximumHandSize; }
            set { SetValue(MaximumHandSizeProperty, value, ref m_maximumHandSize); }
        }

        /// <summary>
        /// If a player is required to draw more cards than are left in his or her library, he or she draws the remaining cards, and then loses the game the next time a player would receive priority.
        /// </summary>
        public bool HasDrawnMoreCardsThanAvailable
        {
            get { return m_hasDrawnMoreCardsThanAvailable; }
            set { SetValue(HasDrawnMoreCardsThanAvailableProperty, value, ref m_hasDrawnMoreCardsThanAvailable); }
        }

        #region Zones

        /// <summary>
        /// Cards in the player's library.
        /// </summary>
        public ICardCollection Library
        {
            get { return Manager.Zones.Library[this]; }
        }

        /// <summary>
        /// Cards in the player's hand.
        /// </summary>
        public ICardCollection Hand
        {
            get { return Manager.Zones.Hand[this]; }
        }

        /// <summary>
        /// Cards in the player's graveyard.
        /// </summary>
        public ICardCollection Graveyard
        {
            get { return Manager.Zones.Graveyard[this]; }
        }

        /// <summary>
        /// Cards in play controlled by the player.
        /// </summary>
        public ICardCollection Battlefield
        {
            get { return Manager.Zones.Battlefield[this]; }
        }

        /// <summary>
        /// Cards removed from the game controlled by the player.
        /// </summary>
        public ICardCollection Exile
        {
            get { return Manager.Zones.Exile[this]; }
        }

        /// <summary>
        /// Cards phased out controlled by the player
        /// </summary>
        public ICardCollection PhasedOut
        {
            get { return Manager.Zones.PhasedOut[this]; }
        }

        public PlayerManaPool ManaPool
        {
            get { return m_manaPool; }
        }

        #endregion

        #endregion

        #region Methods

        protected override void Init()
        {
            base.Init();
            Manager.Zones.EnsurePlayerHasZone(this);
        }

        #region Static methods

        /// <summary>
        /// Enumerates the players of the game, starting with the given <paramref name="startingPlayer"/>.
        /// </summary>
        /// <param name="startingPlayer"></param>
        /// <param name="loop">Whether to stop the enumeration after one complete iteration</param>
        /// <returns></returns>
        public static IEnumerable<Player> Enumerate(Player startingPlayer, bool loop)
        {
            Throw.IfNull(startingPlayer, "startingPlayer");
            Throw.InvalidArgumentIf(startingPlayer.Manager == null, "Player is not part of a game", "startingPlayer");

            int startingIndex = startingPlayer.Manager.Players.IndexOf(startingPlayer);
            int i = startingIndex;

            Throw.InvalidArgumentIf(startingIndex < 0, "Invalid player", "startingPlayer");

            do
            {
                yield return startingPlayer.Manager.Players[i];
                i = (i + 1) % startingPlayer.Manager.Players.Count;
            }
            while (loop || i != startingIndex);
        }

        /// <summary>
        /// Gets the player that comes after the given <paramref name="player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Player GetNextPlayer(Player player)
        {
            Throw.IfNull(player, "player");
            Throw.InvalidArgumentIf(player.Manager == null, "Player is not part of a game", "player");

            int index = player.Manager.Players.IndexOf(player);
            index = (index + 1) % player.Manager.Players.Count;
            return player.Manager.Players[index];
        }

        #endregion

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[Player: {0}]", string.IsNullOrEmpty(Name) ? "<No Name>" : Name);
        }

        #endregion
    }
}

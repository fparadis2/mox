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
using Mox.Collections;

namespace Mox.Replication
{
    /// <summary>
    /// Manages normal visibility of objects in an MTG game.
    /// </summary>
    public class MTGGameVisibilityStrategy : IVisibilityStrategy<Player>
    {
        #region Variables

        private readonly Game m_game;
        private readonly Dictionary<int, List<int>> m_visibility = new Dictionary<int, List<int>>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public MTGGameVisibilityStrategy(Game game)
        {
            Throw.IfNull(game, "game");

            m_game = game;
            m_game.Objects.CollectionChanged += Objects_CollectionChanged;
            m_game.PropertyChanged += m_game_PropertyChanged;
            m_game.Objects.ForEach(RegisterObject);
        }

        public void Dispose()
        {
            m_game.Objects.CollectionChanged -= Objects_CollectionChanged;
            m_game.PropertyChanged -= m_game_PropertyChanged;
        }

        #endregion

        #region Methods

        #region Visibility Computation

        /// <summary>
        /// Returns true if the given <paramref name="gameObject"/> is visible to the given <paramref name="player"/>.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="player">The player to test for. Null means a spectator.</param>
        /// <returns></returns>
        public bool IsVisible(Object gameObject, Player player)
        {
            Throw.IfNull(gameObject, "gameObject");
            return GetVisible(gameObject, player);
        }

        /// <summary>
        /// Returns true if the given <paramref name="gameObject"/> is visible to the given <paramref name="player"/>.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="player">The player to test for. Null means a spectator.</param>
        /// <returns></returns>
        private static bool ComputeIsVisible(Object gameObject, Player player)
        {
            Throw.IfNull(gameObject, "gameObject");

            if (gameObject is Player)
            {
                return true;
            }
            
            if (gameObject is Card)
            {
                return IsCardVisible((Card)gameObject, player);
            }
            
            if (gameObject is Ability)
            {
                return ComputeIsVisible(((Ability)gameObject).Source, player);
            }

            return true;
        }

        private static bool IsCardVisible(Card card, Player player)
        {
            return card.IsVisible(player);
        }

        #endregion

        #region Update

        private IEnumerable<Player> AllPlayersIncludingSpectator
        {
            get
            {
                yield return null; // Spectator

                foreach (Player player in Player.Enumerate(m_game.Players.First(), false))
                {
                    yield return player;
                }
            }
        }

        private void RegisterObject(Object obj)
        {
            foreach (Player player in AllPlayersIncludingSpectator)
            {
                SetVisible(obj, player, ComputeIsVisible(obj, player));
            }
        }

        private void UnregisterObject(Object obj)
        {
        }

        private void UpdateCardVisibility(Card card)
        {
            UpdateObjectVisibility(card);

            foreach (Ability ability in card.Abilities)
            {
                UpdateObjectVisibility(ability);
            }
        }

        private void UpdateObjectVisibility(Object obj)
        {
            foreach (Player player in AllPlayersIncludingSpectator)
            {
                bool oldVisibility = GetVisible(obj, player);
                if (oldVisibility != ComputeIsVisible(obj, player))
                {
                    SetVisible(obj, player, !oldVisibility);
                    OnObjectVisibilityChanged(new VisibilityChangedEventArgs<Player>(obj, player, !oldVisibility));
                }
            }
        }

        private bool GetVisible(Object obj, Player player)
        {
            List<int> players;
            if (m_visibility.TryGetValue(obj.Identifier, out players))
            {
                int playerIdentifier = player == null ? ObjectManager.InvalidIdentifier : player.Identifier;
                return players.Contains(playerIdentifier);
            }
            return false;
        }

        private void SetVisible(Object obj, Player player, bool visible)
        {
            int playerIdentifier = player == null ? ObjectManager.InvalidIdentifier : player.Identifier;

            List<int> players;
            if (m_visibility.TryGetValue(obj.Identifier, out players))
            {
                if (visible)
                {
                    Debug.Assert(!players.Contains(playerIdentifier));
                    players.Add(playerIdentifier);
                }
                else
                {
                    players.Remove(playerIdentifier);
                }
            }
            else if (visible)
            {
                players = new List<int> { playerIdentifier };
                m_visibility.Add(obj.Identifier, players);
            }
        }

        #endregion

        #endregion

        #region Event Handlers

        void Objects_CollectionChanged(object sender, CollectionChangedEventArgs<Object> e)
        {
            e.Synchronize(RegisterObject, UnregisterObject);
        }

        void m_game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Property == Card.ZoneIdProperty)
            {
                UpdateCardVisibility((Card)e.Object);
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the visibility of an object changes
        /// </summary>
        public event EventHandler<VisibilityChangedEventArgs<Player>> ObjectVisibilityChanged;

        /// <summary>
        /// Triggers the ObjectVisibilityChanged event.
        /// </summary>
        protected void OnObjectVisibilityChanged(VisibilityChangedEventArgs<Player> e)
        {
            ObjectVisibilityChanged.Raise(this, e);
        }

        #endregion
    }
}

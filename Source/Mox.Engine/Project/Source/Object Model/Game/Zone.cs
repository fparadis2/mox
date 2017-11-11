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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Mox.Transactions;

namespace Mox
{
    /// <summary>
    /// A zone is a collection of cards, with a specific meaning with regards to the game.
    /// </summary>
    public class Zone
    {
        #region Inner Types

        /// <summary>
        /// Possible zone ids.
        /// </summary>
        public enum Id
        {
            Library,
            Hand,
            Graveyard,
            Battlefield,
            Stack,
            Exile,
            PhasedOut
        }

        [Serializable]
        private class ShuffleCommand : Command
        {
            #region Variables

            private readonly int[] m_shuffleIndices;
            private readonly Id m_zoneId;
            private readonly Resolvable<Player> m_player;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public ShuffleCommand(Zone zone, Player player, int[] shuffleIndices)
            {
                m_zoneId = zone.ZoneId;
                m_player = player;
                m_shuffleIndices = shuffleIndices;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Executes (does or redoes) the command.
            /// </summary>
            public override void Execute(ObjectManager objectManager)
            {
                var game = (Game)objectManager;
                var zone = game.Zones[m_zoneId];
                var items = (CardsByPlayer)zone[m_player.Resolve(objectManager)];

                // Simple Fisher-Yates algorithm (http://en.wikipedia.org/wiki/Fisher-Yates_shuffle).
                int n = items.Count;
                Debug.Assert(m_shuffleIndices.Length == n, "Synchronization problem");

                while (n-- > 1)
                {
                    int k = m_shuffleIndices[n];    // 0 <= k < n.
                    Card temp = items[n];           // swap array[n] with array[k] (does nothing if k == n).
                    items[n] = items[k];
                    items[k] = temp;
                }

                game.Zones.OnCardCollectionChanged(CardCollectionChangedEventArgs.CollectionShuffled(items));
            }

            /// <summary>
            /// Unexecutes (undoes) the command.
            /// </summary>
            public override void Unexecute(ObjectManager objectManager)
            {
                var game = (Game)objectManager;
                var zone = game.Zones[m_zoneId];
                var items = (CardsByPlayer)zone[m_player.Resolve(objectManager)];

                /// Inverse of the above algorithm.
                Debug.Assert(m_shuffleIndices.Length == items.Count, "Synchronization problem");

                for (int n = 0; n < items.Count; n++)
                {
                    int k = m_shuffleIndices[n];
                    Card temp = items[n];
                    items[n] = items[k];
                    items[k] = temp;
                }

                game.Zones.OnCardCollectionChanged(CardCollectionChangedEventArgs.CollectionShuffled(items));
            }

            #endregion
        }

        private class CardsByPlayer : List<Card>, ICardCollection
        {
            #region Variables

            private readonly Zone m_ownerZone;
            private readonly Player m_ownerPlayer;

            #endregion

            #region Constructor

            public CardsByPlayer(Zone zone, Player player)
            {
                Debug.Assert(zone != null);
                Debug.Assert(player != null);

                m_ownerZone = zone;
                m_ownerPlayer = player;
            }

            #endregion

            #region Properties

            public Zone Zone
            {
                get { return m_ownerZone; }
            }

            public Player Player
            {
                get { return m_ownerPlayer; }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Moves the given <paramref name="cards"/> to the top of this zone.
            /// </summary>
            /// <param name="cards"></param>
            public void MoveToTop(IEnumerable<Card> cards)
            {
                Move(cards, -1);
            }

            /// <summary>
            /// Moves the given <paramref name="cards"/> to the bottom of this zone.
            /// </summary>
            /// <param name="cards"></param>
            public void MoveToBottom(IEnumerable<Card> cards)
            {
                Move(cards, 0);
            }

            private void Move(IEnumerable<Card> cards, int position)
            {
                using (m_ownerPlayer.Manager.Controller.BeginCommandGroup())
                {
                    List<Card> cardsToMove = new List<Card>(cards);

                    // Change controllers if needed
                    foreach (Card card in cardsToMove)
                    {
                        if (card.Controller != m_ownerPlayer)
                        {
                            card.Controller = m_ownerPlayer;
                        }
                    }

                    //for (int i = cardsToMove.Count - 1; i >= 0; i--) // Enumerating backwards ensure consistent rollback with transactions
                    foreach (Card card in cardsToMove)
                    {
                        card.Move(m_ownerZone, position);
                    }
                }
            }

            /// <summary>
            /// Shuffles the cards contained in this zone, for the current player.
            /// </summary>
            public void Shuffle()
            {
                int n = Count;
                int[] indices = m_ownerPlayer.Manager.Random.Shuffle(n);
                Debug.Assert(indices.Length == n, "Bad shuffle algorithm");

                m_ownerPlayer.Manager.Controller.Execute(new ShuffleCommand(m_ownerZone, m_ownerPlayer, indices));
            }

            public override string ToString()
            {
                return $"{m_ownerZone.Name} ({m_ownerPlayer.Name})";
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Id m_id;

        private readonly List<Card> m_allCards = new List<Card>();
        private CardsByPlayer[] m_cardsPerPlayer = new CardsByPlayer[4];

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a zone with the given <paramref name="id"/>.
        /// </summary>
        public Zone(Id id)
        {
            m_id = id;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the zone.
        /// </summary>
        public string Name
        {
            get 
            {

                return ZoneId.ToString();
            }
        }

        public Id ZoneId
        {
            get { return m_id; }
        }

        /// <summary>
        /// All the cards currently in the zone (for all the players).
        /// </summary>
        public IList<Card> AllCards
        {
            get { return m_allCards.AsReadOnly(); }
        }

        public virtual bool IsOwned 
        { 
            get { return false; } 
        }

        /// <summary>
        /// Cards currently in this zone and controlled by the given <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public ICardCollection this[Player controller]
        {
            get 
            {
                return GetCardsForController(controller);
            }
        }

        #endregion

        #region Methods

        #region Moving

        internal void EnsurePlayerHasZone(Player player)
        {
            if (player.Index >= m_cardsPerPlayer.Length)
            {
                int length = m_cardsPerPlayer.Length;
                do
                {
                    length *= 2;
                }
                while (player.Index >= length);

                Array.Resize(ref m_cardsPerPlayer, length);
            }

            if (m_cardsPerPlayer[player.Index] == null)
            {
                m_cardsPerPlayer[player.Index] = new CardsByPlayer(this, player);
            }
        }

        private CardsByPlayer GetCardsForController(Player controller)
        {
            Debug.Assert(controller != null);
            return m_cardsPerPlayer[controller.Index];
        }

        internal ICardCollection Add(Card card, ref int position)
        {
            var playerZone = GetCardsForController(card.Controller);

            m_allCards.Add(card);
            position = Add(playerZone, card, position);

            return playerZone;
        }

        internal ICardCollection Remove(Card card, out int position)
        {
            CardsByPlayer playerZone = GetCardsForController(card.Controller);

            position = playerZone.IndexOf(card);
            Debug.Assert(position >= 0);
            playerZone.RemoveAt(position);

            bool wasThere = m_allCards.Remove(card);
            Debug.Assert(wasThere, "Incoherent state!");

            return playerZone;
        }

        internal void UpdateController(Card card, Player oldController, Player newController, int position)
        {
            if (oldController != newController)
            {
                var oldCollection = GetCardsForController(oldController);
                oldCollection.Remove(card);
                
                var newCollection = GetCardsForController(newController);
                position = Add(newCollection, card, position);

                card.Manager.Zones.OnCardCollectionChanged(CardCollectionChangedEventArgs.CardMoved(oldCollection, newCollection, card, position));
            }
        }

        private static int Add(IList<Card> list, Card card, int position)
        {
            if (position == -1)
            {
                list.Add(card);
                return list.Count - 1;
            }
            else
            {
                list.Insert(position, card);
                return position;
            }
        }

        /// <summary>
        /// Called to know whether the given <paramref name="card"/> can be added to this zone.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public virtual bool CanAddCard(Card card)
        {
            return true;
        }

        /// <summary>
        /// Called to know whether the given card can change controller in this zone.
        /// </summary>
        public virtual bool CanChangeController(Card card, Player newController)
        {
            return true;
        }

        #endregion

        #region Misc

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[Zone: {0}]", Name);
        }

        #endregion

        #endregion
    }
}

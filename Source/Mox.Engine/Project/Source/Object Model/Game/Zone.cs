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
        internal class ChangeZoneOrControllerCommand : Object.SetValueCommand
        {
            #region Variables

            private readonly int m_newZonePosition;
            private readonly int m_oldZonePosition = -1;

            #endregion

            #region Constructor

            public ChangeZoneOrControllerCommand(Object obj, PropertyBase property, object newValue, Object.ISetValueAdapter adapter, int newZonePosition)
                : base(obj, property, newValue, adapter)
            {
                Debug.Assert(obj is Card);
                Card card = (Card)obj;

                if (card.Zone != null)
                {
                    var zoneCards = card.Zone[card.Controller];
                    int oldZonePosition = card.Zone[card.Controller].IndexOf(card);
                    Debug.Assert(oldZonePosition != -1);
                    m_oldZonePosition = oldZonePosition < zoneCards.Count - 1 ? oldZonePosition : -1;
                }
                m_newZonePosition = newZonePosition;
            }

            #endregion

            #region Properties

            public override bool IsEmpty
            {
                get
                {
                    return base.IsEmpty && m_oldZonePosition == m_newZonePosition;
                }
            }

            #endregion

            #region Methods

            protected override void SetValue(Object obj, object value, bool executing)
            {
                Debug.Assert(Property == Card.ZoneIdProperty || Property == Card.ControllerProperty);

                Card card = (Card)obj;
                Player oldController = card.Controller;
                Zone oldZone = card.Zone;

                using (SuspendPropertyChangedEvents(obj))
                {
                    base.SetValue(obj, value, executing);

                    int position = executing ? m_newZonePosition : m_oldZonePosition;

                    using (card.Manager.Zones.BeginUpdate())
                    {
                        Zone newZone = card.Zone;
                        CacheSynchronizer.UpdateZone(card, oldZone, newZone, position, Property == Card.ZoneIdProperty);

                        Player newController = card.Controller;
                        newZone.UpdateController(card, oldController, newController, position);
                    }
                }
            }

            #endregion
        }

        internal class CacheSynchronizer
        {
            #region Variables

            private readonly Game m_game;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            public CacheSynchronizer(Game game)
            {
                m_game = game;
                m_game.Objects.CollectionChanged += (sender, e) => e.Synchronize(RegisterObject, UnregisterObject);
            }

            #endregion

            #region Methods

            #region Register

            private void RegisterObject(Object o)
            {
                if (o is Card)
                {
                    RegisterCard((Card)o);
                }
            }

            private void RegisterCard(Card card)
            {
                card.PropertyChanging += Card_PropertyChanging;
                UpdateZone(card, null, card.Zone);
            }

            #endregion

            #region Unregister

            private void UnregisterObject(Object o)
            {
                if (o is Card)
                {
                    UnregisterCard((Card)o);
                }
            }

            private void UnregisterCard(Card card)
            {
                UpdateZone(card, card.Zone, null);
                card.PropertyChanging -= Card_PropertyChanging;
            }

            #endregion

            #region Update

            private static void UpdateZone(Card card, Zone oldZone, Zone newZone)
            {
                UpdateZone(card, oldZone, newZone, -1, false);
            }

            internal static void UpdateZone(Card card, Zone oldZone, Zone newZone, int position, bool forceMove)
            {
                if (oldZone != newZone || forceMove)
                {
                    // Remove from old zone.
                    if (oldZone != null)
                    {
                        bool wasThere = oldZone.Remove(card);
                        Debug.Assert(wasThere || forceMove, "Incoherent state");
                    }

                    // Add to new zone.
                    if (newZone != null)
                    {
                        newZone.Add(card, position);
                    }
                }
            }

            #endregion

            #endregion

            #region Event Handlers

            private void Card_PropertyChanging(object sender, PropertyChangingEventArgs e)
            {
                Card card = (Card)sender;

                if (e.Property == Card.ZoneIdProperty)
                {
                    if (!e.Cancel)
                    {
                        Zone newZone = m_game.Zones[(Id)e.NewValue];

                        if (!newZone.CanAddCard(card))
                        {
                            e.Cancel = true;
                        }
                        else if (card.Zone != null)
                        {
                            card.Manager.Events.Trigger(new Events.ZoneChangeEvent(card, card.Zone, newZone));
                        }
                    }
                }
                else if (e.Property == Card.ControllerProperty && card.Zone != null)
                {
                    e.Cancel = card.Zone.OnCardControllerChanging(card, (Player)e.NewValue);
                }
            }

            #endregion
        }

        private class CardByPlayerProxy : ReadOnlyCollection<Card>, ICardCollection
        {
            #region Inner Types
            
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

                private IList<Card> GetItems(ObjectManager objectManager)
                {
                    // This has to be one of the most beautiful piece of code ever written... NOT
                    Game game = (Game)objectManager;
                    IList<Card> items = ((CardByPlayerProxy)game.Zones[m_zoneId][m_player.Resolve(objectManager)]).Items;
                    Debug.Assert(!items.IsReadOnly, "Humm someone broke this thing :)");
                    return items;
                }

                /// <summary>
                /// Executes (does or redoes) the command.
                /// </summary>
                public override void Execute(ObjectManager objectManager)
                {
                    IList<Card> items = GetItems(objectManager);

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
                }

                /// <summary>
                /// Unexecutes (undoes) the command.
                /// </summary>
                public override void Unexecute(ObjectManager objectManager)
                {
                    IList<Card> items = GetItems(objectManager);

                    /// Inverse of the above algorithm.
                    Debug.Assert(m_shuffleIndices.Length == items.Count, "Synchronization problem");

                    for(int n = 0; n < items.Count; n++)
                    {
                        int k = m_shuffleIndices[n];
                        Card temp = items[n];
                        items[n] = items[k];
                        items[k] = temp;
                    }
                }

                #endregion
            }

            #endregion

            #region Variables

            private readonly Zone m_ownerZone;
            private readonly Player m_ownerPlayer;

            #endregion

            #region Constructor

            public CardByPlayerProxy(Zone zone, Player player, IList<Card> cards)
                : base(cards)
            {
                Debug.Assert(zone != null);
                Debug.Assert(player != null);

                m_ownerZone = zone;
                m_ownerPlayer = player;
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
                using (m_ownerPlayer.Manager.Controller.BeginTransaction())
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
                int n = Items.Count;
                int[] indices = m_ownerPlayer.Manager.Random.Shuffle(Items.Count);
                Debug.Assert(indices.Length == n, "Bad shuffle algorithm");

                m_ownerPlayer.Manager.Controller.Execute(new ShuffleCommand(m_ownerZone, m_ownerPlayer, indices));
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Id m_id;

        private readonly List<Card> m_allCards = new List<Card>();
        private readonly Dictionary<Player, List<Card>> m_cards = new Dictionary<Player, List<Card>>();

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

        internal Id ZoneId
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

        /// <summary>
        /// Cards currently in this zone and controlled by the given <paramref name="controller"/>.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public ICardCollection this[Player controller]
        {
            get 
            {
                return new CardByPlayerProxy(this, controller, GetCardsForController(controller));
            }
        }

        #endregion

        #region Methods

        #region Moving

        private IList<Card> GetCardsForController(Player controller)
        {
            Debug.Assert(controller != null);

            List<Card> cards;
            if (!m_cards.TryGetValue(controller, out cards))
            {
                cards = new List<Card>();
                m_cards.Add(controller, cards);
            }
            return cards;
        }

        private void Add(Card card, int position)
        {
            OnAddingCard(card);

            IList<Card> playerZone = GetCardsForController(card.Controller);
            m_allCards.Add(card);
            Add(playerZone, card, position);
        }

        private bool Remove(Card card)
        {
            OnRemovingCard(card);

            bool wasThere = GetCardsForController(card.Controller).Remove(card);
            wasThere &= m_allCards.Remove(card);
            return wasThere;
        }

        private void UpdateController(Card card, Player oldController, Player newController, int position)
        {
            if (oldController != newController)
            {
                if (oldController != null)
                {
                    GetCardsForController(oldController).Remove(card);
                }

                if (newController != null)
                {
                    Add(GetCardsForController(newController), card, position);
                }
            }
        }

        private static void Add(IList<Card> list, Card card, int position)
        {
            if (position == -1)
            {
                list.Add(card);
            }
            else
            {
                list.Insert(position, card);
            }
        }

        /// <summary>
        /// Called to know whether the given <paramref name="card"/> can be added to this zone.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        protected virtual bool CanAddCard(Card card)
        {
            return true;
        }

        /// <summary>
        /// Called *before* a card is added to this zone.
        /// </summary>
        /// <param name="card"></param>
        protected virtual void OnAddingCard(Card card)
        {
        }

        /// <summary>
        /// Called before removing a card from this zone.
        /// </summary>
        /// <param name="card"></param>
        protected virtual void OnRemovingCard(Card card)
        {
        }

        /// <summary>
        /// Called when the controller of a card in this zone changes.
        /// </summary>
        protected virtual bool OnCardControllerChanging(Card card, Player newController)
        {
            return false;
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

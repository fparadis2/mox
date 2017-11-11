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

using Mox.Collections;

namespace Mox
{
    partial class Game
    {
        #region Inner Types

        private class BattlefieldZone : Zone
        {
            #region Constructor

            public BattlefieldZone()
                : base(Id.Battlefield)
            {
            }

            #endregion

            #region Methods

            /// <summary>
            /// Cannot add instants and sorceries in play
            /// </summary>
            /// <param name="card"></param>
            /// <returns></returns>
            public override bool CanAddCard(Card card)
            {
                if (card.IsAny(Type.Instant | Type.Sorcery))
                {
                    return false;
                }

                return base.CanAddCard(card);
            }

            #endregion
        }

        /// <summary>
        /// A zone where cards can only be controlled by their owner.
        /// </summary>
        private class OwnedZone : Zone
        {
            #region Constructor

            public OwnedZone(Id id)
                : base(id)
            {
            }

            #endregion

            #region Properties

            public override bool IsOwned
            {
                get { return true; }
            }

            #endregion

            #region Methods

            public override bool CanChangeController(Card card, Player newController)
            {
                if (newController != card.Owner)
                {
                    return false;
                }

                return base.CanChangeController(card, newController);
            }

            #endregion
        }

        /// <summary>
        /// Zones of the game.
        /// </summary>
        public class GameZones
        {
            #region Variables

            private readonly Scope m_updateScope = new Scope();

            private readonly Zone m_library;
            private readonly Zone m_hand;
            private readonly Zone m_graveyard;
            private readonly Zone m_battlefield;
            private readonly Zone m_stack;
            private readonly Zone m_exile;
            private readonly Zone m_phasedOut;

            #endregion

            #region Constructor

            internal GameZones(Game game)
            {
                m_library = new OwnedZone(Zone.Id.Library);
                m_hand = new OwnedZone(Zone.Id.Hand);
                m_graveyard = new OwnedZone(Zone.Id.Graveyard);
                m_battlefield = new BattlefieldZone();
                m_stack = new Zone(Zone.Id.Stack);
                m_exile = new Zone(Zone.Id.Exile);
                m_phasedOut = new Zone(Zone.Id.PhasedOut);
            }

            #endregion

            #region Properties

            /// <summary>
            /// Library.
            /// </summary>
            public Zone Library
            {
                get { return m_library; }
            }

            /// <summary>
            /// Hand.
            /// </summary>
            public Zone Hand
            {
                get { return m_hand; }
            }

            /// <summary>
            /// Graveyard.
            /// </summary>
            public Zone Graveyard
            {
                get { return m_graveyard; }
            }

            /// <summary>
            /// Battlefield.
            /// </summary>
            public Zone Battlefield
            {
                get { return m_battlefield; }
            }

            /// <summary>
            /// The stack.
            /// </summary>
            public Zone Stack
            {
                get { return m_stack; }
            }

            /// <summary>
            /// Removed From The Game.
            /// </summary>
            public Zone Exile
            {
                get { return m_exile; }
            }

            /// <summary>
            /// Phased Out.
            /// </summary>
            public Zone PhasedOut
            {
                get { return m_phasedOut; }
            }

            internal Zone this[Zone.Id id]
            {
                get
                {
                    switch (id)
                    {
                        case Zone.Id.Library:
                            return m_library;

                        case Zone.Id.Hand:
                            return m_hand;

                        case Zone.Id.Graveyard:
                            return m_graveyard;

                        case Zone.Id.Battlefield:
                            return m_battlefield;

                        case Zone.Id.Stack:
                            return m_stack;

                        case Zone.Id.Exile:
                            return m_exile;

                        case Zone.Id.PhasedOut:
                            return m_phasedOut;

                        default:
                            throw new ArgumentException("Unknown zone");
                    }
                }
            }

            #endregion

            #region Methods

            internal void EnsurePlayerHasZone(Player player)
            {
                m_library.EnsurePlayerHasZone(player);
                m_hand.EnsurePlayerHasZone(player);
                m_graveyard.EnsurePlayerHasZone(player);
                m_battlefield.EnsurePlayerHasZone(player);
                m_stack.EnsurePlayerHasZone(player);
                m_exile.EnsurePlayerHasZone(player);
                m_phasedOut.EnsurePlayerHasZone(player);
            }

            #endregion

            #region Updating

            internal IDisposable BeginUpdate()
            {
                return m_updateScope.Begin();
            }

            internal bool IsUpdating
            {
                get { return m_updateScope.InScope; }
            }

            #endregion

            #region Events

            public event EventHandler<CardCollectionChangedEventArgs> CardCollectionChanged;

            internal void OnCardCollectionChanged(CardCollectionChangedEventArgs e)
            {
                CardCollectionChanged?.Invoke(this, e);
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly GameZones m_zones;
        private readonly SpellStack m_spellStack;

        #endregion

        #region Properties

        /// <summary>
        /// Zones of the game.
        /// </summary>
        public GameZones Zones
        {
            get { return m_zones; }
        }

        /// <summary>
        /// Spell stack.
        /// </summary>
        public SpellStack SpellStack
        {
            get { return m_spellStack; }
        }

        #endregion
    }

    public class CardCollectionChangedEventArgs : EventArgs
    {
        public ICardCollection OldCollection { get; private set; }
        public ICardCollection NewCollection { get; private set; }

        public ChangeType Type { get; private set; }
        public Card Card { get; private set; }
        public int NewPosition { get; private set; }

        public static CardCollectionChangedEventArgs CollectionShuffled(ICardCollection collection)
        {
            return new CardCollectionChangedEventArgs { OldCollection = collection, NewCollection = collection, Type = ChangeType.Shuffle };
        }

        public static CardCollectionChangedEventArgs CardMoved(ICardCollection oldCollection, ICardCollection newCollection, Card card, int position)
        {
            return new CardCollectionChangedEventArgs
            {
                Type = ChangeType.CardMoved,
                OldCollection = oldCollection,
                NewCollection = newCollection,
                Card = card,
                NewPosition = position
            };
        }

        public enum ChangeType
        {
            Shuffle,
            CardMoved
        }
    }
}

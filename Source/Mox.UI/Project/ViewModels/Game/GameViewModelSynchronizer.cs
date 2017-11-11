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
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using Mox.Collections;
using System.Linq;

namespace Mox.UI.Game
{
    public class GameViewModelSynchronizer : IDisposable
    {
        #region Inner Types

        private class CardSynchroInfo
        {
            public readonly CardViewModel ViewModel;
            public CardCollectionViewModel CurrentCollection;

            public CardSynchroInfo(CardViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }

        #endregion

        #region Variables

        private readonly Dispatcher m_dispatcher;
        private readonly GameViewModel m_model;
        private readonly Mox.Game m_game;
        private readonly Player m_mainPlayer;

        private readonly Dictionary<int, CardSynchroInfo> m_cards = new Dictionary<int, CardSynchroInfo>();
        private readonly Dictionary<int, PlayerViewModel> m_players = new Dictionary<int, PlayerViewModel>();

        #endregion

        #region Constructor

        public GameViewModelSynchronizer(GameViewModel modelToSynchronizeTo, Mox.Game sourceGame, Player mainPlayer, Dispatcher dispatcher)
        {
            Throw.IfNull(modelToSynchronizeTo, "modelToSynchronizeTo");
            Throw.IfNull(sourceGame, "sourceGame");
            Throw.InvalidArgumentIf(mainPlayer != null && mainPlayer.Manager != sourceGame, "Invalid player", "mainPlayer");

            m_model = modelToSynchronizeTo;
            m_game = sourceGame;
            m_mainPlayer = mainPlayer;
            m_dispatcher = dispatcher;

            m_model.Source = m_game;

            m_game.Objects.CollectionChanged += Objects_CollectionChanged;
            m_game.Objects.ForEach(o => Register(o, true));

            m_game.SpellStack.CollectionChanged += SpellStack_CollectionChanged;
            m_game.SpellStack.ForEach(PushSpell);

            RegisterZones();
        }

        public void Dispose()
        {
            m_game.SpellStack.ForEach(PopSpell);
            m_game.SpellStack.CollectionChanged -= SpellStack_CollectionChanged;

            m_game.Objects.ForEach(Unregister);
            m_game.Objects.CollectionChanged -= Objects_CollectionChanged;
        }

        #endregion

        #region Methods

        #region Register / Unregister

        private void Register(Object obj, bool init)
        {
            if (obj is Card)
            {
                RegisterCard((Card)obj, init);
            }
            else if (obj is Player)
            {
                RegisterPlayer((Player)obj);
            }
            else if (obj is GameState)
            {
                RegisterGameState((GameState)obj);
            }
            else if (obj is CombatData)
            {
                RegisterCombatData((CombatData)obj);
            }
        }

        private void Unregister(Object obj)
        {
            if (obj is Card)
            {
                UnregisterCard((Card)obj);
            }
            else if (obj is Player)
            {
                UnregisterPlayer((Player)obj);
            }
            else if (obj is GameState)
            {
                UnregisterGameState((GameState)obj);
            }
            else if (obj is CombatData)
            {
                UnregisterCombatData((CombatData)obj);
            }
        }

        #region RegisterCard

        private void RegisterCard(Card card, bool init)
        {
            Debug.Assert(card != null);

            CardViewModel cardViewModel = new CardViewModel(m_model) { Identifier = card.Identifier, Source = card };
            CardSynchroInfo synchroInfo = new CardSynchroInfo(cardViewModel);
            m_cards.Add(card.Identifier, synchroInfo);
            m_model.AllCards.Add(cardViewModel);

            if (!init)
            {
                var collection = card.Zone[card.Controller];
                var collectionViewModel = GetCollection(card.ZoneId, card.Controller);

                if (collectionViewModel != null)
                {
                    RefreshZone(collection, collectionViewModel);
                    synchroInfo.CurrentCollection = collectionViewModel;
                }
            }

            UpdateAllProperties(card, cardViewModel);

            card.PropertyChanged += card_PropertyChanged;
        }

        private void UnregisterCard(Card card)
        {
            Debug.Assert(card != null);
            Debug.Assert(m_cards.ContainsKey(card.Identifier));

            CardSynchroInfo synchroInfo = GetCardSynchroInfo(card);

            if (synchroInfo.CurrentCollection != null)
                synchroInfo.CurrentCollection.Remove(synchroInfo.ViewModel);

            card.PropertyChanged -= card_PropertyChanged;

            m_cards.Remove(card.Identifier);
            m_model.AllCards.Remove(synchroInfo.ViewModel);
        }

        #endregion

        #region RegisterPlayer

        private void RegisterPlayer(Player player)
        {
            Debug.Assert(player != null);

            PlayerViewModel playerViewModel = new PlayerViewModel(m_model) { Identifier = player.Identifier, Source = player };
            m_players.Add(player.Identifier, playerViewModel);

            if (player == m_mainPlayer)
            {
                m_model.MainPlayer = playerViewModel;
                m_model.Players.Insert(0, playerViewModel);
            }
            else
            {
                m_model.Players.Add(playerViewModel);
                playerViewModel.Battlefield.InvertY = true;
            }

            player.PropertyChanged += player_PropertyChanged;
            player.ManaPool.Changed += ManaPool_Changed;

            PlayerViewModel playerModel = GetPlayerViewModel(player);
            if (playerModel != null)
            {
                UpdateAllProperties(player, playerModel);
                UpdateManaPool(player.ManaPool, playerModel.ManaPool);
            }
        }

        private void UnregisterPlayer(Player player)
        {
            Debug.Assert(player != null);

            player.PropertyChanged -= player_PropertyChanged;
            player.ManaPool.Changed -= ManaPool_Changed;

            PlayerViewModel playerViewModel = GetPlayerViewModel(player);

            if (player == m_mainPlayer)
            {
                m_model.MainPlayer = null;
            }

            m_model.Players.Remove(playerViewModel);
            m_players.Remove(player.Identifier);
        }

        #endregion

        #region RegisterGameState

        private void RegisterGameState(GameState state)
        {
            Debug.Assert(state != null);

            state.PropertyChanged += state_PropertyChanged;
            UpdateAllProperties(state, m_model.State);
        }

        private void UnregisterGameState(GameState state)
        {
            Debug.Assert(state != null);

            state.PropertyChanged -= state_PropertyChanged;
        }

        #endregion

        #region RegisterCombatData

        private void RegisterCombatData(CombatData combatData)
        {
            Debug.Assert(combatData != null);

            combatData.PropertyChanged += combatData_PropertyChanged;
            UpdateAttackers(combatData.Attackers);
        }

        private void UnregisterCombatData(CombatData combatData)
        {
            Debug.Assert(combatData != null);

            combatData.PropertyChanged -= combatData_PropertyChanged;
        }

        void combatData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Property == CombatData.AttackersProperty)
            {
                UpdateAttackers((DeclareAttackersResult)e.NewValue);
            }
        }

        private void UpdateAttackers(DeclareAttackersResult attackersResult)
        {
            attackersResult = attackersResult ?? DeclareAttackersResult.Empty;

            foreach (var card in m_model.AllCards)
            {
                card.IsAttacking = attackersResult.IsAttacking(card.Source);
            }
        }

        #endregion

        #region Zones

        private void RegisterZones()
        {
            m_game.Zones.CardCollectionChanged += Zones_CardCollectionChanged;

            foreach (var player in m_game.Players)
            {
                var playerViewModel = GetPlayerViewModel(player);
                Debug.Assert(playerViewModel != null);

                RefreshZone(player.Library, playerViewModel.Library);
                RefreshZone(player.Hand, playerViewModel.Hand);
                RefreshZone(player.Graveyard, playerViewModel.Graveyard);
                RefreshZone(player.Battlefield, playerViewModel.Battlefield);
            }
        }

        private void RefreshZone(ICardCollection collection, CardCollectionViewModel collectionViewModel)
        {
            collectionViewModel.Clear();

            foreach (var card in collection)
            {
                var cardSyncInfo = GetCardSynchroInfo(card);
                collectionViewModel.Add(cardSyncInfo.ViewModel);
                cardSyncInfo.CurrentCollection = collectionViewModel;
            }
        }

        private void UnregisterZones()
        {
            m_game.Zones.CardCollectionChanged -= Zones_CardCollectionChanged;
        }

        private void Zones_CardCollectionChanged(object sender, CardCollectionChangedEventArgs e)
        {
            BeginDispatch(() =>
            {
                switch (e.Type)
                {
                    case CardCollectionChangedEventArgs.ChangeType.Shuffle:
                        {
                            var collection = e.NewCollection;
                            var collectionViewModel = GetCollection(collection);

                            if (collectionViewModel != null)
                                RefreshZone(collection, collectionViewModel);

                            break;
                        }

                    case CardCollectionChangedEventArgs.ChangeType.CardMoved:
                        {
                            var cardSyncInfo = GetCardSynchroInfo(e.Card);
                            var cardViewModel = cardSyncInfo.ViewModel;

                            var oldCollectionViewModel = GetCollection(e.OldCollection);
                            if (oldCollectionViewModel != null)
                                oldCollectionViewModel.Remove(cardViewModel);

                            var newCollectionViewModel = GetCollection(e.NewCollection);
                            if (newCollectionViewModel != null)
                                newCollectionViewModel.Insert(e.NewPosition, cardViewModel);

                            cardSyncInfo.CurrentCollection = newCollectionViewModel;

                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
            });
        }

        private CardCollectionViewModel GetCollection(ICardCollection collection)
        {
            return GetCollection(collection.Zone.ZoneId, collection.Player);
        }

        private CardCollectionViewModel GetCollection(Zone.Id id, Player player)
        {
            PlayerViewModel playerModel = GetPlayerViewModel(player);
            if (playerModel == null)
                return null;

            switch (id)
            {
                case Zone.Id.Library:
                    return playerModel.Library;
                case Zone.Id.Hand:
                    return playerModel.Hand;
                case Zone.Id.Graveyard:
                    return playerModel.Graveyard;
                case Zone.Id.Battlefield:
                    return playerModel.Battlefield;
                case Zone.Id.Stack:
                case Zone.Id.Exile:
                case Zone.Id.PhasedOut:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #region Misc

        private object GetViewModel(Object obj)
        {
            if (obj is Card)
            {
                return GetCardViewModel((Card)obj);
            }
            if (obj is Player)
            {
                return GetPlayerViewModel((Player)obj);
            }
            if (obj is GameState)
            {
                return m_model.State;
            }

            throw new NotImplementedException();
        }

        public PlayerViewModel GetPlayerViewModel(Player player)
        {
            return m_players[player.Identifier];
        }

        public CardViewModel GetCardViewModel(Card card)
        {
            return GetCardSynchroInfo(card).ViewModel;
        }

        private CardSynchroInfo GetCardSynchroInfo(Card card)
        {
            CardSynchroInfo info = m_cards[card.Identifier];
            Debug.Assert(info != null && info.ViewModel != null, "Synchronization problem");
            return info;
        }

        private void BeginDispatch(System.Action action)
        {
            if (m_dispatcher != null && m_dispatcher.Thread != Thread.CurrentThread)
            {
                m_dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            }
            else
            {
                action();
            }
        }

        #endregion

        #region Update

        #region Generic

        private void UpdateAllProperties(Object obj, object viewModel)
        {
            foreach (var property in PropertyBase.GetAllProperties(obj.GetType()))
            {
                UpdateProperty(property.Name, viewModel, obj.GetValue(property));
            }
        }

        private void UpdateProperty(string propertyName, object viewModelObject, object value)
        {
            PropertyInfo propertyInfo = viewModelObject.GetType().GetProperty(propertyName);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                if (value is Object)
                {
                    value = GetViewModel((Object)value);
                }

                propertyInfo.SetValue(viewModelObject, value, null);
            }
        }

        #endregion

        private static void UpdateManaPool(PlayerManaPool manaPool, ManaPoolViewModel viewModel)
        {
            foreach (var mana in viewModel.AllMana)
            {
                mana.Amount = manaPool[mana.Color];
            }
        }

        private static void UpdateCardCollection(CardSynchroInfo synchroInfo, CardCollectionViewModel newCollection)
        {
            Debug.Assert(synchroInfo != null);

            if (synchroInfo.CurrentCollection != null)
            {
                bool result = synchroInfo.CurrentCollection.Remove(synchroInfo.ViewModel);
                Debug.Assert(result, "Synchronisation problem");
                synchroInfo.CurrentCollection = null;
            }

            if (newCollection != null)
            {
                synchroInfo.CurrentCollection = newCollection;
                newCollection.Add(synchroInfo.ViewModel);
            }
        }

        #endregion

        #region Spell Stack

        private void SpellStack_CollectionChanged(object sender, CollectionChangedEventArgs<Spell> e)
        {
            BeginDispatch(() => e.Synchronize(PushSpell, PopSpell));
        }

        private void PushSpell(Spell spell)
        {
            var viewModel = new SpellViewModel
            {
                CardIdentifier = spell.Source.CardIdentifier,
                AbilityText = spell.Ability.AbilityText
            };

            var spells = m_model.SpellStack.Spells;
            spells.Add(viewModel);
        }

        private void PopSpell(Spell spell)
        {
            // Should always be the last one
            var spells = m_model.SpellStack.Spells;
            spells.RemoveAt(spells.Count - 1);
        }

        #endregion

        #endregion

        #region Event Handlers

        void Objects_CollectionChanged(object sender, CollectionChangedEventArgs<Object> e)
        {
            e.Synchronize(o => Register(o, false), Unregister);
        }

        void card_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                BeginDispatch(() =>
                {
                    CardSynchroInfo synchroInfo = GetCardSynchroInfo((Card)e.Object);
                    var cardViewModel = synchroInfo.ViewModel;

                    UpdateProperty(e.Property.Name, cardViewModel, e.NewValue);

                    cardViewModel.OnModelPropertyChanged(e);

                    if (synchroInfo.CurrentCollection != null)
                        synchroInfo.CurrentCollection.OnCardChanged(cardViewModel, e);
                });
            }
        }

        void player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                PlayerViewModel playerViewModel = GetPlayerViewModel((Player)e.Object);
                if (playerViewModel != null)
                {
                    UpdateProperty(e.Property.Name, playerViewModel, e.NewValue);
                }
            }
        }

        void state_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                GameStateViewModel stateModel = m_model.State;
                UpdateProperty(e.Property.Name, stateModel, e.NewValue);
            }
        }

        void ManaPool_Changed(object sender, EventArgs e)
        {
            var changedPool = (PlayerManaPool)sender;

            PlayerViewModel playerViewModel = GetPlayerViewModel(changedPool.Player);
            if (playerViewModel != null)
            {
                UpdateManaPool(changedPool, playerViewModel.ManaPool);
            }
        }

        #endregion
    }
}

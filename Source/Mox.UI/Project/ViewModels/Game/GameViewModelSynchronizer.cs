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
            m_game.Objects.ForEach(Register);

            m_game.SpellStack.CollectionChanged += SpellStack_CollectionChanged;
            m_game.SpellStack.ForEach(PushSpell);
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

        private void Register(Object obj)
        {
            if (obj is Card)
            {
                RegisterCard((Card)obj);
            }
            else if (obj is Player)
            {
                RegisterPlayer((Player)obj);
            }
            else if (obj is GameState)
            {
                RegisterGameState((GameState)obj);
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
        }

        #region RegisterCard

        private void RegisterCard(Card card)
        {
            Debug.Assert(card != null);

            CardViewModel cardViewModel = new CardViewModel(m_model) { Identifier = card.Identifier, Source = card };
            CardSynchroInfo synchroInfo = new CardSynchroInfo(cardViewModel);
            m_cards.Add(card.Identifier, synchroInfo);
            m_model.AllCards.Add(cardViewModel);

            card.PropertyChanged += card_PropertyChanged;
            UpdateCardCollection(synchroInfo, GetCollection(card));
            UpdateAllProperties(card, cardViewModel);
        }

        private void UnregisterCard(Card card)
        {
            Debug.Assert(card != null);
            Debug.Assert(m_cards.ContainsKey(card.Identifier));

            CardSynchroInfo synchroInfo = GetCardSynchroInfo(card);

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

        #region RegisterCard

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

        private CardCollectionViewModel GetCollection(Card card)
        {
            if (card.ZoneId == Zone.Id.Stack)
            {
                return m_model.StackCards;
            }

            PlayerViewModel playerModel = GetPlayerViewModel(card.Controller);

            if (playerModel == null)
            {
                return null;
            }

            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(typeof(PlayerViewModel))[card.ZoneId.ToString()];

            if (descriptor == null)
            {
                return null;
            }

            return (CardCollectionViewModel)descriptor.GetValue(playerModel);
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

            if (propertyInfo != null)
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

        void Objects_CollectionChanged(object sender, Collections.CollectionChangedEventArgs<Object> e)
        {
            e.Synchronize(Register, Unregister);
        }

        void card_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                CardSynchroInfo synchroInfo = GetCardSynchroInfo((Card)e.Object);
                if (e.Property == Card.ZoneIdProperty || e.Property == Card.ControllerProperty)
                {
                    BeginDispatch(() => UpdateCardCollection(synchroInfo, GetCollection((Card)e.Object)));
                }
                else
                {
                    BeginDispatch(() => UpdateProperty(e.Property.Name, synchroInfo.ViewModel, e.NewValue));
                }
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

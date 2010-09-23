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
using System.Linq;
using System.Text;
using System.Windows;

namespace Mox.UI
{
    public class GameViewModel : ViewModel
    {
        #region Variables
        
        private readonly InteractionModel m_interactionModel;
        private readonly ObservableCollection<PlayerViewModel> m_players = new ObservableCollection<PlayerViewModel>();

        private readonly ObservableCollection<CardViewModel> m_allCards = new ObservableCollection<CardViewModel>();
        private readonly CardCollectionViewModel m_stack = new CardCollectionViewModel();
        private readonly GameStateViewModel m_state = new GameStateViewModel();

        private PlayerViewModel m_mainPlayer;

        #endregion

        #region Constructor

        public GameViewModel()
        {
            m_interactionModel = new InteractionModel(this);

            m_state.PropertyChanged += m_state_PropertyChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Main player of the game (player the user is playing)
        /// </summary>
        public PlayerViewModel MainPlayer
        {
            get { return m_mainPlayer; }
            set 
            {
                if (m_mainPlayer != value)
                {
                    m_mainPlayer = value;
                    OnPropertyChanged("MainPlayer");
                }
            }
        }

        /// <summary>
        /// Whether the <see cref="MainPlayer"/> is active.
        /// </summary>
        public bool IsActivePlayer
        {
            get { return State.ActivePlayer == null || MainPlayer == State.ActivePlayer; }
        }

        /// <summary>
        /// Players in the game.
        /// </summary>
        /// <remarks>
        /// Player 0 is always the main player.
        /// </remarks>
        public ObservableCollection<PlayerViewModel> Players
        {
            get { return m_players; }
        }

        /// <summary>
        /// Interaction model.
        /// </summary>
        public InteractionModel Interaction
        {
            get { return m_interactionModel; }
        }

        /// <summary>
        /// All cards currently in the model.
        /// </summary>
        public ICollection<CardViewModel> AllCards
        {
            get { return m_allCards; }
        }

        /// <summary>
        /// Source.
        /// </summary>
        public Game Source
        {
            get;
            internal set;
        }

        /// <summary>
        /// The game stack.
        /// </summary>
        public CardCollectionViewModel Stack
        {
            get { return m_stack; }
        }

        /// <summary>
        /// The current game state.
        /// </summary>
        public GameStateViewModel State
        {
            get { return m_state; }
        }

        #endregion

        #region Methods

        public void ResetInteraction()
        {
            Interaction.UserChoiceInteraction = null;

            AllCards.ForEach(card => card.ResetInteraction());
            Players.ForEach(player => player.ResetInteraction());
        }

        #endregion

        #region Event Handlers

        void m_state_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ActivePlayer":
                    OnPropertyChanged("IsActivePlayer");
                    break;
            }
        }

        #endregion
    }
}

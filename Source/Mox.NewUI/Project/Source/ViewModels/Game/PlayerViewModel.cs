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
using System.Windows.Input;

namespace Mox.UI.Game
{
    public class PlayerViewModel : GameElementViewModel
    {
        #region Variables

        private readonly CardCollectionViewModel m_hand = new CardCollectionViewModel();
        private readonly CardCollectionViewModel m_library = new CardCollectionViewModel();
        private readonly CardCollectionViewModel m_graveyard = new CardCollectionViewModel();
        private readonly CardCollectionViewModel m_battlefield = new CardCollectionViewModel();

        private readonly ManaPoolViewModel m_manaPool = new ManaPoolViewModel();

        private bool m_canBeChosen;
        private string m_name;
        private int m_life;

        #endregion

        #region Constructor

        public PlayerViewModel(GameViewModel gameViewModel)
            : base(gameViewModel)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Name of the player.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name != value)
                {
                    m_name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        /// <summary>
        /// Life of the player.
        /// </summary>
        public int Life
        {
            get { return m_life; }
            set 
            {
                if (m_life != value)
                {
                    m_life = value;
                    NotifyOfPropertyChange(() => Life);
                }
            }
        }

        /// <summary>
        /// Whether the player can be "selected" by the user.
        /// </summary>
        public bool CanBeChosen
        {
            get { return m_canBeChosen; }
            set 
            {
                if (m_canBeChosen != value)
                {
                    m_canBeChosen = value;
                    NotifyOfPropertyChange(() => CanBeChosen);
                }
            }
        }

        /// <summary>
        /// Mana pool of the player.
        /// </summary>
        public ManaPoolViewModel ManaPool
        {
            get { return m_manaPool; }
        }

        public CardCollectionViewModel Hand
        {
            get { return m_hand; }
        }

        public CardCollectionViewModel Battlefield
        {
            get { return m_battlefield; }
        }

        public CardCollectionViewModel Library
        {
            get { return m_library; }
        }

        public CardCollectionViewModel Graveyard
        {
            get { return m_graveyard; }
        }

        /// <summary>
        /// Identifier of the player.
        /// </summary>
        public int Identifier { get; internal set; }

        /// <summary>
        /// Underlying player.
        /// </summary>
        public Player Source 
        { 
            get; internal set; 
        }

        public bool IsMainPlayer
        {
            get { return GameViewModel.MainPlayer == this; }
        }

        #endregion

        #region Methods

        internal void ResetInteraction()
        {
            CanBeChosen = false;
            ManaPool.ResetInteraction();
        }

        public void Choose()
        {
            if (CanBeChosen)
            {
                GameViewModel.Interaction.OnPlayerChosen(new PlayerChosenEventArgs(this));
            }
        }

        #region Commands

        #region ChoosePlayer Command

        /// <summary>
        /// Command to choose a player.
        /// </summary>
        public ICommand ChooseCommand
        {
            get { return new RelayCommand(p => CanBeChosen, p => Choose()); }
        }

        #endregion

        #endregion

        #endregion
    }
}

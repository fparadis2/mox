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
using Caliburn.Micro;

namespace Mox.UI.Game
{
    /// <summary>
    /// Root class for player interaction by a UI.
    /// </summary>
    public class InteractionModel : PropertyChangedBase
    {
        #region Variables

        private readonly GameViewModel m_owner;
        private UserChoiceInteractionModel m_userChoiceInteraction;

        #endregion

        #region Constructor

        public InteractionModel(GameViewModel owner)
        {
            Throw.IfNull(owner, "owner");
            m_owner = owner;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Current choice interaction, if any.
        /// </summary>
        public UserChoiceInteractionModel UserChoiceInteraction
        {
            get { return m_userChoiceInteraction; }
            set 
            {
                if (m_userChoiceInteraction != value)
                {
                    m_userChoiceInteraction = value;
                    NotifyOfPropertyChange(() => UserChoiceInteraction);
                    NotifyOfPropertyChange(() => IsUserInteractionVisible);
                }
            }
        }

        public bool IsUserInteractionVisible
        {
            get { return UserChoiceInteraction != null; }
        }

        #region Commands

        #region SelectChoice

        public void SelectChoice(UserChoiceModel userChoice)
        {
            Throw.InvalidArgumentIf(UserChoiceInteraction == null || !UserChoiceInteraction.Choices.Contains(userChoice), "Choice is not in the Choices collection", "parameter");
            OnUserChoiceSelected(new ItemEventArgs<UserChoiceModel>(userChoice));
        }

        #endregion

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the user selects a choice
        /// </summary>
        public event EventHandler<ItemEventArgs<UserChoiceModel>> UserChoiceSelected;

        /// <summary>
        /// Triggers the UserChoiceSelected event.
        /// </summary>
        protected void OnUserChoiceSelected(ItemEventArgs<UserChoiceModel> e)
        {
            UserChoiceSelected.Raise(this, e);
        }

        /// <summary>
        /// Triggered when a card is chosen by the user
        /// </summary>
        public event EventHandler<CardChosenEventArgs> CardChosen;

        /// <summary>
        /// Triggers the CardChosen event.
        /// </summary>
        protected internal void OnCardChosen(CardChosenEventArgs e)
        {
            CardChosen.Raise(this, e);
        }

        /// <summary>
        /// Triggered when a player is chosen by the user
        /// </summary>
        public event EventHandler<PlayerChosenEventArgs> PlayerChosen;

        /// <summary>
        /// Triggers the PlayerChosen event.
        /// </summary>
        protected internal void OnPlayerChosen(PlayerChosenEventArgs e)
        {
            PlayerChosen.Raise(this, e);
        }

        #endregion
    }
}
